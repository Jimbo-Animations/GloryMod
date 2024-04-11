using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;

namespace GloryMod.Items.IgnitedIdol
{
    internal class DivineBeacon : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12; // This helps sort inventory know that this is a boss summoning Item.

            // If this would be for a vanilla boss that has no summon item, you would have to include this line here:
            // NPCID.Sets.MPAllowedEnemies[NPCID.Plantera] = true;

            // Otherwise the UseItem code to spawn it will not work in multiplayer
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 40;
            Item.maxStack = 1;
            Item.value = 100;
            Item.rare = ItemRarityID.Quest;
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(NPCType<NPCs.IgnitedIdol.IgnitedIdol>()) && (player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                int type = NPCType<NPCs.IgnitedIdol.IgnitedIdol>();

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // If the player is not in multiplayer, spawn directly
                    NPC.SpawnBoss((int)player.position.X, (int)player.position.Y, type, player.whoAmI);
                }
                else
                {
                    // If the player is in multiplayer, request a spawn
                    // This will only work if NPCID.Sets.MPAllowedEnemies[type] is true, which we set in MinionBossBody
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                }
            }

            return true;
        }

        public override void AddRecipes()
        {

            CreateRecipe()
                .AddIngredient(ItemID.TorchGodsFavor)
                .AddIngredient(ItemID.UltrabrightTorch)
                .AddIngredient(ItemID.DemonTorch)
                .AddIngredient(ItemID.BoneTorch)
                .AddIngredient(ItemID.CoralTorch)
                .AddIngredient(ItemID.PinkTorch)
                .AddIngredient(ItemID.ShimmerTorch)
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}
