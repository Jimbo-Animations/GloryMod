using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Projectiles
{
    internal class DevilsScythe : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(74);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
        }

        public float visibility;
        Player target;

        public override void OnSpawn(IEntitySource source)
        {
            target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];

            Projectile.spriteDirection = Projectile.direction = target.Center.X > Projectile.Center.X ? 1 : -1;

            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.Distance(target.Center) <= 37 && target.friendly;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 37;
        }


        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] % 2 == 1)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 65, Scale: visibility * 3);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
            }

            if (Projectile.ai[0] % 5 == 1)
            {
                Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 37 * visibility).RotatedBy(Projectile.velocity.ToRotation()), 65, new Vector2(0, 2 * visibility).RotatedBy(Projectile.velocity.ToRotation()), Scale: visibility);
                Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -37 * visibility).RotatedBy(Projectile.velocity.ToRotation()), 65, new Vector2(0, -2 * visibility).RotatedBy(Projectile.velocity.ToRotation()), Scale: visibility);
            }

            target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            visibility = MathHelper.SmoothStep(visibility, 1, 0.15f);      

            int age = 300 - Projectile.timeLeft;
            float homingFactor = 1 / (float)Math.Max(age * 2 / 6, 75 - age);

            float velocityFactor = Math.Min(1 + age / 7.5f, 75f);

            Vector2 goalVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * velocityFactor;
            Projectile.velocity += (goalVelocity - Projectile.velocity) * homingFactor;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * velocityFactor;

            Projectile.rotation += velocityFactor * 0.033f * Projectile.spriteDirection;
            Projectile.scale = visibility;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            SpriteEffects effects = SpriteEffects.None;
            if (Projectile.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255 / i, 255 / i, 255 / i, 255 / (i + 1)) * visibility, Projectile.oldRot[i], drawOrigin, Projectile.scale * visibility, effects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * visibility, Projectile.rotation, drawOrigin, Projectile.scale * visibility, effects, 0);          

            return false;
        }
    }
}
