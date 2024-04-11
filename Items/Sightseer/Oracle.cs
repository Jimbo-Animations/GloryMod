﻿using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using Terraria.Audio;

namespace GloryMod.Items.Sightseer
{
    internal class Oracle : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.width = 48;
            Item.height = 48;

            Item.damage = 18;
            Item.knockBack = 3f;
            Item.shootsEveryUse = true;
            Item.autoReuse = true;

            Item.UseSound = SoundID.DD2_GhastlyGlaiveImpactGhost;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 2, silver: 50);
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ProjectileType<OracleSwing>();
        }

        public override bool AltFunctionUse(Player player)
        {
            Vector2 teleportTo = player.position + new Vector2(MathHelper.Clamp(player.Distance(Main.MouseWorld), 0, 150), 0).RotatedBy(player.DirectionTo(Main.MouseWorld).ToRotation());

            return teleTimer > 400 && Collision.CanHit(player.position, (int)player.Size.X, (int)player.Size.Y, teleportTo, (int)player.Size.X, (int)player.Size.Y);
        }

        int teleTimer = 450;

        public override void UpdateInventory(Player player)
        {
            teleTimer++;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 teleportTo = player.position + new Vector2(MathHelper.Clamp(player.Distance(Main.MouseWorld), 0, 150), 0).RotatedBy(player.DirectionTo(Main.MouseWorld).ToRotation());

                player.position = teleportTo;
                player.immune = true;
                player.immuneTime = 15;

                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, player.Center);

                for (int i = 0; i < 24; i++)
                {
                    int dust = Dust.NewDust(player.Center, 0, 0, 111, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / 24);
                }

                teleTimer = 0;
                player.AddBuff(BuffType<OracleCooldown>(), 450);

                return null;
            }
            else
            {
                Item.noUseGraphic = false;
                Item.noMelee = false;
                return null;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float adjustedItemScale = player.GetAdjustedItemScale(Item);
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);

            return false;
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

    class OracleSwing : ModProjectile
    {
        public override void SetStaticDefaults() =>
            (ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type], Main.projFrames[Type]) = (true, 4);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = 300f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.aiStyle = -1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            Player player = Main.player[Projectile.owner];
            float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1];
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation;
            float scaleMulti = 0.6f;
            float scaleAdder = 1f;
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = scaleAdder + percentageOfLife * scaleMulti;
            float dustRotation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f;
            Vector2 dustPosition = Projectile.Center + dustRotation.ToRotationVector2() * 84f * Projectile.scale;
            Vector2 dustVelocity = (dustRotation + Projectile.ai[0] * MathHelper.PiOver2).ToRotationVector2();
            if (Main.rand.NextFloat() * 2f < Projectile.Opacity)
            {
                Dust coloredDust = Dust.NewDustPerfect(Projectile.Center + dustRotation.ToRotationVector2() * (Main.rand.NextFloat() * 80f * Projectile.scale + 20f * Projectile.scale), DustID.GemSapphire, dustVelocity * 1f, default, default, 0.6f);
                coloredDust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
                coloredDust.noGravity = true;
            }
            if (Main.rand.NextFloat() * 1.5f < Projectile.Opacity)
            {
                Dust.NewDustPerfect(dustPosition, DustID.GemSapphire, dustVelocity, default, default, 1.2f).noGravity = true;
            }
            Projectile.scale *= Projectile.ai[2];
            if (Projectile.localAI[0] >= Projectile.ai[1])
            {
                Projectile.Kill();
            }
            for (float i = -MathHelper.PiOver4; i <= MathHelper.PiOver4; i += MathHelper.PiOver2)
            {
                Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center + (Projectile.rotation + i).ToRotationVector2() * 70f * Projectile.scale, new Vector2(60f * Projectile.scale, 60f * Projectile.scale));
                Projectile.EmitEnchantmentVisualsAt(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float coneLength = 94f * Projectile.scale;
            float collisionRotation = MathHelper.Pi * 2f / 25f * Projectile.ai[0];
            float maximumAngle = MathHelper.PiOver4;
            float coneRotation = Projectile.rotation + collisionRotation;
            if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle))
            {
                return true;
            }
            float backOfTheSwing = Utils.Remap(Projectile.localAI[0], Projectile.ai[1] * 0.3f, Projectile.ai[1] * 0.5f, 1f, 0f);
            if (backOfTheSwing > 0f)
            {
                float coneRotation2 = coneRotation - MathHelper.PiOver4 * Projectile.ai[0] * backOfTheSwing;
                if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation2, maximumAngle))
                {
                    return true;
                }
            }
            return false;
        }

        public override void CutTiles()
        {
            Vector2 starting = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 60f * Projectile.scale;
            Vector2 ending = (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * 60f * Projectile.scale;
            float width = 60f * Projectile.scale;
            Utils.PlotTileLine(Projectile.Center + starting, Projectile.Center + ending, width, DelegateMethods.CutTiles);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.SilverBulletSparkle,
                new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox) }, Projectile.owner);
            hit.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.SilverBulletSparkle,
                new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox) }, Projectile.owner);
            info.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 position = Projectile.Center - Main.screenPosition;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle sourceRectangle = texture.Frame(1, 4);
            Vector2 origin = sourceRectangle.Size() / 2f;
            float scale = Projectile.scale * 1.1f;
            SpriteEffects spriteEffects = ((!(Projectile.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
            float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1];
            float lerpTime = Utils.Remap(percentageOfLife, 0f, 0.6f, 0f, 1f) * Utils.Remap(percentageOfLife, 0.6f, 1f, 1f, 0f);
            float lightingColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
            lightingColor = Utils.Remap(lightingColor, 0.2f, 1f, 0f, 1f);
            Color backDarkColor = new Color(25, 100, 180);
            Color middleMediumColor = new Color(43, 178, 237);
            Color frontLightColor = new Color(47, 250, 255);
            Color whiteTimesLerpTime = Color.White * lerpTime * 0.5f;
            whiteTimesLerpTime.A = (byte)(whiteTimesLerpTime.A * (1f - lightingColor));
            Color faintLightingColor = whiteTimesLerpTime * lightingColor * 0.5f;
            faintLightingColor.G = (byte)(faintLightingColor.G * lightingColor);
            faintLightingColor.B = (byte)(faintLightingColor.R * (0.25f + lightingColor * 0.75f));
            Main.EntitySpriteDraw(texture, position, sourceRectangle, backDarkColor * lightingColor * lerpTime, Projectile.rotation + Projectile.ai[0] * MathHelper.PiOver4 * -1f * (1f - percentageOfLife), origin, scale, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, sourceRectangle, faintLightingColor * 0.2f, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, sourceRectangle, middleMediumColor * lightingColor * lerpTime * 0.3f, Projectile.rotation, origin, scale, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, sourceRectangle, frontLightColor * lightingColor * lerpTime * 0.5f, Projectile.rotation, origin, scale * 0.975f, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), Color.White * 0.6f * lerpTime, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), Color.White * 0.5f * lerpTime, Projectile.rotation + Projectile.ai[0] * -0.05f, origin, scale * 0.8f, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), Color.White * 0.4f * lerpTime, Projectile.rotation + Projectile.ai[0] * -0.1f, origin, scale * 0.6f, spriteEffects, 0f);
            for (float i = 0f; i < 8f; i += 1f)
            {
                float edgeRotation = Projectile.rotation + Projectile.ai[0] * i * (MathHelper.Pi * -2f) * 0.025f + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4) * Projectile.ai[0];
                Vector2 drawPos = position + edgeRotation.ToRotationVector2() * ((float)texture.Width * 0.5f - 6f) * scale;
                DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos, new Color(255, 255, 255, 0) * lerpTime * (i / 9f), middleMediumColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, edgeRotation, new Vector2(0f, Utils.Remap(percentageOfLife, 0f, 1f, 3f, 0f)) * scale, Vector2.One * scale);
            }
            Vector2 drawPos2 = position + (Projectile.rotation + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4) * Projectile.ai[0]).ToRotationVector2() * ((float)texture.Width * 0.5f - 4f) * scale;
            DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos2, new Color(255, 255, 255, 0) * lerpTime * 0.5f, middleMediumColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, 0f, new Vector2(2f, Utils.Remap(percentageOfLife, 0f, 1f, 4f, 1f)) * scale, Vector2.One * scale);
            return false;
        }

        private static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness)
        {
            Texture2D sparkleTexture = TextureAssets.Extra[98].Value;
            Color bigColor = shineColor * opacity * 0.5f;
            bigColor.A = 0;
            Vector2 origin = sparkleTexture.Size() / 2f;
            Color smallColor = drawColor * 0.5f;
            float lerpValue = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);
            Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X) * lerpValue;
            Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y) * lerpValue;
            bigColor *= lerpValue;
            smallColor *= lerpValue;
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, 0f + rotation, origin, scaleUpDown, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, 0f + rotation, origin, scaleUpDown * 0.6f, dir);
        }
    }

    class OracleCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
