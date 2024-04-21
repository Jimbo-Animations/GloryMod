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
            Intro = 0,
            Test = 1,
            Debris = 2,
            DevilScythes = 3,
            Fireballs = 4
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float MinionCount => ref NPC.ai[2];
        public ref float WhichWall => ref NPC.ai[3];

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {

            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {

            }
        }

        private bool startPhase2;
        private bool startPhase3;
        private bool phase2Started;
        private bool phase3Started;
        private bool deathAnimationStarted;
        private bool useShield;

        private float spawnCenterY;
        private float targetDistance;
        private float targetSpeed;
        private float targetHeadPlacement;
        private float aimTowards;

        SlotId ShieldNoise = SlotId.Invalid;

        public override void OnSpawn(IEntitySource source)
        {
            ShieldNoise = SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop with { IsLooped = true }, NPC.Center);

            spawnCenterY = NPC.Center.Y;
            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            NPC.netUpdate = true;
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            UpdateSound();

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            // Remove all buffs when invulnerable.

            if (NPC.dontTakeDamage == true)
            {
                //cancel all buffs
                for (int i = 0; i < NPC.buffTime.Length; i++)
                {
                    NPC.buffTime[i] = 0;
                }
            }

            // Keep a count of Forsaken for phase 1.

            MinionCount = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == NPCType<Forsaken>() && Main.npc[i].ai[3] == NPC.whoAmI)
                {
                    MinionCount++;
                }
            }

            switch (AIstate)
            {
                case AttackPattern.Intro:

                    if (AITimer > 60)
                    {
                        MinionCount = 5;

                        for (int i = -2; i < 3; i++)
                        {
                            minion = Main.npc[NPC.NewNPC(NPC.InheritSource(NPC), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Forsaken>())];
                            minion.ai[2] = i;
                            minion.ai[3] = NPC.whoAmI;
                        }

                        AITimer = 0;
                        NPC.ai[0]++;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.Test:

                    WallMovement(target.position, NPC.DirectionTo(target.Center).ToRotation(), NPC.spriteDirection,
                    (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? 20 : 2 - (MinionCount * 0.25f),
                    (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? 0.025f : 0.05f);

                    if (AITimer >= 200)
                    {
                        AITimer = 0;
                        NPC.ai[0]++;
                        NPC.netUpdate = true;
                    }


                    break;

                case AttackPattern.Debris:

                    WallMovement(target.position, AITimer < 200 ? NPC.velocity.ToRotation() : NPC.DirectionTo(target.Center).ToRotation(), NPC.spriteDirection,
                    (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? 20 : 2 - (MinionCount * 0.25f),
                    (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? 0.025f : 0.05f, 0.05f);


                    Vector2 rubbleStart = NPC.Top + new Vector2((Main.rand.NextFloat(501) + (AITimer * 8) - 480) * NPC.spriteDirection, -100);
                    Vector2 rubbleEnd = Systems.Utils.findCeilingAbove(rubbleStart);
                    animState = AITimer < 120 ? 1 : 0;

                    if (AITimer % 10 == 1 && AITimer < 120) Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(30 * NPC.spriteDirection, 30).RotatedBy(NPC.rotation), Vector2.Zero, ProjectileType<YellowRoar>(), 0, 1, target.whoAmI);

                    if (AITimer == 10)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyDeath with { Volume = 5 }, target.Center);
                    }

                    if (ScreenUtils.screenShaking < 5 && AITimer < 120 && AITimer >= 10) ScreenUtils.screenShaking = 5;

                    if (AITimer < 200 && AITimer >= 60 && AITimer % 4 == 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), rubbleEnd, new Vector2(NPC.spriteDirection, 0), ProjectileType<BBDebris>(), 80, 1, target.whoAmI);
                        NPC.netUpdate = true;
                    }

                    if (AITimer > 300)
                    {
                        AITimer = 0;
                        NPC.ai[0]++;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.DevilScythes:

                    if (AITimer == 1) 
                    {
                        targetHeadPlacement = target.position.Y + Main.rand.Next(-350, 351);
                    }

                    if (AITimer < 150)
                    {
                        aimTowards = NPC.DirectionTo(target.Center).ToRotation();
                    }

                    WallMovement(AITimer <= 360 ? new Vector2(target.position.X, targetHeadPlacement) : target.position, AITimer <= 360 ? aimTowards : NPC.DirectionTo(target.Center).ToRotation(), NPC.spriteDirection,
                    (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? 20 : 2 - (MinionCount * 0.25f),
                    (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? 0.025f : 0.05f, 0.05f, 0.0667f);

                    if (AITimer >= 150 && AITimer % 15 == 1 && AITimer < 300)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(((AITimer * 6) - 650) * NPC.spriteDirection, 0).RotatedBy(NPC.rotation), Vector2.Zero, ProjectileType<BBPurpleRune>(), 100, 1, target.whoAmI);
                        NPC.netUpdate = true;
                    }

                    if (AITimer >= 450)
                    {
                        AITimer = 0;
                        NPC.ai[0]++;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.Fireballs:

                    WallMovement(target.position, NPC.DirectionTo(target.Center).ToRotation(), NPC.spriteDirection,
                  (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? 20 : (AITimer <= 150 ? 1 - MinionCount * .1f : 2 - (MinionCount * .25f)),
                  (target.Center.X > NPC.Center.X + 650 && NPC.spriteDirection == 1 || target.Center.X < NPC.Center.X - 650 && NPC.spriteDirection == -1) ? .025f : .05f);

                    if (AITimer <= 150)
                    {
                        animState = 1;

                        if (AITimer == 150)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(10 * NPC.spriteDirection, 0).RotatedBy(MathHelper.ToRadians(NPC.rotation)), ProjectileType<BBFireBallLarge>(), 1000, 1, target.whoAmI);
                            NPC.netUpdate = true;
                        }
                    }
                    else animState = 0;

                    if (AITimer >= 300)
                    {
                        AITimer = 0;
                        NPC.ai[0] = 1;
                        NPC.netUpdate = true;
                    }

                    break;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {

        }
    }
}
