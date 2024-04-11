using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GloryMod.NPCs.BasaltBarriers.Projectiles
{
    internal class ForsakenExplosion : ModProjectile
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
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 28;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

            SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);

            int numDusts = 30;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 3f);
                Main.dust[dust].noGravity = true;
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
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, 0.1f);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Rectangle result = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
            int num = (int)Utils.Remap(Projectile.ai[0] * 2, 0, 200, 10, 40);
            result.Inflate(num, num);
            hitbox = result;
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale * (Projectile.ai[0] * 0.03f + 1), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 150, 50, 50) * visibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.07f + 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }

    class BBExplosionSmall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 9;
        }

        public override void SetDefaults()
        {
            Projectile.width = 82;
            Projectile.height = 82;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 36;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            int numDusts = 25;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Bottom, 0, 0, 6, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(2, 9), 0).RotatedBy(i * -MathHelper.Pi / numDusts);
            }
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= 5)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 9)
                {
                    Projectile.frame = 9;
                }
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, .1f);
            glowVisibility = MathHelper.SmoothStep(glowVisibility, 0, .2f);
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Bottom.Distance(target.Center) <= 82;
        }

        float visibility = 1;
        float glowVisibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Projectile.type] * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center- Main.screenPosition, null, new Color(200, 150, 50, 150) * glowVisibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.05f + 0.1f), SpriteEffects.None, 0);

            return false;
        }
    }

    class BBExplosionMedium : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 18;
        }

        public override void SetDefaults()
        {
            Projectile.width = 230;
            Projectile.height = 230;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 72;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 2 }, Projectile.Center);

            int numDusts = 45;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Bottom - new Vector2(0, 20), 0, 0, 6, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5, 16), 0).RotatedBy(i * -MathHelper.Pi / numDusts);
            }
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= 5)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 18)
                {
                    Projectile.frame = 18;
                }
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 20 ? 0 : 1, .1f);
            glowVisibility = MathHelper.SmoothStep(glowVisibility, 0, .175f);
        }

        public override bool CanHitPlayer(Player target)
        {
            return (Projectile.Bottom - new Vector2(0, 10)).Distance(target.Center) <= 230 && Projectile.frame > 1 && Projectile.frame < 8;
        }

        float visibility = 1;
        float glowVisibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Projectile.type] * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 150, 50, 150) * glowVisibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.075f + 0.25f), SpriteEffects.None, 0);


            return false;
        }
    }

    class BBExplosionLarge : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/BasaltBarriers/Projectiles/BBExplosionLarge" + whichSheet;

        public static Asset<Texture2D> Texture0;
        public static Asset<Texture2D> Texture1;
        public static Asset<Texture2D> Texture2;
        public static Asset<Texture2D> Texture3;
        public static Asset<Texture2D> Texture4;
        public override void Load()
        {
           Texture0 = Request<Texture2D>("GloryMod/NPCs/BasaltBarriers/Projectiles/BBExplosionLarge0");
           Texture1 = Request<Texture2D>("GloryMod/NPCs/BasaltBarriers/Projectiles/BBExplosionLarge1");
           Texture2 = Request<Texture2D>("GloryMod/NPCs/BasaltBarriers/Projectiles/BBExplosionLarge2");
           Texture3 = Request<Texture2D>("GloryMod/NPCs/BasaltBarriers/Projectiles/BBExplosionLarge3");
           Texture4 = Request<Texture2D>("GloryMod/NPCs/BasaltBarriers/Projectiles/BBExplosionLarge4");
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;

        }

        public override void SetDefaults() 
        {
            Projectile.width = 978;
            Projectile.height = 978;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 175;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        private int whichSheet;

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= 5)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 7)
                {
                    if (whichSheet < 5)
                    {
                        Projectile.frame = 0;
                        whichSheet++;
                    }
                    else Projectile.frame = 6;
                }

                if (whichSheet == 0 && Projectile.frame == 3)
                {
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 2 }, Projectile.Center);

                    int numDusts = 150;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(Projectile.Bottom - new Vector2(0, 300), 0, 0, 6, Scale: 4f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(20, 41), 0).RotatedBy(i * -MathHelper.Pi / numDusts);
                    }                   
                }

                if (whichSheet == 0 && Projectile.frame > 2 && Systems.ScreenUtils.screenShaking < 10) Systems.ScreenUtils.screenShaking = 10f;
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 20 ? 0 : 1, .1f);
            if ((whichSheet == 0 && Projectile.frame > 2) || whichSheet > 0) 
            {
                glowVisibility = MathHelper.SmoothStep(glowVisibility, 0, .1f);
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            if (whichSheet == 0)
            {
                return (Projectile.Bottom - new Vector2(0, 440)).Distance(target.Center) <= 900 && Projectile.frame > 2;

            }

            else if (whichSheet == 1)
            {
                return (Projectile.Bottom - new Vector2(0, 440)).Distance(target.Center) <= 900 && Projectile.frame < 5;
            }

            else return false;
        }

        float visibility = 1;
        float glowVisibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Projectile.type] * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            if ((whichSheet == 0 && Projectile.frame > 2) || whichSheet > 0) Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 150, 50, 150) * glowVisibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.15f), SpriteEffects.None, 0);

            return false;
        }
    }
}
