using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using System.Collections.Generic;

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
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            int numDusts = 30;
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

                if (Projectile.frame > 5) Projectile.hostile = false;
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, .1f);
            glowVisibility = MathHelper.SmoothStep(glowVisibility, 0, .2f);
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
            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Volume = 2.25f, Pitch = -0.75f }, Projectile.Center);

            int numDusts = 50;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Bottom - new Vector2(0, 20), 0, 0, 6, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(7, 21), 0).RotatedBy(i * -MathHelper.Pi / numDusts);
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

                if (Projectile.frame == 1)
                {
                    Systems.ScreenUtils.screenShaking = 5f;

                    for (int i = -1; i < 2; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Center, new Vector2(0, -Main.rand.NextFloat(5, 10)).RotatedBy((i * MathHelper.PiOver2 / 3)).RotatedByRandom(MathHelper.ToRadians(10)), 
                            ProjectileType<BBFireBallSmall>(), 75, 0, Projectile.owner);
                    }
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

        private int whichSheet;
        Player target;

        public override void AI()
        {
            target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
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
                    SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/Boom") with { Volume = 1.5f, Pitch = -.5f }, Projectile.Center);
                    flash.Add(new Tuple<Vector2, float, float>(Projectile.Center, 0f, 100f));

                    int numDusts = 200;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(Projectile.Bottom - new Vector2(0, 300), 0, 0, 6, Scale: 5f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25, 51), 0).RotatedBy(i * -MathHelper.Pi / numDusts);
                    }

                    for (int i = -2; i < 2; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Center, new Vector2(0, -Main.rand.NextFloat(15, 21)).RotatedBy(i * MathHelper.PiOver2 / 4).RotatedByRandom(MathHelper.ToRadians(15)), 
                            ProjectileType<BBFireBallSmall>(), 75, 0, Projectile.owner);
                    }

                    for (int i = -1; i < 2; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_ReleaseEntity(), Projectile.Center, new Vector2(0, -Main.rand.NextFloat(15, 21)).RotatedBy(i * MathHelper.PiOver2 / 3).RotatedByRandom(MathHelper.ToRadians(15)), 
                            ProjectileType<BBFireBallMedium>(), 100, 0, Projectile.owner);
                    }
                }

                if (whichSheet == 0 && Projectile.frame > 2 && Systems.ScreenUtils.screenShaking < 12) Systems.ScreenUtils.screenShaking = 12f;
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
                return Projectile.Center.Distance(target.Center) <= 489 && Projectile.frame > 2;

            }

            else if (whichSheet == 1)
            {
                return Projectile.Center.Distance(target.Center) <= 489 && Projectile.frame < 5;
            }

            else return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.FinalDamage *= target.Distance(Projectile.Center) > 200 ? target.Distance(Projectile.Center) > 400 ?  0.2f : 0.5f : 1;
        }

        float visibility = 1;
        float glowVisibility = 1;
        List<Tuple<Vector2, float, float>> flash = new List<Tuple<Vector2, float, float>>();
        public override bool PreDraw(ref Color lightColor)
        {    
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Texture2D pulse = Request<Texture2D>("GloryMod/CoolEffects/Textures/PulseCircle").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Projectile.type] * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            if ((whichSheet == 0 && Projectile.frame > 2) || whichSheet > 0) Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 150, 50, 150) * glowVisibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.33f), SpriteEffects.None, 0);

            for (int i = 0; i < flash.Count; i++)
            {
                if (i >= flash.Count)
                {
                    break;
                }

                flash[i] = new Tuple<Vector2, float, float>(flash[i].Item1, flash[i].Item2 + flash[i].Item3, flash[i].Item3);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                Main.EntitySpriteDraw(pulse, flash[i].Item1 - Main.screenPosition, null, new Color(200, 250, 100, 25) * visibility, 0, pulse.Size() / 2, flash[i].Item2 / pulse.Width, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                if (flash[i].Item2 >= target.Distance(flash[i].Item1) + Main.screenWidth * 3)
                {
                    flash.RemoveAt(i);
                }
            }

            return false;
        }
    }
}
