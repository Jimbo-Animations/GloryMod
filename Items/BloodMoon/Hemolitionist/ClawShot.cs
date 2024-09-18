using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace GloryMod.Items.BloodMoon.Hemolitionist
{
    class ClawShot : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Generic;

            Item.noMelee = true;
            Item.damage = 800;
            Item.knockBack = 10;

            Item.Size = new Vector2(52, 22);
            Item.useTime = Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ProjectileType<ClawShotProj>();
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Pink;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }
    }

    class ClawShotProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        Player player => Main.player[Projectile.owner];
        float timer;
        float starOpacity;

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(22);

            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.frameCounter++;

            if (player.channel)
            {
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, Projectile.rotation - MathHelper.ToRadians(90f));

                // Controls the movement of the gun.

                Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], Projectile.timeLeft > 100 && Projectile.timeLeft < 110 ? MathHelper.ToRadians(30f * -Projectile.direction) : 0, Projectile.timeLeft > 110 ? .25f : .125f);
                Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.timeLeft > 101 && Projectile.timeLeft < 115 ? 4 : 0, .1f);

                player.direction = Projectile.Center.X >= player.Center.X ? 1 : -1;
                Projectile.spriteDirection = Projectile.direction = player.direction;
                Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation() + (Projectile.ai[1] * Projectile.spriteDirection);
                Projectile.Center = player.MountedCenter + new Vector2(22, 0).RotatedBy(Projectile.rotation);

                if (Projectile.ai[0] <= 100)
                {
                    player.statLife -= 1;
                    if (player.statLife <= 0) player.KillMe(PlayerDeathReason.ByCustomReason(player.name + " drew too much blood"), 1, 0);
                }

                Projectile.timeLeft = 2;
            }
            else if (Projectile.ai[0] < 100 && !player.dead) player.Heal((int)Projectile.ai[0] / 2);

            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;

                if (Projectile.ai[0] < 110 && Projectile.frame > 4) Projectile.frame = 4;
            }

            if (Projectile.ai[0] > 120) Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask1 = Request<Texture2D>(Texture + "Mask1").Value;
            Texture2D mask2 = Request<Texture2D>(Texture + "Mask2").Value;
            SpriteEffects effects;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            effects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            timer += 0.1f;

            if (timer >= MathHelper.Pi) timer = 0f;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(12 - Projectile.ai[2], 0).RotatedBy(Projectile.rotation), frame, lightColor * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(mask1, Projectile.Center - Main.screenPosition + new Vector2(12 - Projectile.ai[2], 0).RotatedBy(Projectile.rotation), frame, Color.White * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(mask2, Projectile.Center - Main.screenPosition + new Vector2(12 - Projectile.ai[2], 0).RotatedBy(Projectile.rotation), frame, Color.White * .5f * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            return false;
        }
    }
}
