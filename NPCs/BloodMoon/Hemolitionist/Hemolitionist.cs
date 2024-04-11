using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using GloryMod.Systems;
using System.Collections.Generic;
using GloryMod.Systems.BossBars;
using GloryMod.Items.BloodMoon.Hemolitionist;

namespace GloryMod.NPCs.BloodMoon.Hemolitionist
{
    [AutoloadBossHead]
    internal class Hemolitionist : ModNPC
    { 
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                new FlavorTextBestiaryInfoElement("Built as a weapon of war, the Hemolitionist is the strongest and fiercest of the cybernetic beasts." +
                "It is powered by a dark essence found in the blood of the living, which coincidentally manifests in large quantities during a Blood Moon.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 72;
            NPC.height = 72;
            if (Main.getGoodWorld == true) NPC.scale *= 1.2f;
            DrawOffsetY = 4f;

            NPC.damage = 0;
            NPC.defense = 30;
            NPC.lifeMax = Main.getGoodWorld ? 40000 : 30000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 50, 0, 0);
            NPC.npcSlots = 100f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = new SoundStyle("GloryMod/Music/HemoHit", 4, SoundType.Sound) with { Volume = 0.33f };
            NPC.DeathSound = SoundID.NPCDeath14;

            NPC.BossBar = GetInstance<HemolitionistBossBar>();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Bleeding, 300, true);
        }

        private int ThrusterRotation = 1;

        public override void FindFrame(int frameHeight)
        {
            if (ScreamTimer > 0)
            {
                ScreamTimer--;
            }

            if (ThrusterRotation == 1)
            {
                if (ScreamTimer > 0)
                {
                    NPC.frame.Y = 240;
                }
                else
                {
                    NPC.frame.Y = 0;
                }                            
            }
            if (ThrusterRotation == 2)
            {
                if (ScreamTimer > 0)
                {
                    NPC.frame.Y = 320;
                }
                else
                {
                    NPC.frame.Y = 80;
                }                
            }
            if (ThrusterRotation == 3)
            {
                if (ScreamTimer > 0)
                {
                    NPC.frame.Y = 400;
                }
                else
                {
                    NPC.frame.Y = 160;
                }               
            }
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }
    
        private float ScreamTimer = 0;
        private float BodyAlpha = 0;
        private float speed = 1;
        private float AuraAlpha = 0;
        private float telegraphAlpha = 1;
        private float twitchOut = 0;
        private bool startMusic = false;
        private bool isCharging = false;
        private bool useRayTelegraph = false;
        private bool useAudioQueue = true;
        private bool Phase2 = false;
        private bool Phase3 = false;
        private bool startDeathScene = false;
        NPC target;

        private enum AttackPattern : byte
        {
            IntroCutscene = 0,
            DashnSpray = 1,
            BulletShower = 2,
            BloodRay = 3,
            PulseStopper = 4,
            SuperAAMoment = 5,
            DeathAnimation = 6
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            int goalDirection = player.Center.X < NPC.Center.X ? -1 : 1;
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            if (isCharging == true && AuraAlpha < 1)
            {
                AuraAlpha += 0.05f;
            }
            else if (isCharging == false && AuraAlpha > 0)
            {
                AuraAlpha -= 0.05f;
            }

            if (NPC.life <= NPC.lifeMax * 2 / 3)
            {
                Phase2 = true;

                if (NPC.life <= NPC.lifeMax / 3)
                {
                    Phase3 = true;
                }
            }

            if (!player.active || player.dead || !Main.bloodMoon)
            {
                if (NPC.ai[0] != 0)
                {
                    NPC.active = false;
                    SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, NPC.Center);                 
                    int numDusts = 30;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(15, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    }
                }           
            }

            if (startMusic == true)
            {
                Main.musicFade[Main.curMusic] = MathHelper.Lerp(0, 1, 1);
                Music = MusicLoader.GetMusicSlot(Mod, "Music/Anemia_Full_Loop2");
            }
            else
            {
                if (NPC.AnyNPCs(NPCType<RedtideWarrior>()))
                {
                    Music = MusicLoader.GetMusicSlot(Mod, "Music/Redtide_intro");
                }
                else
                {
                    Main.musicFade[Main.curMusic] = MathHelper.Lerp(1, 0, 1);
                    Music = MusicLoader.GetMusicSlot(Mod, "Music/Silence");
                }                
            }

            switch (AIstate)
            {
                case AttackPattern.IntroCutscene:

                    if (NPC.ai[1] == 0)
                    {
                        NPC.alpha = 255;
                        NPC.dontTakeDamage = true;
                    }

                    if (GlorySystem.DownedRedTideWarrior == false)
                    {
                        NPC.hide = true;

                        if (!NPC.AnyNPCs(NPCType<RedtideWarrior>()) && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<RedtideWarrior>());
                            target = Main.npc[NPC.FindFirstNPC(NPCType<RedtideWarrior>())];
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            NPC.ai[2]++;

                            if (NPC.ai[2] == 1000)
                            {
                                NPC.position = target.Center - NPC.Size / 2;
                                NPC.hide = false;
                                GlorySystem.DownedRedTideWarrior = true;
                            }
                        }
                    }
                    else
                    {
                        if (NPC.ai[1] == 0)
                        {
                            ScreenUtils.ChangeCameraPos(NPC.Center, 200, 1.25f);
                        }
                        NPC.ai[1]++;
                    }

                    if (NPC.ai[1] >= 51)
                    {
                        NPC.alpha -= 5;
                    }

                    if (NPC.ai[1] == 150)
                    {
                        BodyAlpha = 1;
                        NPC.damage = 200;
                        NPC.velocity += new Vector2(0, -2f);

                        SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SanguineBlast>(), 200, 8f, player.whoAmI, 0f, 0f);
                        }
                        NPC.netUpdate = true;

                        if (NPC.AnyNPCs(NPCType<RedtideWarrior>()))
                        {
                            target.ai[1] = 2;
                        }

                        int numDusts = 45;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(25, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }
                    }
                    else if (NPC.ai[1] < 150 && NPC.ai[2] > 1050)
                    {
                        NPC.velocity = new Vector2(0, -0.1f);

                        if (Main.rand.NextBool(2))
                        {
                            int numDusts = Main.rand.Next(1, 10) * (int)(NPC.scale);
                            for (int i = 0; i < numDusts; i++)
                            {
                                Vector2 dustDirection = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);

                                int dust = Dust.NewDust(NPC.Center + dustDirection * Main.rand.Next(150, 200) * NPC.scale, 0, 0, 266, Scale: 2f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = dustDirection * Main.rand.Next(-10, -5) * NPC.scale;
                            }
                        }
                    }

                    if (NPC.ai[1] == 200)
                    {
                        ScreamTimer = 100;
                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistRoar") with { Volume = 2 }, NPC.Center);
                        startMusic = true;
                        ThrusterRotation = 2;
                        ScreenUtils.ChangeCameraPos(NPC.Center, 100, 2f);
                        NPC.ai[3] = 11;
                    }
                    if (NPC.ai[1] <= 310 && NPC.ai[1] >= 200)
                    {
                        ScreenUtils.screenShaking = 10f;
                        NPC.velocity *= 0.95f;

                        if (NPC.ai[3] >= 11)
                        {
                            rings.Add(new Tuple<Vector2, float, float>(NPC.Center, 0f, 200f));
                            NPC.ai[3] = 0;
                        }
                        NPC.ai[3]++;

                        int dust = Dust.NewDust(NPC.Center + new Vector2(0, 24).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 1f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
                    }
                    if (NPC.ai[1] == 310)
                    {
                        ThrusterRotation = 3;
                    }
                    if (NPC.ai[1] == 330)
                    {
                        NPC.ai[3] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 1;
                        NPC.dontTakeDamage = false;
                    }

                    NPC.velocity *= 0.99f;

                    break;

                case AttackPattern.DashnSpray:

                    NPC.ai[1]++;

                    if (NPC.ai[1] <= 100)
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2, 0.075f);

                        if (NPC.Distance(player.Center + new Vector2(-goalDirection * 500, 0)) > 50)
                        {
                            float speed = 0.8f;
                            NPC.velocity *= 0.95f;
                            NPC.velocity += NPC.DirectionTo(player.Center + new Vector2(-goalDirection * 500, 0)) * speed;
                            ThrusterRotation = 3;
                        }
                        else
                        {
                            NPC.velocity *= 0.95f;
                        }
                    }
                    if (NPC.ai[1] == 100 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.velocity += new Vector2(0, -35).RotatedBy(NPC.rotation);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SanguineBlast>(), 100, 8f, player.whoAmI, 0f, 0f);
                        SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                        ThrusterRotation = 1;
                        NPC.netUpdate = true;
                    }
                    if (NPC.ai[1] > 100)
                    {
                        NPC.velocity *= 0.98f;
                        if (NPC.ai[1] > 120)
                        {
                            NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(player.Center).ToRotation() - MathHelper.PiOver2, 0.075f);
                            ThrusterRotation = 2;

                            if (NPC.ai[1] >= 140)
                            {
                                NPC.ai[2]++;
                                isCharging = false;

                                if (NPC.ai[2] == 20 && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.ai[2] = 0;
                                    NPC.ai[3]++;
                                    SoundEngine.PlaySound(SoundID.Item171, NPC.position);
                                    NPC.velocity += new Vector2(0, -1f).RotatedBy(NPC.rotation);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-44, -30).RotatedBy(NPC.rotation), new Vector2(0, 8).RotatedBy(0.255f + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(44, -30).RotatedBy(NPC.rotation), new Vector2(0, 8).RotatedBy(-0.255f + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-48, 6).RotatedBy(NPC.rotation), new Vector2(0, 9).RotatedBy(0.13f + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(48, 6).RotatedBy(NPC.rotation), new Vector2(0, 9).RotatedBy(-0.13f + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-46, 32).RotatedBy(NPC.rotation), new Vector2(0, 10).RotatedBy(0.05f + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(46, 32).RotatedBy(NPC.rotation), new Vector2(0, 10).RotatedBy(-0.05f + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                    NPC.netUpdate = true;
                                }
                            }                           
                        }
                    }

                    if (Main.getGoodWorld == true)
                    {
                        if (NPC.ai[3] == 3)
                        {
                            changeAttacks(1);
                            NPC.ai[1] = 0;
                            NPC.ai[2] = 0;
                            NPC.ai[3] = 0;
                        }
                    }
                    else
                    {
                        if (Phase2 == false && Phase3 == false && NPC.ai[3] == 1)
                        {
                            changeAttacks(1);
                            NPC.ai[1] = 0;
                            NPC.ai[2] = 0;
                            NPC.ai[3] = 0;
                        }
                        if (Phase2 == true && Phase3 == false && NPC.ai[3] == 2)
                        {
                            changeAttacks(1);
                            NPC.ai[1] = 0;
                            NPC.ai[2] = 0;
                            NPC.ai[3] = 0;
                        }
                        if (Phase2 == true && Phase3 == true && NPC.ai[3] == 3)
                        {
                            changeAttacks(1);
                            NPC.ai[1] = 0;
                            NPC.ai[2] = 0;
                            NPC.ai[3] = 0;
                        }
                    }

                    break;

                case AttackPattern.BulletShower:

                    if (NPC.ai[1] < 10)
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.05f, 0.05f);
                        if (NPC.rotation < -1)
                        {
                            NPC.rotation = -1;
                        }
                        if (NPC.rotation > 1)
                        {
                            NPC.rotation = 1;
                        }

                        if (NPC.Distance(player.Center + new Vector2(0, -500)) > 50)
                        {
                            float speed = 1f;
                            NPC.velocity *= 0.95f;
                            NPC.velocity += NPC.DirectionTo(player.Center + new Vector2(0, -500)) * speed;
                            ThrusterRotation = 3;

                            if (NPC.Distance(player.Center) < 450)
                            {
                                NPC.velocity -= NPC.DirectionTo(player.Center) * speed / 2;
                                NPC.velocity *= 0.97f;
                            }
                        }
                        else
                        {
                            NPC.velocity *= 0.95f;
                            NPC.ai[1]++;
                        }
                    }
                    else
                    {
                        NPC.ai[2]++;
                        ScreamTimer = 2;
                        NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.05f, 0.05f);
                        if (NPC.rotation < -1)
                        {
                            NPC.rotation = -1;
                        }
                        if (NPC.rotation > 1)
                        {
                            NPC.rotation = 1;
                        }

                        if (NPC.Distance(player.Center + new Vector2(0, -500)) > 50)
                        {
                            float speed = 1f;
                            NPC.velocity *= 0.95f;
                            NPC.velocity += NPC.DirectionTo(player.Center + new Vector2(0, -500)) * speed;
                            ThrusterRotation = 3;

                            if (NPC.Distance(player.Center) < 450)
                            {
                                NPC.velocity -= NPC.DirectionTo(player.Center) * speed / 2;
                                NPC.velocity *= 0.97f;
                            }
                        }
                        else
                        {
                            NPC.velocity *= 0.95f;
                        }

                        if (NPC.ai[2] == 5 && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.ai[2] = -0;
                            NPC.ai[3]++;

                            SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.position);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 25).RotatedBy(NPC.rotation), new Vector2(Main.rand.NextFloat(10f, 12f), Main.rand.NextFloat(1f, 1.5f)), ProjectileType<NecroticBolt>(), 100, 3f, Main.myPlayer);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 25).RotatedBy(NPC.rotation), new Vector2(Main.rand.NextFloat(-10f, -12f), Main.rand.NextFloat(1f, 1.5f)), ProjectileType<NecroticBolt>(), 100, 3f, Main.myPlayer);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 25).RotatedBy(NPC.rotation), new Vector2(Main.rand.NextFloat(-9f, 9f), Main.rand.NextFloat(1f, 1.5f)), ProjectileType<NecroticBolt>(), 100, 3f, Main.myPlayer);

                            if (Main.getGoodWorld == true)
                            {
                                if (NPC.ai[3] >= 20)
                                {
                                    changeAttacks(2);
                                    NPC.ai[1] = 0;
                                    NPC.ai[2] = 0;
                                    NPC.ai[3] = 0;
                                }
                            }
                            else
                            {
                                if (Phase2 == false && Phase3 == false && NPC.ai[3] == 12)
                                {
                                    changeAttacks(2);
                                    NPC.ai[1] = 0;
                                    NPC.ai[2] = 0;
                                    NPC.ai[3] = 0;
                                }
                                if (Phase2 == true && Phase3 == false && NPC.ai[3] == 16)
                                {
                                    changeAttacks(2);
                                    NPC.ai[1] = 0;
                                    NPC.ai[2] = 0;
                                    NPC.ai[3] = 0;
                                }
                                if (Phase2 == true && Phase3 == true && NPC.ai[3] == 20)
                                {
                                    changeAttacks(2);
                                    NPC.ai[1] = 0;
                                    NPC.ai[2] = 0;
                                    NPC.ai[3] = 0;
                                }
                            }
                            NPC.netUpdate = true;
                        }
                    }

                    break;

                case AttackPattern.BloodRay:

                    if (NPC.ai[1] <= 60)
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(player.Center).ToRotation() - MathHelper.PiOver2, 0.075f);

                        if (NPC.ai[1] == 60)
                        {
                            useRayTelegraph = true;
                            SoundEngine.PlaySound(SoundID.Item33, NPC.position);
                            NPC.ai[1]++;
                        }

                        if (NPC.Distance(player.Center) > 450)
                        {
                            float speed = 0.5f;
                            NPC.velocity *= 0.98f;

                            NPC.velocity += NPC.DirectionTo(player.Center) * speed;
                            ThrusterRotation = 2;
                        }
                        else
                        {
                            NPC.velocity *= 0.95f;

                            if (NPC.ai[1] == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item170, NPC.position);
                            }

                            NPC.ai[1]++;
                        }
                    }
                    else
                    {
                        NPC.velocity *= 0.95f;
                        telegraphAlpha -= 0.025f;
                        NPC.ai[1]++;

                        if (NPC.ai[1] >= 100)
                        {
                            if (NPC.ai[1] == 100)
                            {
                                useRayTelegraph = false;
                                ThrusterRotation = 1;
                                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistDeathray") with { Volume = 1.5f }, NPC.Center);
                            }

                            NPC.ai[2]++;

                            if (NPC.ai[2] <= 100 && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                ScreenUtils.screenShaking = 2f;
                                ScreamTimer = 2;
                                Vector2 projRotation = new Vector2(0, 1).RotatedBy(NPC.rotation);
                                int proj = Projectile.NewProjectile(NPC.GetSource_ReleaseEntity(), NPC.Center, projRotation, ProjectileType<NecroticRay>(), 150, 0f);
                                Main.projectile[proj].ai[0] = NPC.whoAmI;
                                if (NPC.ai[2] <= 5)
                                {
                                    Main.projectile[proj].ai[1] = 1;
                                }
                                else
                                {
                                    Main.projectile[proj].ai[1] = 0;

                                    if (NPC.ai[2] >= 95)
                                    {
                                        Main.projectile[proj].ai[1] = 1;
                                    }
                                }

                                float rotationSpeed = Phase3 == true ? 0.04f : 0.03f;
                                if (Main.getGoodWorld == true)
                                {
                                    rotationSpeed = 0.04f;
                                }

                                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(player.Center).ToRotation() - MathHelper.PiOver2, rotationSpeed);
                                NPC.velocity += new Vector2(0, -0.25f).RotatedBy(NPC.rotation);
                            }
                            else
                            {
                                if (Main.getGoodWorld == true && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int numProj = 12;
                                    for (int i = 0; i < numProj; i++)
                                    {
                                        SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.position);
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 25).RotatedBy(NPC.rotation), new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / numProj + NPC.rotation), ProjectileType<NecroticBolt>(), 150, 3f, Main.myPlayer, 0, 1);
                                    }
                                }
                                else if (Phase2 == true && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int numProj = 10;
                                    for (int i = 0; i < numProj; i++)
                                    {
                                        SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.position);
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 25).RotatedBy(NPC.rotation), new Vector2(5, 0).RotatedBy(i * MathHelper.TwoPi / numProj + NPC.rotation), ProjectileType<NecroticBolt>(), 150, 3f, Main.myPlayer, 0, 1);
                                    }
                                }

                                changeAttacks(3);
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[3] = 0;
                                telegraphAlpha = 1;
                                NPC.netUpdate = true;
                            }
                        }
                    }

                    break;

                case AttackPattern.PulseStopper:

                    if (useAudioQueue == true)
                    {
                        ScreamTimer = 100;

                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistPulseIndicator"), NPC.Center);
                        useAudioQueue = false;
                    }

                    if (NPC.ai[1] <= 50)
                    {
                        if (NPC.ai[3] > 0)
                        {
                            NPC.ai[1]++;
                        }

                        NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2, 0.1f);
                        if (NPC.Distance(player.Center) > 400)
                        {
                            if (NPC.Distance(player.Center) > 1200)
                            {
                                float speed = 0.6f;
                                NPC.velocity *= 0.94f;
                            }
                            else
                            {
                                float speed = 0.4f;
                                NPC.velocity *= 0.96f;
                            }

                            NPC.velocity += NPC.DirectionTo(player.Center) * speed;
                            ThrusterRotation = 2;
                        }
                        else
                        {
                            NPC.velocity *= 0.95f;
                            ThrusterRotation = 3;

                            if (NPC.ai[3] == 0)
                            {
                                NPC.ai[1]++;
                            }
                        }
                    }
                    else
                    {
                        if (Main.getGoodWorld == true)
                        {
                            if (NPC.ai[2] == 1)
                            {
                                ScreamTimer = 60;
                                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistEnergyScream") with { PitchVariance = 0.5f }, NPC.Center);
                                ScreenUtils.screenShaking = 5f;
                                isCharging = true;
                            }

                            if (NPC.ai[2] >= 20 && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[3]++;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SanguinePulse>(), 100, 3f, Main.myPlayer, 0, 1);
                                int numProj = 18;
                                Vector2 randomizer = new Vector2(0, 5).RotatedByRandom(MathHelper.TwoPi);
                                for (int i = 0; i < numProj; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, randomizer.RotatedBy(i * MathHelper.TwoPi / numProj), ProjectileType<NecroticBolt>(), 150, 3f, Main.myPlayer, 0, 1);
                                }
                                isCharging = false;
                                NPC.netUpdate = true;
                            }

                            NPC.ai[2]++;
                            NPC.velocity *= 0.95f;
                            ThrusterRotation = 3;

                            if (NPC.ai[3] >= 3)
                            {
                                changeAttacks(4);
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[3] = 0;
                                useAudioQueue = true;
                            }
                        }
                        else
                        {
                            if (Phase3 == true)
                            {
                                if (NPC.ai[2] == 1)
                                {
                                    ScreamTimer = 60;
                                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistEnergyScream") with { PitchVariance = 0.5f }, NPC.Center);
                                    ScreenUtils.screenShaking = 5f;
                                    isCharging = true;
                                }

                                if (NPC.ai[2] >= 20 && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.ai[1] = 0;
                                    NPC.ai[2] = 0;
                                    NPC.ai[3]++;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SanguinePulse>(), 100, 3f, Main.myPlayer, 0, 1);
                                    int numProj = 16;
                                    Vector2 randomizer = new Vector2(0, 5).RotatedByRandom(MathHelper.TwoPi);
                                    for (int i = 0; i < numProj; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, randomizer.RotatedBy(i * MathHelper.TwoPi / numProj), ProjectileType<NecroticBolt>(), 150, 3f, Main.myPlayer, 0, 1);
                                    }
                                    isCharging = false;
                                    NPC.netUpdate = true;
                                }
                            }
                            else
                            {
                                if (NPC.ai[2] == 10)
                                {
                                    ScreamTimer = 60;
                                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistEnergyScream") with { PitchVariance = 0.5f }, NPC.Center);
                                    ScreenUtils.screenShaking = 5f;
                                    isCharging = true;
                                }

                                if (NPC.ai[2] >= 30 && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.ai[1] = 0;
                                    NPC.ai[2] = 0;
                                    NPC.ai[3]++;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SanguinePulse>(), 100, 3f, Main.myPlayer, 0, 1);

                                    if (Phase2 == true)
                                    {
                                        int numProj = 16;
                                        Vector2 randomizer = new Vector2(0, 5).RotatedByRandom(MathHelper.TwoPi);
                                        for (int i = 0; i < numProj; i++)
                                        {
                                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, randomizer.RotatedBy(i * MathHelper.TwoPi / numProj), ProjectileType<NecroticBolt>(), 150, 3f, Main.myPlayer, 0, 1);
                                        }
                                    }

                                    isCharging = false;
                                    NPC.netUpdate = true;
                                }
                            }

                            NPC.ai[2]++;
                            NPC.velocity *= 0.95f;
                            ThrusterRotation = 3;

                            if (NPC.ai[3] >= 3)
                            {
                                changeAttacks(4);
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[3] = 0;
                                useAudioQueue = true;
                            }
                        }
                    }

                    break;

                case AttackPattern.SuperAAMoment:

                    NPC.velocity *= 0.95f;
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(player.Center).ToRotation() - MathHelper.PiOver2, 0.075f);
                    ThrusterRotation = 3;

                    float leniency = 20;

                    if (Phase3 == true)
                    {
                        leniency = 10;
                    }

                    if (Main.getGoodWorld == true)
                    {
                        leniency = 0;
                    }

                    if (NPC.ai[1] == 20)
                    {
                        NPC.velocity = Vector2.Zero;
                        NPC.rotation = 0;
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal with { Volume = 1.2f }, player.Center);
                        SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, NPC.Center);

                        int numDusts = 30;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(15, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        NPC.alpha = 255;
                        BodyAlpha = 0;
                        NPC.dontTakeDamage = true;
                        NPC.damage = 0;
                    }

                    NPC.ai[1]++;

                    if (NPC.ai[1] >= 40 && NPC.ai[1] < 60)
                    {
                        NPC.alpha -= 13;
                        if (NPC.ai[1] == 40)
                        {
                            NPC.position = player.Center - NPC.Size / 2 + new Vector2(Main.rand.NextFloat(-25, -10), 0).RotatedByRandom(MathHelper.TwoPi);
                        }
                        else
                        {
                            if (Main.rand.NextBool(2))
                            {
                                int numDusts = Main.rand.Next(1, 10) * (int)(NPC.scale);
                                for (int i = 0; i < numDusts; i++)
                                {
                                    Vector2 dustDirection = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);

                                    int dust = Dust.NewDust(NPC.Center + dustDirection * Main.rand.Next(150, 200) * NPC.scale, 0, 0, 266, Scale: 2f);
                                    Main.dust[dust].noGravity = true;
                                    Main.dust[dust].noLight = true;
                                    Main.dust[dust].velocity = dustDirection * Main.rand.Next(-10, -5) * NPC.scale;
                                }
                            }
                        }
                    }
                    else if (NPC.ai[1] >= 60 + leniency)
                    {
                        if (NPC.ai[1] == 60 + leniency && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            ScreenUtils.screenShaking = 5f;
                            BodyAlpha = 1;
                            NPC.alpha = 0;
                            NPC.damage = 180;
                            SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SanguineBlast>(), 50000, 8f, player.whoAmI, 0f, 0f);
                            NPC.dontTakeDamage = false;

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

                        if (NPC.ai[1] == 90 + leniency && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (Phase3 == true && NPC.ai[3] != 1)
                            {
                                NPC.velocity += new Vector2(0, -1f).RotatedBy(NPC.rotation);
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[3] = 1;
                                SoundEngine.PlaySound(SoundID.Item171, NPC.position);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-46, 32).RotatedBy(NPC.rotation), new Vector2(0, 11).RotatedBy(0 + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(46, 32).RotatedBy(NPC.rotation), new Vector2(0, 11).RotatedBy(0 + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                            }
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Item171, NPC.position);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-46, 32).RotatedBy(NPC.rotation), new Vector2(0, 11).RotatedBy(0 + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(46, 32).RotatedBy(NPC.rotation), new Vector2(0, 11).RotatedBy(0 + NPC.rotation), ProjectileType<BloodBullet>(), 100, 3f, Main.myPlayer);

                                changeAttacks(5);
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[3] = 0;
                            }
                            NPC.netUpdate = true;
                        }
                    }

                    break;

                case AttackPattern.DeathAnimation:

                    NPC.velocity *= 0.9f;
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.X * 0.05f, 0.075f);
                    ThrusterRotation = 2;
                    NPC.ai[1]++;

                    if (NPC.ai[1] == 20)
                    {
                        ScreenUtils.ChangeCameraPos(NPC.Center, 130, 1.5f);
                    }

                    if (NPC.ai[1] >= 20)
                    {
                        twitchOut += 0.0005f;
                        BodyAlpha -= 0.01f;
                        NPC.rotation += Main.rand.NextFloat(-twitchOut, twitchOut);
                    }

                    if (NPC.ai[1] == 50)
                    {
                        ScreamTimer = 100;
                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/HemolitionistRoarAlt"), NPC.Center);
                    }
                    if (NPC.ai[1] > 50)
                    {
                        ScreenUtils.screenShaking = 5f;
                        int dust = Dust.NewDust(NPC.Center + new Vector2(0, 24).RotatedBy(NPC.rotation), 0, 0, 5, Scale: 1f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedByRandom(MathHelper.TwoPi);
                    }

                    if (NPC.ai[1] > 150 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<SanguineBlast>(), 200, 8f, player.whoAmI, 0f, 0f);
                        NPC.SimpleStrikeNPC(10000, 0, true, 0, DamageClass.Default, true);
                    }
                    NPC.netUpdate = true;

                    break;
            }

            void changeAttacks(float oldAttack)
            {
                if (Phase2 == true)
                {
                    float Choice = Main.rand.Next(new int[] { 1, 2, 3, 4, 5 });
                    while (Choice == oldAttack)
                    {
                        Choice = Main.rand.Next(new int[] { 1, 2, 3, 4, 5 });
                    }
                    if (Choice != oldAttack)
                    {
                        NPC.ai[0] = Choice;
                    }
                }
                else
                {
                    float Choice = Main.rand.Next(new int[] { 1, 2, 3, 4 });
                    while (Choice == oldAttack)
                    {
                        Choice = Main.rand.Next(new int[] { 1, 2, 3, 4 });
                    }
                    if (Choice != oldAttack)
                    {
                        NPC.ai[0] = Choice;
                    }
                }
                NPC.netUpdate = true;
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && startDeathScene == false)
            {
                NPC.life = 1;
                NPC.ai[0] = 6;
                NPC.ai[1] = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                isCharging = false;
                AuraAlpha = 0;
                telegraphAlpha = 0;
                useRayTelegraph = false;
                NPC.dontTakeDamage = true;
                NPC.damage = 0;
                startDeathScene = true;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            var SpawnRule = Main.ItemDropsDB.GetRulesForNPCID(NPCID.BloodNautilus, false);
            foreach (var dropRule in SpawnRule)
            {
                npcLoot.Add(dropRule);
            }
            npcLoot.Add(ItemDropRule.Common(ItemID.ChumBucket, minimumDropped: 12, maximumDropped: 20));
            npcLoot.Add(ItemDropRule.Common(ItemType<HemotechThruster>()));
        }

        List<Tuple<Vector2, float, float>> rings = new List<Tuple<Vector2, float, float>>();
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D textureMask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f) + new Vector2(0, 5);
            Vector2 drawPos = NPC.Center - screenPos;
            Texture2D ringTexture = (Texture2D)Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_5");
            Player player = Main.player[NPC.target];
            Vector2 ringRotation = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);

            if (!NPC.IsABestiaryIconDummy)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, NPC.GetAlpha(new Color(255, 0, 0)) * AuraAlpha * ((1 - i / (float)NPC.oldPos.Length) * 0.95f), NPC.rotation, drawOrigin, NPC.scale * 1.25f, SpriteEffects.None, 0);
                }

                if (useRayTelegraph == true)
                {
                    Color color = new Color(255, 0, 0) * telegraphAlpha;
                    Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(0, 224).RotatedBy(NPC.rotation), NPC.Center + new Vector2(0, 24).RotatedBy(NPC.rotation), color);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor * BodyAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(textureMask, drawPos, NPC.frame, NPC.GetAlpha(new Color(255, 255, 255)), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                for (int i = 0; i < rings.Count; i++)
                {
                    if (i >= rings.Count)
                    {
                        break;
                    }

                    Main.EntitySpriteDraw(ringTexture, rings[i].Item1 - Main.screenPosition, null, new Color(255, 15, 30, 50), ringRotation.ToRotation(), ringTexture.Size() / 2, rings[i].Item2 / ringTexture.Width, SpriteEffects.None, 0);

                    rings[i] = new Tuple<Vector2, float, float>(rings[i].Item1, rings[i].Item2 + rings[i].Item3, rings[i].Item3);
                    if (rings[i].Item2 >= player.Distance(rings[i].Item1) + Main.screenWidth * 3)
                    {
                        rings.RemoveAt(i);
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
            }
            else
            {
                spriteBatch.Draw(texture, drawPos, NPC.frame, new Color(255, 255, 255), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(textureMask, drawPos, NPC.frame, new Color(255, 255, 255), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }        
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }
    }

    class SanguineBlast : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/Glow_1";

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 600, true);
        }

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 20;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            if (Projectile.ai[0] >= 1)
            {
                Projectile.alpha += 12;
            }
            else
            {
                int numDusts = 45;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 266, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25) * Projectile.scale, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }

            Projectile.ai[0]++;
            Projectile.scale *= 1.06f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Center();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            int Texturenumber = 15;
            for (int i = 0; i < Texturenumber; i++)
            {
                Main.EntitySpriteDraw(texture, (Projectile.Center + new Vector2(Main.rand.NextFloat(50) * Projectile.scale, 0).RotatedByRandom(MathHelper.TwoPi)) - Main.screenPosition, frame, Projectile.GetAlpha(new Color(174, 47, 47)),
                Projectile.rotation, origin, (Projectile.scale * Main.rand.NextFloat(2)), SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class BloodBullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 400;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        private bool activation = false;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Projectile.rotation += 0.44f * (float)Projectile.direction;
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[0] == 0)
            {
                int numDusts = 30;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 266, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(7), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }

            Projectile.ai[0]++;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position - Projectile.velocity, Projectile.width, Projectile.height, 266, 0f, 0f, 100, Color.White, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }

            if (Projectile.timeLeft <= 20)
            {
                Projectile.alpha -= 12;
                Projectile.scale *= 1.05f;
                Projectile.velocity *= 0.98f;
            }

            if (Projectile.Distance(player.Center) < 100 && activation == false)
            {
                activation = true;
                Projectile.timeLeft = 20;
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item110, Projectile.position);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, ProjectileType<SanguineBlast>(), (Projectile.damage), 8f, Projectile.owner, 0f, 0f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 300, true);
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D trailtexture = Request<Texture2D>(Texture + "Trail").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trailtexture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f), Projectile.velocity.ToRotation(), drawOrigin, Projectile.scale * (1.2f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale / i, SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class NecroticBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 400;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 300, true);
        }

        public override void AI()
        {
            Projectile.rotation += 0.44f * (float)Projectile.direction;
            Player player = Main.player[Projectile.owner];

            switch (Projectile.ai[1])
            {
                case 0:
                    //Drop after a set tme.

                    if (Projectile.ai[0] >= 24)
                    {
                        Projectile.velocity.X *= 0.95f;
                        Projectile.velocity.Y *= 1.025f;
                    }

                    break;

                case 1:
                    //Gain a speed boost.

                    if (Projectile.ai[0] == 60)
                    {
                        Projectile.velocity *= 3f;
                        int numDusts = 10;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 0, 0, 114, Scale: 1.2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Vector2 trueVelocity = new Vector2(3, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            trueVelocity.X *= 0.5f;
                            trueVelocity = trueVelocity.RotatedBy(Projectile.velocity.ToRotation());
                            Main.dust[dust].velocity = trueVelocity;
                        }
                    }

                    break;
            }

            if (Projectile.ai[0] == 0)
            {
                int numDusts = 5;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 266, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }

            Projectile.ai[0]++;

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position - Projectile.velocity, Projectile.width, Projectile.height, 266, 0f, 0f, 100, Color.White, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }

            if (Projectile.timeLeft <= 20)
            {
                Projectile.alpha += 12;
                Projectile.velocity *= 0.98f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D trailtexture = Request<Texture2D>(Texture + "Trail").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trailtexture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f), Projectile.velocity.ToRotation(), drawOrigin, Projectile.scale * (1.2f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale / i, SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class NecroticRay : Deathray
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 100000;
        }

        public override string Texture => "GloryMod/NPCs/BloodMoon/Hemolitionist/NecroticRay";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 2;
            MoveDistance = 20f;
            RealMaxDistance = 3000f;
            bodyRect = new Rectangle(0, 10, Projectile.width, Projectile.height);
            headRect = new Rectangle(0, 0, Projectile.width, Projectile.height);
            tailRect = new Rectangle(0, 20, Projectile.width, Projectile.height);
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 300, true);
            target.AddBuff(BuffID.WitheredWeapon, 300, true);
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            DrawLaser(Main.spriteBatch, Request<Texture2D>(Texture).Value, Position(), Projectile.velocity, bodyRect.Height, -1.57f, 1f, MaxDistance, (int)MoveDistance);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

        }

        public override void PostAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = 0; i < RealMaxDistance; i += 10)
            {
                if (Main.rand.NextBool(50))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.NextFloat(-3, 0), 0) + Vector2.UnitX.RotatedBy(Projectile.rotation) * i, 114, Vector2.UnitX.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(10, 20), 0, default, 1f * Main.rand.NextFloat(0.8f, 1.2f));
                    dust.noGravity = true;
                }
            }

            if (Projectile.ai[1] == 0)
            {
                bodyRect = new Rectangle(0, 10, Projectile.width, Projectile.height);
                headRect = new Rectangle(0, 0, Projectile.width, Projectile.height);
                tailRect = new Rectangle(0, 20, Projectile.width, Projectile.height);
            }
            if (Projectile.ai[1] == 1)
            {
                bodyRect = new Rectangle(10, 10, Projectile.width, Projectile.height);
                headRect = new Rectangle(10, 0, Projectile.width, Projectile.height);
                tailRect = new Rectangle(10, 20, Projectile.width, Projectile.height);
            }
        }

        public override Vector2 Position()
        {
            return Main.npc[(int)Projectile.ai[0]].Center + new Vector2(0, 10).RotatedBy(Main.npc[(int)Projectile.ai[0]].rotation);
        }
    }

    class SanguinePulse : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/Glow_5";

        public override void SetDefaults()
        {
            Projectile.width = 1000;
            Projectile.height = 1000;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40;
            Projectile.alpha = 0;
            Projectile.scale = 1.5f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 300, true);
        }

        private float rotation = 0;

        public override void AI()
        {
            if (Projectile.ai[0] >= 1)
            {
                Projectile.alpha += 12;             
            }
            else
            {
                rotation = Main.rand.NextFloat(-0.25f, 0.25f);
            }

            if (Projectile.ai[0] > 20)
            {
                Projectile.hostile = false;
            }

            Projectile.ai[0]++;
            Projectile.scale *= 1.075f;
            Projectile.rotation += rotation;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 300;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Center();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 0, 0)), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
