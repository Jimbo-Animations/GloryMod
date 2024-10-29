using System.IO;
using GloryMod.Systems;
using GloryMod.Systems.BossBars;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;


namespace GloryMod.NPCs.PellucidWolfer
{
    partial class WolferHead : WormHead
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(52);

            NPC.damage = 0;
            NPC.lifeMax = Main.getGoodWorld ? 14000 : 10000;
            NPC.defense = 12;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.npcSlots = 25f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.HitSound = SoundID.DD2_WitherBeastCrystalImpact;
            NPC.DeathSound = SoundID.DD2_WitherBeastDeath with { Volume = 3, Pitch = -.3f };

            Music = MusicID.OtherworldlyInvasion;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask1 = Request<Texture2D>(Texture + "Mask1").Value;
            Texture2D mask2 = Request<Texture2D>(Texture + "Mask2").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = SpriteEffects.None;

            return false;
        }
    }

    class WolferBody : WormBody
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(38);

            NPC.damage = 0;
            NPC.lifeMax = Main.getGoodWorld ? 14000 : 10000;
            NPC.defense = 12;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 25f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.HitSound = SoundID.DD2_WitherBeastCrystalImpact;
            NPC.DeathSound = SoundID.DD2_WitherBeastDeath with { Volume = 3, Pitch = -.3f };

            Music = MusicID.OtherworldlyInvasion;
        }

        public override void Init()
        {
            WolferHead.CommonWormInit(this);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask1 = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = SpriteEffects.None;

            return false;
        }
    }

    class WolferTail : WormTail
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(36);

            NPC.damage = 0;
            NPC.lifeMax = Main.getGoodWorld ? 14000 : 10000;
            NPC.defense = 12;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 25f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.HitSound = SoundID.DD2_WitherBeastCrystalImpact;
            NPC.DeathSound = SoundID.DD2_WitherBeastDeath with { Volume = 3, Pitch = -.3f };

            Music = MusicID.OtherworldlyInvasion;
        }
        public override void Init()
        {
            WolferHead.CommonWormInit(this);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask1 = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = SpriteEffects.None;

            return false;
        }
    }
}
