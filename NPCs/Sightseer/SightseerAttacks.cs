using Terraria.Audio;
using GloryMod.Systems;
using GloryMod.NPCs.Sightseer.Minions;
using GloryMod.NPCs.Sightseer.Projectiles;

namespace GloryMod.NPCs.Sightseer
{
    internal partial class Sightseer : ModNPC
    {
        private void Hiding(int lastAttack)
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(0, -1000), true);
                SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f, Volume = 1.25f }, NPC.Center);

                AITimer++;
                NPC.dontTakeDamage = true;
                NPC.chaseable = false;
                NPC.hide = true;
                beVisible = false;
                NPC.rotation = 0;
            }

            if (NPC.Distance(target.Center + new Vector2(0, -1000)) > 10) NPC.velocity += NPC.DirectionTo(target.Center + new Vector2(0, -400)) * 0.5f;
            NPC.velocity *= 0.98f;
            NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);

            AITimer++;

            if (AITimer > (phase2Started || phase4Started ? 60 : 75))
            {
                NPC.netUpdate = true;
                NPC.hide = false;

                ChangePosition(target.Center + new Vector2(0, -250), false);
                ChooseAttack(lastAttack);               
            }
        }

        private void Stunned(int lastAttack, int stunTime = 200)
        {
            if (AITimer == 0)
            {
                NPC.dontTakeDamage = false;
                useSilhouette = false;
                NPC.chaseable = true;
                SoundEngine.PlaySound(SoundID.DD2_LightningBugDeath with { Pitch = -0.35f }, NPC.Center);
                int numDusts = 24;

                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 111, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }

            AITimer++;
            NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);
            NPC.velocity += NPC.DirectionTo(target.Center) * 0.075f;
            NPC.velocity *= 0.98f;

            Lighting.AddLight(NPC.Center, 0.5f, 0.5f, 0.75f);

            if (AITimer >= stunTime)
            {
                NPC.dontTakeDamage = true;
                NPC.chaseable = false;
                NPC.netUpdate = true;
                ChooseAttack(lastAttack);
            }
        }

        private void IntroScene()
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(0, -1000), false);
                Music = MusicLoader.GetMusicSlot(Mod, "Music/Sightseer_Intro");
            }
            AITimer++;

            if (AITimer == 300)
            {
                ChangePosition(target.Center + new Vector2(0, -250), true);
                ScreenUtils.ChangeCameraPos(NPC.Center, 200, 1.25f);
                beVisible = true;
            }

            if (AITimer == 460)
            {
                NPC.frame.Y = 0;
                animState = 1;
            }

            if (AITimer == 480)
            {
                eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 0.5f));
                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerShriek") with { Volume = 1.25f }, NPC.Center);
                ScreenUtils.screenShaking = 10f;

                Main.NewLightning();
            }

            if (AITimer >= 480 && AITimer <= 501 && AITimer % 10 == 1)
            {
                rings.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 200f));
            }

            if (AITimer > 500)
            {
                beVisible = false;
                visibility = MathHelper.SmoothStep(visibility, 0, 0.2f);
            }

            if (AITimer >= 600)
            {
                ResetValues();
                NPC.ai[0] = 1;
                NPC.netUpdate = true;
            }
        }

        private void MirrorChase(int mirageCount = 4)
        {
            if (AITimer == 0)
            {
                NPC.rotation = NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;
                ChangePosition(target.Center + new Vector2(400, 0).RotatedByRandom(MathHelper.TwoPi), true);              
            }
            AITimer++;

            if (AITimer == 10) eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 0.25f));

            if (AITimer > 150)
            {
                RepeatAttack++;
                AITimer = 1;

                float swapWith = Main.rand.Next(2, mirageCount);

                if (RepeatAttack < 3)
                {
                    ChangePosition(target.Center + new Vector2(NPC.Center.X - target.Center.X, NPC.Center.Y - target.Center.Y).RotatedBy(MathHelper.TwoPi / mirageCount * swapWith), false);
                    NPC.rotation += MathHelper.TwoPi / mirageCount * swapWith;
                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), NPC.Center);
                }
                else
                {
                    if (target.HasBuff<SeersTag>() == false) SetStun(1);
                    else SetHide(1);
                }

                NPC.netUpdate = true;
            }

            if (NPC.Distance(target.Center) > 10)
            {
                NPC.velocity += NPC.DirectionTo(target.Center) * (AITimer > 60 ? 0.5f : 0.05f);
                NPC.velocity *= 0.99f;
            }

            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
        }

        private void WiggleWalls(int attackRate = 100, int wallCount = 4)
        {
            if (AITimer == 0) ChangePosition(target.Center + new Vector2(0, -400), true);
            AITimer++;

            if (NPC.Distance(target.Center + new Vector2(0, -400)) > 10) NPC.velocity += NPC.DirectionTo(target.Center + new Vector2(0, -400)) * 0.5f;
            NPC.velocity *= 0.98f;
            NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);

            if (AITimer % attackRate == 1 && AITimer != 1)
            {
                if (RepeatAttack < wallCount)
                {
                    eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 0.5f));

                    int whichWallIsFake = Main.rand.NextBool() ? -1 : 1;
                    for (int i = -3; i < 5; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center + new Vector2(500 * target.direction, 120 * i), new Vector2(-5 * target.direction, 0), ProjectileType<WigglyShot>(), 75, 0, target.whoAmI, 0, whichWallIsFake, Main.rand.NextBool() ? -1 : 1);
                    }

                    for (int i = -5; i < 6; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center + new Vector2(90 * i, -500), new Vector2(0, 5), ProjectileType<WigglyShot>(), 60, 0, target.whoAmI, 0, -whichWallIsFake, Main.rand.NextBool() ? -1 : 1);
                    }

                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), target.Center);
                    RepeatAttack++;
                    AIRandomizer = AIRandomizer == -1 ? 1 : -1;
                }
                else
                {
                    if (target.HasBuff<SeersTag>() == false) SetStun(2);
                    else SetHide(2);
                }

                NPC.netUpdate = true;
            }
        }

        private void RapidDash(int projCount = 8, int minionTimer = 50)
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(0, -250), true);
            }

            AITimer++;
            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);

            if (AITimer <= 120)
            {
                NPC.velocity.X += 0.75f * AIRandomizer;
                NPC.velocity *= 0.95f;

                Vector2 spawnMinions = target.Center + new Vector2(Main.rand.Next(-200, 201), Main.rand.Next(-300, -201));

                if (phase2Started && AITimer % minionTimer == 1)
                {
                    minion = Main.npc[NPC.NewNPC(NPC.InheritSource(NPC), (int)spawnMinions.X, (int)spawnMinions.Y, NPCType<SightseerMinion>())];
                    minion.ai[0] = NPC.whoAmI;
                    NPC.netUpdate = true;
                }

                if (AITimer >= 40)
                {
                    if (AITimer == 40) eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1));
                    beVisible = false;
                    visibility = MathHelper.Lerp(visibility, 0, 0.1f);
                }
            }

            if (AITimer >= 150)
            {
                if (AITimer == 150)
                {
                    ChangePosition(target.Center + new Vector2(-400 * AIRandomizer, 0), true);
                    ramming = true;
                    beVisible = true;
                    visibility = 1;

                    NPC.velocity += NPC.DirectionTo(target.Center) * 20f;
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, NPC.Center);

                    for (int i = 0; i < projCount; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1).RotatedBy(MathHelper.TwoPi * i / projCount), ProjectileType<WigglyShot>(), 60, 0, target.whoAmI, 1, 0, Main.rand.NextBool() ? -1 : 1);
                    }

                    NPC.netUpdate = true;
                }

                NPC.velocity += NPC.DirectionTo(target.Center) * 0.65f;
                NPC.velocity *= 0.995f;
            }

            if (AITimer == 240)
            {
                if (target.HasBuff<SeersTag>() == false) SetStun(3);
                else SetHide(3);
                NPC.netUpdate = true;
            }
        }

        private void MineField(int fieldSize = 8, float speedMult = 0.45f)
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(250 * -target.direction, 0), true);
                NPC.rotation = NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;
                NPC.netUpdate = true;
            }
            AITimer++;

            if (AITimer == 30)
            {
                for (int i = 1; i <= fieldSize; i++)
                {
                    for (int j = 0; j < 5 + i; j++)
                    {
                        int Proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center, i % 2 == 1 ? new Vector2(1) : new Vector2(-1), ProjectileType<OrbitShot>(), 60, 0, target.whoAmI, 55 + (110 * i), j, 5 + i);
                        Main.projectile[Proj].hostile = j % 3 != 1;
                    }
                }

                eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1));

                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), target.Center);
                SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f }, NPC.Center);
            }

            if (AITimer > 90)
            {
                if (NPC.Distance(target.Center) > 10)
                {
                    NPC.velocity += NPC.DirectionTo(target.Center) * speedMult;
                    NPC.velocity *= 0.9f;
                }

                ramming = true;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
            }

            if (AITimer > 330)
            {
                if (target.HasBuff<SeersTag>() == false) SetStun(4);
                else SetHide(4);
                NPC.netUpdate = true;
            }
        }

        private void JellyTransition(int summonTimer = 60)
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(0, -1000), false);
                beVisible = false;
                NPC.hide = true;
            }
            AITimer++;

            Vector2 spawnMinions = target.Center + new Vector2(0, Main.rand.NextFloat(450, 501)).RotatedBy(AITimer * AIRandomizer * 0.033f);

            if (AITimer < 450)
            {
                if (NPC.Distance(target.Center + new Vector2(0, -1000)) > 10) NPC.velocity += NPC.DirectionTo(target.Center + new Vector2(0, -1000)) * 0.5f;
                NPC.velocity *= 0.98f;

                if (AITimer % summonTimer == 1)
                {
                    minion = Main.npc[NPC.NewNPC(NPC.InheritSource(NPC), (int)spawnMinions.X, (int)spawnMinions.Y, NPCType<SightseerMinion>())];
                    minion.ai[0] = NPC.whoAmI;

                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), NPC.Center);
                    NPC.netUpdate = true;
                }
            }

            if (AITimer == 450)
            {
                ChangePosition(spawnMinions, true);
                ramming = true;
                NPC.rotation = NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;
                NPC.velocity = Vector2.Zero;
                NPC.hide = false;
                beVisible = true;
                NPC.frame.Y = 0;
                animState = 1;
                animSpeed = 10;

                eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1));

                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1).RotatedBy(MathHelper.TwoPi * i / 8), ProjectileType<WigglyShot>(), 60, 0, target.whoAmI, 1, 0, Main.rand.NextBool() ? -1 : 1);
                }
                SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f }, NPC.Center);
                NPC.netUpdate = true;
            }

            if (AITimer >= 480)
            {
                if (AITimer == 480)
                {
                    NPC.velocity += new Vector2(25, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2);
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, NPC.Center);
                }

                NPC.velocity += NPC.DirectionTo(target.Center) * 0.15f;
                NPC.velocity *= 0.99f;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
            }

            if (AITimer > 600)
            {
                phase2Started = true;

                if (target.HasBuff<SeersTag>() == false) SetStun(5);
                else SetHide(5);
                NPC.netUpdate = true;
            }
        }

        void Phase2Transition()
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(0, -400), true);
                SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f }, NPC.Center);
                Music = MusicLoader.GetMusicSlot(Mod, "Music/Sightseer_Phase_2_w_Intro");
            }
            AITimer++;

            if (AITimer < 60)
            {
                NPC.velocity *= 0.96f;
                NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);
            }

            if (RepeatAttack < 20 && Main.rand.NextBool(20 - (int)RepeatAttack))
            {
                Vector2 velocity = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
                int dust = Dust.NewDust(NPC.Center + (velocity * Main.rand.NextFloat(150, 250)), 0, 0, 111, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-10, -15), 0).RotatedBy(velocity.ToRotation());
            }

            if (AITimer > 60 && AITimer < 780)
            {
                NPC.rotation += Main.rand.NextFloat(-0.01f * RepeatAttack, 0.01f * RepeatAttack);
            }

            if (AITimer % (60 - (RepeatAttack * 2)) == 1 && AITimer > 1 && AITimer < 720 && RepeatAttack < 20)
            {
                RepeatAttack++;
                ChangePosition(target.Center + new Vector2(0, Main.rand.Next(301)).RotatedByRandom(MathHelper.TwoPi), true);
                NPC.rotation = Main.rand.NextFloat(-MathHelper.TwoPi, MathHelper.TwoPi);

                SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f }, NPC.Center);
                eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1f));
            }
            if (RepeatAttack == 20)
            {
                RepeatAttack++;
                ChangePosition(target.Center + new Vector2(0, -1000), true);
                visibility = 0;
                beVisible = false;
            }

            if (AITimer == 690)
            {
                ChangePosition(target.Center + new Vector2(0, -400), false);
                ScreenUtils.ChangeCameraPos(NPC.Center, 150, 1.25f);
            }

            if (AITimer == 780)
            {
                ChangePosition(NPC.Center, true);
                NPC.rotation = 0;
                visibility = 1;
                spriteWidth = 1;

                animState = 5;
                useSilhouette = false;

                int numDusts = 24;

                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 111, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerShriek") with { Volume = 1.25f }, NPC.Center);
                ScreenUtils.screenShaking = 15f;

                Main.NewLightning();

                int gore1 = Mod.Find<ModGore>("SightseerGore1").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore1);
                int gore2 = Mod.Find<ModGore>("SightseerGore2").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore2);
                int gore3 = Mod.Find<ModGore>("SightseerGore3").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore3);
                int gore4 = Mod.Find<ModGore>("SightseerGore4").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore4);
                NPC.netUpdate = true;
            }

            if (AITimer >= 780 && AITimer % 10 == 1)
            {
                rings.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 200f));
            }

            if (AITimer > 840)
            {
                phase3Started = true;
                NPC.ai[0] = 7;
                ResetValues();
                NPC.netUpdate = true;
            }
        }

        void TeleRush(int attackRate, int attackCount)
        {
            if (AITimer == 0)
            {
                flash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1));
                NPC.frame.Y = 0;
                animState = 4;
                animSpeed = 6;
                AIRandomizer = phase4Started ? Main.rand.Next(4, 6) : Main.rand.Next(2, 4);
                beVisible = false;
            }
            AITimer++;

            Vector2 spawnMinions = NPC.Center + new Vector2(0, Main.rand.Next(0, 101)).RotatedByRandom(MathHelper.TwoPi);

            if (AITimer % attackRate == 1 && AITimer != 1)
            {
                RepeatAttack++;
                NPC.hide = true;

                if (RepeatAttack <= attackCount)
                {
                    minion = Main.npc[NPC.NewNPC(NPC.InheritSource(NPC), (int)spawnMinions.X, (int)spawnMinions.Y, NPCType<SightseerClone>())];
                    minion.ai[0] = NPC.whoAmI;
                    if (RepeatAttack == AIRandomizer)
                    {
                        minion.ai[1] = 0;
                        AIRandomizer += phase4Started ? Main.rand.Next(4, 7) : Main.rand.Next(2, 4);
                    }
                    else minion.ai[1] = 1;

                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), NPC.Center);
                    NPC.netUpdate = true;
                }
            }

            if (NPC.Distance(target.Center) > 300)
            {
                NPC.velocity += NPC.DirectionTo(target.Center) * 0.5f;
                NPC.velocity *= 0.98f;
            }
            else NPC.velocity *= 0.96f;

            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
            visibility = MathHelper.Lerp(visibility, 0, 0.05f);

            if (AITimer > attackRate * (attackCount + 1))
            {
                NPC.hide = false;
                ChangePosition(NPC.Center, true);

                if (target.HasBuff<SeersTag>() == false) SetStun(7);
                else SetHide(7);

                NPC.netUpdate = true;
            }
        }

        int ringRand;
        void MirageRings(int ringCount, int attackRate)
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(0, -400).RotatedByRandom(MathHelper.ToRadians(10)), true);
                NPC.velocity = NPC.DirectionTo(target.Center).RotatedBy(MathHelper.PiOver4 * AIRandomizer) * 5f;

                animState = 5;
                animSpeed = 5;

                ringRand = ringCount + Main.rand.Next(0, 2);
            }
            AITimer++;

            if (AITimer >= attackRate || AITimer >= attackRate / 2 && RepeatAttack == 0)
            {
                if (RepeatAttack < ringRand)
                {
                    Vector2 randomizer = new Vector2(100, 0).RotatedByRandom(MathHelper.TwoPi);

                    for (int i = 0; i < 9; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center + randomizer.RotatedBy(i * MathHelper.TwoPi / 9), target.velocity * 0.25f, ProjectileType<HomingShot>(), 75, 0, target.whoAmI, 0, 0, i % 3 == 1 ? 1 : 0);
                    }

                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), NPC.Center);
                }
                else
                {
                    animState = 3;
                    eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1f));

                    for (int i = -2; i < 3; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(target.Center).RotatedBy(i * MathHelper.Pi / 5), ProjectileType<HomingShot>(), 75, 0, target.whoAmI, 1);
                    }

                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), NPC.Center);
                }              

                AITimer = 1;
                RepeatAttack++;
                if (RepeatAttack == ringRand) SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f, Volume = 1.25f }, NPC.Center);
                NPC.netUpdate = true;
            }

            if (RepeatAttack <= ringRand) NPC.velocity += NPC.DirectionTo(target.Center).RotatedBy(NPC.Distance(target.Center) < 400 ? MathHelper.PiOver4 * AIRandomizer : 0) * 1.2f;
            NPC.velocity *= 0.96f;

            NPC.rotation = NPC.rotation.AngleTowards(RepeatAttack <= ringRand ? NPC.velocity.ToRotation() + MathHelper.PiOver2 : NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, 0.15f);

            if (AITimer >= attackRate * 2 / 3 && RepeatAttack > ringRand)
            {
                if (target.HasBuff<SeersTag>() == false) SetStun(8);
                else SetHide(8);
                NPC.netUpdate = true;
            }
        }

        void FakeOut(int teleCount)
        {
            AITimer++;

            if (AITimer % 15 == 1 && RepeatAttack < teleCount)
            {
                RepeatAttack++;
                ChangePosition(target.Center + (RepeatAttack == teleCount ? (target.velocity != Vector2.Zero ? new Vector2(200, 0).RotatedBy(target.velocity.ToRotation()) : new Vector2(200 * target.direction, 0)) : new Vector2(0, Main.rand.Next(251)).RotatedByRandom(MathHelper.TwoPi)), true);
                NPC.rotation = NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;

                if (RepeatAttack == teleCount)
                {
                    eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1f));
                    ramming = true;
                    for (int i = -2; i < 3; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(target.Center).RotatedBy(i * MathHelper.Pi / 5), ProjectileType<WigglyShot>(), 75, 0, target.whoAmI, 0, 0, Main.rand.NextBool() ? -1 : 1);
                    }
                    SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f }, NPC.Center);
                }

                NPC.netUpdate = true;
            }

            if (AITimer >= (15 * teleCount) + 60)
            {
                if (target.HasBuff<SeersTag>() == false) SetStun(9);
                else SetHide(9);
                NPC.netUpdate = true;
            }
        }

        void Vortex(int attackRate)
        {
            Vector2 targetGround = Systems.Utils.findGroundUnder(target.Center) + new Vector2(0, -250);

            if (AITimer == 0) 
            {
                ChangePosition(targetGround, true);
                beVisible = false;
                NPC.hide = true;
                NPC.rotation = 0;
                NPC.netUpdate = true;
            }
            AITimer++;

            if (AITimer == 15)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SightseerAnomaly>(), 75, 0, target.whoAmI, 0, attackRate);
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, NPC.Center);
                NPC.netUpdate = true;
            }

            if (AITimer >= 30 && AITimer < 420 && NPC.Distance(target.Center) >= 250)
            {
                target.velocity += target.DirectionTo(NPC.Center) * MathHelper.Clamp((target.Distance(NPC.Center) * 0.0025f) - 0.5f, 0.001f, 2.5f);

                if (NPC.Distance(target.Center) > 500) target.RemoveAllGrapplingHooks();
            }

            if (AITimer == 450)
            {
                NPC.hide = false;
                beVisible = true;
                eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1f));
                ChangePosition(NPC.Center, true);
            }

            NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, 0.1f);

            if (AITimer > 500)
            {           
                if (target.HasBuff<SeersTag>() == false) SetStun(10);
                else SetHide(10);
                NPC.netUpdate = true;
            }
        }

        void MirrorChase2()
        {
            if (AITimer == 0)
            {
                NPC.rotation = NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;
                ChangePosition(target.Center + new Vector2(400 * AIRandomizer, 0).RotatedByRandom(MathHelper.ToRadians(15)), true);

                animState = 3;
                animSpeed = 5;
            }
            AITimer++;

            if (AITimer == 10) eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 0.25f));

            if (AITimer < 90)
            {
                if (AITimer <= 60) NPC.velocity += NPC.DirectionTo(target.Center) * 0.05f;          
                if (AITimer == 75) 
                {
                    animState = 4;
                    NPC.frame.Y = 0;
                }

                NPC.rotation = AITimer <= 60 ? NPC.velocity.ToRotation() + MathHelper.PiOver2 : NPC.rotation.AngleTowards((NPC.DirectionTo(target.Center + target.velocity * (NPC.Distance(target.Center) / 10)) * 25).ToRotation() + MathHelper.PiOver2, 0.1f);
            }

            if (AITimer == 90)
            {
                NPC.velocity = NPC.velocity = NPC.DirectionTo(target.Center + target.velocity * (NPC.Distance(target.Center) / 10)) * 25;
                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                ramming = true;

                SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, NPC.Center);
                NPC.netUpdate = true;
            }

            NPC.velocity *= 0.99f;

            if (AITimer > 150)
            {
                if (target.HasBuff<SeersTag>() == false) 
                {
                    SetStun(11);
                    ChangePosition(target.Center + new Vector2(200 * target.direction, 0), true);
                }
                else SetHide(11);
                NPC.netUpdate = true;
            }
        }

        void DeathAnim()
        {
            if (AITimer == 0)
            {
                ChangePosition(target.Center + new Vector2(0, -200), true);
                eyeFlash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 0.5f));
                rings.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 200f));
                ScreenUtils.ChangeCameraPos(NPC.Center, 180, 1.25f);
                SoundEngine.PlaySound(SoundID.DD2_LightningBugDeath with { Pitch = -0.35f }, NPC.Center);
                NPC.rotation = 0;

                NPC.netUpdate = true;
            }
            AITimer++;

            NPC.rotation = 0 + Main.rand.NextFloat(-0.01f * RepeatAttack, 0.01f * RepeatAttack);

            if (AITimer % 5 == 1 && AITimer != 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, 5).RotatedByRandom(MathHelper.TwoPi), ProjectileType<WigglyShot>(), 0, 0, target.whoAmI, 1, Main.rand.NextBool() ? -1 : 1, Main.rand.NextBool() ? -1 : 1);
                }

                for (int i = 0; i < 5; i++) Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 33, Scale: 2f)].noGravity = true;

                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), NPC.Center);
                ScreenUtils.screenShaking = 5f;
                RepeatAttack++;
                NPC.netUpdate = true;
            }

            if (AITimer == 180) Main.NewLightning();

            if (AITimer > 180)
            {
                NPC.active = false;
                NPC.NPCLoot();
                ScreenUtils.screenShaking = 15f;
                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerShriek") with { Volume = 1.25f }, NPC.Center);

                NPC.netUpdate = true;
            }
        }
    }

    partial class Sightseer : ModNPC //Extra AI features
    {
        private void ChooseAttack(int lastAttack)
        {
            int Choice;

            Choice = phase3Started ? (phase4Started ? Main.rand.Next(new int[] { 7, 8, 9, 10, 11 }) : Main.rand.Next(new int[] { 7, 8, 9, 10 })) : Main.rand.Next(new int[] { 1, 2, 3, 4 });

            while (Choice == lastAttack) Choice = phase3Started ? (phase4Started ? Main.rand.Next(new int[] { 7, 8, 9, 10, 11 }) : Main.rand.Next(new int[] { 7, 8, 9, 10 })) : Main.rand.Next(new int[] { 1, 2, 3, 4 });

            if (Choice != lastAttack)
            {
                NPC.ai[0] = Choice;
            }

            if (Choice == 1) visibility = 0;

            if (startPhase3 == true && phase3Started == false) NPC.ai[0] = 6;
            if (startPhase2 == true && phase2Started == false) NPC.ai[0] = 5;

            ResetValues();
        }

        private void SetStun(int lastAttack)
        {
            ResetValues();
            NPC.ai[0] = -1;
            RepeatAttack = lastAttack;
        }

        private void SetHide(int lastAttack)
        {
            ResetValues();
            NPC.ai[0] = -2;
            RepeatAttack = lastAttack;
        }

        private void ResetValues()
        {
            AITimer = 0;
            RepeatAttack = 0;
            animState = phase3Started ? 3 : 0;
            animSpeed = 6;
            AIRandomizer = Main.rand.NextBool() ? -1 : 1;
            useSilhouette = true;
            beVisible = true;
            ramming = false;

            if (target.HasBuff<SeersTag>() == true)
            {
                target.ClearBuff(BuffType<SeersTag>());
            }
        }

        private void ChangePosition(Vector2 changeTo, bool useEffects)
        {
            if (useEffects)
            {
                ScreenUtils.screenShaking = 5f;
                spriteWidth = 0;
                visibility = 0;
            }

            NPC.position = changeTo - NPC.Size / 2;
            NPC.velocity = Vector2.Zero;
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
            NPC.netUpdate = true;
        }
    }
}
