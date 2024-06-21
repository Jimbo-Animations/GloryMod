using Terraria.Audio;

namespace GloryMod.NPCs.IceFish
{
    partial class HM : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            NPCID.Sets.TrailCacheLength[NPC.type] = 6;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 54;
            NPC.height = 54;

            NPC.damage = 0;
            NPC.defense = Main.hardMode ? 20 : 10;
            NPC.lifeMax = Main.getGoodWorld ? (Main.hardMode ? 18000 : 1800) : (Main.hardMode ? 14000 : 1400);
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            NPC.npcSlots = 50f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.DD2_WitherBeastCrystalImpact;
            NPC.DeathSound = null;
            NPC.coldDamage = true;
            NPC.netAlways = true;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void PostAI()
        {
            bool collision = CheckTileCollision();
            bool cheese = platformCheese();

            if (NPC.soundDelay == 0 && collision)
            {
                // Play sounds quicker the closer the NPC is to the target location
                float num1 = NPC.Distance(target.Center) / 40f;

                if (num1 < 10)
                    num1 = 10f;

                if (num1 > 20)
                    num1 = 20f;

                NPC.soundDelay = (int)num1;

                SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
            }

            if (cheese)
                enrageTimer++;
            else enrageTimer--;

            MathHelper.Clamp(enrageTimer, 0, 200);

            if (enrageTimer >= 200) ResetValues(true);

            NPC.damage = jumping ? 90 : 0;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            bool collision = CheckTileCollision();
            bool cheese = platformCheese();

            Vector2 standardTarget = Systems.Utils.findGroundUnder(target.Top);

            switch (AIstate)
            {
                case AttackPattern.Enraged:

                    AITimer++;
                    TunnelAbout(standardTarget, ref collision, 5, 5, 200, 200);

                    if (AITimer != 1 && AITimer % 10 == 1)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center - new Vector2(Main.rand.NextFloat(-500, 501), 1000), new Vector2(0, Main.rand.NextFloat(5, 9)).RotatedByRandom(MathHelper.ToRadians(15)), ProjectileID.FrostWave, 120 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI);
                        }

                        NPC.netUpdate = true;
                    }

                    if (AITimer > 240) ResetValues();

                    break;

                case AttackPattern.StartBattle:

                    TimerRand = Main.rand.NextFloat(300, 361);
                    InitializeAIStates();
                    NPC.ai[0]++;

                    NPC.netUpdate = true;

                    break;


                case AttackPattern.Idle:

                    AITimer++;
                    TunnelAbout(standardTarget, ref collision);

                    if (AITimer > TimerRand)
                    {
                        PickAttack();
                        AITimer = 0;

                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.JumpStrike:

                    if (AITimer == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 2, Pitch = -.2f }, NPC.Center);
                        moveToZone = Systems.Utils.findGroundUnder(target.Top + new Vector2(Main.rand.NextFloat(300, 401) * (Main.rand.NextBool() ? 1 : -1), 0));
                    }

                    if (AITimer < 75) AITimer++;

                    if (AITimer < 75) TunnelAbout(Systems.Utils.findGroundUnder(moveToZone), ref collision, 8, 7, 10, 400);

                    if (AITimer >= 75 && !hasJumped)
                    {
                        if (AITimer == 75)
                        {
                            moveToZone = target.Top;
                            NPC.netUpdate = true;
                        }

                        if (collision)
                        {
                            NPC.velocity += NPC.DirectionTo(moveToZone) * .4f;

                            if (Math.Abs(NPC.velocity.X) > 12) NPC.velocity.X += NPC.velocity.X > 0 ? -0.4f : 0.4f;
                            if (Math.Abs(NPC.velocity.Y) > 12) NPC.velocity.Y += NPC.velocity.Y > 0 ? -0.4f : 0.4f;

                            NPC.velocity *= .975f;

                            animState = 1;
                            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.Y * .075f * NPC.spriteDirection, 0.1f);

                            int dust = Dust.NewDust(Systems.Utils.findSurfaceAbove(NPC.Center + new Vector2(Main.rand.Next(-40, 41), 0)), 0, 0, 51, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity.Y -= Main.rand.NextFloat(1, 4);
                        }
                        else
                        {
                            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 2 }, NPC.Center);

                            for (int i = -5; i < 5; i++)
                            {
                                int num5 = Main.rand.Next(Main.projFrames[962] * 4);
                                num5 = 6 + Main.rand.Next(6);

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), Systems.Utils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(10, 21) * i, 0)),
                                    new Vector2(NPC.velocity.X * .5f + Main.rand.NextFloat(-2, 3), NPC.velocity.Y * .5f + Main.rand.NextFloat(-2, 3) - 2), ProjectileID.DeerclopsRangedProjectile, 75 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, num5);
                                }
                            }

                            hasJumped = true;
                            jumping = true;
                            Systems.ScreenUtils.screenShaking += 2;

                            for (int i = 0; i < 20; i++)
                            {
                                int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-72, 72), 0), 0, 0, 51, Scale: 1.5f);
                                Main.dust[dust].noGravity = false;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.Pi);
                            }

                            NPC.netUpdate = true;
                        }
                    }

                    if (jumping && !collision)
                    {
                        animState = 2;

                        NPC.velocity.Y += .2f;
                        NPC.velocity *= .99f;
                        NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.Y * .075f * NPC.spriteDirection, 0.1f);
                    }

                    if (jumping && collision)
                    {
                        jumping = false;
                        animState = 0;
                        Systems.ScreenUtils.screenShaking += 3;
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<HMShockwave>(), 60 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI);
                        }

                        NPC.netUpdate = true;
                    }

                    if (hasJumped && !jumping)
                    {
                        TunnelAbout(standardTarget, ref collision, 6, 4, 500, 250);
                        AITimer++;

                        if (AITimer > 120) ResetValues();
                    }

                    break;

                case AttackPattern.SpikeSkim:

                    if (AITimer == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 2, Pitch = -.2f }, NPC.Center);
                        moveToZone = Systems.Utils.findGroundUnder(target.Top + new Vector2(500 * (target.Center.X > NPC.Center.X ? 1 : -1), -50));
                        AITimer++;

                        int numDusts = 30;

                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(moveToZone, 0, 0, 92, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Vector2 velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        NPC.netUpdate = true;
                    }

                    TunnelAbout(jumping ? Systems.Utils.findGroundUnder(target.Top) : moveToZone, ref collision, jumping ? 12 : 8, 4, jumping ? 500 : 10, jumping ? 32 : 50);

                    if (NPC.Distance(Systems.Utils.findSurfaceAbove(NPC.Center)) < (jumping ? 32 : 50) || !collision) NPC.velocity.Y += .3f;

                    if (AITimer > 20 && !jumping)
                    {
                        NPC.velocity.X = 12 * (target.Center.X > NPC.Center.X ? 1 : -1);
                        jumping = true;
                        animState = 1;

                        SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Volume = 2, Pitch = -.2f }, NPC.Center);

                        NPC.netUpdate = true;
                    }

                    if (AITimer % 5 == 1 && collision && jumping)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), Systems.Utils.findSurfaceAbove(NPC.Center), new Vector2(NPC.velocity.X * .1f, -1).RotatedByRandom(MathHelper.ToRadians(15)), ProjectileID.DeerclopsIceSpike, 75 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, .5f + (AITimer * .005f));
                        }

                        Systems.ScreenUtils.screenShaking += 1;
                        NPC.netUpdate = true;
                    }

                    if (AITimer > 100) ResetValues();

                    if (Math.Abs(NPC.Center.X - moveToZone.X) < 50 || jumping) AITimer++;

                    break;


                case AttackPattern.DebrisShower:

                    if (AITimer == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 2, Pitch = -.2f }, NPC.Center);
                        moveToZone = Systems.Utils.findGroundUnder(target.Top + new Vector2(Main.rand.NextFloat(450, 601) * (Main.rand.NextBool() ? 1 : -1), 0));
                        AITimer++;

                        NPC.netUpdate = true;
                    }

                    if (NPC.Distance(moveToZone + new Vector2(0, 300)) < 40 && AITimer < 35) AITimer++;

                    if (AITimer <= 35) 
                    {
                        TunnelAbout(Systems.Utils.findGroundUnder(moveToZone), ref collision, 5, 5, 20, 300);

                        if (AITimer == 35)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_DrakinHurt with { Volume = 2, Pitch = -.2f }, NPC.Center);

                            int numDusts = 30;

                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(moveToZone + new Vector2(0, 300), 0, 0, 92, Scale: 3f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Vector2 velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            }

                            AITimer++;
                            NPC.netUpdate = true;
                        }
                    }

                    else if (!hasJumped)
                    {                       
                        if (collision)
                        {
                            TunnelAbout(standardTarget, ref collision, 3, 17, 20, -300);

                            animState = 1;

                            int dust = Dust.NewDust(Systems.Utils.findSurfaceAbove(NPC.Center + new Vector2(Main.rand.Next(-40, 41), 0)), 0, 0, 51, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity.Y -= Main.rand.NextFloat(1, 4);
                        }
                        else
                        {
                            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 2 }, NPC.Center);

                            for (int i = -9; i < 10; i++)
                            {
                                int num5 = Main.rand.Next(Main.projFrames[962] * 4);
                                num5 = 6 + Main.rand.Next(6);

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), Systems.Utils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(10, 15) * i, 0)),
                                    new Vector2(5 * (target.Center.X > NPC.Center.X ? 1 : -1), -Main.rand.NextFloat(5, 11)), ProjectileID.DeerclopsRangedProjectile, 75 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, num5);
                                }
                            }

                            hasJumped = true;
                            jumping = true;
                            Systems.ScreenUtils.screenShaking += 2;

                            for (int i = 0; i < 20; i++)
                            {
                                int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-72, 72), 0), 0, 0, 51, Scale: 1.5f);
                                Main.dust[dust].noGravity = false;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.Pi);
                            }

                            NPC.netUpdate = true;
                        }
                    }

                    if (jumping && !collision)
                    {
                        animState = 2;

                        NPC.velocity.Y += .2f;
                        NPC.velocity *= .99f;
                        NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.Y * .075f * NPC.spriteDirection, 0.1f);
                    }

                    if (jumping && collision)
                    {
                        jumping = false;
                        animState = 0;
                        Systems.ScreenUtils.screenShaking += 3;
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<HMShockwave>(), 60 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI);
                        }

                        NPC.netUpdate = true;
                    }

                    if (hasJumped && !jumping)
                    {
                        TunnelAbout(standardTarget, ref collision, 6, 4, 500, 250);
                        AITimer++;

                        if (AITimer > 90) ResetValues();
                    }

                    break;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2((NPC.frame.Width * .5f) + (NPC.frame.Width * .35f * NPC.spriteDirection), NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects;

            NPC.spriteDirection = NPC.direction = NPC.velocity.X > 0 ? 1 : -1;

            blurAlpha = MathHelper.SmoothStep(blurAlpha, animState == 1 || animState == 2 ? 1 : 0, .1f);

            if (NPC.spriteDirection > 0)
            {
                effects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                effects = SpriteEffects.None;
            }

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, (Color.White * .25f) * blurAlpha * ((1 - i / (float)NPC.oldPos.Length) * 0.95f), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }
}
