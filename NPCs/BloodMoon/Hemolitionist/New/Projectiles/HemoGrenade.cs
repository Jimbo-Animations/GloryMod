using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BloodMoon.Hemolitionist.New.Projectiles
{
    internal class HemoGrenade : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(12);
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        private bool activation = false;
        private bool landed = false;
        public override void AI()
        {
            Projectile.velocity *= .99f;
            Projectile.rotation += Projectile.velocity.X * .05f;
            Player target = Main.player[Projectile.owner];

            if (Projectile.ai[0] == 0)
            {
                if (Projectile.Distance(target.Center) < 60 && !activation && Projectile.timeLeft < 570)
                {
                    activation = true;
                    Projectile.timeLeft = 20;
                    SoundEngine.PlaySound(SoundID.Item75, Projectile.Center);
                }

                if (Projectile.wet)
                {
                    if (!landed && !activation)
                    {
                        landed = true;
                        Projectile.timeLeft = 120;
                    }

                    Projectile.velocity.Y -= .33f;
                    Projectile.velocity.Y *= .9f;
                }
                else Projectile.velocity.Y += .25f;

                MathHelper.Clamp(Projectile.velocity.Y, -10, 10);

            }
            else
            {
                if (!activation)
                {
                    activation = true;
                    Projectile.timeLeft = 60;
                }

                Projectile.tileCollide = false;                
            }

            visibility = MathHelper.Lerp(visibility, 1, .15f);
            ringVisibility = MathHelper.SmoothStep(ringVisibility, Projectile.timeLeft <= 20 ? 0 : 1, .15f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.Y > 2 || Projectile.velocity.Y < -2)
            {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            }           

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            Projectile.velocity *= .35f;
            if (!landed && !activation) 
            {
                landed = true;
                Projectile.timeLeft = 120;
            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<GrenadeExplosion>(), 200, 0, Projectile.owner);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        float visibility;
        float ringVisibility;
        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D radius = Request<Texture2D>("GloryMod/CoolEffects/Textures/PulseCircle").Value;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            float mult = 0.975f + ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) * .05f);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255, 30, 15, 255) * visibility * ((1 - i / (float)Projectile.oldPos.Length) * .95f), Projectile.rotation, texture.Size() / 2, Projectile.scale * 1.25f, SpriteEffects.None, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center + new Vector2(2 * visibility, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - Main.screenPosition, null,
                new Color(255, 30, 15, 255) * visibility, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }

            if (Projectile.ai[0] == 0) Main.EntitySpriteDraw(radius, Projectile.Center - Main.screenPosition, null, new Color(255, 30, 15, 255) * .85f * ringVisibility * mult, Projectile.rotation, radius.Size() / 2, (.195f * mult) * ringVisibility , SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    internal class GrenadeExplosion : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/BloodMoon/BloodDrone/DroneExplosion";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.scale = 2;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 24;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source != Projectile.GetSource_ReleaseEntity()) Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            int numDusts = 30;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 266, Scale: 3f);
                Main.dust[dust].noGravity = false;
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

                if (Projectile.frame > 2 & Projectile.frame < 6) Projectile.hostile = true;
                else Projectile.hostile = false;
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, 0.1f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Bleeding, 300, true);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Rectangle result = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
            int num = (int)Terraria.Utils.Remap(Projectile.ai[0] * 8, 0, 200, 10, 40);
            result.Inflate(num, num);
            hitbox = result;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.Distance(Projectile.Center) <= Projectile.Hitbox.X / 2;
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / 14);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale * (Projectile.ai[0] * 0.03f + 1), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(250, 100, 100, 100) * visibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.07f + 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }
}
