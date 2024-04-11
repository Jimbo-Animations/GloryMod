using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.Audio;

namespace GloryMod.Items.Geomancer
{
    internal class OrbOfPondering : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 16;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<OrbPlayer>().orbEquipped = true;
        }
    }

    class OrbPlayer : ModPlayer
    {
        public int orbTimer;
        public bool orbEquipped;

        public override void ResetEffects()
        {
            orbEquipped = false;         
        }

        public override void PostUpdate()
        {
            if (orbEquipped && Player.velocity.X == 0 && Player.velocity.Y == 0 && (!Player.dead || Player.active)) orbTimer++;
            else orbTimer = 0;

            if (orbTimer >= 120)
            {
                if (orbTimer == 120)
                {
                    SoundEngine.PlaySound(SoundID.Item15, Player.Center);

                    for (int i = 0; i < 24; i++)
                    {
                        Dust.NewDustPerfect(Player.MountedCenter, 110, new Vector2(4, 0).RotatedBy(MathHelper.TwoPi * i / 24f), Scale: 1f).noGravity = true;
                    }
                }

                Player.AddBuff(BuffType<OrbBuff>(), 60, false);
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (Player.HasBuff<OrbBuff>() && orbEquipped)
            {
                Main.dust[Dust.NewDust(Player.position, Player.width, Player.height, 110, Scale: 1f)].noGravity = true;
            }
        }
    }

    class OrbBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 2;
            player.jumpSpeedBoost += 2f;

            player.lifeRegen += 4;
            player.manaRegen += 2;
        }
    }
}
