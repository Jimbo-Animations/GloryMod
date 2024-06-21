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
    internal class DroneBullet : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/SemiStar";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.tileCollide = false;
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
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity *= 1.05f;

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, 0.1f);
            if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Projectile.Center, 266, new Vector2(Main.rand.NextFloat(3), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.ToRadians(90)), Scale: 2f);
        }

        float visibility;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255, 30, 15, 255) * visibility, Projectile.rotation, texture.Size() / 2, new Vector2(visibility - (i / (float)Projectile.oldPos.Length), visibility + 0.5f - i / (float)Projectile.oldPos.Length * 0.75f * (1 + (Projectile.velocity.ToRotation() * .01f))), SpriteEffects.None, 0);
            }
                
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 225, 255) * visibility, Projectile.rotation, texture.Size() / 2, new Vector2(visibility - 0.5f, visibility), SpriteEffects.None, 0);
            return false;
        }
    }
}
