using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using System.IO;
using static Terraria.ModLoader.ModContent;
using GloryMod.Items.Sightseer;

namespace GloryMod.NPCs.Nerd
{
    [AutoloadBossHead]
    partial class Nerd : ModNPC
    {
        public override void SetStaticDefaults()
        {
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Graveyard,
                new FlavorTextBestiaryInfoElement("What the actual fuck is this")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(50);
            DrawOffsetY = -4f;
            NPC.scale = Main.getGoodWorld ? 1.5f : 1f;

            NPC.damage = 0;
            NPC.defense = 12;
            NPC.lifeMax = Main.getGoodWorld ? 12000 : 9000;
            NPC.knockBackResist = 0;
            NPC.value = Item.buyPrice(0, 10, 0, 0);
            NPC.npcSlots = 50f;
            NPC.boss = true;
            NPC.chaseable = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.DD2_JavelinThrowersHurt;
            NPC.DeathSound = SoundID.DD2_JavelinThrowersDeath;

            Music = MusicLoader.GetMusicSlot(Mod, "Music/Nerd_Alert");
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        private enum AttackPattern
        {
            NextBotChase = 0,
            SpazMyTism = 1,
            SpookyDashes = 2,
            NextBotChase2 = 3,
            LaserBarrage = 4
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float AIAggression => ref NPC.ai[2];
        public ref float RepeatAttack => ref NPC.ai[3];

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(AITimer);
                writer.Write(AIAggression);
                writer.Write(RepeatAttack);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                AITimer = reader.ReadUInt16();
                AIAggression = reader.ReadUInt16();
                RepeatAttack = reader.ReadUInt16();
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Obstructed, 60, true);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.SnowGlobe));
            npcLoot.Add(ItemDropRule.Common(ItemID.Goggles));
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            AIAggression = NPC.life <= NPC.lifeMax / 2 ? NPC.life <= NPC.lifeMax / 4 ? 2 : 1 : 0;
            NPC.direction = target.Center.X > NPC.Center.X ? -1 : 1;

            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation(), 0.1f);

            if ((!target.active || target.dead || Main.dayTime) && NPC.ai[0] != 0)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }

            switch (AIstate)
            {
                case AttackPattern.NextBotChase:

                    NextBotChase();

                    break;

                case AttackPattern.SpazMyTism:

                    SpazMyTism();

                    break;

                case AttackPattern.SpookyDashes:

                    SpookyDashes();

                    break;

                case AttackPattern.NextBotChase2:

                    NextBotChase2();

                    break;

                case AttackPattern.LaserBarrage:

                    LaserBarrage();

                    break;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects;

            if (NPC.spriteDirection > 0)
            {
                effects = SpriteEffects.FlipVertically;
            }
            else
            {
                effects = SpriteEffects.None;
            }

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * ((1 - i / (float)NPC.oldPos.Length) * 0.95f), NPC.rotation, drawOrigin, NPC.scale * 1.25f, effects, 0);
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }
}
