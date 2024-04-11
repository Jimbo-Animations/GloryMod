using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System;
using Terraria.DataStructures;

namespace GloryMod.NPCs.IgnitedIdol
{
    internal class WrathfulEmbers : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 30;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 30;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            Projectile.ai[1]++;
            Projectile.localAI[0]++;
            Projectile.localAI[1]++;
            Projectile.velocity *= 0.95f;

            if (Projectile.ai[1] >= 6)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 6)
                {
                    Projectile.frame = 0;
                }
            }

            if (Projectile.localAI[0] >= 20 && Projectile.timeLeft <= 20)
            {
                Projectile.hostile = true;
            }
            else
            {
                Projectile.hostile = false;
            }

            if (Projectile.localAI[1] >= 600)
            {
                Projectile.Kill();
            }

            if (Main.rand.NextBool(30) && Projectile.hostile == true)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(0, Projectile.width / 2), 0).RotatedByRandom(MathHelper.TwoPi), 59, Projectile.DirectionFrom(Projectile.Center) * Main.rand.NextFloat(1, 2), 0, default, 1.5f);
                dust.noGravity = true;
            }

            //Makes the projectile die.
            if (owner.ai[2] != 1)
            {
                Projectile.timeLeft = 21;
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffType<FlamesOfRetribution>(), 150, true);
        }

        private float expand = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            if (Projectile.timeLeft < 20)
            {
                expand = MathHelper.Lerp(expand, 0, 0.05f);
            }
            else
            {
                expand = MathHelper.Lerp(expand, 1, 0.05f);
            }

            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            float projScale = expand * (float)(5 + Math.Sin(Projectile.localAI[0] / 3f)) / 5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 175, 250) * expand, 0, glow.Size() / 2 + new Vector2(2, -20 * projScale), 0.25f * projScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * expand, 0, drawOrigin, 1 * projScale, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
