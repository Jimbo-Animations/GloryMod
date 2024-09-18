using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using GloryMod.Systems.BossBars;

namespace GloryMod.NPCs.BloodMoon.Hemolitionist.New
{
    [AutoloadBossHead]
    partial class Hemolitionist : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 10;
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
            NPC.Size = new (50, 50);
            NPC.hide = true;
            NPC.dontTakeDamage = true;

            NPC.damage = 0;
            NPC.defense = 30;
            NPC.lifeMax = Main.getGoodWorld ? 32500 : 25000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 50, 0, 0);
            NPC.npcSlots = 50f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = new SoundStyle("GloryMod/Music/HemoHit", 4, SoundType.Sound) with { Volume = 0.33f };
            NPC.DeathSound = null;

            NPC.BossBar = GetInstance<HemolitionistBossBar>();
        }

        public override void BossHeadRotation(ref float rotation)
        {
            rotation = NPC.rotation;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return target.dead || !Main.bloodMoon;
        }

        public override void AI()
        {
            int goalDirection = target.Center.X < NPC.Center.X ? -1 : 1;
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            switch (AIstate)
            {
                case AttackPattern.StartBattle:

                    StartBattle();

                    break;

                case AttackPattern.RedtideIntro:

                    RedTideIntro();

                    break;

                case AttackPattern.Intro:

                    NormalIntro();

                    break;

                case AttackPattern.DashAttack:

                    DashAttack(target.Center + new Vector2(400 * -goalDirection, 0), Phase2Started ? .08f : .075f, Phase2Started ? .6f : .45f);

                    break;

                case AttackPattern.CannonVolleys:

                    CannonVolleys(target.Center, Phase2Started ? .08f : .075f, Phase2Started ? .7f : .6f);

                    break;

                case AttackPattern.CarpetBomb:

                    CarpetBomb(target.Center, Phase2Started ? .85f : .7f, Phase2Started ? 4 : 6);

                    break;

                case AttackPattern.MissileBarrage:

                    MissileBarrage(target.Center, Phase2Started ? .06f : .07f, Phase2Started ? .75f : .6f);

                    break;

                case AttackPattern.Deathray:

                    Deathray(target.Center);

                    break;

                case AttackPattern.Telefrag:

                    Telefrag(target.Center);

                    break;

                case AttackPattern.Phase2:

                    Phase2();

                    break;

                case AttackPattern.DeathAnim:

                    DeathCutscene();

                    break;
            }
        }
    }
}