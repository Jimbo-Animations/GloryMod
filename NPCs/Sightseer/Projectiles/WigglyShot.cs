using Terraria.DataStructures;
using Terraria.Audio;

namespace GloryMod.NPCs.Sightseer.Projectiles
{
    internal class WigglyShot : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
        }

        private Vector2 startPosition;
        private Vector2 startVelocity;
        private Vector2 goalPosition;
        float opacity;
        float accelerate = 1;
        public override void OnSpawn(IEntitySource source)
        {
            startPosition = Projectile.Center;
            startVelocity = Projectile.velocity;
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (!target.HasBuff(BuffType<SeersTag>()))
            {
                CombatText.NewText(target.getRect(), new Color(255, 186, 101), "Tagged!", true, false);
                SoundEngine.PlaySound(SoundID.Zombie105, target.Center);
            }

            target.AddBuff(BuffType<SeersTag>(), 600, true);
        }

        public override void AI()
        {
            switch (Projectile.ai[0])
            {
                case 0:

                    if (Projectile.localAI[0] > 30) accelerate = MathHelper.Lerp(accelerate, 7f, 0.02f);
                    goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * (2f * accelerate) * Projectile.localAI[0] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 10f * (float)Math.Sin(Projectile.ai[2] + Projectile.localAI[0] / 5f) * (float)Math.Sin(MathHelper.Pi * Projectile.localAI[0] / 50f);

                    break;

                case 1:

                    goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * 10 * Projectile.localAI[0] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 20f * (float)Math.Sin(Projectile.ai[2] + Projectile.localAI[0] / 5f) * (float)Math.Sin(MathHelper.Pi * Projectile.localAI[0] / 70f);

                    break;
            }
            if (Projectile.ai[1] == 1)
            {
                Projectile.hostile = false;
            }

            Projectile.velocity = goalPosition - Projectile.Center;
            Projectile.localAI[0]++;
            opacity = Projectile.timeLeft <= 15 ? MathHelper.SmoothStep(opacity, 0, 0.2f) : MathHelper.SmoothStep(opacity, 1, 0.2f);
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(20, 0, 30, 250)) * opacity;
                Vector2 trailSize = new Vector2(0.9f - i / (float)Projectile.oldPos.Length + (float)Math.Sin(timer) * 0.1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color, Projectile.rotation + MathHelper.PiOver2, origin, trailSize * opacity, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>(Texture).Value;

            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, Projectile.oldPos[1] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, Projectile.ai[1] == 1 ? new Color(0, 100, 250) * opacity : Color.White * opacity, Projectile.rotation, origin, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.1f, SpriteEffects.None, 0);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[1] - Projectile.position + Projectile.Center - Main.screenPosition + new Vector2(4 - (2 * opacity), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), sourceRectangle, Projectile.ai[1] == 1 ? new Color(0, 100, 250) * opacity * 0.5f : new Color(47, 250, 255) * opacity * 0.5f,
                Projectile.rotation, origin, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.05f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
