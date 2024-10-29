using System.Collections.Generic;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace GloryMod.Items.BloodMoon.Hemolitionist
{
    class ClawBuster : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;

            Item.noMelee = true;
            Item.damage = 80;
            Item.knockBack = 5;

            Item.Size = new Vector2(22);
            Item.useTime = Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ProjectileType<ClawBusterProj>();
            Item.shootSpeed = 1;
            Item.UseSound = SoundID.Zombie72;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Pink;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = new TooltipLine(Mod, "Tooltip#0", "Uses your blood as ammo!")
            {
                OverrideColor = new Color(255, 0, 0)
            };
            tooltips.Add(line);
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
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false;
        }
    }

    class ClawBusterProj : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        float timer;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(62, 34);
            Projectile.scale = 1;
            Projectile.alpha = 0;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 60;
        }

        int duration;

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.spriteDirection = Main.MouseWorld.X > player.MountedCenter.X ? 1 : -1;

            duration = player.itemAnimationMax;
            Projectile.timeLeft = duration;
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

            Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], Projectile.timeLeft > duration * .6 && Projectile.timeLeft < duration * .7f ? MathHelper.ToRadians(40f * -Projectile.direction) : 0, Projectile.timeLeft > duration / 2 ? .33f : .1f);
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.timeLeft > duration * .55 && Projectile.timeLeft < duration * .7f ? 8 : 0, .1f);
            Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.ai[1];

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            Vector2 armPosition = player.Center;

            armPosition.Y += player.gfxOffY;
            Projectile.Center = armPosition;

            if (Projectile.frameCounter++ > 2 && Projectile.timeLeft >= 15)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame > 4 && Projectile.timeLeft > 30) Projectile.frame = 4;
                if (Projectile.frame >= 7) Projectile.frame = 0;
            }

            if (Projectile.timeLeft == 40)
            {
                SoundEngine.PlaySound(SoundID.Item125, Projectile.Center);
                if (player.statLife <= 40) player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " drew too much blood"), 40 + (int)(40 * player.endurance), 0, quiet: true, dodgeable: false, armorPenetration: 1000);
                else
                {
                    CombatText.NewText(player.getRect(), Color.Red, -40, false, true);
                    player.statLife -= 40;
                    player.lifeRegenTime = 0;
                    player.AddBuff(BuffType<ClawBusterBuff>(), 480);

                    for (int i = -2; i < 3; i++)
                    {
                        var proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(22 - Projectile.ai[2], -8 * Projectile.spriteDirection).RotatedBy(Projectile.velocity.ToRotation()), new Vector2(20, 0).RotatedBy((i * MathHelper.ToRadians(20) / 5) + Projectile.velocity.ToRotation()), ProjectileType<HemoBlastFriendly>(), Projectile.damage, 0, player.whoAmI, i);
                        Main.projectile[proj].ai[0] = i;
                    }

                    int numDusts = 10;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(Projectile.Center + new Vector2(22 - Projectile.ai[2], -8 * Projectile.spriteDirection).RotatedBy(Projectile.velocity.ToRotation()), 0, 0, 114, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(6), 0).RotatedBy((i * MathHelper.Pi / numDusts) + Projectile.velocity.ToRotation());
                    }
                }
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
            Texture2D mask1 = Request<Texture2D>(Texture + "Mask1").Value;
            Texture2D mask2 = Request<Texture2D>(Texture + "Mask2").Value;
            Vector2 origin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            SpriteEffects effects;

            if (Projectile.spriteDirection > 0) effects = SpriteEffects.None;
            else effects = SpriteEffects.FlipVertically;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(22 - Projectile.ai[2], -8 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), frame, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(mask1, Projectile.Center - Main.screenPosition + new Vector2(22 - Projectile.ai[2], -8 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), frame, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(mask2, Projectile.Center - Main.screenPosition + new Vector2(22 - Projectile.ai[2], -8 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), frame, Color.White * .25f, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }

    class HemoBlastFriendly : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/BloodMoon/Hemolitionist/New/Projectiles/HemoBlast";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(32);
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
            Projectile.penetrate = 1;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, 4);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.timeLeft < 40) Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] * -.0075f);
            if (Projectile.timeLeft < 35) Projectile.velocity *= .95f;     

            if (Projectile.frameCounter++ > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, 0.1f);
            if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Projectile.Center, 266, new Vector2(Main.rand.NextFloat(3), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.ToRadians(90)), Scale: 2);
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<HemoExplosionFriendly>(), Projectile.damage, 0, Projectile.owner);
        }

        float visibility;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_7").Value;

            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / 8);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255, 30, 15, 255) * visibility, Projectile.oldRot[i] + MathHelper.PiOver2, trail.Size() / 2, new Vector2(visibility + .5f - (i / (float)Projectile.oldPos.Length), visibility - i / (float)Projectile.oldPos.Length * .75f * (1 + (Projectile.velocity.ToRotation() * .05f))), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(255, 30, 15, 255) * visibility, Projectile.rotation, glow.Size() / 2, new Vector2(visibility, visibility), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * visibility, Projectile.rotation, drawOrigin, new Vector2(visibility, visibility), SpriteEffects.None, 0);

            return false;
        }
    }

    class HemoExplosionFriendly : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/BloodMoon/BloodDrone/DroneExplosion";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 24;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            int numDusts = 30;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 266, Scale: 3f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }
        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.frameCounter++ >= 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0; 
                if (Projectile.frame >= 7) Projectile.frame = 7;

                if (Projectile.frame > 1 & Projectile.frame < 6) Projectile.friendly = true;
                else Projectile.friendly = false;
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, 0.1f);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Rectangle result = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
            int num = (int)Terraria.Utils.Remap(Projectile.ai[0] * 4, 0, 200, 10, 40);
            result.Inflate(num, num);
            hitbox = result;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return target.Distance(Projectile.Center) <= Projectile.Hitbox.X / 2;
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale * (Projectile.ai[0] * 0.03f + 1), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(250, 100, 100, 100) * visibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.07f + 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }

    class ClawBusterBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false; // Causes this buff not to persist when exiting and rejoining the world
        }

        // Allows you to make this buff give certain effects to the given player
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ClawBusterPlayer>().Healing = true;
        }
    }

    class ClawBusterPlayer : ModPlayer
    {
        // Flag checking when life regen debuff should be activated
        public bool Healing;

        public override void ResetEffects()
        {
            Healing = false;
        }

        public override void PostUpdate()
        {
            if (Healing)
            {
                if (Main.rand.NextBool()) Main.dust[Dust.NewDust(Player.position, Player.width, Player.height, 5, Scale: 1f)].noGravity = true;
            }
        }

        public override void UpdateLifeRegen()
        {
            if (Healing)
            {
                Player.lifeRegen += 10;
                if (Player.lifeRegenTime <= 0) Player.lifeRegenTime = 1;
            }
        }
    }
}

