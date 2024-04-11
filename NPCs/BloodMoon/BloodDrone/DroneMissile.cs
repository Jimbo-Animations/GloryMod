using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Microsoft.Xna.Framework.Audio;
using Terraria.DataStructures;
using GloryMod.Systems;
using System.Collections.Generic;
using System;

namespace GloryMod.NPCs.BloodMoon.BloodDrone
{
    internal class DroneMissile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            visibility = MathHelper.Lerp(visibility, 1, 0.1f);
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.ai[0]++;

            if (Projectile.ai[0] >= 6)
            {
                Projectile.frame++;
                Projectile.ai[0] = 0;
                if (Projectile.frame >= 3)
                {
                    Projectile.frame = 0;
                }
            }

            int age = 150 - Projectile.timeLeft;
            float homingFactor = 1f / (float)Math.Max(age / 2f, 50f - age / 2f);

            float velocityFactor = Math.Min(1 + age / 5f, 50f);

            Vector2 goalVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * velocityFactor;
            Projectile.velocity += (goalVelocity - Projectile.velocity) * homingFactor;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * velocityFactor;

            if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Projectile.Center, 266, new Vector2(Main.rand.NextFloat(3), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.ToRadians(90)), Scale: 2f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), Projectile.damage, 0, Projectile.owner);
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), Projectile.damage, 0, Projectile.owner);
        }

        float visibility;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(250, 100, 100, 50) * visibility, Projectile.rotation + MathHelper.PiOver2, trail.Size() / 2, visibility - (i / (float)Projectile.oldPos.Length), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, Projectile.rotation, drawOrigin, new Vector2(visibility * 0.5f + 0.5f, visibility * 0.5f + 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }
}
