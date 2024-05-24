using System.IO;
using GloryMod.NPCs.BloodMoon.BloodDrone;
using GloryMod.NPCs.BloodMoon.Hemolitionist;
using GloryMod.Systems;
using Terraria.Audio;
using Terraria.GameContent;

namespace GloryMod.NPCs.BloodMoon.BloodSeekerBeast
{
    class BSBHead : WormHead
    {
        public override int BodyType => NPCType<BSBBody>();

        public override int TailType => NPCType<BSBTail>();

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 18;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

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
            NPC.defense = 40;
            NPC.lifeMax = Main.getGoodWorld ? 28000 : 20000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 10, 0, 0);
            NPC.npcSlots = 25f;
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
            // Set the segment variance
            // If you want the segment length to be constant, set these two properties to the same value
            MinSegmentLength = 15;
            MaxSegmentLength = 15;
            CanFly = true;

            CustomBehavior = true;
            NumberBodySegments = true;

            CommonWormInit(this);
        }

        // This method is invoked from ExampleWormHead, ExampleWormBody and ExampleWormTail
        internal static void CommonWormInit(Worm worm)
        {
            worm.Acceleration = 0.075f;
            worm.MoveSpeed = 15;
            worm.AttackTail = true;
        }

        private int animState = 0;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 2;
            NPC.frameCounter++;

            if (animState == 0) (NPC.frame.Y, NPC.frame.X, NPC.damage) = (0, 0, 0);

            if (animState == 1)
            {
                NPC.frame.X = 0;

                if (NPC.frameCounter > 4)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0f;

                    if (NPC.frame.Y == frameHeight * 3)
                    {                        
                        NPC.damage = 250;
                        NPC.velocity += new Vector2(phase2 ? 12 : 10, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2);

                        SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack, NPC.Center);

                        int numDusts = 30;

                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Vector2 trueVelocity = new Vector2(5, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            trueVelocity.X *= 0.5f;
                            trueVelocity = trueVelocity.RotatedBy(NPC.velocity.ToRotation()) + new Vector2(5, 0).RotatedBy(NPC.velocity.ToRotation());
                            Main.dust[dust].velocity = trueVelocity;
                        }
                    }

                    if (NPC.frame.Y == frameHeight * 8)
                    {
                        SoundEngine.PlaySound(SoundID.DeerclopsIceAttack with { Pitch = -.1f }, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 randomizer = new Vector2(0, 5).RotatedBy(NPC.velocity.ToRotation());
                            for (int i = 0; i < 10; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, randomizer.RotatedBy(i * MathHelper.TwoPi / 10), ProjectileType<DroneBullet>(), 100, 3f, Main.myPlayer);
                            }

                            NPC.netUpdate = true;
                        }
                    }
                }

                if (NPC.frame.Y >= frameHeight * 12)
                {
                    NPC.frame.Y = 0;
                    animState = 0;
                }
            }

            if (animState == 2)
            {
                NPC.frame.X = NPC.frame.Width;

                if (NPC.frameCounter > 4)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0f;

                    if (NPC.frame.Y == frameHeight * 4)
                    {
                        if (NPC.ai[2] == 3)
                        {
                            NPC.damage = 250;
                            NPC.velocity += new Vector2((phase2 ? 18 : 15) + MathHelper.Clamp(NPC.Distance(target.Center) / 100, 1, 15), 0).RotatedBy(NPC.rotation - MathHelper.PiOver2);

                            SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack, NPC.Center);

                            int numDusts = 40;

                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 2f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Vector2 trueVelocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                                trueVelocity.X *= 0.5f;
                                trueVelocity = trueVelocity.RotatedBy(NPC.velocity.ToRotation()) + new Vector2(5, 0).RotatedBy(NPC.velocity.ToRotation());
                                Main.dust[dust].velocity = trueVelocity;
                            }
                        }

                        if (NPC.ai[2] == 0)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 2.5f, Pitch = -.3f }, NPC.Center);
                        }

                        if (NPC.ai[2] == 4)
                        {
                            SoundEngine.PlaySound(SoundID.Roar with { Volume = 1.5f, Pitch = -.3f }, NPC.Center);
                            SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 2.5f, Pitch = -.3f }, NPC.Center);
                        }
                    }

                    if (NPC.frame.Y >= frameHeight * 5 && NPC.frame.Y < frameHeight * 13)
                    {
                        if (NPC.ai[2] == 3)
                        {
                            SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(10, 0).RotatedBy(NPC.velocity.ToRotation() + MathHelper.ToRadians(15)).RotatedByRandom(MathHelper.ToRadians(5)), ProjectileType<DroneBullet>(), 100, 3f, Main.myPlayer);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(10, 0).RotatedBy(NPC.velocity.ToRotation() - MathHelper.ToRadians(15)).RotatedByRandom(MathHelper.ToRadians(5)), ProjectileType<DroneBullet>(), 100, 3f, Main.myPlayer);
                                NPC.netUpdate = true;
                            }
                        }

                        if (NPC.ai[2] == 0 || NPC.ai[2] == 4)
                        {
                            ScreenUtils.screenShaking = 5;

                            int dust = Dust.NewDust(NPC.Center + new Vector2(0, 24).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
                        }
                    }

                    if (NPC.frame.Y > frameHeight * 13 && NPC.ai[2] == 3) NPC.damage = 0;

                    if (NPC.frame.Y >= frameHeight * 18)
                    {
                        NPC.frame.Y = 0;
                        NPC.frame.X = 0;
                        animState = 0;
                    }
                }
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.WriteVector2(choosePoint);
                writer.Write(phase2);
                writer.Write(NPC.localAI[0]);
                writer.Write(NPC.localAI[1]);
                writer.Write(NPC.localAI[2]);
                writer.Write(NPC.localAI[3]);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                choosePoint = reader.ReadVector2();
                phase2 = reader.ReadBoolean();
                NPC.localAI[0] = reader.ReadInt16();
                NPC.localAI[1] = reader.ReadInt16();
                NPC.localAI[2] = reader.ReadInt16();
                NPC.localAI[3] = reader.ReadInt16();
            }
        }

        private enum AttackPattern
        {
            StartBattle = 0,
            StandardBite = 1,
            CoilVortex = 2,
            LaserDash = 3,
            PhaseTransition = 4,
            DeathAnimation = 5
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[2];
            set => NPC.localAI[2] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[3];
        Vector2 choosePoint;
        bool phase2;

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override void PostAI()
        {
            if (phase2) NPC.localAI[3]++;

            if (NPC.localAI[3] >= 360)
            {
                NPC.localAI[3] = 0;
                NPC.netUpdate = true;
            }
        }

        public override void AI()
        {
            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 0;

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            Vector2 targetGround = Systems.Utils.findGroundUnder(target.Center);
            Vector2 NPCGround = Systems.Utils.findGroundUnder(NPC.Center);

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
                    else WormMovement(phase2 ? 15 : 12, phase2 ? 0.35f: .25f, target.Center);

                    if (NPC.Distance(target.Center) <= 250 && animState == 0 && AITimer != (phase2 ? 360 : 420) && AITimer != (phase2 ? 720 : 840)
                        && Systems.Utils.CloseTo(NPC.rotation - MathHelper.PiOver2, NPC.DirectionTo(target.Center).ToRotation(), .15f)) animState = 1;


                    if (animState == 0 && NPC.life < (NPC.lifeMax * .65f) && !phase2)
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
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), choosePoint, Vector2.Zero, ProjectileType<BSBVortex>(), 150, 0, target.whoAmI);

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
            }
        }

        void PlaySound(int soundRand)
        {
            SoundEngine.PlaySound((soundRand == 3 ? SoundID.Zombie68 : soundRand == 2 ? SoundID.Zombie67 : SoundID.Zombie66) with { Volume = 2, Pitch = -.1f }, NPC.Center);
        }

        void WormMovement(float speed, float acceleration, Vector2 moveTo)
        {
            float targetXPos, targetYPos;

            Vector2 forcedTarget = ForcedTargetPosition ?? moveTo;
            // Using a ValueTuple like this allows for easy assignment of multiple values
            (targetXPos, targetYPos) = (forcedTarget.X, forcedTarget.Y);

            // Copy the value, since it will be clobbered later
            Vector2 npcCenter = NPC.Center;

            float targetRoundedPosX = (float)((int)(targetXPos / 16f) * 16);
            float targetRoundedPosY = (float)((int)(targetYPos / 16f) * 16);
            npcCenter.X = (float)((int)(npcCenter.X / 16f) * 16);
            npcCenter.Y = (float)((int)(npcCenter.Y / 16f) * 16);
            float dirX = targetRoundedPosX - npcCenter.X;
            float dirY = targetRoundedPosY - npcCenter.Y;

            float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

            float absDirX = Math.Abs(dirX);
            float absDirY = Math.Abs(dirY);
            float newSpeed = speed / length;
            dirX *= newSpeed;
            dirY *= newSpeed;

            if ((NPC.velocity.X > 0 && dirX > 0) || (NPC.velocity.X < 0 && dirX < 0) || (NPC.velocity.Y > 0 && dirY > 0) || (NPC.velocity.Y < 0 && dirY < 0))
            {
                // The NPC is moving towards the target location
                if (NPC.velocity.X < dirX)
                    NPC.velocity.X += acceleration;
                else if (NPC.velocity.X > dirX)
                    NPC.velocity.X -= acceleration;

                if (NPC.velocity.Y < dirY)
                    NPC.velocity.Y += acceleration;
                else if (NPC.velocity.Y > dirY)
                    NPC.velocity.Y -= acceleration;

                // The intended Y-velocity is small AND the NPC is moving to the left and the target is to the right of the NPC or vice versa
                if (Math.Abs(dirY) < speed * 0.2 && ((NPC.velocity.X > 0 && dirX < 0) || (NPC.velocity.X < 0 && dirX > 0)))
                {
                    if (NPC.velocity.Y > 0)
                        NPC.velocity.Y += acceleration * 2f;
                    else
                        NPC.velocity.Y -= acceleration * 2f;
                }

                // The intended X-velocity is small AND the NPC is moving up/down and the target is below/above the NPC
                if (Math.Abs(dirX) < speed * 0.2 && ((NPC.velocity.Y > 0 && dirY < 0) || (NPC.velocity.Y < 0 && dirY > 0)))
                {
                    if (NPC.velocity.X > 0)
                        NPC.velocity.X = NPC.velocity.X + acceleration * 2f;
                    else
                        NPC.velocity.X = NPC.velocity.X - acceleration * 2f;
                }
            }
            else if (absDirX > absDirY)
            {
                // The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
                if (NPC.velocity.X < dirX)
                    NPC.velocity.X += acceleration * 1.1f;
                else if (NPC.velocity.X > dirX)
                    NPC.velocity.X -= acceleration * 1.1f;

                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
                {
                    if (NPC.velocity.Y > 0)
                        NPC.velocity.Y += acceleration;
                    else
                        NPC.velocity.Y -= acceleration;
                }
            }
            else
            {
                // The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
                if (NPC.velocity.Y < dirY)
                    NPC.velocity.Y += acceleration * 1.1f;
                else if (NPC.velocity.Y > dirY)
                    NPC.velocity.Y -= acceleration * 1.1f;

                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
                {
                    if (NPC.velocity.X > 0)
                        NPC.velocity.X += acceleration;
                    else
                        NPC.velocity.X -= acceleration;
                }
            }
        }

        float laserOpacity;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
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

            return false;
        }
    }

    class BSBBody : WormBody
    {
        public override string Texture => "GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBody";

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
            NPC.defense = 40;
            NPC.lifeMax = Main.getGoodWorld ? 28000 : 20000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 10, 0, 0);
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

        public override void AI()
        {
            NPC.direction = NPC.position.X > NPC.oldPosition.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            whichSpike = ((int)NPC.ai[2] + 3) / 4;

            float mult = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.15f;
            spikeRotation = (NPC.ai[2] + 1) / 10 + mult;

            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 0;

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
            Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center - new Vector2(NPC.spriteDirection > 0 ? -164 : 164, 0).RotatedBy(NPC.rotation), NPC.Center - new Vector2(NPC.spriteDirection > 0 ? -14 : 14, 0).RotatedBy(NPC.rotation), color);

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
            NPC.defense = 15;
            NPC.lifeMax = Main.getGoodWorld ? 28000 : 20000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 10, 0, 0);
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

        public override void AI()
        {
            NPC.direction = NPC.position.X > NPC.oldPosition.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            if (HeadSegment.ai[2] != 0 && HeadSegment.ai[2] != 5) NPC.dontTakeDamage = false;
            else NPC.dontTakeDamage = true; 
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
