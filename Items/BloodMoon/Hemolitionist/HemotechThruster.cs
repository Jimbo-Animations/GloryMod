using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace GloryMod.Items.BloodMoon.Hemolitionist
{
    internal class HemotechThruster : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Registers a vertical animation with 4 frames and each one will last 5 ticks (1/12 second)
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 18));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true; // Makes the item have an animation while in world (not held.). Use in combination with RegisterItemAnimation

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.wingTimeMax *= 2;
            player.jumpSpeedBoost += 2f;
            player.maxFallSpeed += 2f;

            if (player.velocity.Y != 0)
            {
                player.maxRunSpeed *= 2f;
                player.runAcceleration += 0.2f;
            }
        }
    }
}
