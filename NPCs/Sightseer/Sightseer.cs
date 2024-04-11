using Terraria.GameContent.Bestiary;
using GloryMod.Systems;
using Terraria.GameContent.ItemDropRules;
using GloryMod.Systems.BossBars;
using System.IO;
using Terraria.GameContent;
using Terraria.Audio;
using GloryMod.Items.Sightseer;

namespace GloryMod.NPCs.Sightseer
{
    [AutoloadBossHead]
   internal partial class Sightseer : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Rain,
                new FlavorTextBestiaryInfoElement("Fleeting and elusive; The Sightseer's arrival is widely considered as a sign of upcoming disaster. It's graceful, aquatic exterior hides a more sinister form underneath...")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(100);
            DrawOffsetY = -4f;

            NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = Main.getGoodWorld ? 6000 : 5000;
            NPC.knockBackResist = 0.15f;
            NPC.value = Item.buyPrice(0, 1, 50, 0);
            NPC.npcSlots = 100f;
            NPC.boss = true;
            NPC.chaseable = false;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.DeathSound = SoundID.DD2_LightningBugDeath with { Pitch = -0.33f };

            NPC.BossBar = GetInstance<SightseerBossBar>();
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        private enum AttackPattern
        {
            Hide = -2,
            Stunned = -1,
            IntroScene = 0,
            MirrorChase = 1,
            WiggleWalls = 2,
            RapidDash = 3,
            MineField = 4,
            JellynRings = 5,
            Phase2Transition = 6,
            TeleRush = 7,
            MirageRings = 8,
            FakeOut = 9,
            Vortex = 10,
            MirrorChase2 = 11,
            DeathAnimation = 12
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float AIRandomizer => ref NPC.ai[2];
        public ref float RepeatAttack => ref NPC.ai[3];

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(animState);
                writer.Write(animSpeed);
                writer.Write(ramming);
                writer.Write(startPhase2);
                writer.Write(startPhase3);
                writer.Write(phase2Started);
                writer.Write(phase3Started);
                writer.Write(phase4Started);
                writer.Write(deathAnimationStarted);

                writer.Write(timer);
                writer.Write(visibility);
                writer.Write(spriteWidth);
                writer.Write(useSilhouette);
                writer.Write(beVisible);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                animState = reader.ReadInt32();
                animSpeed = reader.ReadInt32();
                ramming = reader.ReadBoolean();
                startPhase2 = reader.ReadBoolean();
                startPhase3 = reader.ReadBoolean();
                phase2Started = reader.ReadBoolean();
                phase3Started = reader.ReadBoolean();
                phase4Started = reader.ReadBoolean();
                deathAnimationStarted = reader.ReadBoolean();

                timer = reader.ReadInt32();
                visibility = reader.ReadInt32();
                spriteWidth = reader.ReadInt32();
                useSilhouette = reader.ReadBoolean();
                beVisible = reader.ReadBoolean();
            }
        }

        public static int secondStageHeadSlot = -1;

        public override void Load()
        {
            // We want to give it a second boss head icon, so we register one
            string texture = BossHeadTexture + "_2"; // Our texture is called "ClassName_Head_Boss_SecondStage"
            secondStageHeadSlot = Mod.AddBossHeadTexture(texture, -1); // -1 because we already have one registered via the [AutoloadBossHead] attribute, it would overwrite it otherwise
        }

        public override void BossHeadSlot(ref int index)
        {
            int slot = secondStageHeadSlot;
            if (phase3Started && slot != -1)
            {
                // If the boss is in its second stage, display the other head icon instead
                index = slot;
            }
            if (NPC.dontTakeDamage)
            {
                index = -1;
            }
        }

        public override bool CheckActive()
        {
            return true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (!target.HasBuff(BuffType<SeersTag>()))
            {
                CombatText.NewText(target.getRect(), new Color(255, 186, 101), "Tagged!", true, false);
                SoundEngine.PlaySound(SoundID.Zombie105, target.Center);
            }

            target.AddBuff(BuffType<SeersTag>(), 600, true);        
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            var SpawnRule = Main.ItemDropsDB.GetRulesForNPCID(NPCID.EyeofCthulhu, false);
            foreach (var dropRule in SpawnRule)
            {
                npcLoot.Add(dropRule);
            }
            npcLoot.Add(ItemDropRule.Common(ItemType<OtherworldlyFlesh>(), 1, 6, 6));
        }

        private int animState = 0;
        private int animSpeed = 6;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 3;
            NPC.frameCounter++;

            if (NPC.frameCounter > animSpeed)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;
            }

            switch (animState)
            {
                case 0:

                    if (NPC.frame.Y >= frameHeight * 4) NPC.frame.Y = 0;
                    NPC.frame.X = 0;

                    break;

                case 1:

                    if (NPC.frame.Y >= frameHeight * 3)
                    {
                        NPC.frame.Y = 0;
                        animState = 2;
                    }

                    NPC.frame.X = NPC.frame.Width;

                    break;

                case 2:

                    if (NPC.frame.Y >= frameHeight * 4) NPC.frame.Y = 0;
                    NPC.frame.X = NPC.frame.Width * 2;

                    break;

                case 3:

                    if (NPC.frame.Y >= frameHeight * 8 || NPC.frame.Y < frameHeight * 4) NPC.frame.Y = frameHeight * 4;
                    NPC.frame.X = 0;

                    break;

                case 4:

                    if (NPC.frame.Y >= frameHeight * 7 || NPC.frame.Y < frameHeight * 4)
                    {
                        if (NPC.frame.Y >= frameHeight * 7) animState = 5;
                        NPC.frame.Y = frameHeight * 4;
                    }

                    NPC.frame.X = NPC.frame.Width;

                    break;

                case 5:

                    if (NPC.frame.Y >= frameHeight * 8 || NPC.frame.Y < frameHeight * 4) NPC.frame.Y = frameHeight * 4;
                    NPC.frame.X = NPC.frame.Width * 2;

                    break;
            }
        }

        private bool ramming;
        private bool startPhase2;
        private bool phase2Started;
        private bool startPhase3;
        private bool phase3Started;
        private bool phase4Started;
        private bool deathAnimationStarted;
        NPC minion;

        public override void HitEffect(NPC.HitInfo hit) 
        {
            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 1;

            if ((startPhase2 && !phase2Started) || (startPhase3 && !phase3Started))
            {
                modNPC.DR = 0.5f;
            }

            for (int i = 0; i < 5; i++) Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 33, Scale: 2f)].noGravity = true;

            if (NPC.life <= 0 && !deathAnimationStarted)
            {
                NPC.life = 1;     
                animState = 3;
                animSpeed = 4;
                NPC.ai[0] = 12;
                ResetValues();
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
            }

        }

        public override void OnKill()
        {         
            NPC.downedBoss1 = true;
            Main.StopRain();
            Main.windSpeedTarget = 0f;
            int numDusts = 24;

            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 111, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            int gore5 = Mod.Find<ModGore>("SightseerGore5").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore5);
            int gore6 = Mod.Find<ModGore>("SightseerGore6").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore6);
            int gore7 = Mod.Find<ModGore>("SightseerGore7").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore7);
            int gore8 = Mod.Find<ModGore>("SightseerGore8").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore8);
            int gore9 = Mod.Find<ModGore>("SightseerGore9").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(Main.rand.NextFloat(5)).RotatedByRandom(MathHelper.TwoPi), gore9);
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            startPhase2 = NPC.life <= NPC.lifeMax * 0.8f;
            startPhase3 = NPC.life <= NPC.lifeMax * 0.6f;
            phase4Started = NPC.life <= NPC.lifeMax * 0.3f;
            NPC.damage = ramming ? (phase3Started ? 100 : 90) : 0;

            if (NPC.dontTakeDamage == true)
            {
                //cancel all buffs
                for (int i = 0; i < NPC.buffTime.Length; i++)
                {
                    NPC.buffTime[i] = 0;
                }
            }

            if (!Main.raining || Main.windSpeedCurrent > 0.5f)
            {
                Main.StartRain();
                Main.windSpeedTarget = 0.5f;
                Main.UseStormEffects = true;
            }

            if ((!target.active || target.dead || Main.dayTime) && NPC.ai[0] != 0)
            {
                ChangePosition(target.Center + new Vector2(0, -2000), true);
                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerShriek"), NPC.Center);
                NPC.active = false;
                int numDusts = 24;

                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 111, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                Main.StopRain();
                Main.windSpeedTarget = 0f;

                NPC.netUpdate = true;
            }

            switch (AIstate)
            {
                case AttackPattern.Hide:

                    Hiding((int)RepeatAttack);
                    animSpeed = 8;

                    break;

                case AttackPattern.Stunned:

                    int stunTime = 200;

                    if (RepeatAttack == 3) stunTime = 150;
                    if (RepeatAttack == 4) stunTime = 240;
                    if (RepeatAttack == 5) stunTime = 300;

                    if (RepeatAttack == 7) stunTime = 240;
                    if (RepeatAttack == 9 || RepeatAttack == 11) stunTime = 100;

                    Stunned((int)RepeatAttack, stunTime);
                    animSpeed = 8;

                    break;

                case AttackPattern.IntroScene:

                    IntroScene();
                    animSpeed = 7;

                    break;

                case AttackPattern.MirrorChase:

                    MirrorChase(phase2Started ? 6 : 4);
                    ramming = AITimer > 45 ? true : false;
                    animSpeed = ramming ? 4 : 8;

                    break;


                case AttackPattern.WiggleWalls:

                    WiggleWalls(phase2Started ? 80 : 100);
                    animSpeed = NPC.Distance(target.Center + new Vector2(0, -400)) > 10 ? 5 : 8;

                    break;

                case AttackPattern.RapidDash:

                    RapidDash(phase2Started ? 12 : 8);

                    if (ramming)
                    {
                        animState = 2;
                        animSpeed = 5;
                    }

                    break;


                case AttackPattern.MineField:

                    MineField(phase2Started ? 9 : 8);

                    animSpeed = AITimer > 60 ? 5 : 10;

                    break;

                case AttackPattern.JellynRings:

                    JellyTransition();

                    break;

                case AttackPattern.Phase2Transition:

                    Phase2Transition();

                    break;

                case AttackPattern.TeleRush:

                    TeleRush(phase4Started ? 24 : 40, phase4Started ? 16 : 10);

                    break;

                case AttackPattern.MirageRings:

                    MirageRings(phase4Started ? 4 : 3, phase4Started ? 100 : 120);

                    break;

                case AttackPattern.FakeOut:

                    FakeOut(phase4Started ? 5 : 7);

                    break;

                case AttackPattern.Vortex:

                    Vortex(phase4Started ? 50 : 75);

                    break;

                case AttackPattern.MirrorChase2:

                    MirrorChase2();

                    break;

                case AttackPattern.DeathAnimation:

                    DeathAnim();

                    break;
            }
        }
    }
}
