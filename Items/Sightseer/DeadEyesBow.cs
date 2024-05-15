using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace GloryMod.Items.Sightseer
{
    class DeadEyesBow : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.DefaultToRangedWeapon(ProjectileID.PurificationPowder, AmmoID.Arrow, 35, 8f, true);
            Item.SetWeaponValues(21, 3f);

            Item.width = 28;
            Item.height = 46;
            Item.scale = 1f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item5;
            Item.useTurn = false;

            Item.value = Item.sellPrice(gold: 2, silver: 50);
            Item.rare = ItemRarityID.Blue;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 12f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int projectileID = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            Projectile projectile = Main.projectile[projectileID];

            DeadEyeEffect globalProjectile = projectile.GetGlobalProjectile<DeadEyeEffect>();
            globalProjectile.DeadEyeDupe = true;

            return false;
        }

        public override bool NeedsAmmo(Player player)
        {
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DemoniteBar, 5);
            recipe.AddIngredient(ItemType<OtherworldlyFlesh>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrimtaneBar, 5);
            recipe.AddIngredient(ItemType<OtherworldlyFlesh>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    class DeadEyeEffect : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool DeadEyeDupe;

        public int Dupe;
        public int DupeTimer;
        public int DupeMax = 4;

        public override void PostAI(Projectile projectile)
        {
            if (DeadEyeDupe)
            {
                DupeTimer++;

                if (DupeTimer >= 15 && Dupe < DupeMax)
                {
                    Dupe++;
                    DupeTimer = 0;

                    Vector2 dupePos = projectile.Center + new Vector2(Main.rand.Next(-25, 26), Main.rand.Next(-25, 26)).RotatedBy(projectile.rotation);

                    int dupedProj = Projectile.NewProjectile(projectile.GetSource_FromThis(), dupePos, projectile.velocity, projectile.type, projectile.damage / 2, projectile.knockBack / 2, projectile.owner);
                    Main.projectile[dupedProj].alpha = 150;

                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), dupePos);

                    int numDusts = 12;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(dupePos, 0, 0, 111, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    }
                }
            }
        }
    }
}
