using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BloodMoon.BloodDrone
{
    internal class DroneExplosion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 24;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

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
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= 4)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 7;
                }

                if (Projectile.frame > 1 & Projectile.frame < 6) Projectile.hostile = true;
                else Projectile.hostile = false;
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

        public override bool CanHitPlayer(Player target)
        {
            return target.Distance(Projectile.Center) <= Projectile.Hitbox.X / 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[2] > 0) 
            {
                CombatText.NewText(target.getRect(), Color.Red, "KABOOMM!!!", true);
                target.AddBuff(BuffID.Bleeding, 300, true);
            }
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
}
