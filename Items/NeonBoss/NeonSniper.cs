using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using static Terraria.ModLoader.ModContent;

namespace GloryMod.Items.NeonBoss
{
    class NeonSniper : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 28;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item33;
            Item.value = Item.sellPrice(gold: 2);
            Item.autoReuse = true;
            Item.shoot = ProjectileType<NeonBoltFriendly>();
            Item.shootSpeed = 1;
            Item.useAmmo = AmmoID.Gel;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 68f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }

        private float projCount = 1;
        private int counter;
        public override bool? UseItem(Player player)
        {
            projCount++;

            if (projCount > 3)
            {
                projCount = 1;
            }

            return true;
        }

        public override bool NeedsAmmo(Player player)
        {
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float numberProjectiles = projCount;
            float rotation = MathHelper.ToRadians(2f * numberProjectiles);
            position += Vector2.Normalize(velocity) * 1f;

            if (projCount == 1)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            else
            {
                for (int i = 0; i < numberProjectiles; i++)
                {
                    Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1)));
                    Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
                }
            }

            int numDusts = 9 + (int)projCount;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(position, 0, 0, 262, Scale: 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Vector2 trueVelocity = new Vector2(2 + projCount, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                trueVelocity.X *= 0.5f;
                trueVelocity = trueVelocity.RotatedBy(velocity.ToRotation());
                Main.dust[dust].velocity = trueVelocity;
            }

            counter = 0;
            return false;
        }

        public override void UpdateInventory(Player player)
        {
            counter++;

            if (counter >= 60)
            {
                projCount = 0;
            }
        }
    }

    class NeonBoltFriendly : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/NeonBoss/NeonLightning";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Neon Bolt");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 3;
            Projectile.scale = 1.2f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        private Vector2 startPosition;
        private Vector2 startVelocity;

        public override void AI()
        {
            if (Projectile.timeLeft == 300)
            {
                startPosition = Projectile.Center;
                startVelocity = Projectile.velocity;
            }

            if (pierceCap >= 3)
            {
                Projectile.friendly = false;
            }

            Vector2 goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * 12 * Projectile.ai[1] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 12 * (float)Math.Sin(Projectile.ai[0] + Projectile.ai[1] / 5f) * (float)Math.Sin(MathHelper.Pi * Projectile.ai[1] / 15f);

            Projectile.velocity = goalPosition - Projectile.Center;
            Projectile.rotation += 0.4f;
            Projectile.ai[1]++;
        }

        private int pierceCap = 0;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            pierceCap++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.GetAlpha(new Color(255, 231, 86) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f))), Projectile.rotation, drawOrigin, Projectile.scale * (1.3f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
