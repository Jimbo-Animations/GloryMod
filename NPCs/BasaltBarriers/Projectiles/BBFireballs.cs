using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Projectiles
{
    internal class BBFireBallSmall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.Distance(target.Center) <= 15 && target.friendly;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 15;
        }

        public override void AI()
        {
            if (Projectile.lavaWet) Projectile.Kill();
            Projectile.tileCollide = Projectile.velocity.Y > 0;

            Projectile.ai[0]++;
            Projectile.ai[1]++;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.99f;

            if (Projectile.ai[1] >= 4)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 0;
                }
            }

            if (Projectile.ai[0] > 30)
            {
                Projectile.velocity.Y += .2f;
            }

            if (Projectile.ai[0] % 3 == 1)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, Scale: 2);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
            }
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Top + new Vector2 (0, 4), Vector2.Zero, ProjectileType<BBExplosionSmall>(), Projectile.damage, 0, Projectile.owner);
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(200, 150 - i, 50 - i, 50 - (i * 2)) * visibility, Projectile.oldRot[i] + MathHelper.PiOver2, trail.Size() / 2, new Vector2(Projectile.scale * 1.25f - (i / (float)Projectile.oldPos.Length), Projectile.scale * .75f - (i / (float)Projectile.oldPos.Length)), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, Projectile.rotation, drawOrigin + new Vector2(frame.Width * .15f, 0), Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 150, 50, 50) * visibility, Projectile.rotation, glow.Size() / 2 - new Vector2(glow.Width * .2f, 0), Projectile.scale * .3f, SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }

    internal class BBFireBallMedium : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(54);
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }


        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.Distance(target.Center) <= 27 && target.friendly;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 27;
        }

        public override void AI()
        {
            if (Projectile.lavaWet) Projectile.Kill();
            Projectile.tileCollide = Projectile.velocity.Y > 0;

            Projectile.ai[0]++;
            Projectile.ai[1]++;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.99f;

            if (Projectile.ai[1] >= 4)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 0;
                }
            }

            if (Projectile.ai[0] > 30)
            {
                Projectile.velocity.Y += .2f;
            }

            if (Projectile.ai[0] % 3 == 1)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, Scale: 3);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
            }
        }


        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Top - new Vector2(0, 12), Vector2.Zero, ProjectileType<BBExplosionMedium>(), Projectile.damage, 0, Projectile.owner);
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(200, 150 - (i * 2), 50 - i, 50 - (i * 2)) * visibility, Projectile.oldRot[i] + MathHelper.PiOver2, trail.Size() / 2, new Vector2(Projectile.scale * 1.67f - (i / (float)Projectile.oldPos.Length), Projectile.scale * 1f - (i / (float)Projectile.oldPos.Length)), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, Projectile.rotation, drawOrigin + new Vector2(frame.Width * .15f, 0), Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 150, 50, 50) * visibility, Projectile.rotation, glow.Size() / 2 - new Vector2(glow.Width * .15f, 0), Projectile.scale * .5f, SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }

    internal class BBFireBallLarge : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(86);
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }


        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.Distance(target.Center) <= 43 && target.friendly;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 43;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 70;
            height = 70;
            fallThrough = false;

            return true;
        }

        public override void AI()
        {
            if (Projectile.lavaWet) Projectile.Kill();
            Projectile.tileCollide = Projectile.velocity.Y > 0;

            Projectile.ai[0]++;
            Projectile.ai[1]++;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.995f;

            if (Projectile.ai[1] >= 4)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 0;
                }
            }

            if (Projectile.ai[0] > 30)
            {
                Projectile.velocity.Y += .2f;
            }

            if (Projectile.ai[0] % 2 == 1)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, Scale: 3);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
            }
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(200, 150 - (i * 3), 50 - i, 60 - (i * 2)) * visibility, Projectile.oldRot[i] + MathHelper.PiOver2, trail.Size() / 2, new Vector2(Projectile.scale * 2.25f - (i * 1.5f / (float)Projectile.oldPos.Length), Projectile.scale * 1.5f - (i * 1.25f / (float)Projectile.oldPos.Length)), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, Projectile.rotation, drawOrigin + new Vector2(frame.Width * .2f, 0), Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center + new Vector2(0, 5).RotatedBy(Projectile.rotation) - Main.screenPosition, null, new Color(200, 150, 50, 50) * visibility, Projectile.rotation, glow.Size() / 2 - new Vector2(glow.Width * .1f, 0), Projectile.scale * 0.75f, SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }
}
