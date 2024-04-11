using Terraria.GameContent.Creative;

namespace GloryMod.Items.Sightseer
{
    internal class OtherworldlyFlesh : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(22, 34);
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);
        }
    }
}
