using System.Collections.Generic;
using System.IO;
using GloryMod.Systems;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.Items.HM
{
    internal class BlunderAxe : ModItem
    {
        public override void SetDefaults()
        {
            Item.Size = new Vector2(88, 36);
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 10);

            Item.DefaultToRangedWeapon(ProjectileID.PurificationPowder, AmmoID.Bullet, 60, 10f);
            Item.SetWeaponValues(110, 6);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = new TooltipLine(Mod, "Tooltip#0", "How do you even hold this!?")
            {
                OverrideColor = Systems.Utils.ColorLerpCycle(Main.GlobalTimeWrappedHourly, 2, new Color[] { new Color(40, 250, 60, 50), new Color(150, 70, 130, 50) })
            };
            tooltips.Add(line);
        }

        public override bool NeedsAmmo(Player player)
        {
            return player.altFunctionUse == 2;
        }

        public override void UseAnimation(Player player)
        {
            if (player.altFunctionUse != 2)
            {
                Item.UseSound = new SoundStyle("GloryMod/Music/Shotgun");
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
            else
            {
                Item.UseSound = SoundID.DD2_MonkStaffSwing;
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 40f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                ScreenUtils.screenShaking += 1;

                var proj = Projectile.NewProjectileDirect(source, position, velocity, ProjectileType<BlunderAxeProj>(), damage, knockback, player.whoAmI, 1);
                proj.DamageType = DamageClass.Ranged;
                Projectile.NewProjectileDirect(source, position, velocity, ProjectileType<BlunderSpike>(), damage, knockback, player.whoAmI, type);
            }
            else
            {
                var proj = Projectile.NewProjectileDirect(source, position, velocity, ProjectileType<BlunderAxeProj>(), damage * 2, knockback * 2, player.whoAmI, 0);
                proj.DamageType = DamageClass.Melee;
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FrostCore, 1);
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddIngredient(ItemID.Boomstick, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }

    class BlunderAxeProj : ModProjectile
    {
        public override string Texture => "GloryMod/Items/HM/BlunderAxe";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(88, 36);
            Projectile.scale = 1;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ownerHitCheck = true;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 30;
        }

        int duration;
        int frameCounter;

        private Player player => Main.player[Projectile.owner];

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Main.MouseWorld.X > player.MountedCenter.X ? 1 : -1;

            duration = player.itemAnimationMax;
            Projectile.timeLeft = duration;
            if (Projectile.ai[0] == 0) Projectile.rotation = MathHelper.ToRadians(Projectile.spriteDirection > 0 ? -45 : -135);
            else Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
            writer.Write((sbyte)Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Normalize(Projectile.velocity);
            player.heldProj = Projectile.whoAmI;

            if (Projectile.ai[0] == 0) // Swing as an axe
            {
                Projectile.ai[1] = MathHelper.SmoothStep(Projectile.ai[1], Projectile.timeLeft > duration * .6f ? MathHelper.ToRadians(60f * -Projectile.direction) : MathHelper.ToRadians(120f * Projectile.direction), Projectile.timeLeft > duration * .65f ? .125f : .3f);
                Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.timeLeft > duration * .3f && Projectile.timeLeft < duration * .9f ? 6 : 0, .1f);
                Projectile.rotation = MathHelper.ToRadians(Projectile.spriteDirection > 0 ? -45 : -135) + MathHelper.ToRadians(30f * -Projectile.direction) + Projectile.ai[1];

                if (Projectile.timeLeft == 36) 
                {
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
                    Projectile.friendly = true;
                }

                if (Projectile.timeLeft <= 18) Projectile.friendly = false;
            }
            else // Shoot as a gun
            {
                // Controls the movement of the gun.

                Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], Projectile.timeLeft > duration * .75 && Projectile.timeLeft < duration * .975f ? MathHelper.ToRadians(30f * -Projectile.direction) : 0, Projectile.timeLeft > duration / 2 ? .25f : .125f);
                Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.timeLeft > duration * .6 && Projectile.timeLeft < duration * .95f ? 4 : 0, .1f);
                Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.ai[1];
              
                if (frameCounter++ >= 3)
                {
                    Projectile.frame++;
                }
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            Vector2 armPosition = player.Center;

            armPosition.Y += player.gfxOffY;
            Projectile.Center = armPosition;

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }
        }

        // Find the start and end of the sword and use a line collider to check for collision with enemies
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = player.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
        }

        // Do a similar collision check for tiles
        public override void CutTiles()
        {
            Vector2 start = player.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
            Terraria.Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Make knockback go away from player     
            modifiers.HitDirectionOverride = target.position.X > player.MountedCenter.X ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.Center);
            target.AddBuff(BuffID.Frostburn2, 300);
            ScreenUtils.screenShaking += 1;

            for (int i = 0; i < 5; i++)
            {
                int dustType = DustID.IceGolem;
                var dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType);

                dust.velocity.X += Main.rand.NextFloat(-0.1f, 0.1f);
                dust.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);

                dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.Center);
            target.AddBuff(BuffID.Frostburn2, 300);
            ScreenUtils.screenShaking += 1;

            for (int i = 0; i < 5; i++)
            {
                int dustType = DustID.IceGolem;
                var dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType);

                dust.velocity.X += Main.rand.NextFloat(-0.2f, 0.21f);
                dust.velocity.Y += Main.rand.NextFloat(-0.2f, 0.21f);

                dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.031f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D flash = Request<Texture2D>(Texture + "Flash").Value;
            Vector2 origin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            SpriteEffects effects;

            int frameHeight = flash.Height / 4;
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, flash.Width, frameHeight);

            if (Projectile.ai[0] == 0) // Swing as an axe
            {
                if (Projectile.spriteDirection > 0) effects = SpriteEffects.FlipVertically;
                else effects = SpriteEffects.None;

                for (int i = 1; i < Projectile.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(mask, Projectile.oldPos[i] - Projectile.position + Projectile.Center + new Vector2(40, 8 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) - Main.screenPosition, default, Color.White * (Projectile.ai[2] * .1f) * (1 - i / (float)Projectile.oldPos.Length) * 0.25f, Projectile.oldRot[i] + MathHelper.Pi, origin, Projectile.scale, effects, 0);
                }

                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(40, 8 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), default, lightColor, Projectile.rotation + MathHelper.Pi, origin, Projectile.scale, effects, 0);
                Main.spriteBatch.Draw(mask, Projectile.Center - Main.screenPosition + new Vector2(40, 8 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), default, Color.White * (Projectile.ai[2] * .1f), Projectile.rotation + MathHelper.Pi, origin, Projectile.scale, effects, 0);
            }
            else // Shoot as a gun
            {
                if (Projectile.spriteDirection > 0) effects = SpriteEffects.None;
                else effects = SpriteEffects.FlipVertically;

                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(42 - Projectile.ai[2], 0).RotatedBy(Projectile.rotation), default, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
                Main.spriteBatch.Draw(mask, Projectile.Center - Main.screenPosition + new Vector2(42 - Projectile.ai[2], 0).RotatedBy(Projectile.rotation), default, Color.White * (Projectile.ai[2] * .1f), Projectile.rotation, origin, Projectile.scale, effects, 0);

                origin = new Vector2(flash.Width * 0.5f, frameHeight * 0.5f);

                Main.spriteBatch.Draw(flash, Projectile.Center - Main.screenPosition + new Vector2(92 - Projectile.ai[2], -4 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), sourceRectangle, Color.White, Projectile.rotation, origin, Projectile.scale / 2, effects, 0);
            }        

            return false;
        }
    }

    class BlunderSpike : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }


        public override void SetDefaults()
        {
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.Size = new Vector2(22);
            Projectile.scale = 1;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 60;
        }

        float opacity;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);

            Projectile.timeLeft += 10;
            Projectile.velocity = Projectile.DirectionFrom(target.Center) * Projectile.velocity.Length();
            Projectile.velocity *= .75f;
            Projectile.ai[1]++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);

            Projectile.timeLeft += 10;
            Projectile.velocity = Projectile.DirectionFrom(target.Center) * Projectile.velocity.Length();
            Projectile.velocity *= .75f;
            Projectile.ai[1]++;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            Projectile.velocity *= .75f;
            Projectile.timeLeft += 10;
            Projectile.ai[1]++;


            return Projectile.ai[1] > 3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            opacity = MathHelper.Lerp(opacity, 1, 0.2f);
            Projectile.rotation += Projectile.velocity.Length() / 50;
            Projectile.velocity *= .98f;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DeerclopsIceAttack, Projectile.Center);
            int numProj = 15;

            for (int i = 0; i < 5; i++)
            {
                int dustType = DustID.IceGolem;
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);

                dust.velocity.X += Main.rand.NextFloat(-0.1f, 0.1f);
                dust.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);

                dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
            }

            for (int i = 0; i < 5; i++)
            {
                int dustType = DustID.FrostStaff;
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);

                dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
                dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

                dust.scale *= 1.5f + Main.rand.NextFloat(-0.03f, 0.03f);
                dust.noGravity = true;
            }

            for (int i = 0; i < numProj; i++)
            {
                Vector2 newVelocity = new Vector2(12, 0).RotatedByRandom(MathHelper.ToRadians(12));

                newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, newVelocity.RotatedBy(Projectile.rotation - (i * MathHelper.TwoPi / numProj)), (int)Projectile.ai[0], (int)(Projectile.damage * .5f), Projectile.knockBack);
            }
        }

        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Color auroraColor = Systems.Utils.ColorLerpCycle(Main.GlobalTimeWrappedHourly, 2, new Color[] { new Color(40, 250, 60, 50), new Color(150, 70, 130, 50) });

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = auroraColor * opacity;
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f), Projectile.rotation + MathHelper.PiOver2, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, position + new Vector2(4 * opacity, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), sourceRectangle, auroraColor * .5f * opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, position, sourceRectangle, lightColor * opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
