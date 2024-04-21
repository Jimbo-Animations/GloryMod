using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Projectiles
{
    internal class BBPurpleRune : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(100);
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
        }

        public float visibility;
        float timer;
        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

            SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse, Projectile.position);
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 15 ? 0 : 1, 0.1f);
            Projectile.rotation += 0.033f;

            if (Projectile.ai[0] % 2 == 1)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 65, Scale: visibility * 2);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
            }

            if (Projectile.ai[0] >= 6)
            {
                Projectile.frame++;
                Projectile.ai[0] = 0;
                if (Projectile.frame >= 8)
                {
                    Projectile.frame = 0;
                }
            }

            if (Projectile.timeLeft == 15)
            {
                int numDusts = 30;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 65, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Center, Vector2.Zero, ProjectileType<DevilsScythe>(), Projectile.damage, 0, Projectile.owner);
                SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, Projectile.position);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D head = Request<Texture2D>(Texture + "Head").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * visibility, Projectile.rotation, drawOrigin, Projectile.scale * visibility, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(head, Projectile.Center - Main.screenPosition, frame, Color.White * visibility, 0, drawOrigin, Projectile.scale * visibility, SpriteEffects.None, 0);

            return false;
        }
    }
}
