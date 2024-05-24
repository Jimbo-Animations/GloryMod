using System.Collections.Generic;
using GloryMod.Systems;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Projectiles
{
    class BasaltBarrierFireWallCore : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 100000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 434;
            Projectile.height = 368;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 340;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        private int frameX = 0;
        private int wallFrameY;
        private int frameTimer;

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.ai[1]++;

            if (Projectile.ai[0] >= 5)
            {
                Projectile.frame++;
                frameTimer++;
                Projectile.ai[0] = 0;

                if (frameTimer == 8) SoundEngine.PlaySound(SoundID.Zombie104 with { Pitch = -.25f }, Projectile.Center);

                if (frameTimer > 8) 
                {
                    wallFrameY++;

                    if (frameTimer <= 16)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (ScreenUtils.screenShaking < 2) ScreenUtils.screenShaking = 2;

                            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y - (1720 * (frameTimer - 8) / 8)), new Vector2(1 * i > 0 ? 1 : -1, 0), ProjectileType<BBSpiritBolt>(), 75, 0, Projectile.owner);
                            Main.projectile[proj].ai[1] = Main.rand.NextFloat(-3, 4);
                            Main.projectile[proj].ai[2] = Main.rand.NextFloat(6, 9);
                        }
                    }
                }

                if (wallFrameY >= 14) wallFrameY = 6;

                if ((frameX == 1 && Projectile.frame >= 6) || Projectile.frame >= 8)
                {
                    if (frameX < 2) frameX++;
                    Projectile.frame = 0;
                }
            }

            if (Projectile.timeLeft > 60 && frameX > 0) Projectile.hostile = true;
            else Projectile.hostile = false;

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft > 60 ? 1 : 0, Projectile.timeLeft > 60 ? .125f : .1f);
            width = MathHelper.Lerp(width, Projectile.timeLeft > 60 && frameTimer >= 8 ? 1 : 0, Projectile.timeLeft > 60 ? .125f : .1f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Rectangle beamBox = new((int)Projectile.position.X + 8 + Projectile.width / 3, (int)Projectile.position.Y - 1720, 140, 2088);
            if (targetHitbox.Intersects(beamBox))
                return true;
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = new Rectangle((int)Projectile.position.X + 62, (int)Projectile.position.Y + (Projectile.height / 2), Projectile.width - 124, Projectile.height / 2);
        }

        float visibility = 0;
        float width = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = new(texture.Width / 3 * frameX, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width / 3, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new(texture.Width / 3 * 0.5f, Projectile.height * 0.5f);

            Texture2D wall = Request<Texture2D>("GloryMod/NPCs/BasaltBarriers/Projectiles/BasaltBarrierFireWall").Value;
            Rectangle wallFrame = new(0, wall.Height / 14 * wallFrameY, wall.Width, wall.Height / 14);
            Vector2 wallOrigin = new(wall.Width * 0.5f, wall.Height / 28);

            Texture2D deathray = Request<Texture2D>("GloryMod/CoolEffects/Textures/Deathray").Value;
            Vector2 rayOrigin = new(deathray.Width * 0.5f, deathray.Height);

            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * visibility, 0, drawOrigin, new Vector2(Projectile.timeLeft > 60 ? 1 : width, 1), SpriteEffects.None, 0);

            if (frameTimer >= 8)
            {
                for (int i = 0; i < 10; i++)
                {
                    Main.spriteBatch.Draw(wall, Projectile.Top - new Vector2(-1, 86 + (172 * i)) - Main.screenPosition, wallFrame, Color.White * visibility, 0, wallOrigin, new Vector2(Projectile.timeLeft > 60 ? 1 : width, 1), SpriteEffects.None, 0);
                }           
            }

            float mult = 0.95f + ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) * .05f);

            Main.spriteBatch.Draw(deathray, Projectile.Bottom + new Vector2(1, -8) - Main.screenPosition, null, new Color (250, 250, 200) * visibility * .25f, 0, rayOrigin, new Vector2((mult * width) + (6 * width), 2080), SpriteEffects.None, 0);
            Main.spriteBatch.Draw(glow, Projectile.Bottom - new Vector2(0, 32) - Main.screenPosition, null, new Color(250, 250, 200) * visibility * .25f, 0, glow.Size() / 2, new Vector2(3.5f * width, 1), SpriteEffects.None, 0);

            return false;
        }
    }
}
