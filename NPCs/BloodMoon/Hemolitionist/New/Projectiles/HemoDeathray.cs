using GloryMod.Systems;
using SteelSeries.GameSense.DeviceZone;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BloodMoon.Hemolitionist.New.Projectiles
{
    internal class NecroticRay : Deathray
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 100000;
        }

        public override string Texture => "GloryMod/NPCs/BloodMoon/Hemolitionist/New/Projectiles/NecroticRay";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 2;

            MoveDistance = 20f;
            RealMaxDistance = 3000f;
            headRect = new Rectangle(28, 0, 28, 12);
            bodyRect = new Rectangle(28, 12, 28, 12);           
            tailRect = new Rectangle(28, 24, 28, 12);
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 300, true);
        }

        public override void PostDraw(Color lightColor)
        {
            float projScale = .1f + (float)Math.Sin(Projectile.ai[1] / 33f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            DrawLaser(Main.spriteBatch, Request<Texture2D>(Texture).Value, Position(), Projectile.velocity, bodyRect.Height * projScale, -1.57f, projScale, MaxDistance, (int)MoveDistance);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

        }

        public override void PostAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            float projScale = .1f + (float)Math.Sin(Projectile.ai[1] / 33f);

            for (int i = 0; i < RealMaxDistance; i += 10)
            {
                if (Main.rand.NextBool(50))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.NextFloat(-3, 0), 0) + Vector2.UnitX.RotatedBy(Projectile.rotation) * i, 114, Vector2.UnitX.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(10, 20), 0, default, Main.rand.NextFloat(.6f, 1.1f) + projScale);
                    dust.noGravity = true;
                }
            }

            if (Projectile.ai[1] <= 10)
            {
                headRect = new Rectangle(0, 0, 28, 12);
                bodyRect = new Rectangle(0, 12, 28, 12);
                tailRect = new Rectangle(0, 24, 28, 12);
            }
            if (Projectile.ai[1] > 90)
            {
                headRect = new Rectangle(56, 0, 28, 12);
                bodyRect = new Rectangle(56, 12, 28, 12);
                tailRect = new Rectangle(56, 24, 28, 12);
            }
        }

        public override Vector2 Position()
        {
            return Main.npc[(int)Projectile.ai[0]].Center + new Vector2(0, 6).RotatedBy(Main.npc[(int)Projectile.ai[0]].rotation);
        }
    }
}
