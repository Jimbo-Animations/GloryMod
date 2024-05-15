using Terraria.DataStructures;
using Terraria.Audio;

namespace GloryMod.NPCs.Sightseer.Projectiles
{
    internal class SightseerAnomaly : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(70);
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 450;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
        }

        float opacity;
        float eyeOpacity;
        float timer;
        Vector2 randomizer;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

            randomizer = new Vector2(Main.rand.NextBool() ? 1 : -1, 0);
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
            Projectile.ai[0]++;

            if (Projectile.ai[0] % 5 == 1)
            {
                Projectile.frame++;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 0;
                }
            }

            Vector2 velocity = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
            int dust = Dust.NewDust(Projectile.Center + (velocity * Main.rand.NextFloat(200, 221) * opacity), 0, 0, 33, Scale: 2f * opacity);
            Main.dust[dust].noGravity = false;
            Main.dust[dust].noLight = true;
            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-8, -12) * opacity, 0).RotatedBy(velocity.ToRotation());

            if ((Projectile.ai[0] - 60) % Projectile.ai[1] == 1 && Projectile.timeLeft > 90)
            {
                Projectile.hostile = true;

                for (int i = 0; i < 16; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + randomizer.RotatedBy(MathHelper.TwoPi * i / 16), randomizer.RotatedBy(MathHelper.TwoPi * i / 16), ProjectileType<WigglyShot>(), 75, 0, Projectile.owner, 0, i % 2 == 1 ? 1 : 0, Main.rand.NextBool() ? -1 : 1);
                }

                SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), Projectile.Center);
                randomizer = randomizer.RotatedBy(randomizer.ToRotation() + 0.33f);
            }

            opacity = Projectile.timeLeft > 60 ? MathHelper.SmoothStep(opacity, 1, 0.15f) : MathHelper.SmoothStep(opacity, 0, 0.15f);
            eyeOpacity = Projectile.timeLeft > 60 && Projectile.ai[0] > 60 ? MathHelper.SmoothStep(eyeOpacity, 1, 0.25f) : MathHelper.SmoothStep(eyeOpacity, 0, 0.25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/Vortex").Value;

            int frameHeight;
            int frameY;
            Rectangle sourceRectangle;
            Vector2 origin;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(0, 100, 250) * opacity * 0.5f, Main.GameUpdateCount * 0.025f, texture.Size() / 2, Projectile.scale * opacity * 2, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(0, 100, 250) * opacity, Main.GameUpdateCount * 0.05f, texture.Size() / 2, Projectile.scale * opacity, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            texture = Request<Texture2D>(Texture).Value;
            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White * opacity, Projectile.rotation, origin, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.05f, SpriteEffects.None, 0);

            texture = Request<Texture2D>(Texture + "Mask").Value;
            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(4 - (2 * opacity), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), sourceRectangle, new Color(47, 250, 255) * opacity * 0.5f,
                Projectile.rotation, origin, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.05f, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>("GloryMod/NPCs/Sightseer/Projectiles/WigglyShot").Value;
            Vector2 eyePosition = Projectile.Center + new Vector2(MathHelper.Clamp(Projectile.Distance(target.Center) * 0.02f, 0, 10), 0).RotatedBy(Projectile.DirectionTo(target.Center).ToRotation());

            Main.EntitySpriteDraw(texture, eyePosition - Main.screenPosition, null, Color.White * eyeOpacity, 0, texture.Size() / 2f, new Vector2(1, eyeOpacity), SpriteEffects.None, 0);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, eyePosition - Main.screenPosition + new Vector2(4 - (2 * eyeOpacity), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), null, new Color(47, 250, 255) * eyeOpacity * 0.5f,
                0, texture.Size() / 2f, new Vector2 (1, eyeOpacity), SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
