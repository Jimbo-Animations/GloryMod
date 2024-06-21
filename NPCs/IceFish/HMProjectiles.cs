using System.Collections.Generic;
using Terraria.DataStructures;

namespace GloryMod.NPCs.IceFish
{
    internal class HMShockwave : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/Glow_1";

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DD2OgreSmash);
            AIType = ProjectileID.DD2OgreSmash;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.velocity.Y == 0;
        }
    }

    internal class BlueRoar : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/PulseCircle";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 100000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
        }

        Player target;
        public override void OnSpawn(IEntitySource source)
        {
            flash.Add(new Tuple<Vector2, float, float>(Projectile.Center, 0f, 75f));
        }

        public override void AI()
        {
            target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            visibility = MathHelper.SmoothStep(visibility, 0.1f, .2f);
        }

        float visibility = .5f;
        List<Tuple<Vector2, float, float>> flash = new List<Tuple<Vector2, float, float>>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D flashTexture = Request<Texture2D>(Texture).Value;

            for (int i = 0; i < flash.Count; i++)
            {
                if (i >= flash.Count)
                {
                    break;
                }

                flash[i] = new Tuple<Vector2, float, float>(flash[i].Item1, flash[i].Item2 + flash[i].Item3, flash[i].Item3);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                Main.EntitySpriteDraw(flashTexture, flash[i].Item1 - Main.screenPosition, null, Color.LightBlue * visibility, 0, flashTexture.Size() / 2, flash[i].Item2 / flashTexture.Width, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                if (flash[i].Item2 >= target.Distance(flash[i].Item1) + Main.screenWidth * 3)
                {
                    flash.RemoveAt(i);
                }
            }

            return false;
        }
    }
}
