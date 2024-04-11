using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Projectiles
{
    internal class BBDebris : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
        }

        float spawnY;
        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

            Projectile.frame = Main.rand.Next(0, 3) % Main.projFrames[Projectile.type];
            Projectile.scale = Main.rand.NextFloat(.75f, 1.25f);

            spawnY = Projectile.position.Y;
        }

        public override void AI()
        {
            Projectile.tileCollide = Projectile.position.Y > spawnY + 160;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.1f;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = .5f }, Projectile.position);

            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.Center + new Vector2(Main.rand.NextFloat(10, -10), Main.rand.NextFloat(10, -10)), 0, 0, 36, Scale: 2f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi);
            }

            for (int i = 0; i < 5; i++)
            {
                int gore = Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(3), 0).RotatedByRandom(MathHelper.TwoPi), 99, 2);
                Main.gore[gore].rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, lightColor * (.9f - i / (float)Projectile.oldPos.Length), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
