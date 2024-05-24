using Terraria.Audio;
using Terraria.DataStructures;
using System.IO;
using Terraria.GameContent.Bestiary;

namespace GloryMod.NPCs.BloodMoon.BloodDrone
{
    internal class BloodDrone : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 19;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

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
                new FlavorTextBestiaryInfoElement("Armored, aggressive drones that prowl the skies of the Bloodmoon. While typically skilled and strategic in combat, they will enter a blind rage if their true self is revealed.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 30;
            NPC.height = 30;
            DrawOffsetY = 4f;

            NPC.damage = 0;
            NPC.defense = 20;
            NPC.lifeMax = Main.getGoodWorld ? (Main.hardMode ? 500 : 200) : (Main.hardMode ? 300 : 150);
            NPC.knockBackResist = Main.getGoodWorld ? 0.4f : 0.5f;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.npcSlots = 5f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.Item52 with { Pitch = -0.25f };
            NPC.DeathSound = SoundID.DD2_KoboldExplosion;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        private enum AttackPattern
        {
            Activate = 0,
            FlyAbout = 1,
            QuickBurst = 2,
            HomingBlast = 3,
            Desperation = 4,
            Kamikaze = 5
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float AIRandomizer => ref NPC.ai[2];
        public ref float AIDifficulty => ref NPC.ai[3];

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        private int RotorFrameY;
        private int RotorTilt;
        private int animState;
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter > 5)
            {
                NPC.frame.Y += frameHeight;
                RotorFrameY += frameHeight;
                NPC.frameCounter = 0f;
            }

            if (RotorFrameY >= frameHeight * 3) RotorFrameY = 0;

            switch (animState)
            {
                case 0:

                    if (RotorFrameY < frameHeight * 3) RotorTilt = 0;
                    if (NPC.frame.Y >= frameHeight * 3 && NPC.frame.Y < frameHeight * 5) RotorTilt = frameHeight * 3;
                    if (NPC.frame.Y >= frameHeight * 5) RotorTilt = frameHeight * 6;
                    if (NPC.frame.Y >= frameHeight * 6) NPC.frame.Y = 0;

                    break;

                case 1:

                    if (NPC.frame.Y <= frameHeight * 6) NPC.frame.Y = frameHeight * 7;
                    if (NPC.frame.Y >= frameHeight * 11)
                    {
                        NPC.frameCounter = 0;
                        animState = 2;
                    }
                    RotorTilt = 0;

                    break;

                case 2:

                    if (NPC.frame.Y >= frameHeight * 11) NPC.frame.Y = frameHeight * 10;
                    RotorTilt = 0;

                    break;

                case 3:

                    if (NPC.frame.Y <= frameHeight * 10) NPC.frame.Y = frameHeight * 11;
                    if (NPC.frame.Y >= frameHeight * 13)
                    {
                        NPC.frameCounter = 0;
                        animState = 0;
                    }
                    RotorTilt = 0;

                    break;

                case 4:

                    if (RotorFrameY < frameHeight * 16) RotorTilt = 0;
                    if (NPC.frame.Y >= frameHeight * 16 && NPC.frame.Y < frameHeight * 18) RotorTilt = frameHeight * 3;
                    if (NPC.frame.Y >= frameHeight * 18) RotorTilt = frameHeight * 6;
                    if (NPC.frame.Y <= frameHeight * 13 || NPC.frame.Y >= frameHeight * 19) NPC.frame.Y = frameHeight * 13;

                    break;

            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(AITimer);
                writer.Write(AIRandomizer);
                writer.Write(AIDifficulty);
                writer.Write(phase2);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                AITimer = reader.ReadUInt16();
                AIRandomizer = reader.ReadUInt16();
                AIDifficulty = reader.ReadUInt16();
                phase2 = reader.ReadBoolean();
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            AIDifficulty = 1;
            if (Main.getGoodWorld) AIDifficulty++;
            if (Main.hardMode) AIDifficulty++;
        }

        public override void OnKill()
        {
            int numDusts = 12;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), (int)AIDifficulty * 30 + 30, 0, target.whoAmI);

            int gore2 = Mod.Find<ModGore>("BloodDroneGore2").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.Next(10), 0).RotatedByRandom(MathHelper.TwoPi), gore2);
            int gore3 = Mod.Find<ModGore>("BloodDroneGore3").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.Next(10), 0).RotatedByRandom(MathHelper.TwoPi), gore3);
            int gore4 = Mod.Find<ModGore>("BloodDroneGore4").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.Next(10), 0).RotatedByRandom(MathHelper.TwoPi), gore4);
            int gore5 = Mod.Find<ModGore>("BloodDroneGore5").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.Next(10), 0).RotatedByRandom(MathHelper.TwoPi), gore5);
        }

        bool phase2;
        Vector2 moveTo;
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            Vector2 targetGround = Systems.Utils.findGroundUnder(target.Center);
            Vector2 NPCGround = Systems.Utils.findGroundUnder(NPC.Center);
            Vector2 HybridGround = Systems.Utils.findGroundUnder(new Vector2(target.Center.X, NPC.Center.Y));

            if (!phase2 && NPC.life < NPC.lifeMax / 2)
            {
                phase2 = true;

                AITimer = 0;
                NPC.defense = 10;
                NPC.knockBackResist = 0.25f;
                AIRandomizer = Main.rand.Next(250 - ((int)AIDifficulty * 10), 315 - ((int)AIDifficulty * 15));
                NPC.ai[0] = 4;

                int numDusts = 10;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 266, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                int gore1 = Mod.Find<ModGore>("BloodDroneGore1").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(NPC.velocity.X / 2, -5), gore1);
                SoundEngine.PlaySound(SoundID.DD2_WyvernScream, NPC.Center);
                animState = 4;
                NPC.netUpdate = true;
            }

            switch (AIstate)
            {
                case AttackPattern.Activate:

                    AIRandomizer = Main.rand.Next(195 - ((int)AIDifficulty * 15), 261 - ((int)AIDifficulty * 20));
                    NPC.ai[0] = 1;
                    PlaySound(Main.rand.Next(4));
                    SetGoal(new Vector2(target.Center.X, HybridGround.Y - 300));
                    NPC.netUpdate = true;

                    break;

                case AttackPattern.FlyAbout:

                    if (NPC.Distance(moveTo) <= 10 || NPC.Distance(HybridGround) > 300) SetGoal(new Vector2(target.Center.X, HybridGround.Y - 300), AIDifficulty * 10 + 75, AIDifficulty * 10 + 25);
                    NPC.velocity += NPC.DirectionTo(moveTo) * (AIDifficulty * 0.1f + 0.1f);

                    AITimer++;
                    NPC.velocity *= 0.98f;
                    NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);

                    if (AITimer > AIRandomizer)
                    {
                        AITimer = 0;
                        NPC.ai[0] = Main.rand.NextBool(3) ? 3 : 2;
                        PlaySound(Main.rand.Next(4));
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.QuickBurst:

                    if (AITimer == 0) animState = 1;

                    AITimer++;
                    NPC.velocity *= 0.98f;
                    NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);

                    if (NPC.Distance(NPCGround) < 300) NPC.velocity.Y -= AIDifficulty * 0.1f + 0.1f;
                    else NPC.velocity.Y += 0.1f;

                    if (AITimer > 30 && AITimer <= 90 && AITimer % 10 == 1)
                    {
                        int numDusts = 10;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 266, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        SoundEngine.PlaySound(SoundID.Item75, NPC.Center);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 6).RotatedBy(NPC.rotation), NPC.DirectionTo(target.Center).RotatedByRandom(MathHelper.ToRadians(15)) * 2, ProjectileType<DroneBullet>(), (int)AIDifficulty * 20 + 20, 0, target.whoAmI);

                        NPC.netUpdate = true;
                    }
                    if (AITimer == 90) animState = 3;

                    if (AITimer > 100)
                    {
                        AIRandomizer = Main.rand.Next(195 - ((int)AIDifficulty * 15), 261 - ((int)AIDifficulty * 20));
                        AITimer = 0;
                        NPC.ai[0] = 1;
                        PlaySound(Main.rand.Next(4));
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.HomingBlast:

                    if (AITimer == 0) animState = 1;

                    AITimer++;
                    NPC.velocity += NPC.DirectionTo(moveTo) * (AIDifficulty * 0.1f + 0.1f);
                    NPC.velocity *= 0.98f;
                    NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);

                    if (AITimer <= 90)
                    {
                        if (AITimer % 3 == 1)
                        {
                            int dust = Dust.NewDust(NPC.Center + new Vector2(0, 6).RotatedBy(NPC.rotation), 0, 0, 114, Scale: AITimer * 0.04f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                        }

                        moveTo = HybridGround + new Vector2(450 * (NPC.Center.X > target.Center.X ? 1 : -1), -300);

                        if (AITimer == 90)
                        {
                            NPC.velocity += NPC.DirectionFrom(target.Center) * 5;
                            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, NPC.Center);

                            int numDusts = 12;
                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 2f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            }

                            for (int i = -1; i < 2; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 6).RotatedBy(NPC.rotation), NPC.DirectionTo(target.Center).RotatedBy(i * MathHelper.Pi / 3) * 5, ProjectileType<DroneMissile>(), (int)AIDifficulty * 25 + 25, 0, target.whoAmI);
                            }

                            animState = 3;
                            NPC.netUpdate = true;
                        }
                    }

                    if (AITimer > 100)
                    {
                        AIRandomizer = Main.rand.Next(195 - ((int)AIDifficulty * 15), 261 - ((int)AIDifficulty * 20));
                        AITimer = 0;
                        NPC.ai[0] = 1;
                        PlaySound(Main.rand.Next(4));
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.Desperation:

                    if (NPC.Distance(moveTo) <= 10 || NPC.Distance(HybridGround) > 300) SetGoal(new Vector2(target.Center.X, HybridGround.Y - 300), AIDifficulty * 10 + 80, AIDifficulty * 10 + 40);
                    NPC.velocity += NPC.DirectionTo(moveTo) * (AIDifficulty * 0.1f + 0.25f);

                    AITimer++;
                    NPC.velocity *= 0.98f;
                    NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.2f);

                    if (AITimer > 30 && AITimer % 30 == 1)
                    {
                        int numDusts = 10;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 266, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        SoundEngine.PlaySound(SoundID.Item75, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 6).RotatedBy(NPC.rotation), NPC.DirectionTo(target.Center).RotatedByRandom(MathHelper.ToRadians(30)) * 2, ProjectileType<DroneBullet>(), (int)AIDifficulty * 20 + 20, 0, target.whoAmI);
                        }
                        NPC.netUpdate = true;
                    }

                    if (AITimer > AIRandomizer)
                    {
                        AITimer = 0;
                        NPC.ai[0] = 5;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.Kamikaze:

                    if (AITimer == 0) SoundEngine.PlaySound(SoundID.DD2_WyvernScream, NPC.Center);
                    AITimer++;

                    NPC.velocity += NPC.DirectionTo(target.Center) * (AIDifficulty * 0.1f + 0.25f);
                    NPC.velocity *= 0.98f;
                    NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);
                    NPC.damage = (int)AIDifficulty * 25 + 25;

                    if (NPC.Distance(target.Center) <= 40) NPC.SimpleStrikeNPC(1000, 0, true, 0, DamageClass.Default, true);

                    break;
            }
        }

        void PlaySound(int soundRand)
        {
            SoundEngine.PlaySound(soundRand == 3 ? SoundID.Zombie69 : soundRand == 2 ? SoundID.Zombie70 : soundRand == 1 ? SoundID.Zombie71 : SoundID.Zombie72, NPC.Center);
        }

        void SetGoal(Vector2 goalPoint, float varianceX = 1, float varianceY = 1)
        {
            moveTo = new Vector2(goalPoint.X + Main.rand.NextFloat(-varianceX, varianceX + 1), goalPoint.Y + Main.rand.NextFloat(-varianceY, varianceY + 1));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D rotor = Request<Texture2D>(Texture + "Rotor").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            int frameHeight = texture.Height / Main.npcFrameCount[NPC.type];
            Rectangle rotorRect = new Rectangle(0, RotorFrameY + RotorTilt, rotor.Width, frameHeight);

            spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(mask, drawPos, NPC.frame, NPC.GetAlpha(new Color(255, 255, 255)), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(rotor, drawPos, rotorRect, NPC.GetAlpha(new Color(255, 255, 255)), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
