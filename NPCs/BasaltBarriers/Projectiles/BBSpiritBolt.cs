using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Projectiles
{
    internal class BBSpiritBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(54);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 360;
            Projectile.alpha = 0;
        }

        private Vector2 startPosition;
        private Vector2 startVelocity;
        private Vector2 goalPosition;
        float opacity;

        public override void OnSpawn(IEntitySource source)
        {
            startPosition = Projectile.Center;
            startVelocity = Projectile.velocity;

            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 27;
        }

        public override void AI()
        {
            opacity = MathHelper.SmoothStep(opacity, Projectile.timeLeft <= 10 ? 0 : 1, 0.2f);
            Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;

            goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * Projectile.ai[2] * Projectile.localAI[0] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 20f * (float)Math.Sin(Projectile.ai[1] + Projectile.localAI[0] / 5f) * (float)Math.Sin(MathHelper.Pi * Projectile.localAI[0] / 50f);
            Projectile.localAI[0]++;
            Projectile.ai[0]++;

            if (Projectile.ai[0] >= 4)
            {
                Projectile.frame++;
                Projectile.ai[0] = 0;

                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, Scale: 1);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;

                if (Projectile.frame >= 8)
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.velocity = goalPosition - Projectile.Center;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            SpriteEffects effects = SpriteEffects.None;
            if (Projectile.spriteDirection == 1) effects = SpriteEffects.FlipVertically;

            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(200, 150 - i, 50 - (i * 2), 50 - (i * 2)) * opacity, Projectile.oldRot[i] + MathHelper.PiOver2, trail.Size() / 2, new Vector2(Projectile.scale * 1.25f - (i / (float)Projectile.oldPos.Length), Projectile.scale * .75f - (i / (float)Projectile.oldPos.Length)), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * opacity, Projectile.rotation, drawOrigin - new Vector2(texture.Width * opacity * .25f, 0), Projectile.scale * opacity, effects, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(170, 50, 130, 50) * opacity, Projectile.rotation, glow.Size() / 2, Projectile.scale * opacity * .4f, SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }
}
