using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using GloryMod.Systems;
using System.IO;
using Terraria.GameContent;
using GloryMod.NPCs.BasaltBarriers.Projectiles;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Minions
{
    internal class Forsaken : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.ShadowFlame] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(40);
            DrawOffsetY = -4f;

            NPC.damage = 0;
            NPC.defense = 25;
            NPC.lifeMax = Main.getGoodWorld ? 1500 : 1200;
            NPC.knockBackResist = 0.1f;
            NPC.value = Item.buyPrice(0, 0, 0, 5);
            NPC.npcSlots = 1f;
            NPC.chaseable = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = false;
            NPC.HitSound = SoundID.NPCHit41 with { Pitch = -0.5f };
            NPC.DeathSound = SoundID.NPCDeath43 with { Pitch = -0.5f };
            NPC.behindTiles = true;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        NPC owner;

        public ref float HitCooldown => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float SpawnOrder => ref NPC.ai[2];
        public ref float WhoIsOwner => ref NPC.ai[3];

        private Vector2 positionPoint;
        private Vector2 positionModifier;
        private float AIState;

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(AIState);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                AIState = reader.ReadInt32();
            }
        }

        public override void AI()
        {
            owner = Main.npc[(int)WhoIsOwner];

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            AITimer++;
            HitCooldown--;

            positionPoint = owner.Center + positionModifier;
            NPC.velocity *= 0.96f;

            if (NPC.Distance(positionPoint) > 20)
            {
                NPC.velocity += NPC.DirectionTo(positionPoint) * 0.5f;
            }

            // Head rotation.
            if (AIState != 3) NPC.rotation = NPC.rotation.AngleLerp(NPC.DirectionTo(target.Center).ToRotation() + (NPC.spriteDirection == 1 ? MathHelper.Pi : -MathHelper.Pi), 0.05f);

            switch (AIState)
            {
                case 0: // Passive

                    if (AITimer % 200 == 1) FindPosition();

                    break;

                case 1: // Attacking

                    if (AITimer % 3 == 1)
                    {
                        Vector2 dustRand = new Vector2(Main.rand.NextFloat(20, 30), 0).RotatedByRandom(MathHelper.TwoPi);
                        int dust = Dust.NewDust(NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation) + dustRand, 0, 0, 6, Scale: AITimer * 0.05f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = false;
                        Main.dust[dust].velocity = -dustRand * 0.1f;
                        Main.dust[dust].velocity += NPC.velocity * .5f;
                        Main.dust[dust].position += NPC.velocity;
                    }

                    if (AITimer == 10) SoundEngine.PlaySound(SoundID.DD2_DrakinBreathIn with { Pitch = - .25f}, NPC.Center);

                    if (AITimer == 54)
                    {
                        int numDusts = 16;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation), 0, 0, 6, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Vector2 trueVelocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            trueVelocity.X *= 0.5f;
                            trueVelocity = trueVelocity.RotatedBy(NPC.rotation) - new Vector2(4, 0).RotatedBy(NPC.rotation);
                            Main.dust[dust].velocity = trueVelocity;
                        }

                        NPC.velocity += new Vector2(-2 * NPC.spriteDirection, 0).RotatedBy(NPC.rotation);

                        SoundEngine.PlaySound(SoundID.DD2_DrakinShot, NPC.Center);
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation), new Vector2(1, 0).RotatedBy(NPC.rotation + MathHelper.Pi), ProjectileType<BBSpiritBolt>(), 80, 0, target.whoAmI);
                        Main.projectile[proj].ai[1] = Main.rand.NextFloat(-3, 4);
                        Main.projectile[proj].ai[2] = Main.rand.NextFloat(8, 13);
                        NPC.netUpdate = true;
                    }

                    break;

                case 2: // Dying

                    NPC.rotation += 0.0025f * (Main.rand.NextBool() ? AITimer : -AITimer);
                    heat = MathHelper.Lerp(heat, 1, 0.0111f);

                    if (AITimer % 3 == 1)
                    {
                        Vector2 dustRand = new Vector2(Main.rand.NextFloat(30, 50), 0).RotatedByRandom(MathHelper.TwoPi);
                        int dust = Dust.NewDust(NPC.Center + dustRand, 0, 0, 6, Scale: AITimer * 0.067f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = false;
                        Main.dust[dust].velocity = -dustRand * 0.1f;
                        Main.dust[dust].velocity += NPC.velocity * .5f;
                        Main.dust[dust].position += NPC.velocity;
                    }

                    if (AITimer == 30) SoundEngine.PlaySound(SoundID.DD2_DrakinBreathIn with { Pitch = -.33f, Volume = 1.25f }, NPC.Center);

                    if (AITimer > 90)
                    {
                        int numDusts = 25;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 6, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = false;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5, 10), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<ForsakenExplosion>(), 100 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 0, target.whoAmI);
                        SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);
                        ScreenUtils.screenShaking = 5f;

                        NPC.active = false;
                        NPC.netUpdate = true;
                    }

                    break;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (HitCooldown <= 0)
            {
                HitCooldown = 100;
                AITimer = 0;
                AIState = 1;
                NPC.netUpdate = true;
            }

            if (NPC.life <= 0)
            {
                AITimer = 0;
                AIState = 2;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width();
            if (AIState == 1 && AITimer > 24) NPC.frameCounter++;
            if (AIState != 1) NPC.frame.Y = 0;

            if (NPC.frameCounter > 5)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;
            }
            if (NPC.frame.Y >= frameHeight * 6)
            {
                NPC.frame.Y = 0;
                AIState = 0;
                AITimer = 0;
                NPC.netUpdate = true;
            }
        }

        void FindPosition()
        {
            float direction = owner.spriteDirection;           
            positionModifier = owner.ai[2] > 1 ? new Vector2(Main.rand.NextFloat(200, 401) * direction, 0).RotatedBy(SpawnOrder * MathHelper.ToRadians(160) / 5) : new Vector2(Main.rand.NextFloat(200, 401) * direction, 0).RotatedByRandom(MathHelper.PiOver4);
        }

        float timer;
        float heat;
        float sparkle;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D textureMask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D chain = Request<Texture2D>(Texture + "Chain").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);

            SpriteEffects effects = SpriteEffects.None;
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipVertically;
            
            Vector2 ownerCenter = owner.Center;
            Vector2 center = NPC.Center;
            Vector2 distToProj = ownerCenter - NPC.Center;
            float rotation = distToProj.ToRotation() + MathHelper.PiOver2;
            float distance = distToProj.Length();
            while (distance > 18f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                
                distToProj *= 18f;                     
                center += distToProj;                  
                distToProj = ownerCenter - center;   
                distance = distToProj.Length();               

                //Draw chain
                Main.spriteBatch.Draw(chain, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 10, 18), Lighting.GetColor((int)center.X / 16, (int)center.Y / 16), rotation,
                    new Vector2(10 * 0.5f, 18 * 0.5f), 1f, effects, 0f);
            }

            Main.spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            Main.spriteBatch.Draw(textureMask, NPC.Center - screenPos, NPC.frame,
            Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : Color.White, NPC.rotation, drawOrigin, NPC.scale, effects, 0);

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            if (AIState == 2) // draw translucent orange textures over the enemy to make it look like it is expelling heat.
            {
                for (int i = 0; i < 4; i++)
                {
                    Main.EntitySpriteDraw(texture, NPC.Center + new Vector2(4 - (5 * heat), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - screenPos, NPC.frame,
                    Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : new Color(255, 125, 25, 50) * heat, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }
            }

            // Silly sparkles.

            Texture2D star = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;

            sparkle = MathHelper.SmoothStep(sparkle, AIState == 1 ? 1 : 0, AIState == 1 ? 0.15f : 0.25f);
            
            Main.EntitySpriteDraw(star, NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation) - screenPos, null,
            Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= .1f ? Color.Black : Color.Orange * sparkle, NPC.rotation, star.Size() / 2, sparkle * new Vector2(.6f, .3f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation) - screenPos, null,
            Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= .1f ? Color.Black : Color.Orange * sparkle, NPC.rotation + MathHelper.PiOver2, star.Size() / 2, sparkle * new Vector2(.6f, .3f), SpriteEffects.None, 0);

            Main.EntitySpriteDraw(star, NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation) - screenPos, null,
            Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= .1f ? Color.Black : Color.White * sparkle, NPC.rotation, star.Size() / 2, sparkle * new Vector2(.6f, .3f) * .6f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation) - screenPos, null,
            Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= .1f ? Color.Black : Color.White * sparkle, NPC.rotation + MathHelper.PiOver2, star.Size() / 2, sparkle * new Vector2(.6f, .3f) * .6f, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(glow, NPC.Center + new Vector2(-8, -10 * NPC.spriteDirection).RotatedBy(NPC.rotation) - screenPos, null,
            Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= .1f ? Color.Black : Color.Orange * sparkle * .1f, 0, glow.Size() / 2, sparkle * .5f, SpriteEffects.None, 0);

            return false;
        }
    }
}
