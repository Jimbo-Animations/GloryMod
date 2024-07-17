using System.IO;
using GloryMod.NPCs.BloodMoon.BloodDrone;
using GloryMod.Systems;
using GloryMod.Systems.BossBars;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;

namespace GloryMod.NPCs.BloodMoon.BloodSeekerBeast
{
    [AutoloadBossHead]
    partial class BSBHead : WormHead
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 18;
            NPCID.Sets.ReflectStarShotsInForTheWorthy[NPC.type] = true;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                new FlavorTextBestiaryInfoElement("The impenetrable shell and voracious appetite of the Bloodseeker Beast is unmatched by the serpents of the known world, held back only by a bloody, exposed tail. " +
                " Some believe its unfinished back end is the root cause of its perpetual anger and violence.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(40);

            NPC.damage = 0;
            NPC.lifeMax = Main.getGoodWorld ? 20000 : 15000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 8, 0, 0);
            NPC.npcSlots = 25f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.immortal = true;
            NPC.reflectsProjectiles = true;

            NPC.HitSound = new SoundStyle("GloryMod/Music/HemoHit", 4, SoundType.Sound) with { Volume = 0.33f };
            NPC.DeathSound = SoundID.NPCDeath14;

            NPC.BossBar = GetInstance<HemolitionistBossBar>();
            Music = MusicID.OtherworldlyInvasion;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override void AI()
        {
            // Various pieces of misc code I couldn't find a way to make it look more organized

            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 0;

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

            // Force a netupdate if the NPC's velocity changed sign and it was not "just hit" by a player
            if (((NPC.velocity.X > 0 && NPC.oldVelocity.X < 0) || (NPC.velocity.X < 0 && NPC.oldVelocity.X > 0) || (NPC.velocity.Y > 0 && NPC.oldVelocity.Y < 0) || (NPC.velocity.Y < 0 && NPC.oldVelocity.Y > 0)) && !NPC.justHit)
                NPC.netUpdate = true;

            // Makes the boss enter its despawn state if there are no players left to fight

            if (NPC.ai[2] != 0 && !NPC.HasValidTarget)
            {
                AITimer = 0;
                NPC.ai[2] = 5;
                NPC.netUpdate = true;
            }

            switch (AIstate)
            {
                case AttackPattern.StartBattle:

                    AITimer++;
                    WormMovement(5, .15f, target.Center);

                    if (AITimer == 60) animState = 2;

                    if (AITimer > 120)
                    {
                        AITimer = 0;
                        NPC.ai[2]++;
                        NPC.netUpdate = true;
                    }

                    break;


                case AttackPattern.StandardBite:

                    if (animState == 0) AITimer++;

                    if (animState == 1 && NPC.frame.Y >= 306)
                    {
                        NPC.velocity *= 0.98f;
                    }
                    else WormMovement(12, phase2 ? 0.35f : .25f, target.Center);

                    if (NPC.Distance(target.Center) <= 250 && animState == 0 && AITimer != (phase2 ? 360 : 420) && AITimer != (phase2 ? 720 : 840)
                        && Systems.Utils.CloseTo(NPC.rotation - MathHelper.PiOver2, NPC.DirectionTo(target.Center).ToRotation(), .15f)) animState = 1;


                    if (animState == 0 && NPC.life < (NPC.lifeMax * .5f) && !phase2)
                    {
                        AITimer = 0;
                        NPC.ai[2] = 4;
                        NPC.netUpdate = true;
                    }

                    if (AITimer == (phase2 ? 360 : 420))
                    {
                        AITimer = 0;
                        NPC.ai[2] = 2;
                        NPC.netUpdate = true;
                    }

                    if (AITimer == (phase2 ? 720 : 840))
                    {
                        AITimer = 0;
                        NPC.ai[2] = 3;
                        NPC.netUpdate = true;
                    }

                    break;


                case AttackPattern.CoilVortex:

                    if (AITimer == 0)
                    {
                        AITimer++;
                        choosePoint = target.Center;
                        while (choosePoint.Distance(Systems.Utils.findGroundUnder(choosePoint)) < 250) choosePoint -= new Vector2(0, 100);

                        NPC.netUpdate = true;

                        SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 2, Pitch = -.1f }, NPC.Center);
                    }

                    NPC.velocity += NPC.DirectionTo(choosePoint).RotatedBy(NPC.Distance(choosePoint) < 400 ? -MathHelper.PiOver4 : 0) * (phase2 ? 1.35f : 1.3f);
                    NPC.velocity *= 0.95f;

                    if (NPC.Distance(choosePoint) < 400) AITimer++;

                    if (AITimer % 3 == 1)
                    {
                        int dust = Dust.NewDust(choosePoint, 0, 0, 114, Scale: MathHelper.Clamp(AITimer * 0.04f, 1, 4));
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                    }

                    if (AITimer > 100 && NPC.Distance(choosePoint) < 400)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen with { Volume = 2 }, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), choosePoint, Vector2.Zero, ProjectileType<BSBVortex>(), 150, 0, target.whoAmI);
                        }
                        AITimer = 420;
                        NPC.ai[2] = 1;
                        NPC.netUpdate = true;
                    }

                    break;


                case AttackPattern.LaserDash:

                    AITimer++;

                    if (AITimer == 1) PlaySound(Main.rand.Next(4));

                    choosePoint = target.Center + target.velocity * (NPC.Distance(target.Center) / 20);

                    if (AITimer == 75) animState = 2;

                    if (NPC.frame.Y < 408) WormMovement(6, phase2 ? .25f : .2f, choosePoint);
                    else NPC.velocity *= .99f;

                    if (AITimer > 150)
                    {
                        AITimer = 0;
                        NPC.ai[2] = 1;
                        NPC.netUpdate = true;
                    }

                    break;


                case AttackPattern.PhaseTransition:

                    AITimer++;

                    if (AITimer == 1)
                    {
                        animState = 2;
                    }
                    NPC.velocity += NPC.DirectionTo(target.Center).RotatedBy(NPC.Distance(target.Center) < 500 ? MathHelper.PiOver4 : 0) * 1.35f;
                    NPC.velocity *= 0.97f;

                    if (AITimer > 360)
                    {
                        phase2 = true;
                        AITimer = 0;
                        NPC.ai[2] = 1;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.Despawn:

                    AITimer++;
                    NPC.velocity.Y += 8;

                    if (AITimer > 90)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            var n = Main.npc[i];
                            if (!NPC.active) continue;

                            if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
                            {
                                n.active = false;
                            }
                        }
                    }
                        break;
                    
            }          
        }

        public override void PostAI()
        {
            if (phase2) NPC.localAI[3]++;

            if (NPC.localAI[3] >= 360)
            {
                NPC.localAI[3] = 0;
                NPC.netUpdate = true;
            }

            if (!NPC.HasValidTarget && NPC.timeLeft < 90)
            {
                NPC.active = false;
            }
        }

        float laserOpacity;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects;

            //Makes sure it does not draw its normal code for its bestiary entry.
            if (!NPC.IsABestiaryIconDummy)
            {
                Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;

                if (NPC.spriteDirection > 0)
                {
                    effects = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    effects = SpriteEffects.None;
                }

                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

                if (AIstate == AttackPattern.LaserDash)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                    Color color = new Color(255, 0, 0) * laserOpacity;
                    Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -10 : 10, -1000).RotatedBy(NPC.rotation), NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -10 : 10, 0).RotatedBy(NPC.rotation), color);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
                }

                laserOpacity = MathHelper.SmoothStep(laserOpacity, AIstate == AttackPattern.LaserDash && animState == 0 ? 1 : 0, .2f);
            }
            else
            {
                Texture2D textureBody = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBody").Value;
                Texture2D textureBody2 = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBody2").Value;
                Texture2D textureTail = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBTail").Value;
                Texture2D topSpikes = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBTopSpike").Value;
                Texture2D bottomSpikes = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBottomSpike").Value;
                Rectangle spikeFrame = new(0, 0, topSpikes.Width, topSpikes.Height / 4);
                Vector2 spikeOrigin1 = new(topSpikes.Width * 0.5f, topSpikes.Height / 4);
                Vector2 spikeOrigin2 = new(bottomSpikes.Width * 0.5f, 0);

                spriteBatch.Draw(textureBody2, drawPos + new Vector2(86, 0), null, Color.White, NPC.rotation - MathHelper.PiOver2, textureBody.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(textureBody, drawPos + new Vector2(42, 0), null, Color.White, NPC.rotation - MathHelper.PiOver2, textureBody.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation - MathHelper.PiOver2, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(topSpikes, drawPos + new Vector2(44, -14), spikeFrame, Color.White, NPC.rotation, spikeOrigin1, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(bottomSpikes, drawPos + new Vector2(44, 14), spikeFrame, Color.White, NPC.rotation, spikeOrigin2, NPC.scale, SpriteEffects.None, 0f);

                spikeFrame = new(0, topSpikes.Height / 4, topSpikes.Width, topSpikes.Height / 4);

                spriteBatch.Draw(topSpikes, drawPos + new Vector2(88, -14), spikeFrame, Color.White, NPC.rotation, spikeOrigin1, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(bottomSpikes, drawPos + new Vector2(88, 14), spikeFrame, Color.White, NPC.rotation, spikeOrigin2, NPC.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
    class BSBBody : WormBody
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(42);

            NPC.damage = 0;
            NPC.lifeMax = Main.getGoodWorld ? 20000 : 15000;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;

            NPC.HitSound = new SoundStyle("GloryMod/Music/HemoHit", 4, SoundType.Sound) with { Volume = 0.33f };
            NPC.DeathSound = SoundID.NPCDeath14;
        }

        public override void Init()
        {
            BSBHead.CommonWormInit(this);
        }

        int whichSpike;
        float spikeRotation;
        bool unleashedSpikes = false;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(unleashedSpikes);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                unleashedSpikes = reader.ReadBoolean();
            }
        }

        public override bool CheckActive()
        {
            return HeadSegment.ai[2] != 5;
        }

        public override void AI()
        {
            NPC.direction = NPC.position.X > NPC.oldPosition.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            whichSpike = ((int)NPC.ai[2] + 3) / 4;

            float mult = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.15f;
            spikeRotation = (NPC.ai[2] + 1) / 10 + mult;

            if (HeadSegment.ai[2] == 4 && HeadSegment.ai[3] == 120 + (NPC.ai[2] * 15))
            {
                SoundEngine.PlaySound(SoundID.Item61, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), new Vector2(0, 6.5f).RotatedBy(NPC.rotation + MathHelper.PiOver4), ProjectileType<BSBSpineRocket>(), 150, 3f, Main.myPlayer, whichSpike, 0);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), new Vector2(0, 6.5f).RotatedBy(NPC.rotation - MathHelper.PiOver4), ProjectileType<BSBSpineRocket>(), 150, 3f, Main.myPlayer, whichSpike, 1);

                    unleashedSpikes = true;
                    NPC.netUpdate = true;
                }
            }

            if (unleashedSpikes)
            {
                if (Main.rand.NextBool(50)) Dust.NewDustPerfect(NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), 5, new Vector2(Main.rand.NextFloat(6), 0).RotatedBy(NPC.rotation + MathHelper.PiOver2), Scale: 1f);
                if (Main.rand.NextBool(50)) Dust.NewDustPerfect(NPC.Center - new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), 5, new Vector2(Main.rand.NextFloat(-6), 0).RotatedBy(NPC.rotation + MathHelper.PiOver2), Scale: 1f);

                if (HeadSegment.ai[2] != 5 && HeadSegment.localAI[3] == 120 + (NPC.ai[2] * 10))
                {
                    SoundEngine.PlaySound(SoundID.Item75, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), new Vector2(0, 4).RotatedBy(NPC.rotation + MathHelper.PiOver2), ProjectileType<DroneBullet>(), 100, 3f, Main.myPlayer, whichSpike, 0);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), new Vector2(0, 4).RotatedBy(NPC.rotation - MathHelper.PiOver2), ProjectileType<DroneBullet>(), 100, 3f, Main.myPlayer, whichSpike, 1);
                        NPC.netUpdate = true;
                    }
                }
            }
        }

        float laserOpacity;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D texture2 = Request<Texture2D>(Texture + "2").Value;

            Vector2 drawOrigin = new(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects;
            SpriteEffects spikeEffects;

            Texture2D topSpikes = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBTopSpike").Value;
            Texture2D bottomSpikes = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBottomSpike").Value;

            Texture2D topMask = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBTopSpikeMask").Value;
            Texture2D bottomMask = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBottomSpikeMask").Value;

            Rectangle spikeFrame = new(0, topSpikes.Height / 4 * whichSpike, topSpikes.Width, topSpikes.Height / 4);
            Vector2 spikeOrigin1 = new(topSpikes.Width * 0.5f, topSpikes.Height / 4);
            Vector2 spikeOrigin2 = new(bottomSpikes.Width * 0.5f, 0);
            if (NPC.spriteDirection > 0)
            {
                effects = SpriteEffects.FlipHorizontally;
                spikeEffects = SpriteEffects.FlipVertically;
            }
            else
            {
                effects = SpriteEffects.None;
                spikeEffects = SpriteEffects.None;
            }

            float mult = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.15f;

            if (spikeRotation > .9f + mult) spikeRotation = 1 + mult;

            spriteBatch.Draw(NPC.ai[1] % 5 == 1 ? texture2 : texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            if (!unleashedSpikes)
            {
                spriteBatch.Draw(topSpikes, NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation) - screenPos, spikeFrame, drawColor, NPC.rotation + MathHelper.PiOver2 - (NPC.spriteDirection > 0 ? spikeRotation : -spikeRotation), NPC.spriteDirection > 0 ? spikeOrigin2 : spikeOrigin1, NPC.scale, spikeEffects, 0f);
                spriteBatch.Draw(bottomSpikes, NPC.Center - new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation) - screenPos, spikeFrame, drawColor, NPC.rotation + MathHelper.PiOver2 + (NPC.spriteDirection > 0 ? spikeRotation : -spikeRotation), NPC.spriteDirection > 0 ? spikeOrigin1 : spikeOrigin2, NPC.scale, spikeEffects, 0f);

                spriteBatch.Draw(topMask, NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation) - screenPos, spikeFrame, Color.White, NPC.rotation + MathHelper.PiOver2 - (NPC.spriteDirection > 0 ? spikeRotation : -spikeRotation), NPC.spriteDirection > 0 ? spikeOrigin2 : spikeOrigin1, NPC.scale, spikeEffects, 0f);
                spriteBatch.Draw(bottomMask, NPC.Center - new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation) - screenPos, spikeFrame, Color.White, NPC.rotation + MathHelper.PiOver2 + (NPC.spriteDirection > 0 ? spikeRotation : -spikeRotation), NPC.spriteDirection > 0 ? spikeOrigin1 : spikeOrigin2, NPC.scale, spikeEffects, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Color color = new Color(255, 0, 0) * laserOpacity;
            Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -164 : 164, 0).RotatedBy(NPC.rotation), NPC.Center + new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), color);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            if (HeadSegment.ai[2] != 5 && HeadSegment.localAI[3] >= 90 + (NPC.ai[2] * 10) && HeadSegment.localAI[3] < 120 + (NPC.ai[2] * 10)) laserOpacity = MathHelper.SmoothStep(laserOpacity, 1, .2f);
            else laserOpacity = MathHelper.SmoothStep(laserOpacity, 0, .2f);

            return false;
        }
    }

    class BSBTail : WormTail
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(40);

            NPC.damage = 0;
            NPC.defense = 20;
            NPC.lifeMax = Main.getGoodWorld ? 20000 : 15000;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;

            NPC.HitSound = SoundID.NPCHit9;
            NPC.DeathSound = SoundID.NPCDeath14;           
        }

        public override void Init()
        {
            BSBHead.CommonWormInit(this);
        }

        public override bool CheckActive()
        {
            return HeadSegment.ai[2] != 5;
        }

        public override void AI()
        {
            NPC.direction = NPC.position.X > NPC.oldPosition.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            if (HeadSegment.ai[2] != 0 && HeadSegment.ai[2] != 5) NPC.dontTakeDamage = false;
            else NPC.dontTakeDamage = true;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (ProjectileID.Sets.CultistIsResistantTo[projectile.type] == true)
            {
                modifiers.FinalDamage *= .6f;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects;

            if (NPC.spriteDirection > 0)
            {
                effects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                effects = SpriteEffects.None;
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }
}
