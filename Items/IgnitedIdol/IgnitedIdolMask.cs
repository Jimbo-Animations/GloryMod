using Terraria.GameContent.Creative;


namespace GloryMod.Items.IgnitedIdol
{
    [AutoloadEquip(EquipType.Head)]
    internal class IgnitedIdolMask : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Quest;
        }
    }
}
