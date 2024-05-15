using Terraria.Audio;
using Terraria.GameContent.Creative;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;

namespace GloryMod.Items.IgnitedIdol
{
    internal class ThePurifier : ModItem
    {
        float power = 100;

        float timer = 0;

        public override Vector2? HoldoutOffset() => new Vector2(-15, 0);

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.damage = 20;
            Item.knockBack = 3f;
            Item.ArmorPenetration = 10;

            Item.shoot = ProjectileType<PurifierFlames>();
            Item.shootSpeed = 15f;

            Item.width = 70;
            Item.height = 22;
            Item.scale = 1f;

            Item.useTime = 5;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.DD2_BetsyFlameBreath;
            Item.useAmmo = AmmoID.Gel;
            Item.autoReuse = true;
            Item.useTurn = false;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (power > 0)
            {
                power--;
            }

            timer = 0;
            position += Vector2.Normalize(velocity) * 20f;
            Projectile.NewProjectile(source, position, velocity * (power * 0.01f + 1f), type, (int)(damage * (power * 0.015f + 0.5f)), knockback * (power * 0.01f), player.whoAmI, power);

            return false;
        }

        public override bool NeedsAmmo(Player player)
        {
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (power < 100)
            {
                timer++;
            }

            if (timer != 0 && power == 100)
            {
                timer = 0;
                SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack);

                for (int i = 0; i < 24; i++)
                {
                    int dust = Dust.NewDust(player.Center, 0, 0, DustID.Torch, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / 24);
                }

            }

            if (timer >= 120)
            { 
                power++;                
            }
        }

        public class PurifierFlames : ModProjectile
        {
            public override void SetStaticDefaults()
            {
                ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
                ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
            }

            public override void SetDefaults()
            {
                Projectile.penetrate = -1;
                Projectile.DamageType = DamageClass.Ranged;
                Projectile.friendly = true;

                Projectile.Size = new Vector2(90);
                Projectile.scale = 1;
                Projectile.alpha = 0;

                Projectile.tileCollide = true;
                Projectile.ignoreWater = false;
                Projectile.timeLeft = 20;

                Projectile.aiStyle = -1;
                Projectile.usesIDStaticNPCImmunity = true;
                Projectile.idStaticNPCHitCooldown = 5;
            }

            public override void OnSpawn(IEntitySource source)
            {
                Projectile.rotation = Main.rand.NextFloat(-MathHelper.TwoPi, MathHelper.TwoPi);
                Projectile.scale = 0.5f + Projectile.ai[0] * 0.005f;
            }

            float opacity;
            public override void AI()
            {
                Projectile.rotation += 0.1f;
                Projectile.velocity *= 0.99f;

                Projectile.velocity.Y += 0.5f;
                Projectile.ai[1]++;

                //Adding dusts
                if (Main.rand.NextBool(20))
                {
                    Dust.NewDust(Projectile.Center + new Vector2(Main.rand.NextFloat(-30f * Projectile.scale, 30f * Projectile.scale), 0).RotatedByRandom(MathHelper.TwoPi), 0, 0, 6, Main.rand.NextFloat(-1, 2), Main.rand.NextFloat(-1, 2), 0, default, 2);
                }

                opacity = Projectile.timeLeft > 10 ? MathHelper.SmoothStep(opacity, 1, 0.25f) : MathHelper.SmoothStep(opacity, 0, 0.25f);
                Projectile.scale *= 1.033f;
            }

            public override void ModifyDamageHitbox(ref Rectangle hitbox)
            {
                Rectangle result = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                int num = (int)Utils.Remap(Projectile.ai[1] * 2, 0, 200, 10, 40);
                result.Inflate(num, num);
                hitbox = result;
            }

            public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
            {
                target.AddBuff(BuffID.OnFire3, 900, false);
            }


            public override bool OnTileCollide(Vector2 oldVelocity)
            {
                return false;
            }

            public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
            {
                width = height = 24;

                return true;
            }

            public override bool PreDraw(ref Color lightColor)
            {
                Texture2D texture = Request<Texture2D>(Texture).Value;
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
                Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255, 100) * (1 - i / (float)Projectile.oldPos.Length) * opacity, Projectile.rotation, drawOrigin, Projectile.scale * (1 - i / (float)Projectile.oldPos.Length), SpriteEffects.None, 0);
                }                

                return false;
            }
        }
    }
}
