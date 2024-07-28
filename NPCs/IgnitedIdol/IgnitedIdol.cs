using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using GloryMod.Systems;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using GloryMod.Items.IgnitedIdol;
using GloryMod.Systems.BossBars;
using System.IO;

namespace GloryMod.NPCs.IgnitedIdol
{
    [AutoloadBossHead]
    internal class IgnitedIdol : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("Once used for worship, this great idol has since been forgotten deep underground, a remnant of a now diminished god. " +
                "Its reignition remains a mystery, but this event might point towards a great reawakening...")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 98;
            NPC.height = 98;
            DrawOffsetY = -4f;
            NPC.alpha = 255;

            NPC.damage = 0;
            NPC.defense = 20;
            NPC.lifeMax = Main.getGoodWorld ? 10000 : 7500;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 1, 50, 0);
            NPC.npcSlots = 50f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.dontTakeDamage = true;

            NPC.BossBar = GetInstance<IgnitedIdolBossBar>();
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Torch, minimumDropped: 50, maximumDropped: 100));
            npcLoot.Add(ItemDropRule.Common(ItemID.TorchGodsFavor));
            npcLoot.Add(ItemDropRule.Common(ItemType<IgnitedIdolMask>()));
            npcLoot.Add(ItemDropRule.OneFromOptions(1, ItemType<LightBringer>(), ItemType<ThePurifier>(), ItemType<RadiantScroll>(), ItemType<AshBearer>()));
        }


        private int animState = 0;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 3;

            if (animState == 0)
            {
                NPC.frame.X = 0;
                NPC.frame.Y = 0;
            }

            if (animState == 1)
            {
                NPC.frame.X = 0;
                NPC.frameCounter++;

                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0f;
                }
                if (NPC.frame.Y >= frameHeight * 7)
                {
                    NPC.frame.Y = 0;
                    animState = 2;
                }
            }

            if (animState == 2)
            {
                NPC.frame.X = NPC.frame.Width;
                NPC.frameCounter++;

                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0f;
                }
                if (NPC.frame.Y >= frameHeight * 6)
                {
                    NPC.frame.Y = 0;
                }
            }

            if (animState == 3)
            {
                NPC.frame.X = NPC.frame.Width * 2;
                NPC.frameCounter++;

                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0f;
                }
                if (NPC.frame.Y >= frameHeight * 6)
                {
                    NPC.frame.Y = 0;
                    NPC.frame.X = 0;
                    animState = 0;
                    bloomAlpha = 0;
                }
            }
        }
        private enum AttackPattern : byte
        {
            IntroCutscene = 0,
            BouncyFire = 1,
            FlamePillars = 2,
            RingBlast = 3,
            LanternBH = 4,
            TransitionScene = 5,
            BouncyFireEX = 6,
            TorchTrail = 7,
            RingBlastEX = 8,
            FlameVortex = 9,
            DeathCutscene = 10
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public float currentDistance = 8;
        public bool hasUsedAttack1 = Main.getGoodWorld;
        public bool hasUsedAttack2 = Main.getGoodWorld;
        public bool hasUsedAttack3 = Main.getGoodWorld;
        public bool hasUsedAttack4 = Main.getGoodWorld;
        private bool attacking = false;
        private bool hasEnteredPhase2 = false;
        private bool changeAppearance = false;
        private bool readyToDie = false;

        NPC Lantern;
        public Vector2 targetLocation;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(currentDistance);
                writer.Write(hasUsedAttack1);
                writer.Write(hasUsedAttack2);
                writer.Write(hasUsedAttack3);
                writer.Write(hasUsedAttack4);
                writer.Write(attacking);
                writer.Write(hasEnteredPhase2);
                writer.Write(changeAppearance);
                writer.Write(readyToDie);

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
                currentDistance = reader.ReadInt32();
                hasUsedAttack1 = reader.ReadBoolean();
                hasUsedAttack2 = reader.ReadBoolean();
                hasUsedAttack3 = reader.ReadBoolean();
                hasUsedAttack4 = reader.ReadBoolean();
                attacking = reader.ReadBoolean();
                hasEnteredPhase2 = reader.ReadBoolean();
                changeAppearance = reader.ReadBoolean();
                readyToDie = reader.ReadBoolean();

                NPC.localAI[0] = reader.ReadInt32();
                NPC.localAI[1] = reader.ReadInt32();
                NPC.localAI[2] = reader.ReadInt32();
                NPC.localAI[3] = reader.ReadInt32();
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Vector2 dustPos = new Vector2(0, Main.rand.NextFloat(0, -NPC.height / 2)).RotatedByRandom(MathHelper.ToRadians(180));

            if (animState != 0)
            {
                if (changeAppearance == true)
                {
                    if (Main.rand.NextBool(5))
                    {
                        Dust dust = Dust.NewDustPerfect(NPC.Center + dustPos, 59, new Vector2(0, Main.rand.NextFloat(-1, -5)).RotatedByRandom(MathHelper.ToRadians(90)), 0, default, 1f * Main.rand.NextFloat(1f, 1.5f));
                        dust.noGravity = true;
                    }
                    Lighting.AddLight(NPC.Center, 1 * bloomAlpha, 1 * bloomAlpha, 2 * bloomAlpha);
                }
                else
                {
                    if (Main.rand.NextBool(5))
                    {
                        Dust dust = Dust.NewDustPerfect(NPC.Center + dustPos, 6, new Vector2(0, Main.rand.NextFloat(-1, -5)).RotatedByRandom(MathHelper.ToRadians(90)), 0, default, 1f * Main.rand.NextFloat(1f, 1.5f));
                        dust.noGravity = true;
                    }
                    Lighting.AddLight(NPC.Center, 2 * bloomAlpha, 1 * bloomAlpha, 1 * bloomAlpha);
                }

            }

            NPC.ai[1] = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == NPCType<AwakenedLantern>() && Main.npc[i].ai[3] == 0)
                {
                    NPC.ai[1]++;
                }
            }

            //Keeps players from leaving the arena.

            currentDistance = MathHelper.Lerp(currentDistance, NPC.ai[1], 0.1f);
            if (target.Distance(NPC.Center) > 200 + (25 * currentDistance) && NPC.ai[0] != 0)
            {
                target.position = target.oldPosition;
                target.velocity += new Vector2(2, 0).RotatedBy(target.DirectionTo(NPC.Center).ToRotation());

                //Secondary check that makes sure player is inside.
                if (target.Distance(NPC.Center) > 200 + (25 * currentDistance) && NPC.ai[0] != 0)
                {
                    target.position += new Vector2(25, 0).RotatedBy(target.DirectionTo(NPC.Center).ToRotation());
                }
            }

            //Makes the arena be at its full size during the intro scene.

            if (target.Distance(NPC.Center) > 400 && NPC.ai[0] == 0)
            {
                target.position = target.oldPosition;
                target.velocity += new Vector2(2, 0).RotatedBy(target.DirectionTo(NPC.Center).ToRotation());

                //Secondary check that makes sure player is inside.
                if (target.Distance(NPC.Center) > 200 + (25 * currentDistance) && NPC.ai[0] != 0)
                {
                    target.position += new Vector2(25, 0).RotatedBy(target.DirectionTo(NPC.Center).ToRotation());
                }
            }


            //Check if the player is dead.
            if (!target.active || target.dead)
            {
                if (NPC.ai[0] != 0)
                {
                    NPC.active = false;
                    SoundEngine.PlaySound(SoundID.Item100, NPC.Center);
                    int numDusts = 50;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(NPC.Center, 0, 0, 6, Scale: 3f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    }
                }
            }

            switch (AIstate)
            {
                case AttackPattern.IntroCutscene:

                    //Intro Animation.

                    NPC.localAI[0]++;
                    Music = MusicLoader.GetMusicSlot(Mod, "Music/Ignited_Idol_Phase_1_full");

                    if (NPC.localAI[0] <= 150)
                    {
                        Lighting.AddLight(NPC.Center, 0.8f * starAlpha, 0.8f * starAlpha, 0.8f * starAlpha);

                        if (NPC.localAI[0] == 1)
                        {
                            SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
                        }
                    }

                    if (NPC.localAI[0] == 150)
                    {
                        flash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0f, 100f));
                        NPC.alpha = 0;
                        ScreenUtils.screenShaking = 10f;
                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/IgnitedIdolIntro"), NPC.Center);

                        int numDusts = 50;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 6, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }
                    }

                    if (NPC.localAI[0] > 150 && NPC.localAI[0] <= 400)
                    {
                        Lighting.AddLight(NPC.Center, 0.2f, 0.1f, 0.1f);
                    }

                    if (NPC.localAI[0] == 400)
                    {
                        animState = 1;
                        SoundEngine.PlaySound(SoundID.Item100, NPC.Center);
                    }

                    if (NPC.localAI[0] == 600)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Lantern = Main.npc[NPC.NewNPC(NPC.InheritSource(NPC), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<AwakenedLantern>())];
                            Lantern.ai[0] = NPC.whoAmI;
                            Lantern.ai[1] = i;
                        }
                        NPC.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.DD2_BetsySummon, NPC.Center);
                    }

                    //Control the camera.

                    NPC.localAI[1]++;

                    if (NPC.localAI[1] == 1)
                    {
                        ScreenUtils.ChangeCameraPos(NPC.Center, 100, 1.2f);
                    }
                    if (NPC.localAI[1] == 100 && NPC.localAI[0] <= 700)
                    {
                        NPC.localAI[1] = 0;
                    }

                    //End scene.

                    if (NPC.localAI[0] > 800)
                    {
                        NPC.ai[0]++;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.dontTakeDamage = false;
                        attacking = true;
                        NPC.netUpdate = true;

                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/IgnitedIdolGroan") with { Volume = 1.5f }, NPC.Center);
                        ScreenUtils.screenShaking = 5f;
                    }

                    break;

                case AttackPattern.BouncyFire:

                    //Shoot projectiles at the player.

                    NPC.localAI[0]++;

                    if (NPC.localAI[0] == 1 && NPC.localAI[1] == 0 && hasUsedAttack1 == false)
                    {
                        int numDusts = 25;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 6, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }
                    }

                    if (NPC.localAI[0] >= 60 && NPC.localAI[1] <= 5)
                    {
                        int proj;
                        proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(hasUsedAttack1 ? 10 : 8, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()).RotatedByRandom(MathHelper.ToRadians(NPC.localAI[1] * 3)), ProjectileType<AwakenedLight>(), 60, 0, target.whoAmI, NPC.whoAmI);
                        Main.projectile[proj].ai[0] = NPC.whoAmI;

                        NPC.localAI[0] = 0;
                        NPC.localAI[1]++;
                        NPC.netUpdate = true;

                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                    }

                    if (NPC.localAI[0] >= 180 && NPC.localAI[1] > 5)
                    {
                        NPC.ai[2] = 1;
                    }

                    if (NPC.localAI[0] == 200)
                    {
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.ai[0] = hasEnteredPhase2 == true ? 5 : NPC.ai[0] + 1;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack1 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.FlamePillars:

                    //Conjure targets to create deathrays.

                    NPC.localAI[0]++;

                    if (NPC.localAI[0] == 20 && NPC.localAI[1] < 4)
                    {
                        int proj;
                        proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center + new Vector2(Main.rand.NextFloat(100, 200), 0).RotatedByRandom(MathHelper.TwoPi), Vector2.Zero, ProjectileType<SeekingStarlight>(), 0, 0, target.whoAmI, NPC.whoAmI, hasUsedAttack2 == true ? 1 : 0);
                        Main.projectile[proj].ai[0] = NPC.whoAmI;

                        if (hasUsedAttack2 == true)
                        {
                            Main.projectile[proj].ai[1] = 1;
                        }
                        else Main.projectile[proj].rotation = MathHelper.ToRadians(NPC.localAI[1] * 90);


                        NPC.localAI[1]++;
                        NPC.ai[2] = Main.rand.Next(0, 8);

                        NPC.netUpdate = true;
                    }

                    if (NPC.localAI[0] >= 200 && NPC.localAI[1] < 4)
                    {
                        NPC.ai[2] = 0;
                        NPC.localAI[0] = 0;
                    }

                    if (NPC.localAI[0] >= 300)
                    {
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.ai[0] = hasEnteredPhase2 == true ? 5 : NPC.ai[0] + 1;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack2 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.RingBlast:

                    //Summon spirits on the edge of the arena.

                    NPC.localAI[0]++;
                    int attackCount = hasUsedAttack3 ? 4 : 3;

                    if (NPC.localAI[0] == 20 && NPC.localAI[1] < attackCount)
                    {
                        targetLocation = NPC.Center + new Vector2(200 + (20 * currentDistance), 0).RotatedByRandom(MathHelper.TwoPi);

                        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), targetLocation, Vector2.Zero, ProjectileType<PrimordialSpirit>(), 72, target.whoAmI);
                        Main.projectile[proj].ai[0] = NPC.whoAmI;

                        NPC.localAI[1]++;
                        NPC.netUpdate = true;
                    }

                    if (NPC.localAI[0] >= 120 && NPC.localAI[1] < attackCount)
                    {
                        NPC.localAI[0] = 0;
                    }

                    if (NPC.localAI[0] >= 200)
                    {
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.ai[0] = hasEnteredPhase2 == true ? 5 : NPC.ai[0] + 1;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack3 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.LanternBH:

                    //Uses all lanterns to attack.

                    NPC.localAI[0]++;
                    attackCount = hasUsedAttack4 ? 5 : 3;

                    if (NPC.localAI[0] == 30)
                    {
                        NPC.ai[2] = 1;
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, NPC.Center);
                    }

                    if (NPC.localAI[0] > 180 && NPC.localAI[1] < attackCount)
                    {
                        NPC.localAI[0] = hasUsedAttack4 ? 140 : 120;
                        NPC.localAI[1]++;
                        if (NPC.ai[3] != 1)
                        {
                            NPC.localAI[3] = 1;
                        }
                        else
                        {
                            NPC.ai[3] = 0;
                        }
                    }

                    if (NPC.localAI[0] >= 200)
                    {
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.ai[0] = hasEnteredPhase2 == true ? 5 : 1;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack4 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.TransitionScene:

                    //Cutscene for phase 2 transition

                    NPC.ai[3]++;
                    NPC.localAI[1]++;
                    NPC.localAI[2]++;

                    if (NPC.ai[3] == 1)
                    {
                        animState = 3;

                        NPC.life = NPC.lifeMax;
                        Music = MusicLoader.GetMusicSlot(Mod, "Music/Ignited_Idol_Phase_2_full");
                        attacking = false;
                    }
                    if (NPC.localAI[2] < 90 || NPC.ai[2] != 8)
                    {
                        Lighting.AddLight(NPC.Center, 0.2f, 0.1f, 0.1f);
                    }

                    if (NPC.localAI[2] >= 120 && NPC.ai[2] < 8)
                    {
                        NPC.localAI[2] = 30;
                        NPC.ai[2]++;
                    }

                    if (NPC.ai[2] >= 1 && Main.rand.NextBool(50 / ((int)NPC.ai[2])) && (NPC.ai[2] > 8 || NPC.localAI[2] < 90))
                    {
                        Vector2 velocity = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
                        int dust = Dust.NewDust(NPC.Center + (velocity * Main.rand.NextFloat(150, 250)), 0, 0, 59, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-10, -15), 0).RotatedBy(velocity.ToRotation());
                    }

                    if (NPC.localAI[2] == 90 && NPC.ai[2] == 8)
                    {
                        flash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0f, 100f));
                        animState = 1;
                        changeAppearance = true;
                        ScreenUtils.screenShaking = 10f;
                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/IgnitedIdolIntro"), NPC.Center);

                        int numDusts = 50;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 59, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }
                    }

                    //End scene.

                    if (NPC.ai[3] == 900)
                    {
                        NPC.ai[0]++;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;

                        NPC.dontTakeDamage = false;
                        attacking = true;
                        NPC.netUpdate = true;

                        if (!Main.getGoodWorld)
                        {
                            hasUsedAttack1 = false;
                            hasUsedAttack2 = false;
                            hasUsedAttack3 = false;
                            hasUsedAttack4 = false;
                        }
                    }

                    break;

                case AttackPattern.BouncyFireEX:

                    //Shoot projectiles at the player. Use churning projectile attack as well.

                    NPC.localAI[0]++;
                    NPC.localAI[2] = 0;

                    if (NPC.localAI[0] == 1 && NPC.localAI[1] == 0)
                    {
                        if (hasUsedAttack1 == true)
                        {
                            NPC.ai[3] = Main.rand.Next(0, 4);
                            NPC.localAI[3] = NPC.ai[3] + 4;
                        }
                        else
                        {
                            NPC.ai[3] = Main.rand.Next(0, 8);
                            NPC.localAI[3] = NPC.ai[3];
                        }
                    }

                    if (NPC.localAI[0] == 1 && NPC.localAI[1] == 0 && hasUsedAttack1 == false)
                    {
                        int numDusts = 25;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 59, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }
                    }

                    if (NPC.localAI[0] >= 60 && NPC.localAI[1] <= 5)
                    {
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(10, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()).RotatedByRandom(MathHelper.ToRadians(NPC.localAI[1] * 3)), ProjectileType<AwakenedLight>(), 72, 0, target.whoAmI);
                        Main.projectile[proj].ai[0] = NPC.whoAmI;
                        Main.projectile[proj].ai[1] = 2;

                        NPC.localAI[0] = 0;
                        NPC.localAI[1]++;

                        if (NPC.localAI[1] == 6)
                        {
                            NPC.localAI[2] = 1;
                        }

                        NPC.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                    }

                    if (NPC.localAI[0] >= 220 && NPC.localAI[1] > 5)
                    {
                        NPC.ai[2] = 1;
                    }

                    if (NPC.localAI[0] == 240)
                    {
                        NPC.ai[0] = readyToDie == true ? 10 : NPC.ai[0] + 1;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack1 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.TorchTrail:

                    //Create fire and shoot predicting bullets.

                    NPC.localAI[0]++;
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] <= 500)
                    {
                        NPC.localAI[2]++;
                    }
                    else
                    {
                        NPC.localAI[2] = 0;
                    }

                    if (NPC.localAI[0] == 20)
                    {
                        int numDusts = 12;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(target.Center, 0, 0, 59, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(5, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }
                    }

                    if (NPC.localAI[0] >= 60 && NPC.localAI[1] < 510)
                    {
                        int proj1 = Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center + new Vector2(Main.rand.NextFloat(-10, 10), 0).RotatedByRandom(MathHelper.TwoPi), new Vector2(Main.rand.NextFloat(0, 2), 0).RotatedByRandom(MathHelper.TwoPi), ProjectileType<WrathfulEmbers>(), 72, 0, target.whoAmI);
                        Main.projectile[proj1].ai[0] = NPC.whoAmI;

                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath, target.Center);
                        NPC.localAI[0] = hasUsedAttack2 ? 40 : 30;
                        NPC.netUpdate = true;
                    }

                    if (NPC.localAI[2] >= 200 - (12.5f * NPC.ai[1]) && NPC.localAI[1] < 510)
                    {
                        targetLocation = NPC.DirectionTo(target.Center + target.velocity * (NPC.Distance(target.Center) / 10)) * 8;

                        int proj2 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, targetLocation, ProjectileType<AwakenedLight>(), 72, 3f, Main.myPlayer);
                        Main.projectile[proj2].ai[0] = NPC.whoAmI;
                        Main.projectile[proj2].ai[1] = 3;

                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                        NPC.localAI[2] = 0;
                        NPC.netUpdate = true;
                    }

                    if (NPC.localAI[1] == 540)
                    {
                        NPC.ai[2] = 1;
                    }

                    if (NPC.localAI[1] == 600)
                    {
                        NPC.ai[0] = readyToDie == true ? 10 : NPC.ai[0] + 1;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack2 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.RingBlastEX:

                    //Summon spirits on the edge of the arena and pincer the player with lanterns.

                    attackCount = hasUsedAttack3 ? 5 : 4;
                    NPC.localAI[0]++;
                    NPC.ai[3]++;

                    if (NPC.localAI[0] == 1 && NPC.localAI[1] == 0)
                    {
                        NPC.ai[2] = Main.rand.Next(0, 4);
                        NPC.localAI[2] = NPC.ai[2] + 4;
                    }

                    if (NPC.localAI[0] == 20 && NPC.localAI[1] < attackCount)
                    {
                        targetLocation = NPC.Center + new Vector2(200 + (20 * currentDistance), 0).RotatedByRandom(MathHelper.TwoPi);

                        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), targetLocation, Vector2.Zero, ProjectileType<PrimordialSpirit>(), 72, target.whoAmI);
                        Main.projectile[proj].ai[0] = NPC.whoAmI;
                        Main.projectile[proj].ai[1] = 1;

                        NPC.localAI[1]++;
                        NPC.netUpdate = true;
                    }

                    if (NPC.localAI[0] > 120)
                    {
                        if (NPC.localAI[1] < attackCount)
                        {
                            NPC.localAI[0] = hasUsedAttack3 ? 10 : 0;
                        }

                        NPC.ai[3] = hasUsedAttack3 ? 10 : 0;
                    }

                    if (NPC.localAI[0] >= 200)
                    {
                        NPC.ai[0] = readyToDie == true ? 10 : NPC.ai[0] + 1;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack3 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.FlameVortex:

                    //Create bullet hell vortex.

                    NPC.ai[2]++;
                    if (NPC.ai[2] == 30)
                    {
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<BlazingInferno>(), 90, target.whoAmI);
                        Main.projectile[proj].ai[0] = NPC.whoAmI;
                        Main.projectile[proj].ai[1] = hasUsedAttack4 == true ? 1 : 0;

                        SoundEngine.PlaySound(SoundID.Item100, NPC.Center);
                    }

                    if (NPC.ai[2] == 930)
                    {
                        NPC.ai[0] = readyToDie == true ? 10 : 6;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.localAI[3] = 0;
                        hasUsedAttack4 = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.DeathCutscene:

                    //Modified vortex attack with death scene.

                    NPC.ai[2]++;

                    if (NPC.ai[2] == 1)
                    {
                        ScreenUtils.ChangeCameraPos(NPC.Center, 200, 1f);
                        SoundEngine.PlaySound(SoundID.DD2_BetsyDeath, NPC.Center);
                        Music = MusicLoader.GetMusicSlot(Mod, "Music/Silence");
                        attacking = false;
                    }

                    if (NPC.ai[2] == 5)
                    {
                        Main.musicFade[Main.curMusic] = MathHelper.Lerp(0, 1, 1);
                        Music = MusicLoader.GetMusicSlot(Mod, "Music/Remnants_of_a_Dying_Ember");
                    }

                    if (Main.rand.NextBool(2) && NPC.ai[2] <= 200)
                    {
                        Vector2 velocity = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
                        int dust = Dust.NewDust(NPC.Center + (velocity * Main.rand.NextFloat(150, 250)), 0, 0, 59, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-10, -15), 0).RotatedBy(velocity.ToRotation());
                    }

                    if (NPC.ai[2] == 200)
                    {
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<BlazingInferno>(), 90, target.whoAmI);
                        Main.projectile[proj].ai[0] = NPC.whoAmI;
                        Main.projectile[proj].ai[1] = 2;

                        SoundEngine.PlaySound(SoundID.Item100, NPC.Center);
                        NPC.netUpdate = true;
                    }

                    if (NPC.ai[2] > 200)
                    {
                        ScreenUtils.ChangeCameraPos(NPC.Center, 40, 1f);
                    }

                    if (NPC.ai[1] == 0 && NPC.ai[2] >= 200)
                    {
                        NPC.ai[3] = 1;
                        if (NPC.localAI[0] == 0)
                        {
                            animState = 3;
                        }
                        NPC.localAI[0]++;
                    }

                    if (NPC.localAI[0] == 100)
                    {
                        NPC.alpha = 255;
                        ScreenUtils.screenShaking = 10f;
                        flash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0f, 100f));
                        SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/IgnitedIdolIntro"), NPC.Center);
                    }

                    if (NPC.localAI[0] == 200)
                    {
                        NPC.SimpleStrikeNPC(Main.rand.Next(15000, 20000), 0, true, 0, DamageClass.Default, false);
                    }

                    break;
            }

            isPlayerActive();
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 1;

            if (target.Distance(NPC.Center) <= 200)
            {
                modNPC.DR = 1 + (1 / 3);
            }

            if (NPC.life <= 0 && hasEnteredPhase2 == false)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                hasEnteredPhase2 = true;
                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/IgnitedIdolGroan") with { Volume = 1.5f, PitchVariance = 0.5f }, NPC.Center);
                ScreenUtils.screenShaking = 5f;

                NPC.netUpdate = true;
            }
            if (NPC.life <= 0 && hasEnteredPhase2 == true && readyToDie != true)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/IgnitedIdolGroan") with { Volume = 1.5f, PitchVariance = 0.5f }, NPC.Center);
                readyToDie = true;

                NPC.netUpdate = true;
            }
        }

        void isPlayerActive()
        {
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(false);
                if (!target.active || target.dead)
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    return;
                }
            }
        }

        List<Tuple<Vector2, float, float>> flash = new List<Tuple<Vector2, float, float>>();
        private float bloomAlpha;
        private float starAlpha;
        private float runeAlpha;
        private float telegraphAlpha;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D texture2 = Request<Texture2D>(Texture + "2").Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D mask2 = Request<Texture2D>(Texture + "Mask2").Value;
            Texture2D glow1 = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Texture2D star = Request<Texture2D>("GloryMod/CoolEffects/Textures/Star").Value;
            Texture2D runeA = Request<Texture2D>("GloryMod/CoolEffects/Textures/rune3").Value;
            Texture2D runeB = Request<Texture2D>("GloryMod/CoolEffects/Textures/rune4").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 glowOrigin = glow1.Size() / 2;
            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);

            //Controls the visibility of various effects.
            if (animState == 1)
            {
                bloomAlpha = MathHelper.Lerp(bloomAlpha, 1, 0.1f);
            }

            if (animState == 3)
            {
                bloomAlpha = MathHelper.Lerp(bloomAlpha, 0, 0.1f);
            }

            if (attacking == true)
            {
                runeAlpha = MathHelper.Lerp(runeAlpha, 1, 0.05f);
            }
            else
            {
                runeAlpha = MathHelper.Lerp(runeAlpha, 0, 0.05f);
            }

            //Controls the visibility and brightness of the drawcode.
            float scale = (NPC.scale * mult) * bloomAlpha;

            //Makes sure it does not draw its normal code for its bestiary entry.
            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                if (changeAppearance == true)
                {
                    spriteBatch.Draw(glow1, drawPos, null, NPC.GetAlpha(new Color(175, 100, 250)) * scale, NPC.rotation, glowOrigin, scale * 1.4f, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(glow1, drawPos, null, NPC.GetAlpha(new Color(250, 175, 100)) * scale, NPC.rotation, glowOrigin, scale * 1.4f, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                if (changeAppearance == true)
                {
                    spriteBatch.Draw(texture2, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin + new Vector2(2, -12), NPC.scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(mask2, drawPos, NPC.frame, NPC.GetAlpha(new Color(255, 255, 255)), NPC.rotation, drawOrigin + new Vector2(2, -12), NPC.scale, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin + new Vector2(2, -12), NPC.scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(mask, drawPos, NPC.frame, NPC.GetAlpha(new Color(255, 255, 255)), NPC.rotation, drawOrigin + new Vector2(2, -12), NPC.scale, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);


                if (changeAppearance == true)
                {
                    spriteBatch.Draw(glow1, drawPos, null, NPC.GetAlpha(new Color(175, 100, 255)) * 0.8f * scale, NPC.rotation, glowOrigin, scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(runeA, drawPos, null, new Color(175, 100, 255) * runeAlpha * scale, Main.GameUpdateCount * 0.01f, runeA.Size() / 2, NPC.scale * runeAlpha * 0.8f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(runeB, drawPos, null, new Color(175, 100, 255) * runeAlpha * scale, Main.GameUpdateCount * -0.02f, runeB.Size() / 2, NPC.scale * runeAlpha * 0.75f, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(glow1, drawPos, null, NPC.GetAlpha(new Color(255, 175, 100)) * 0.8f * scale, NPC.rotation, glowOrigin, scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(runeA, drawPos, null, new Color(255, 175, 100) * runeAlpha * scale, Main.GameUpdateCount * 0.015f, runeA.Size() / 2, NPC.scale * runeAlpha * 0.65f, SpriteEffects.None, 0f);
                }

                if (NPC.ai[0] == 7)
                {
                    if (NPC.localAI[2] >= 60)
                    {
                        telegraphAlpha = MathHelper.Lerp(telegraphAlpha, 1, 0.05f);
                    }
                    else
                    {
                        telegraphAlpha = MathHelper.Lerp(telegraphAlpha, 0, 0.05f);
                    }

                    Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(NPC.Distance(target.Center), 0).RotatedBy(NPC.DirectionTo(target.Center + target.velocity * (NPC.Distance(target.Center) / 8)).ToRotation()), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), new Color(200, 150, 255) * telegraphAlpha);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                if (NPC.ai[0] == 0 && NPC.localAI[0] <= 150) //Used for the intro scene.
                {
                    starAlpha = MathHelper.Lerp(starAlpha, 1, 0.1f);
                    float starShine = starAlpha * (float)(5 + Math.Sin(NPC.localAI[0] / 3f)) / 5f;

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                    spriteBatch.Draw(glow1, drawPos, null, new Color(255, 255, 255) * starShine, NPC.rotation, glowOrigin, starShine, SpriteEffects.None, 0f);
                    spriteBatch.Draw(star, drawPos, null, new Color(255, 255, 255) * starShine, -Main.GameUpdateCount * 0.025f, star.Size() / 2, NPC.scale * starShine * 1.2f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(star, drawPos, null, new Color(255, 255, 255) * starShine * 0.8f, Main.GameUpdateCount * 0.015f, star.Size() / 2, NPC.scale * starShine, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
                }

                for (int i = 0; i < flash.Count; i++)
                {
                    if (i >= flash.Count)
                    {
                        break;
                    }

                    flash[i] = new Tuple<Vector2, float, float>(flash[i].Item1, flash[i].Item2 + flash[i].Item3, flash[i].Item3);
                    float flashAlpha = 30 / (flash[i].Item2 * 0.005f) - 1;
                    Main.EntitySpriteDraw(glow1, flash[i].Item1 - Main.screenPosition, null, new Color(255, 255, 255) * flashAlpha, 0, glowOrigin, flash[i].Item2 / glow1.Width, SpriteEffects.None, 0);

                    if (flash[i].Item2 >= target.Distance(flash[i].Item1) + Main.screenWidth * 3)
                    {
                        flash.RemoveAt(i);
                    }
                }
            }
            else
            {
                animState = 2;
                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale * 0.5f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
