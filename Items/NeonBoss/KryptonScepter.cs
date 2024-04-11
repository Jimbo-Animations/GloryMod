using Terraria.Audio;
using GloryMod.Systems;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using static Terraria.ModLoader.ModContent;
using Terraria;

namespace GloryMod.Items.NeonBoss
{
    class KryptonScepter : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.Magic;
            Item.width = 36;
            Item.height = 36;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 10;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 2);
            Item.shoot = ProjectileType<NeonBoltFriendly>();
            Item.shootSpeed = 1;
            Item.channel = true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }


        private int rockTimer;
        public override void UseAnimation(Player player)
        {
            if (player.channel)
            {
                rockTimer++;
            }
            else
            {
                rockTimer = 0;
            }

            player.AddBuff(BuffID.Featherfall, 40);
        }

        public override bool CanUseItem(Player player)
        {
            return player.velocity.Y >= 0f && player.Bottom.findGroundUnder().Distance(player.Bottom) < 8 && player.statMana >= player.statManaMax2;
        } 

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 groundPosition = player.Center.findGroundUnder();
            player.statMana = 0;

            if (rockTimer >= 40)
            {
                rockTimer = 40;
                SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, player.Center);
                Vector2 spawnonground = new Vector2(Main.rand.NextFloat(-200, 200), 0) + groundPosition;
                Projectile.NewProjectile(source, spawnonground, new Vector2(0, Main.rand.NextFloat(-4, -8)).RotatedByRandom(MathHelper.ToRadians(10)), ProjectileType<KryptonScepterDebris>(), damage, knockback, player.whoAmI);
            }

            ScreenUtils.ScreenShake(10);
            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack, player.Center);

            player.velocity.Y -= 10;
            player.velocity.X *= 1.2f;

            groundPosition = player.Center.findGroundUnder();

            for (int k = 0; k < player.statManaMax2 / 20; k++)
            {
				Vector2 spawnonground = new Vector2(Main.rand.NextFloat(-200, 200), 0) + groundPosition;
				Projectile.NewProjectile(source, spawnonground, new Vector2(0, Main.rand.NextFloat(-8, -12)).RotatedByRandom(MathHelper.ToRadians(10)), ProjectileType<KryptonScepterDebris>(), damage, knockback, player.whoAmI);
			}

            return false;
        }
    }

    class KryptonScepterDebris : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/NeonBoss/NeonTyrantDebris";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Debris");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        private int rngtime = 0;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.boss == true)
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override void AI()
        {
            Projectile.rotation += rngtime * 0.05f;
            Projectile.ai[0]++;

            if (Projectile.ai[0] <= 60 + rngtime)
            {
                Projectile.velocity *= 0.99f;
            }
            else
            {
                if (Projectile.ai[0] == 75 + rngtime)
                {
                    Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld).RotatedByRandom(MathHelper.ToRadians(10)) * (15 - rngtime / 3);
                    Projectile.friendly = true;
                }
                else
                {
                    if (Projectile.ai[0] >= 101 + rngtime)
                    {
                        Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.DirectionTo(Main.MouseWorld).ToRotation(), MathHelper.ToRadians(1f * (Projectile.velocity.Length() / 20))).ToRotationVector2() * Projectile.velocity.Length();
                    }
                    Projectile.velocity *= 1.005f;
                }
                Projectile.tileCollide = true;
            }

            if (Projectile.ai[1] == 0)
            {
                Projectile.frame = Main.rand.Next(0, 3) % Main.projFrames[Projectile.type];
                rngtime = Main.rand.Next(-15, 15);

                for (int i = 0; i < 8; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center + new Vector2(Main.rand.NextFloat(12, -12), Main.rand.NextFloat(0, -5)), 0, 0, 0, Scale: 1.2f);
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].noLight = false;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedByRandom(MathHelper.TwoPi);
                }

                Projectile.ai[1] = 1;
            }

            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 4;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D trailtexture = Request<Texture2D>("GloryMod/NPCs/NeonBoss/NeonTyrantDebrisTrail").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trailtexture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.GetAlpha(new Color(255, 231, 86) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f))), Projectile.rotation, drawOrigin, Projectile.scale * (1.3f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center + new Vector2(Main.rand.NextFloat(10, -10), Main.rand.NextFloat(10, -10)), 0, 0, 1, Scale: 2f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi);
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, ProjectileType<DebrisExplosionFriendly>(), Projectile.damage, 10, Projectile.owner);
        }
    }

    class DebrisExplosionFriendly : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/Glow_1";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Explosion");
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 20;
            Projectile.alpha = 0;
            Projectile.scale = 1f;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.boss == true)
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override void AI()
        {
            if (Projectile.ai[0] >= 1)
            {
                Projectile.alpha += 13;
            }

            Projectile.ai[0]++;
            Projectile.scale *= 1.06f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Center();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            int Texturenumber = 15;
            for (int i = 0; i < Texturenumber; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center + new Vector2(Main.rand.NextFloat(50), 0).RotatedByRandom(MathHelper.TwoPi) - Main.screenPosition, frame,
                Projectile.GetAlpha(new Color(255, 231, 86)), Projectile.rotation, origin, Projectile.scale * Main.rand.NextFloat(2), SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
