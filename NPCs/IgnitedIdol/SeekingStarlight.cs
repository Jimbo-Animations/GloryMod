using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using GloryMod.Systems;
using System;


namespace GloryMod.NPCs.IgnitedIdol
{
    internal class SeekingStarlight : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override string Texture => "GloryMod/CoolEffects/Textures/InvisProj";

        public override void SetDefaults()
        {
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack, Projectile.Center);
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
            Vector2 moveTo = target.Center + target.velocity;

            if (Projectile.Distance(moveTo) > 10 && Projectile.timeLeft > 30)
            {
                Projectile.velocity += Projectile.DirectionTo(moveTo) * 1.5f;
            }

            if (Projectile.timeLeft == 30)
            {
                SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
            }
            
            Projectile.velocity *= 0.9f;
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Right, new Vector2(0, 1).RotatedBy(Projectile.rotation), ProjectileType<DivineRay>(), 90, 0f);
            Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Right, new Vector2(0, -1).RotatedBy(Projectile.rotation), ProjectileType<DivineRay>(), 90, 0f);
            if (Projectile.ai[1] == 1)
            {
                Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Right, new Vector2(1, 0).RotatedBy(Projectile.rotation), ProjectileType<DivineRay>(), 90, 0f);
                Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Right, new Vector2(-1, 0).RotatedBy(Projectile.rotation), ProjectileType<DivineRay>(), 90, 0f);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath, Projectile.Center);
        }

        private float expand = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = Request<Texture2D>("Terraria/Images/Projectile_644").Value;
            Texture2D star = Request<Texture2D>("GloryMod/CoolEffects/Textures/Star").Value;
            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);
            expand = MathHelper.Lerp(expand, 1, 0.1f);
            float scale = (Projectile.scale * mult) * expand;
            float projScale = (float)(5 + Math.Sin(Projectile.timeLeft / 1.5f)) / 5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(glow, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255, 250 - (i * 6), 200 - (i * 12)) * ((1 - i / (float)Projectile.oldPos.Length) * 0.99f) * scale, Projectile.velocity.ToRotation() + MathHelper.PiOver2, glow.Size() / 2 + new Vector2(2, 2), Projectile.scale * (1 - i / (float)Projectile.oldPos.Length) * scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(star, Projectile.Center - Main.screenPosition, null, new Color(255, 250, 200) * scale, -Main.GameUpdateCount * 0.025f, star.Size() / 2, scale * 1.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, Projectile.Center - Main.screenPosition, null, new Color(255, 250, 200) * scale, Main.GameUpdateCount * 0.015f, star.Size() / 2, scale * 1.25f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255) * scale, Projectile.rotation, glow.Size() / 2, scale * projScale * 2, SpriteEffects.None, 0);
            if (Projectile.ai[1] == 1)
            {
                Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255) * scale, Projectile.rotation + MathHelper.PiOver2, glow.Size() / 2, scale * projScale * 2, SpriteEffects.None, 0); ;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class DivineRay : Deathray
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 100000;
        }

        public override string Texture => "GloryMod/CoolEffects/Textures/Deathray";
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 1;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.timeLeft = 200;
            MoveDistance = 0f;
            RealMaxDistance = 3000f;
            bodyRect = new Rectangle(0, 0, Projectile.width, Projectile.height);
            headRect = new Rectangle(0, 0, Projectile.width, Projectile.height);
            tailRect = new Rectangle(0, 0, Projectile.width, Projectile.height);
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        private float rayAlpha;
        public override void PostDraw(Color lightColor)
        {
            float projScale = (float)(5 + Math.Sin(Projectile.localAI[0] / 1.5f)) / 5f;
            if (Projectile.timeLeft < 10)
            {
                rayAlpha = MathHelper.Lerp(rayAlpha, 0, 0.1f);
            }
            else
            {
                rayAlpha = MathHelper.Lerp(rayAlpha, 1, 0.1f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            drawColor = new Color(255, 250, 200) * rayAlpha * projScale;
            DrawLaser(Main.spriteBatch, Request<Texture2D>(Texture).Value, Position(), Projectile.velocity, bodyRect.Height, -1.57f, 1f, MaxDistance, (int)MoveDistance);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffType<FlamesOfJudgement>(), 200, true);
        }

        public override void PostAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.localAI[0]++;

            if (Projectile.timeLeft <= 195)
            {
                Projectile.hostile = true;
            }
        }

        public override Vector2 Position()
        {
            return Projectile.position;
        }
    }
}
