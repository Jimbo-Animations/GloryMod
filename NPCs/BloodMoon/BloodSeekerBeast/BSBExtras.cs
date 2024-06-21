using System.IO;
using GloryMod.Items.BloodMoon;
using GloryMod.Items.BloodMoon.Hemolitionist;
using GloryMod.Items.Sightseer;
using GloryMod.NPCs.BloodMoon.BloodDrone;
using GloryMod.Systems;
using Humanizer;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;

namespace GloryMod.NPCs.BloodMoon.BloodSeekerBeast
{
    partial class BSBHead : WormHead
    {
        public override int BodyType => NPCType<BSBBody>();

        public override int TailType => NPCType<BSBTail>();

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

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override bool CheckActive()
        {
            return NPC.ai[2] != 5;
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
            Despawn = 5
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[2];
            set => NPC.localAI[2] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[3];
        Vector2 choosePoint;
        bool phase2;

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
                            NPC.velocity += new Vector2(15 + MathHelper.Clamp(NPC.Distance(target.Center) / 100, 1, 15), 0).RotatedBy(NPC.rotation - MathHelper.PiOver2);

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

        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 2.5f, Pitch = -.3f }, NPC.Center);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var n = Main.npc[i];
                if (!NPC.active) continue;

                if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), n.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), 150, 0, target.whoAmI);

                        int gore1 = Mod.Find<ModGore>("BSBGore1").Type;
                        int gore2 = Mod.Find<ModGore>("BSBGore2").Type;
                        int gore3 = Mod.Find<ModGore>("BSBGore3").Type;
                        int gore4 = Mod.Find<ModGore>("BSBGore4").Type;
                        Gore.NewGore(NPC.GetSource_FromThis(), n.position, new Vector2(Main.rand.NextFloat(-4, 5), Main.rand.NextFloat(-4, 5)) + NPC.velocity, n.type == Type ? gore4 : n.type == BodyType ? (n.ai[1] % 5 == 1 ? gore2 : gore1) : gore3);

                        n.netUpdate = true;
                    }                       
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            var SpawnRule = Main.ItemDropsDB.GetRulesForNPCID(NPCID.BloodEelHead, false);
            foreach (var dropRule in SpawnRule)
            {
                npcLoot.Add(dropRule);
            }
            npcLoot.Add(ItemDropRule.Common(ItemType<HemolicLance>()));
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
    }
}
