using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using GloryMod.Systems;
using Terraria.GameContent.ItemDropRules;
using GloryMod.Systems.BossBars;
using System.IO;
using Terraria.DataStructures;
using GloryMod.NPCs.BasaltBarriers.Minions;
using ReLogic.Utilities;
using GloryMod.NPCs.BasaltBarriers.Projectiles;
using System.Data;
using System.Collections.Generic;

namespace GloryMod.NPCs.BasaltBarriers.Boss
{
    [AutoloadBossHead]
    partial class BasaltBarrier : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.ShadowFlame] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(140);
            DrawOffsetY = -4f;

            NPC.damage = 0;
            NPC.defense = 25;
            NPC.lifeMax = Main.getGoodWorld ? 15000 : 12000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(1, 0, 0, 0);
            NPC.npcSlots = 50f;
            NPC.boss = true;
            NPC.chaseable = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.NPCHit41 with { Pitch = -0.67f };
            NPC.DeathSound = SoundID.NPCDeath43 with { Pitch = -0.67f };
            NPC.behindTiles = true;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        NPC minion;

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        private enum AttackPattern
        {
            Despawn = -1,
            Intro = 0,
            SoulBlast = 1,
            RedRunes = 2,
            RoarAndCharge = 3,
            LavaWalls = 4,
            PurpleRunes = 5,
            FireBomb = 6,
            Deathray = 7,
            Phase2Transition = 8,
            Phase2Intro = 9,
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float MinionCount => ref NPC.ai[2];
        public ref float WhichWall => ref NPC.ai[3];

        public bool GoingForward = true;

        private bool startPhase2;
        private bool startPhase3;
        private bool phase2Started;
        private bool phase3Started;
        private bool deathAnimationStarted;

        private float spawnCenterY;
        private float targetDistance;
        private float targetSpeed;
        private float targetHeadPlacement;
        private float aimTowards;

        SlotId ShieldNoise = SlotId.Invalid;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(GoingForward);
                writer.Write(startPhase2);
                writer.Write(startPhase3);
                writer.Write(phase2Started);
                writer.Write(phase3Started);
                writer.Write(deathAnimationStarted);

                writer.Write(spawnCenterY);
                writer.Write(targetDistance);
                writer.Write(targetSpeed);
                writer.Write(targetHeadPlacement);
                writer.Write(aimTowards);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                GoingForward = reader.ReadBoolean();
                startPhase2 = reader.ReadBoolean();
                startPhase3 = reader.ReadBoolean();
                phase2Started = reader.ReadBoolean();
                phase3Started = reader.ReadBoolean();
                deathAnimationStarted = reader.ReadBoolean();

                spawnCenterY = reader.ReadInt32();
                targetDistance = reader.ReadInt32();
                targetSpeed = reader.ReadInt32();
                targetHeadPlacement = reader.ReadInt32();
                aimTowards = reader.ReadInt32();
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            ShieldNoise = SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop with { IsLooped = true }, NPC.Center);

            if (WhichWall > 0) NPC.ai[0] = 9;

            spawnCenterY = NPC.Center.Y;
            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            NPC.netUpdate = true;
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            ManageForsaken();

            // Remove all buffs when invulnerable.

            if (NPC.dontTakeDamage == true)
            {
                //cancel all buffs
                for (int i = 0; i < NPC.buffTime.Length; i++)
                {
                    NPC.buffTime[i] = 0;
                }
            }

            switch (AIstate)
            {
                case AttackPattern.Intro:

                    Intro();

                    break;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {

        }
    }
}
