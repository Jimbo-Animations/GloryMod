using GloryMod.Systems;
using GloryMod.Systems.BossBars;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;

namespace GloryMod.NPCs.IceFish
{
    [AutoloadBossHead]
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

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("A massive, apex predator that thrives within the inhospitable chill of the blizzards. Natives to the tundra know to flee at the feeling of vibrations deep beneath the snow.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 54;
            NPC.height = 54;

            NPC.damage = 0;
            NPC.defense = Main.hardMode ? 20 : 10;
            NPC.lifeMax = Main.getGoodWorld ? (Main.hardMode ? 14400 : 2400) : (Main.hardMode ? 12000 : 2000);
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            NPC.npcSlots = 50f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.DD2_WitherBeastCrystalImpact;
            NPC.DeathSound = null;
            NPC.coldDamage = true;

            Music = MusicID.OtherworldlyUGHallow;
            NPC.BossBar = GetInstance<HMBossBarr>();
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return NPC.HasValidTarget;
        }

        public override void PostAI()
        {
            bool collision = CheckTileCollision();
            bool cheese = platformCheese();

            if (NPC.soundDelay == 0 && collision && NPC.ai[0] != 7)
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

            if (cheese) enrageTimer++;
            else enrageTimer--; 

            MathHelper.Clamp(enrageTimer, 0, 150);

            if (NPC.ai[0] != 0 && (enrageTimer >= 150 || !NPC.HasValidTarget || !target.ZoneSnow))
            {
                if (NPC.timeLeft > 10)
                {
                    NPC.timeLeft = 10;
                }

                NPC.velocity.Y += 0.1f;
            }

            damageScale = Main.hardMode ? 2 : 1;
            NPC.damage = jumping ? 105 * damageScale : 0;
            NPC.HitSound = NPC.ai[0] == 6 ? SoundID.DD2_DrakinHurt : SoundID.DD2_WitherBeastCrystalImpact;
            rampUp = MathHelper.Clamp(NPC.GetLifePercent() + .2f, .4f, 1f);

            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = NPC.ai[0] == 6 ? 1 : .1f;
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
                case AttackPattern.StartBattle:

                    TimerRand = Main.rand.NextFloat(300, 361);
                    InitializeAIStates();
                    NPC.ai[0]++;

                    NPC.netUpdate = true;

                    break;

                case AttackPattern.Idle:

                    AITimer++;

                    TunnelAbout(standardTarget, ref collision, true);

                    if (AggravationCount > 0)
                    {                      
                        AITimer = 0;                      

                        if (collision)
                        {
                            NPC.ai[0] = 5;
                            AggravationCount = 0;
                            NPC.netUpdate = true;
                        }
                    }

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

                    if (AITimer < 75) TunnelAbout(Systems.Utils.findGroundUnder(moveToZone), ref collision, true, 8, 7, 10, 400);

                    if (AITimer >= 75 && !hasJumped)
                    {
                        if (AITimer == 75)
                        {
                            moveToZone = target.Top;
                            NPC.netUpdate = true;
                        }

                        if (collision || Math.Abs(NPC.Center.Y - target.Center.Y) > 350)
                        {
                            float jumpSpeed = Main.hardMode ? .45f : .4f;

                            NPC.velocity += NPC.DirectionTo(moveToZone) * jumpSpeed;

                            if (Math.Abs(NPC.velocity.X) > (Main.hardMode ? 13 : 12)) NPC.velocity.X += NPC.velocity.X > 0 ? -jumpSpeed : jumpSpeed;
                            if (Math.Abs(NPC.velocity.Y) > (Main.hardMode ? 13 : 12)) NPC.velocity.Y += NPC.velocity.Y > 0 ? -jumpSpeed : jumpSpeed;

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
                                    var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), Systems.Utils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(10, 21) * i, 0)),
                                    new Vector2(NPC.velocity.X * .5f + Main.rand.NextFloat(-2, 3), NPC.velocity.Y * .65f + Main.rand.NextFloat(-4, 1)), ProjectileID.DeerclopsRangedProjectile, (90 * damageScale) / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, num5);
                                    Main.projectile[proj].scale = Main.hardMode ? 1.25f : 1;
                                }
                            }

                            hasJumped = true;
                            jumping = true;
                            ScreenUtils.screenShaking += 2;

                            for (int i = 0; i < 20; i++)
                            {
                                int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-72, 73), 0), 0, 0, 51, Scale: 1.5f);
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
                        ScreenUtils.screenShaking += 3;
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<HMShockwave>(), (75 * damageScale) / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI);
                        }

                        NPC.netUpdate = true;
                    }

                    if (hasJumped && !jumping)
                    {
                        TunnelAbout(standardTarget, ref collision, true, 6, 4, 500, 250);
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
                            Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        NPC.netUpdate = true;
                    }                 

                    TunnelAbout(jumping ? standardTarget : moveToZone, ref collision, true, jumping ? 12 : 8, 4, jumping ? 500 : 10, jumping ? 32 : 50);

                    if (AITimer > 20 && !jumping)
                    {
                        NPC.velocity.X = 12 * (target.Center.X > NPC.Center.X ? 1 : -1);
                        jumping = true;
                        animState = 1;

                        SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Volume = 2, Pitch = -.2f }, NPC.Center);

                        NPC.netUpdate = true;
                    }

                    if (AITimer % 5 == 1 && (collision || cheese) && jumping)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), cheese ? Systems.Utils.findGroundUnder(new Vector2(NPC.Center.X, target.Top.Y)) : Systems.Utils.findSurfaceAbove(NPC.Center), new Vector2(NPC.velocity.X * .1f, -1).RotatedByRandom(MathHelper.ToRadians(15)), ProjectileID.DeerclopsIceSpike, (90 * damageScale) / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, (.6f * damageScale) + (AITimer * .005f));
                        }

                        ScreenUtils.screenShaking += 1;
                        animState = 1;
                        NPC.netUpdate = true;
                    }

                    if (Math.Abs(NPC.Center.X - moveToZone.X) < 50 || jumping) AITimer++;

                    if (AITimer > (Main.hardMode ? 120 : 100)) ResetValues();

                    break;


                case AttackPattern.DebrisShower:

                    if (AITimer == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 2, Pitch = -.2f }, NPC.Center);
                        moveToZone = Systems.Utils.findGroundUnder(target.Top + new Vector2(Main.rand.NextFloat(400, 501) * (Main.rand.NextBool() ? 1 : -1), 0));
                        AITimer++;

                        NPC.netUpdate = true;
                    }

                    if (Math.Abs(moveToZone.X - NPC.Center.X) <= 30 && AITimer < 35) AITimer++;

                    if (AITimer <= 35)
                    {
                        TunnelAbout(Systems.Utils.findGroundUnder(moveToZone), ref collision, true, 5, 5, 20, 300);

                        if (AITimer == 35)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_DrakinHurt with { Volume = 2, Pitch = -.2f }, NPC.Center);

                            int numDusts = 30;

                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(moveToZone + new Vector2(0, 300), 0, 0, 92, Scale: 3f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            }

                            AITimer++;
                            NPC.netUpdate = true;
                        }
                    }

                    else if (!hasJumped)
                    {
                        if (collision || NPC.Center.Y + 350 < target.Center.Y)
                        {
                            collision = true;

                            TunnelAbout(standardTarget, ref collision, false, Main.hardMode ? 4 : 3, Main.hardMode ? 18 : 17, 20, -300);

                            animState = 1;

                            int dust = Dust.NewDust(Systems.Utils.findSurfaceAbove(NPC.Center + new Vector2(Main.rand.Next(-40, 41), 0)), 0, 0, 51, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity.Y -= Main.rand.NextFloat(1, 4);
                        }
                        else
                        {
                            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 2 }, NPC.Center);

                            for (int i = -(Main.hardMode ? 9 : 6); i < 10; i++)
                            {
                                int num5 = Main.rand.Next(Main.projFrames[962] * 4);
                                num5 = 6 + Main.rand.Next(6);

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), Systems.Utils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(-40, 41), 0)),
                                    new Vector2(Main.rand.NextFloat(3, 5) * (target.Center.X > NPC.Center.X ? 1 : -1), - 10 - (i / 2)), ProjectileID.DeerclopsRangedProjectile, (90 * damageScale) / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, num5);
                                    Main.projectile[proj].scale = Main.hardMode ? 1.25f : 1;
                                }
                            }

                            minion = Main.npc[NPC.NewNPC(NPC.InheritSource(NPC), (int)Systems.Utils.findGroundUnder(NPC.Top).X,
                            (int)Systems.Utils.findGroundUnder(NPC.Top).Y, NPCType<BorealCharge>())];

                            minion.ai[0] = NPC.whoAmI;
                            minion.velocity = new Vector2(Main.rand.NextFloat(3, 5) * (target.Center.X > NPC.Center.X ? 1 : -1), -20);
                            minion.ai[1] = 300 - ((int)(300 * rampUp));

                            hasJumped = true;
                            jumping = true;
                            ScreenUtils.screenShaking += 2;

                            for (int i = 0; i < 20; i++)
                            {
                                int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-72, 73), 0), 0, 0, 51, Scale: 1.5f);
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
                        ScreenUtils.screenShaking += 3;
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<HMShockwave>(), 75 * damageScale / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI);
                        }

                        NPC.netUpdate = true;
                    }

                    if (hasJumped && !jumping)
                    {
                        TunnelAbout(standardTarget, ref collision, true, 6, 4, 500, 250);
                        AITimer++;

                        if (AITimer > 90) ResetValues();
                    }

                    break;

                case AttackPattern.SuperJump:

                    if (AITimer == 0)
                    {
                        int numDusts = 30;

                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 92, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        SoundEngine.PlaySound(SoundID.DD2_DrakinBreathIn with { Volume = 3, Pitch = -.2f }, NPC.Center);
                    }

                    if (AITimer <= 180) AITimer++;

                    if (AITimer < 180) TunnelAbout(standardTarget, ref collision, true, 10, 10, Main.hardMode ? 40 : 20, 500);

                    if (AITimer == 120) SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 3, Pitch = -.2f }, NPC.Center);
                    if (!hasJumped && ScreenUtils.screenShaking < 1) ScreenUtils.screenShaking = 1;

                    if (AITimer >= 180 && !hasJumped)
                    {
                        if (AITimer == 180)
                        {
                            animState = 1;
                            NPC.velocity.Y = -10;
                            NPC.netUpdate = true;
                        }

                        if (collision || Math.Abs(NPC.Center.Y - target.Center.Y) > 350)
                        {
                            NPC.velocity += NPC.DirectionTo(standardTarget) * .5f;

                            if (Math.Abs(NPC.velocity.X) > (Main.hardMode ? 12 : 10)) NPC.velocity.X += NPC.velocity.X > 0 ? -0.5f : 0.5f;
                            if (Math.Abs(NPC.velocity.Y) > (Main.hardMode ? 20 : 15)) NPC.velocity.Y += NPC.velocity.Y > 0 ? -0.5f : 0.5f;

                            NPC.velocity.X *= .98f;

                            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.Y * .075f * NPC.spriteDirection, 0.1f);

                            int dust = Dust.NewDust(Systems.Utils.findSurfaceAbove(NPC.Center + new Vector2(Main.rand.Next(-50, 51), 0)), 0, 0, 51, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity.Y -= Main.rand.NextFloat(1, 4);
                        }
                        else
                        {
                            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 2 }, NPC.Center);

                            for (int i = -(Main.hardMode ? 10 : 6); i < (Main.hardMode ? 10 : 6); i++)
                            {
                                int num5 = Main.rand.Next(Main.projFrames[962] * 4);
                                num5 = 6 + Main.rand.Next(6);

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), Systems.Utils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(10, 21) * i, 0)),
                                    new Vector2(NPC.velocity.X * .5f + Main.rand.NextFloat(-2, 3), NPC.velocity.Y * .65f + Main.rand.NextFloat(-4, 1)), ProjectileID.DeerclopsRangedProjectile, 90 * damageScale / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI, 0, num5);
                                    Main.projectile[proj].scale = Main.hardMode ? 1.25f : 1;
                                }
                            }

                            hasJumped = true;
                            jumping = true;
                            ScreenUtils.screenShaking += 5;

                            for (int i = 0; i < 40; i++)
                            {
                                int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-145, 146), 0), 0, 0, 51, Scale: 1.5f);
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

                        AITimer = 0;
                        animState = 3;
                        NPC.ai[0] = shouldDie ? 7 : 6;

                        ScreenUtils.screenShaking += 5;
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DD2_DrakinHurt with { Volume = 2, Pitch = -.2f }, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<HMShockwave>(), 105 * damageScale / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 1, target.whoAmI);
                        }

                        NPC.netUpdate = true;
                    }

                    break;


                case AttackPattern.Stun:

                    AITimer++;
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.Y * .075f * NPC.spriteDirection, 0.1f);

                    if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(NPC.velocity.X, NPC.velocity.Y))
                    {
                        SoundEngine.PlaySound(SoundID.DeerclopsStep, NPC.Center);

                        NPC.position.Y += NPC.velocity.Y;
                        NPC.velocity = new Vector2(Main.rand.NextFloat(-1.1f, 1.2f), -Main.rand.NextFloat(2, 3.1f));
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        NPC.velocity.Y += .2f;
                        NPC.velocity *= .99f;
                    }

                    if (AITimer > 450 || shouldDie)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 3, Pitch = -.2f }, NPC.Center);
                        ResetValues();
                    }

                    break;

                case AttackPattern.DeathAnim:

                    AITimer++;
                    NPC.rotation = AITimer * (NPC.spriteDirection * 0.025f);
                    NPC.velocity.Y += .3f;

                    if (AITimer == 1)
                    {
                        NPC.NPCLoot();
                        SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 4, Pitch = -.4f }, NPC.Center);
                        NPC.spriteDirection = target.Center.X > NPC.Center.X ? 1 : -1;                           
                        NPC.velocity.Y = -14;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileID.DD2ExplosiveTrapT3Explosion, 0, 0, target.whoAmI);
                        }

                        int numDusts = 40;

                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(moveToZone, 0, 0, 92, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        NPC.netUpdate = true;
                    }

                    if (AITimer >= 200)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 2f, Pitch = -.1f }, NPC.Center);
                        
                        ScreenUtils.screenShaking = 8f;
                        NPC.active = false;
                        NPC.netUpdate = true;
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

            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.ai[0] != 6 && NPC.ai[0] != 7) NPC.spriteDirection = NPC.direction = NPC.velocity.X > 0 ? 1 : -1;

                blurAlpha = MathHelper.SmoothStep(blurAlpha, animState == 1 || animState == 2 ? 1 : 0, .15f);

                if (NPC.spriteDirection > 0) effects = SpriteEffects.FlipHorizontally;
                else effects = SpriteEffects.None;

                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, (Color.White * .25f) * blurAlpha * ((1 - i / (float)NPC.oldPos.Length) * 0.95f), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }

                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, drawPos + new Vector2(4, 0), NPC.frame, drawColor, NPC.rotation, new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .5f), .75f, SpriteEffects.None, 0f);
            }
            

            return false;
        }
    }
}
