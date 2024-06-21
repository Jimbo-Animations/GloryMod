using Terraria.DataStructures;
using Terraria.Audio;

namespace GloryMod.NPCs.Sightseer.Projectiles
{
    internal class HomingShot : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
        }

        float opacity;
        float acceleration;
        float timer;
        public override void OnSpawn(IEntitySource source)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
            Projectile.rotation = Projectile.ai[0] == 0 ? Projectile.DirectionTo(target.Center).ToRotation() : Projectile.velocity.ToRotation();

            if (Projectile.ai[2] == 1) Projectile.hostile = false;
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
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.ai[1]++;

            Projectile.velocity *= 0.98f;

            if (Projectile.ai[1] % 6 == 1)
            {
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            switch (Projectile.ai[0])
            {
                case 0:

                    if (Projectile.ai[1] == 30) SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, Projectile.Center);

                    if (Projectile.ai[1] >= 45)
                    {
                        acceleration = MathHelper.SmoothStep(acceleration, 1, 0.1f);
                        Projectile.velocity = new Vector2(20 * acceleration, 0).RotatedBy(Projectile.rotation);
                        Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.DirectionTo(target.Center).ToRotation(), acceleration * 0.015f);
                    }

                    break;

                case 1:

                    acceleration = MathHelper.SmoothStep(acceleration, 1, 0.1f);
                    Projectile.velocity = new Vector2(20 * acceleration, 0).RotatedBy(Projectile.rotation);
                    Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.DirectionTo(target.Center).ToRotation(), acceleration * 0.01f);

                    break;
            }

            opacity = Projectile.timeLeft <= 15 ? MathHelper.SmoothStep(opacity, 0, 0.2f) : MathHelper.SmoothStep(opacity, 1, 0.2f);
            if (Projectile.timeLeft <= 15) Projectile.damage = 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(20, 0, 30, 250)) * opacity;
                Vector2 trailSize = new Vector2(0.9f - i / (float)Projectile.oldPos.Length + (float)Math.Sin(timer) * 0.1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, color, Projectile.rotation + MathHelper.PiOver2, texture.Size() / 2, trailSize * opacity, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White * opacity, 0, origin, Projectile.scale * 1.25f + (float)Math.Sin(timer) * 0.1f, SpriteEffects.None, 0);

            texture = Request<Texture2D>(Texture + "Eye").Value;

            Vector2 eyePosition = Projectile.Center + new Vector2(MathHelper.Clamp(Projectile.Distance(target.Center) * 0.02f, 0, 10), 0).RotatedBy(Projectile.DirectionTo(target.Center).ToRotation());

            Main.EntitySpriteDraw(texture, eyePosition - Main.screenPosition, null, Projectile.ai[2] == 1 ? new Color(0, 100, 250) * opacity : Color.White * opacity, Projectile.DirectionTo(target.Center).ToRotation(), texture.Size() / 2, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.1f, SpriteEffects.None, 0);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, eyePosition - Main.screenPosition + new Vector2(4 - (2 * opacity), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), null, Projectile.ai[2] == 1 ? new Color(0, 100, 250) * opacity * 0.5f : new Color(47, 250, 255) * opacity * 0.5f,
                Projectile.DirectionTo(target.Center).ToRotation(), texture.Size() / 2, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.05f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
