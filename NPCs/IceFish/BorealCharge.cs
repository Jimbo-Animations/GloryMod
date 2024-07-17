using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.IceFish
{
    internal class BorealCharge : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 25;
            NPC.height = 25;

            NPC.damage = 0;
            NPC.defense = Main.hardMode ? 25 : 10;
            NPC.lifeMax = Main.hardMode ? 3000 : 450;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;

            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.behindTiles = true;
            NPC.coldDamage = true;
            NPC.dontTakeDamage = true;
            NPC.noTileCollide = true;

            NPC.HitSound = SoundID.DD2_WitherBeastCrystalImpact;
            NPC.DeathSound = SoundID.DD2_WitherBeastDeath with { Volume = 3, Pitch = -.3f };
        }

        NPC owner;
        float lifeSpan = 600;
        float prevRotation;
        bool hasCollided;

        public ref float AITimer => ref NPC.ai[1];

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!hasCollided) NPC.frameCounter++;

            if (NPC.frameCounter > 4)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;
            }

            if (NPC.frame.Y >= frameHeight * 7)
            {
                NPC.frame.Y = 0;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.frame.Y = 50 * Main.rand.Next(0, 6);
        }

        public override void AI()
        {
            owner = Main.npc[(int)NPC.ai[0]];
            //if (!owner.active || owner.type != NPCType<HM>()) NPC.life = 0;

            if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(NPC.velocity.X, NPC.velocity.Y) && !hasCollided)
            {
                NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true);
                NPC.position.Y += NPC.velocity.Y;
                NPC.velocity.Y = 0;
                hasCollided = true;

                Systems.ScreenUtils.screenShaking += 1;
                SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 2 }, NPC.Center);

                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-50, 50), 0), 0, 0, 51, Scale: 1.5f);
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.Pi);
                }

                NPC.netUpdate = true;
            }

            if (hasCollided)
            {
                AITimer++;
                NPC.dontTakeDamage = AITimer < lifeSpan ? false : true;
            }
            else
            {
                NPC.velocity.Y += .25f;
                NPC.velocity *= .99f;

                prevRotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
            }

            NPC.rotation = prevRotation;
            NPC.damage = hasCollided ? 0 : (Main.hardMode ? 180 : 90);

            if (AITimer > (lifeSpan * 2 / 3) && AITimer % 5 == 1)
            {
                int dustType = DustID.FrostStaff;
                var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

                dust.velocity.X += Main.rand.NextFloat(-0.1f, 0.1f);
                dust.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);

                dust.scale *= nearingDetonation + Main.rand.NextFloat(-0.05f, 0.05f);
                dust.noGravity = true;             
            }

            if (AITimer == (lifeSpan * .7f)) SoundEngine.PlaySound(SoundID.Item30, NPC.Center);

            if (AITimer == lifeSpan)
            {
                for (int i = -4; i < 5; i++)
                {
                    int dustType = DustID.FrostStaff;
                    var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

                    dust.velocity.X += Main.rand.NextFloat(-0.1f, 0.1f);
                    dust.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);

                    dust.scale *= 2f + Main.rand.NextFloat(-0.05f, 0.05f);
                    dust.noGravity = true;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spikePos = Systems.Utils.findGroundUnder(NPC.Top + new Vector2(-30 * i, 100).RotatedBy(NPC.rotation));
                        float spikePointTo = MathHelper.ToRadians(120) * i / 9;

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spikePos, new Vector2(0, -1).RotatedBy(spikePointTo), ProjectileID.DeerclopsIceSpike, (Main.hardMode ? 180 : 90) / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, Main.rand.NextFloat(1f, 1.36f) + (Main.hardMode ? .5f : 0));
                    }
                }

                NPC.netUpdate = true;
            }

            if (AITimer > (lifeSpan * 1.05f))
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        float auraAlpha;
        float rayAlpha = .33f;
        float timer;
        float nearingDetonation;
        Color auroraColor;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D textureTrail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .75f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects;           

            Color auroraColor = Systems.Utils.ColorLerpCycle(Main.GlobalTimeWrappedHourly, hasCollided ? 3 : 2, new Color[] { Color.Lerp(new Color(40, 250, 60, 50), new Color(40, 60, 200, 50), nearingDetonation), Color.Lerp(new Color(150, 70, 130, 50), new Color(60, 90, 150, 50), nearingDetonation) });

            if (AITimer >= lifeSpan)
            {
                nearingDetonation = MathHelper.SmoothStep(nearingDetonation, 0, .2f);
                auraAlpha = MathHelper.SmoothStep(auraAlpha, 0, .2f);
                rayAlpha = MathHelper.SmoothStep(rayAlpha, 0, .2f);             
            }
            else
            {
                nearingDetonation = MathHelper.SmoothStep(nearingDetonation, AITimer > (lifeSpan * 2 / 3) ? 1 : 0, .1f);
                auraAlpha = MathHelper.SmoothStep(auraAlpha, 1, .1f);
                rayAlpha = MathHelper.SmoothStep(rayAlpha, hasCollided ? 1 : .33f, .15f);              
            }

            timer += 0.1f;          

            if (NPC.spriteDirection > 0)
            {
                effects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                effects = SpriteEffects.None;
            }

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                Vector2 trailVector = hasCollided ? new Vector2(0, (10 + nearingDetonation) * i).RotatedBy(NPC.rotation) + drawPos : NPC.oldPos[i] - NPC.position + drawPos;

                Main.EntitySpriteDraw(textureTrail, trailVector, null, auroraColor * (auraAlpha + nearingDetonation) * ((1 - i / (float)NPC.oldPos.Length) * 0.95f), NPC.rotation, textureTrail.Size() / 2, new Vector2(Main.rand.NextFloat(.9f, 1.2f), 1 + nearingDetonation), effects, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, drawPos + new Vector2((4 + nearingDetonation) * auraAlpha, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, auroraColor * .5f * (auraAlpha + nearingDetonation), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            Main.EntitySpriteDraw(texture, drawPos, NPC.frame, auroraColor * .3f * (auraAlpha + nearingDetonation), NPC.rotation, drawOrigin, NPC.scale, effects, 0);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 5; i++)
            {
                int dustType = 80;
                var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

                dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
                dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

                dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
            }

            if (NPC.life <= 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    int dustType = DustID.IceGolem;
                    var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

                    dust.velocity.X += Main.rand.NextFloat(-0.1f, 0.1f);
                    dust.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);

                    dust.scale *= 2f + Main.rand.NextFloat(-0.03f, 0.03f);
                }

                for (int i = 0; i < 5; i++)
                {
                    int dustType = DustID.FrostStaff;
                    var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

                    dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
                    dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

                    dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
                    dust.noGravity = true;
                }
            }
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<BlueRoar>(), 0, 1, target.whoAmI);
            }

            owner.ai[3]++;
            NPC.netUpdate = true;
        }
    }
}
