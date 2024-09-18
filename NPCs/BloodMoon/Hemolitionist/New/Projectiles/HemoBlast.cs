using GloryMod.NPCs.BloodMoon.BloodDrone;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BloodMoon.Hemolitionist.New.Projectiles
{
    internal class HemoBlast : ModProjectile
    {
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
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

            Projectile.frame = Main.rand.Next(0, 4);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.timeLeft < 45) Projectile.velocity *= .95f;

            if (Projectile.ai[0]++ > 4)
            {
                Projectile.frame++;
                Projectile.ai[0] = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, 0.1f);
            if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Projectile.Center, 266, new Vector2(Main.rand.NextFloat(3), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.ToRadians(90)), Scale: 2);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), 150, 0, Projectile.owner);
            target.AddBuff(BuffID.Bleeding, 300, true);
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), 150, 0, Projectile.owner);
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
}
