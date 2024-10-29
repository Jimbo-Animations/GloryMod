using GloryMod.NPCs.BloodMoon.BloodDrone;
using GloryMod.NPCs.BloodMoon.Hemolitionist.New.Projectiles;
using GloryMod.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace GloryMod.NPCs.BloodMoon.Hemolitionist.New
{
    partial class Hemolitionist : ModNPC
    {
        void StartBattle()
        {
            InitializeAIStates();

            if (GlorySystem.DownedRedTideWarrior == false)
            {
                if (!NPC.AnyNPCs(NPCType<RedtideWarrior>()) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<RedtideWarrior>());
                    targetNPC = Main.npc[NPC.FindFirstNPC(NPCType<RedtideWarrior>())];
                    NPC.netUpdate = true;
                }

                NPC.ai[0] = -1;
                NPC.netUpdate = true;
            }
            else
            {
                NPC.hide = false;
                NPC.ai[0] = 1;
                NPC.netUpdate = true;
            }
        }

        void NormalIntro()
        {
            AITimer++;
            AITimer2++;

            maskAlpha = MathHelper.Lerp(maskAlpha, 1, 0.025f);

            if (AITimer < 100)
            {
                Vector2 dustDirection = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);

                int dust = Dust.NewDust(NPC.Center + dustDirection * Main.rand.Next(150, 200) * NPC.scale, 0, 0, DustID.Clentaminator_Red, AITimer * .025f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = dustDirection * Main.rand.Next(-10, -5);
            }

            if (AITimer == 100)
            {
                bodyAlpha = 1;
                NPC.velocity += new Vector2(0, -2f);
                ScreenUtils.ChangeCameraPos(NPC.Center + new Vector2(0, -100), 160, 2f);
                ScreenUtils.screenShaking = 2f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), 100000, 0, target.whoAmI);
                }

                int numDusts = 45;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(25, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                NPC.netUpdate = true;
            }

            if (AITimer >= 140 && AITimer <= 280)
            {
                NPC.velocity *= 0.95f;

                if (AITimer == 150)
                {
                    bodyAnim = 2;
                    screamTimer = 100;
                }

                if (AITimer == 160) SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistRoar") with { Volume = 2 }, NPC.Center);

                if (AITimer > 160 && AITimer <= 260)
                {
                    startMusic = true;
                    if (ScreenUtils.screenShaking < 10f) ScreenUtils.screenShaking = 10f;

                    if (AITimer2 >= 10)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<RedRoar>(), 0, 0, target.whoAmI);
                        }
                        AITimer2 = 0;
                        NPC.netUpdate = true;
                    }

                    int dust = Dust.NewDust(NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
                }

                if (AITimer == 280)
                {
                    NPC.dontTakeDamage = false;
                    NPC.ai[0] = 2;
                    aiWeights[2] = 0f;
                    ResetValues();
                }
            }

            NPC.velocity *= 0.99f;
        }

        void RedTideIntro()
        {
            AITimer++;
            AITimer2++;

            if (AITimer >= 1000)
            {
                if (AITimer == 1000)
                {
                    NPC.position = targetNPC.Center - NPC.Size / 2;
                    NPC.hide = false;
                    GlorySystem.DownedRedTideWarrior = true;
                    ScreenUtils.ChangeCameraPos(NPC.Center, 200, 1.25f);
                }

                if (AITimer < 1150)
                {
                    Vector2 dustDirection = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);

                    int dust = Dust.NewDust(NPC.Center + dustDirection * Main.rand.Next(150, 200) * NPC.scale, 0, 0, DustID.Clentaminator_Red, AITimer - 1000 * .02f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = dustDirection * Main.rand.Next(-10, -5);
                }

                maskAlpha = MathHelper.Lerp(maskAlpha, 1, .015f);

                if (AITimer == 1150)
                {
                    bodyAlpha = 1;
                    NPC.velocity += new Vector2(0, -2f);
                    Main.musicFade[Main.curMusic] = MathHelper.Lerp(1, 0, 1);
                    SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                    ScreenUtils.screenShaking = 2f;

                    if (NPC.AnyNPCs(NPCType<RedtideWarrior>()))
                    {
                        targetNPC.ai[1] = 2;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), 100000, 0, target.whoAmI);                   

                    int numDusts = 45;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(25, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    }

                    NPC.netUpdate = true;
                }

                if (AITimer <= 1320 && AITimer >= 1200)
                {
                    NPC.velocity *= 0.95f;

                    if (AITimer == 1200)
                    {
                        bodyAnim = 2;
                        screamTimer = 100;
                        ScreenUtils.ChangeCameraPos(NPC.Center, 100, 2f);
                    }

                    if (AITimer == 1210)
                    {
                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistRoar") with { Volume = 2 }, NPC.Center);
                        startMusic = true;
                    }

                    if (AITimer >= 1210)
                    {
                        if (ScreenUtils.screenShaking < 10f) ScreenUtils.screenShaking = 10f;

                        if (AITimer2 >= 10)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<RedRoar>(), 0, 0, target.whoAmI);
                            }
                            AITimer2 = 0;
                            NPC.netUpdate = true;
                        }

                        int dust = Dust.NewDust(NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 1f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
                    }
                }
                if (AITimer == 1340)
                {
                    NPC.dontTakeDamage = false;
                    NPC.ai[0] = 2;
                    aiWeights[2] = 0f;
                    ResetValues();
                }

                NPC.velocity *= 0.99f;
            }
        }

        void DashAttack(Vector2 startPos, float turnSpeed = .075f, float baseSpeed = .45f)
        {
            AITimer++;

            if (AITimer == 1)
            {
                bodyAnim = 2;
                screamTimer = 70;
                SoundEngine.PlaySound(SoundID.Zombie94 with { Volume = 2 }, NPC.Center);
            }

            if (AITimer < 90)
            {
                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, turnSpeed);
                if (NPC.Distance(startPos) > 40) NPC.velocity += NPC.DirectionTo(startPos) * baseSpeed;
                NPC.velocity *= NPC.Distance(startPos) > 70 ? .96f : .9f;

                if (AITimer == 30)
                {
                    armAnim1 = 1;
                    armAnim2 = 1;
                }
            }
            else
            {
                AITimer2++;

                if (AITimer == 90)
                {
                    NPC.velocity = new Vector2(0, -45).RotatedBy(NPC.rotation);
                    SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite with { Volume = 2, Pitch = -.25f }, NPC.Center);
                    meleeActive = true;
                    int numDusts = 12;

                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(NPC.Center + new Vector2(49, 3).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Vector2 trueVelocity = new Vector2(2, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        trueVelocity.X *= 0.5f;
                        trueVelocity = trueVelocity.RotatedBy(NPC.velocity.ToRotation()) + new Vector2(3, 0).RotatedBy(NPC.velocity.ToRotation());
                        Main.dust[dust].velocity = trueVelocity;

                        int dust2 = Dust.NewDust(NPC.Center + new Vector2(-57, 3).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                        Main.dust[dust2].noGravity = true;
                        Main.dust[dust2].noLight = true;
                        Main.dust[dust2].velocity = trueVelocity;
                    }
                }

                if (AITimer <= 160 && AITimer2 >= 6)
                {
                    AITimer2 = 0;
                    bodyAnim = 1;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), Vector2.Zero, ProjectileType<HemoGrenade>(), 75, 0, target.whoAmI);
                        Main.projectile[proj].ai[0] = 1;
                    }

                    NPC.netUpdate = true;
                }

                if (AITimer == 160)
                {
                    armAnim1 = 2;
                    armAnim2 = 2;
                    meleeActive = false;
                }

                if (AITimer < 150) NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

                if (AITimer >= 150 || (!Systems.Utils.CloseTo(NPC.rotation - MathHelper.PiOver2, NPC.DirectionTo(target.Center).ToRotation(), MathHelper.PiOver2) && NPC.Distance(target.Center) > 200))
                {
                    if (!Systems.Utils.CloseTo(NPC.rotation - MathHelper.PiOver2, NPC.DirectionTo(target.Center).ToRotation(), MathHelper.PiOver2) && NPC.Distance(target.Center) > 200 && AITimer < 150) AITimer = 150;

                    NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, turnSpeed);
                    NPC.velocity *= .95f;
                }

                if (AITimer >= 180)
                {
                    ResetValues();
                    PickAttack();
                }
            }
        }

        void CannonVolleys(Vector2 targetPos, float turnSpeed = .075f, float baseSpeed = .6f)
        {
            AITimer++;
            if (AITimer == 1)
            {
                bodyAnim = 2;
                screamTimer = 68;
                ScreenUtils.screenShaking = 5f;
                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistPulseIndicator"), NPC.Center);
            }

            NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(AITimer < 120 ? target.Center : target.Center + target.velocity * (NPC.Distance(target.Center) / (Phase2Started ? 5.5f : 4.5f))).ToRotation() - MathHelper.PiOver2, turnSpeed);
            if (NPC.Distance(targetPos) > 450) NPC.velocity += NPC.DirectionTo(targetPos) * baseSpeed;
            if (NPC.Distance(targetPos) < 100) NPC.velocity += NPC.DirectionFrom(targetPos) * baseSpeed;
            NPC.velocity *= NPC.Distance(targetPos) > 500 ? .96f : .9f;

            if (NPC.Distance(targetPos) < 450 && AITimer < 120) AITimer = 120;

            if (AITimer == 150)
            {
                AITimer3 = target.Center.X < NPC.Center.X ? 1 : -1;
                SoundEngine.PlaySound(SoundID.Zombie71 with { Volume = 2, Pitch = -.25f }, NPC.Center);
                bodyAnim = 1;
            }

            if (AITimer == 172 || AITimer == 202 || AITimer == 232 || AITimer == 262)
                if (AITimer3 > 0) cannonTele1 = 1;
                else cannonTele2 = 1;

            if (AITimer == 180 || AITimer == 210 || AITimer == 240 || AITimer == 270)
                if (AITimer3 > 0) armAnim1 = 3;
                else armAnim2 = 3;

            if (AITimer == 192 || AITimer == 252 || AITimer == 222 || AITimer == 282)
            {
                int numDusts = 10;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center + new Vector2(26 * AITimer3, 53).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(6), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                SoundEngine.PlaySound(SoundID.Item125 with { Volume = 2, Pitch = -.25f }, NPC.Center);
                NPC.velocity += new Vector2(AITimer3 > 0 ? 2 : -2, -4).RotatedBy(NPC.rotation);
                NPC.rotation += AITimer3 > 0 ? -.15f : .15f;

                for (int i = -9; i < 0; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(26 * -AITimer3, 53).RotatedBy(NPC.rotation), new Vector2(0, 20).RotatedBy(NPC.rotation - MathHelper.ToRadians(5 * i) * AITimer3), ProjectileType<HemoBlast>(), 150, 0, target.whoAmI);
                }

                AITimer3 = AITimer3 > 0 ? -1 : 1;
                NPC.netUpdate = true;
            }

            if (AITimer >= 300)
            {
                ResetValues();
                PickAttack();
            }
        }

        void CarpetBomb(Vector2 targetPos, float baseSpeed = .7f, int projectileThickness = 12)
        {
            AITimer++;

            if (AITimer == 1)
            {                
                SoundEngine.PlaySound(SoundID.Zombie68 with { Volume = 2, Pitch = -.25f }, NPC.Center);
                bodyAnim = 1;
            }

            if (AITimer <= 80)
            {
                NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * .05f, .1f);
                MathHelper.Clamp(NPC.rotation, -1, 1);
            }

            if (AITimer < 80)
            {
                if (NPC.Distance(targetPos) > 10) NPC.velocity += NPC.DirectionTo(targetPos) * baseSpeed;
                NPC.velocity *= NPC.Distance(targetPos) > 100 ? .98f : .92f;
            }
            else NPC.velocity *= .97f;

            if (AITimer < 60 && NPC.Distance(targetPos) < 50) AITimer = 60;

            if (AITimer == 80)
            {
                bodyAnim = 2;
                SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 2 }, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite with { Volume = 2, Pitch = -.25f }, NPC.Center);

                screamTimer = 30;
            }

            if (AITimer > 80 && AITimer2 <= 0)
            {
                NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Pi, .1f);

                if (Systems.Utils.CloseTo(NPC.rotation, MathHelper.Pi, .1f))
                {
                    for (int i = -projectileThickness / 2; i < (projectileThickness / 2) + 1; i++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), new Vector2(i * 3, 4).RotatedBy(NPC.rotation), ProjectileType<HemoGrenade>(), 75, 0, target.whoAmI);

                        int dust = Dust.NewDust(NPC.Center + new Vector2(0, 27).RotatedByRandom(MathHelper.PiOver2).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 1f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
                    }

                    SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 2 }, NPC.Center);
                    NPC.velocity += new Vector2(0, -3).RotatedBy(NPC.rotation);
                    AITimer2++;
                    NPC.netUpdate = true;
                }
            }
           

            if (AITimer > 135)
            {
                ResetValues();
                PickAttack();
            }
        }

        /*void CarpetBomb(Vector2 targetPos, float baseSpeed = .7f, int projectileThickness = 6)
        {          
            AITimer++;
            AITimer2++;
            int goalDirection = target.Center.X < NPC.Center.X ? -1 : 1;

            if (AITimer == 1)
            {
                setPoint = targetPos + new Vector2(1500 * -goalDirection, -300);
                SoundEngine.PlaySound(SoundID.Zombie68 with { Volume = 2, Pitch = -.25f }, NPC.Center);
                bodyAnim = 1;
            }

            NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.05f, 0.1f);
            MathHelper.Clamp(NPC.rotation, -1, 1);

            if (AITimer < 80 && NPC.Distance(setPoint) < 100) AITimer = 80;

            if (AITimer == 80)
            {
                setPoint = setPoint + new Vector2(3000 * goalDirection, 0);
                NPC.velocity.X = 25 * goalDirection;
                SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 2 }, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite with { Volume = 2, Pitch = -.25f }, NPC.Center);

                armAnim1 = 1;
                armAnim2 = 1;
                bodyAnim = 2;
                screamTimer = 120;
                meleeActive = true;

                int numDusts = 12;

                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center + new Vector2(49, -17).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Vector2 trueVelocity = new Vector2(2, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    trueVelocity.X *= 0.5f;
                    trueVelocity = trueVelocity.RotatedBy(NPC.rotation + MathHelper.PiOver2) + new Vector2(3, 0).RotatedBy(NPC.rotation);
                    Main.dust[dust].velocity = trueVelocity;

                    int dust2 = Dust.NewDust(NPC.Center - new Vector2(57, 17).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                    Main.dust[dust2].noGravity = true;
                    Main.dust[dust2].noLight = true;
                    Main.dust[dust2].velocity = trueVelocity;
                }
            }

            if (AITimer <= (Phase2Started ? 190 : 200))
            {
                if (NPC.Distance(setPoint) > 100) NPC.velocity += NPC.DirectionTo(setPoint) * baseSpeed;
                NPC.velocity *= NPC.Distance(setPoint) > 200 ? .975f : .9f;

                if (AITimer > 90 && AITimer2 > projectileThickness)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), new Vector2(0, 3).RotatedBy(NPC.rotation), ProjectileType<HemoGrenade>(), 75, 0, target.whoAmI);
                    AITimer2 = 0;
                }

                if (AITimer == (Phase2Started ? 190 : 200))
                {
                    armAnim1 = 2;
                    armAnim2 = 2;
                }

                NPC.netUpdate = true;
            }

            if (AITimer > (Phase2Started ? 210 : 220))
            {
                ResetValues();
                PickAttack();
            }
        }*/

        void MissileBarrage(Vector2 targetPos, float turnSpeed = .07f, float baseSpeed = .7f)
        {
            AITimer++;
            AITimer2++;

            if (AITimer == 1)
            {
                bodyAnim = 1;
                SoundEngine.PlaySound(SoundID.Zombie99 with { Volume = 2, Pitch = -.25f }, NPC.Center);
            }

            if (AITimer <= 142) NPC.velocity *= NPC.Distance(targetPos) > 400 ? .975f : .9f;
            else NPC.velocity *= .95f;
            if (AITimer <= 120)
            {
                if (NPC.Distance(targetPos) > 300) NPC.velocity += NPC.DirectionTo(targetPos) * baseSpeed;
                if (NPC.Distance(targetPos) < 100) NPC.velocity += NPC.DirectionFrom(targetPos) * baseSpeed;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, turnSpeed);

                if (NPC.Distance(targetPos) < 300 && AITimer < 80) AITimer = 80;
            }
            else NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() - MathHelper.PiOver2, turnSpeed);

            if (AITimer == 120)
            {
                bodyAnim = 2;
                screamTimer = 68;
                armAnim1 = 3;
                armAnim2 = 3;
                cannonTimer = 80;

                SoundEngine.PlaySound(SoundID.Zombie96 with { Volume = 2 }, NPC.Center);
                ScreenUtils.screenShaking = 5f;
            }

            if (AITimer >= 142 && AITimer2 > 6 && AITimer3 <= 9)
            {
                AITimer2 = 0;
                AITimer3++;
                NPC.velocity += new Vector2(0, -6).RotatedBy(NPC.rotation);
                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Volume = 2 }, NPC.Center);

                int numDusts = 10;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center + new Vector2(-26, 53).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(6), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);

                    int dust2 = Dust.NewDust(NPC.Center + new Vector2(26, 53).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                    Main.dust[dust2].noGravity = true;
                    Main.dust[dust2].noLight = true;
                    Main.dust[dust2].velocity = new Vector2(Main.rand.NextFloat(6), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var proj1 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-26, 53).RotatedBy(NPC.rotation), new Vector2(0, 20).RotatedBy(NPC.rotation + MathHelper.ToRadians(90 - (6 * AITimer3))), ProjectileType<DroneMissile>(), 150, 0, target.whoAmI);
                    var proj2 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(26, 53).RotatedBy(NPC.rotation), new Vector2(0, 20).RotatedBy(NPC.rotation - MathHelper.ToRadians(90 - (6 * AITimer3))), ProjectileType<DroneMissile>(), 150, 0, target.whoAmI);
                    Main.projectile[proj1].ai[1] = 1;
                    Main.projectile[proj2].ai[1] = 1;
                }

                NPC.netUpdate = true;
            }

            if (AITimer > 240)
            {
                ResetValues();
                PickAttack();
            }
        }

        void Deathray(Vector2 targetPos, float turnSpeed = .05f, float baseSpeed = .6f)
        {
            AITimer++;

            if (AITimer == 1)
            {
                AITimer3 = target.Center.X < NPC.Center.X ? 1 : -1;
                bodyAnim = 2;
                screamTimer = 68;
                ScreenUtils.screenShaking = 5f;
                SoundEngine.PlaySound(SoundID.Zombie95 with { Volume = 2 }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item170 with { Volume = 2 }, targetPos);
            }

            if (AITimer <= 120)
            {
                if (NPC.Distance(targetPos) > 350) NPC.velocity += NPC.DirectionTo(targetPos) * baseSpeed;
                if (NPC.Distance(targetPos) < 100) NPC.velocity += NPC.DirectionFrom(targetPos) * baseSpeed;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, turnSpeed);
                NPC.velocity *= NPC.Distance(targetPos) > 400 ? .98f : .9f;
            }
            else NPC.velocity *= .95f;

            if (AITimer < 150) mouthTele = MathHelper.SmoothStep(mouthTele, 1, .1f);

            if (AITimer == 120)
            {
                SoundEngine.PlaySound(SoundID.Zombie98 with { Volume = 2 }, NPC.Center);
                setPoint = targetPos;

                int numDusts = 24;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(15, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }

            if (AITimer == 138)
            {
                bodyAnim = 2;
                screamTimer = 20;

                deathrayRotation = -.01f;
                if (MathHelper.WrapAngle(NPC.rotation - MathHelper.Pi / 2 - NPC.DirectionTo(targetPos).ToRotation()) > 0)
                    deathrayRotation = .01f;
            }

            if (AITimer >= 150 && AITimer < 250)
            {
                AITimer2++;
                if (AITimer == 150)
                {
                    mouthTele = 0;
                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistDeathray") with { Volume = 1.5f }, NPC.Center);
                    ScreenUtils.screenShaking = 6f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<RedRoar>(), 0, 0, target.whoAmI);
                }

                deathrayRotation = MathHelper.SmoothStep(deathrayRotation, deathrayRotation < 0 ? -1 : 1, .1f);
                NPC.velocity += new Vector2(0, -.25f).RotatedBy(NPC.rotation);
                NPC.rotation += .07f * deathrayRotation;

                if (ScreenUtils.screenShaking < 2) ScreenUtils.screenShaking = 2;
                screamTimer = 11;

                if (AITimer2 % 10 == 1)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), new Vector2(0, 1).RotatedBy(NPC.rotation + (i * MathHelper.PiOver2 / 3)), ProjectileType<DroneBullet>(), 120, 3f, Main.myPlayer, 0, 1);
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_ReleaseEntity(), NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), new Vector2(0, 1).RotatedBy(NPC.rotation), ProjectileType<NecroticRay>(), 200, 0f, target.whoAmI, NPC.whoAmI, AITimer2);

                NPC.netUpdate = true;
            }

            if (AITimer >= 250) NPC.rotation += .07f * deathrayRotation - ((AITimer - 249) * .005f);

            if (AITimer >= 270)
            {
                ResetValues();
                PickAttack();
            }
        }

        void Telefrag(Vector2 targetPos)
        {
            AITimer++;

            if (AITimer == 1 || AITimer == 71)
            {
                NPC.hide = true;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                ScreenUtils.screenShaking += 5f;
                bodyAlpha = 0;
                maskAlpha = 0;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<RedRoar>(), 0, 0, target.whoAmI);

                    for (int i = 0; i < 12; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(2, 0).RotatedBy(i * MathHelper.TwoPi / 12), ProjectileType<DroneBullet>(), 120, 3f, Main.myPlayer, 0, 1);
                    }
                }

                int numDusts = 45;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, NPC.Center);
                if (AITimer == 1) SoundEngine.PlaySound(SoundID.Item162, targetPos); 
                NPC.netUpdate = true;
            }

            if ((AITimer >= 25 && AITimer <= 40) || (AITimer >= 85 && AITimer <= 100))
            {
                if (AITimer == 25 || AITimer == 85)
                {
                    NPC.hide = false;
                    NPC.position = targetPos - NPC.Size / 2 + (AITimer == 25 ? new Vector2(target.velocity != Vector2.Zero ? 500 : 0, 0).RotatedBy(target.velocity.ToRotation()) : Vector2.Zero);
                }

                Vector2 dustDirection = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);

                int dust = Dust.NewDust(NPC.Center + dustDirection * Main.rand.Next(150, 200) * NPC.scale, 0, 0, DustID.Clentaminator_Red, Scale: 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = dustDirection * Main.rand.Next(-10, -5);

                maskAlpha = MathHelper.Lerp(maskAlpha, 1, .1f);
            }

            if (AITimer == 40 || AITimer == 100)
            {
                bodyAlpha = 1;
                NPC.velocity += new Vector2(0, -2f);
                NPC.dontTakeDamage = false;
                ScreenUtils.screenShaking = 2f;

                if (AITimer == 90)
                {
                    armAnim1 = 2;
                    armAnim2 = 2;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), 300, 0, target.whoAmI, 0, 0, 1);
                }

                int numDusts = 45;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(25, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                NPC.netUpdate = true;
            }

            if (AITimer > 120)
            {
                ResetValues();
                InitializeAIStates();
                aiWeights[7] = 0f;
                PickAttack();
            }          
        }

        void Phase2()
        {
            AITimer++;
            AITimer2++;
            NPC.velocity *= .95f;
            NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.05f, 0.1f);
            MathHelper.Clamp(NPC.rotation, -1, 1);

            if (AITimer == 1)
            {
                NPC.dontTakeDamage = true;
                ScreenUtils.ChangeCameraPos(NPC.Center, 180, 1.25f);
                SoundEngine.PlaySound(SoundID.Zombie99 with { Volume = 2, Pitch = -.25f }, NPC.Center);
                bodyAnim = 1;
            }

            if (AITimer >= 90)
            {
                if (AITimer == 90) SoundEngine.PlaySound(SoundID.Item170 with { Volume = 2 }, NPC.Center);
                Phase2Started = true;
                phase2Haze = MathHelper.SmoothStep(phase2Haze, 1, .2f);
            }

            if (AITimer >= 180 && AITimer <= 300)
            {
                if (AITimer == 180)
                {
                    ScreenUtils.ChangeCameraPos(NPC.Center, 120, 2f);
                    bodyAnim = 2;
                    screamTimer = 100;
                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistRoar") with { Volume = 2 }, NPC.Center);
                }

                if (ScreenUtils.screenShaking < 10f) ScreenUtils.screenShaking = 10f;
                NPC.rotation += Main.rand.NextFloat(-.15f, .15f);

                if (AITimer2 >= 10)
                {
                    if (NPC.lifeMax - NPC.life > 750) 
                    {
                        NPC.life += 750;
                        NPC.HealEffect(750, true);
                    }
                    else if (NPC.lifeMax - NPC.life <= 750 && NPC.lifeMax - NPC.life > 0) 
                    {
                        NPC.life += NPC.lifeMax - NPC.life;
                        NPC.HealEffect(NPC.lifeMax - NPC.life, true);
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<RedRoar>(), 0, 0, target.whoAmI);
                    }

                    AITimer2 = 0;
                    NPC.netUpdate = true;
                }

                int dust = Dust.NewDust(NPC.Center + new Vector2(0, 6).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
            }

            if (AITimer > 330)
            {
                NPC.dontTakeDamage = false;
                ResetValues();
                InitializeAIStates();
                NPC.ai[0] = 6;
                aiWeights[6] = 0f;
            }
        }

        void DeathCutscene()
        {
            AITimer++;
            NPC.velocity *= .95f;
            NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.05f, 0.1f);
            MathHelper.Clamp(NPC.rotation, -1, 1);

            if (AITimer == 1)
            {
                ScreenUtils.ChangeCameraPos(NPC.Center, 180, 1.25f);
                SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 2, Pitch = -.35f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);

                bodyAnim = 2;
                screamTimer = 30;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<RedRoar>(), 0, 0, target.whoAmI);
                }

                NPC.netUpdate = true;
            }

            if (AITimer > 40 && AITimer <= 200 && NPC.velocity.Y < 1) NPC.velocity.Y += .05f;

            if (AITimer == 75)
            {
                NPC.velocity += new Vector2(3, -3).RotatedBy(NPC.rotation);
                NPC.rotation += .15f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-26, 33).RotatedBy(NPC.rotation), Vector2.Zero, ProjectileType<DroneExplosion>(),0, 0, target.whoAmI);

                removeArm1 = true;
                int gore = Mod.Find<ModGore>("HemolitionistGore2").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-26, 33).RotatedBy(NPC.rotation), new Vector2(-5, -3), gore);
                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
            }

            if (AITimer == 125)
            {
                NPC.velocity -= new Vector2(5, 6).RotatedBy(NPC.rotation);
                NPC.rotation -= .15f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(26, 33).RotatedBy(NPC.rotation), Vector2.Zero, ProjectileType<DroneExplosion>(), 0, 0, target.whoAmI);

                removeArm2 = true;
                int gore = Mod.Find<ModGore>("HemolitionistGore2").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center + new Vector2(26, 33).RotatedBy(NPC.rotation), new Vector2(7, 0), gore);
                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
            }

            if (AITimer > 150)
            {
                AITimer2 += .0005f;
                NPC.rotation += Main.rand.NextFloat(-AITimer2, AITimer2);
            }

            if (AITimer == 160)
            {
                ScreenUtils.ChangeCameraPos(NPC.Center, 120, 2f);
                bodyAnim = 2;
                screamTimer = 100;
                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistRoarAlt"), NPC.Center);
            }

            if (AITimer > 180)
            {
                if (ScreenUtils.screenShaking > 5f) ScreenUtils.screenShaking = MathHelper.Lerp(ScreenUtils.screenShaking, 5, .1f);
                int dust = Dust.NewDust(NPC.Center + new Vector2(0, 27).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
            }

            if (AITimer == 260)
            {
                readyToDie = true;
                SoundEngine.PlaySound(SoundID.Item62 with { Volume = 2, Pitch = -.2f }, NPC.Center);
                NPC.SimpleStrikeNPC(10000, 0, true, 0, DamageClass.Default, true);
                ScreenUtils.screenShaking += 10;

                int numDusts = 120;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 60), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {                 
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<GrenadeExplosion>(), 0, 8f, target.whoAmI);                 
                }
                NPC.netUpdate = true;
            }    
        }
    }
}
