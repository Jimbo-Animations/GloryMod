using System.Collections.Generic;
using System.IO;
using GloryMod.Systems;
using MonoMod.RuntimeDetour;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.Items.BasaltBarriers
{
    internal class ExtinctionHeater : ModItem
    {
        private int timer = 0;
        private int shotsBeforeReload = 2;

        public override void SetDefaults()
        {
            Item.Size = new Vector2(68, 24);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = new SoundStyle("GloryMod/Music/Shotgun");
            Item.value = Item.sellPrice(gold: 10);

            Item.DefaultToRangedWeapon(ProjectileID.PurificationPowder, AmmoID.Bullet, 30, 10f, true);
            Item.SetWeaponValues(22, 6);
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = new TooltipLine(Mod, "Tooltip#0", "Rip and tear until it is done...")
            {
                OverrideColor = new Color(250, 100, 50)
            };
            tooltips.Add(line);
        }

        public override void UpdateInventory(Player player)
        {
            if (timer++ == 120)
            {
                SoundEngine.PlaySound(SoundID.Item149, player.Center);

                for (int i = 0; i < 24; i++)
                {
                    int dust = Dust.NewDust(player.Center, 0, 0, DustID.MartianSaucerSpark, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / 24);
                }

                shotsBeforeReload = 2;
            }
        }

        public override bool NeedsAmmo(Player player)
        {
            return player.altFunctionUse != 2;
        }

        public override void UseAnimation(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.DefaultToRangedWeapon(ProjectileType<ExtinctionHeaterHook>(), AmmoID.None, 20, 20f, true);
                Item.SetWeaponValues(20, 1);
                Item.UseSound = SoundID.Item99;
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
            else
            {
                Item.DefaultToRangedWeapon(ProjectileID.PurificationPowder, AmmoID.Bullet, 30, 10f, true);
                Item.SetWeaponValues(22, 6);
                Item.UseSound = new SoundStyle("GloryMod/Music/Shotgun");
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) return player.ownedProjectileCounts[ProjectileType<ExtinctionHeaterHook>()] < 1;
            else return shotsBeforeReload > 0;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
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
            const int NumProjectiles = 8;
           
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            else
            {
                timer = 0;
                shotsBeforeReload--;
                ScreenUtils.screenShaking += 1;

                Projectile.NewProjectileDirect(source, position, velocity, ProjectileType<ExtinctionHeaterProj>(), damage, knockback, player.whoAmI);

                for (int i = 0; i < NumProjectiles; i++)
                {
                    Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(12));

                    newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                    Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
                }
            }

            return false;
        }
    }

    class ExtinctionHeaterProj : ModProjectile
    {
        public override string Texture => "GloryMod/Items/BasaltBarriers/ExtinctionHeater";

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(68, 24);
            Projectile.scale = 1;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 30;
        }

        int duration;
        int frameCounter;

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.spriteDirection = Main.MouseWorld.X > player.MountedCenter.X ? 1 : -1;

            duration = player.itemAnimationMax;
            Projectile.timeLeft = Projectile.ai[0] != 0 ? 600 : duration;
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
            Player player = Main.player[Projectile.owner];
            Projectile.velocity = Vector2.Normalize(Projectile.velocity);
            player.heldProj = Projectile.whoAmI;

            // Controls the movement of the gun.

            Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], Projectile.timeLeft > duration * .45 && Projectile.timeLeft < duration * .9f ? MathHelper.ToRadians(40f * -Projectile.direction) : 0, Projectile.timeLeft > duration / 2 ? .33f : .1f);
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.timeLeft > duration * .6 && Projectile.timeLeft < duration * .9f ? 8 : 0, .1f);
            Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.ai[1];

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            Vector2 armPosition = player.Center;

            armPosition.Y += player.gfxOffY;
            Projectile.Center = armPosition;

            if (frameCounter++ >= 3)
            {
                Projectile.frame++;
            }

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D flash = Request<Texture2D>(Texture + "Flash").Value;
            Vector2 origin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            SpriteEffects effects;

            int frameHeight = flash.Height / 4;
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, flash.Width, frameHeight);

            if (Projectile.spriteDirection > 0) effects = SpriteEffects.None;
            else effects = SpriteEffects.FlipVertically;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(32 - Projectile.ai[2], 0).RotatedBy(Projectile.rotation), default, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            origin = new Vector2(flash.Width * 0.5f, frameHeight * 0.5f);

            Main.spriteBatch.Draw(flash, Projectile.Center - Main.screenPosition + new Vector2(80 - Projectile.ai[2], -4 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), sourceRectangle, Color.White, Projectile.rotation, origin, Projectile.scale / 2, effects, 0);

            return false;
        }
    }

    class ExtinctionHeaterHook : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.Size = new Vector2(14, 14);
            Projectile.scale = 1;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 600;
        }

        NPC hitTarget;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 mountedCenter2 = Main.player[Projectile.owner].MountedCenter + new Vector2(0, player.gfxOffY);
            player.heldProj = Projectile.whoAmI;
            int goalDirectionX = player.Center.X < Projectile.Center.X ? 1 : -1;
            Projectile.spriteDirection = goalDirectionX;

            player.direction = goalDirectionX;
            player.heldProj = Projectile.whoAmI;

            // Extend use animation until projectile is killed

            if (Projectile.ai[0] != 2)
            {
                player.itemAnimation = 2;
                player.itemTime = 2;
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            Projectile.rotation = Projectile.DirectionFrom(mountedCenter2).ToRotation();

            if (Projectile.ai[2]++ % 3 == 1)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, Scale: 2);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
            }

            switch (Projectile.ai[0])
            {
                case 0:

                    Projectile.friendly = true;
                    if (Projectile.Distance(mountedCenter2) >= 300) Projectile.ai[0] = 2;

                    break;

                case 1:

                    Projectile.ai[1]++;

                    Vector2 hookOffset = new Vector2(-12, 0).RotatedBy(Projectile.rotation);

                    Projectile.position = (hitTarget.Center + hookOffset) - (Projectile.Size / 2);

                    if (hitTarget.active == false || hitTarget.life <= 0) Projectile.ai[0] = 2;

                    if (Projectile.Distance(mountedCenter2) >= 350 || Projectile.ai[1] > 60) 
                    {
                        Projectile.ai[0] = 2;
                        Projectile.velocity += Projectile.DirectionTo(mountedCenter2) * 15f;
                        if (hitTarget.knockBackResist > 0) hitTarget.velocity += Projectile.DirectionTo(mountedCenter2) * 20f * hitTarget.knockBackResist;
                        else player.velocity += player.DirectionTo(Projectile.Center) * 15f;
                    }

                    break;

                case 2:

                    Projectile.friendly = false;
                    Projectile.velocity += Projectile.DirectionTo(mountedCenter2) * 2f;

                    Projectile.velocity *= 0.9f;
                    if (Projectile.Distance(mountedCenter2) <= 16) Projectile.active = false;

                    break;
            }

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0] = 1;

            target.AddBuff(BuffID.OnFire3, 60);

            hitTarget = target;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D ChainTexture = Request<Texture2D>("GloryMod/Items/BasaltBarriers/ExtinctionHeaterChain").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            SpriteEffects effects = new SpriteEffects();
            SpriteEffects effects2 = new SpriteEffects();
            if (Projectile.spriteDirection == 1) effects = SpriteEffects.FlipVertically;
            else effects2 = SpriteEffects.FlipVertically;

            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter+ new Vector2(0, Main.player[Projectile.owner].gfxOffY) + new Vector2(8, 0).RotatedBy(Projectile.rotation);
            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation();
            float distance = distToProj.Length();
            while (distance > 14f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 14f;                      //speed = 24
                center += distToProj;                   //update draw position
                distToProj = playerCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = Lighting.GetColor((int)center.X / 16, (int)center.Y / 16);

                //Draw chain
                Main.spriteBatch.Draw(ChainTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 14, 10), drawColor, projRotation,
                    new Vector2(14 * 0.5f, 10 * 0.5f), 1f, effects, 0f);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, effects2, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color (250, 150, 100, 50), Projectile.rotation, drawOrigin, Projectile.scale * 1.25f, effects2, 0);

            return false;
        }
    }
}
