using System.Collections.Generic;
using GloryMod.NPCs.BloodMoon.BloodDrone;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BloodMoon.BloodSeekerBeast
{
    class BSBVortex : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/Vortex";

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(70);
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 200;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
        }

        float opacity;
        float timer;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

            int numDusts = 12;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 114, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }

        public override void AI()
        {        
            Projectile.ai[0]++;

            if (Projectile.timeLeft < 170 && Projectile.timeLeft > 60) Projectile.hostile = true;
            else Projectile.hostile = false;

            if (Projectile.ai[0] == 80 || Projectile.ai[0] == 120)
            {
                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Volume = 2 }, Projectile.Center);

                Vector2 randomizer = new Vector2(5, 0).RotatedBy(Projectile.rotation + Main.GameUpdateCount * 0.025f);
                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randomizer.RotatedBy(i * MathHelper.TwoPi / 6), ProjectileType<DroneMissile>(), 150, 3f, Main.myPlayer, 0, 1);
                }
            }

            opacity = MathHelper.SmoothStep(opacity, Projectile.timeLeft > 60 ? 1 : 0, .15f);
            if (Main.rand.NextBool(10)) Dust.NewDustPerfect(Projectile.Center, 266, new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi), Scale: 2f);
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/Vortex").Value;
            Texture2D textureCenter = Request<Texture2D>("GloryMod/CoolEffects/Textures/Starmark").Value;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            float mult = 0.95f + ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 2) * 0.1f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Red * opacity * 0.5f, Main.GameUpdateCount * 0.025f + Projectile.rotation, texture.Size() / 2, Projectile.scale * opacity * 2 * mult, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Red * opacity, Main.GameUpdateCount * 0.05f + Projectile.rotation, texture.Size() / 2, Projectile.scale * opacity * mult, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(textureCenter, Projectile.Center - Main.screenPosition, null, Color.White * opacity, 0, textureCenter.Size() / 2, Projectile.scale * opacity * mult, SpriteEffects.None, 0);

            return false;
        }
    }

    class BSBSpineRocket : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBTopSpike";

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(40);
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
            Projectile.rotation = Projectile.velocity.ToRotation();

            int numDusts = 12;

            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 266, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Vector2 trueVelocity = new Vector2(3, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                trueVelocity.X *= 0.5f;
                trueVelocity = trueVelocity.RotatedBy(Projectile.velocity.ToRotation()) + new Vector2(5, 0).RotatedBy(Projectile.velocity.ToRotation());
                Main.dust[dust].velocity = trueVelocity;
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.Distance(Projectile.Center) > 20 - (Projectile.ai[0] * 3);
        }

        public override bool? CanHitNPC(NPC target)
        {
            return target.friendly && target.Distance(Projectile.Center) > 20 - (Projectile.ai[0] * 3);
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 260)
            {
                Projectile.hostile = true;
            }
            if (Projectile.timeLeft < 240)
            {
                Projectile.ai[2] += .0008f;
            }

            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];

            Projectile.velocity += Projectile.DirectionTo(target.Center) * Projectile.ai[2];
            Projectile.rotation += Projectile.ai[2];

            glowOpacity = MathHelper.SmoothStep(glowOpacity, 1, .1f);
            Projectile.velocity *= .98f;

            if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Projectile.Center, 266, new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi), Scale: 2f);
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<DroneExplosion>(), 150, 0, Projectile.owner);
        }

        float glowOpacity;
        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D topSpikes = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBTopSpike").Value;
            Texture2D bottomSpikes = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBottomSpike").Value;

            Texture2D topMask = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBTopSpikeMask").Value;
            Texture2D bottomMask = Request<Texture2D>("GloryMod/NPCs/BloodMoon/BloodSeekerBeast/BSBBottomSpikeMask").Value;

            Rectangle spikeFrame = new(0, topSpikes.Height / 4 * (int)Projectile.ai[0], topSpikes.Width, topSpikes.Height / 4);
            Vector2 spikeOrigin = new(topSpikes.Width * 0.5f, topSpikes.Height / 8);

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Vector2 spikeOffset = new Vector2 (0, 3 * Projectile.ai[0] * (Projectile.ai[1] == 1 ? 1 : -1)).RotatedBy(Projectile.rotation + (MathHelper.PiOver2 * (Projectile.ai[1] == 1 ? -1 : 1)));

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(Projectile.ai[1] == 1 ? bottomSpikes : topSpikes, Projectile.Center + spikeOffset + new Vector2(4 * glowOpacity, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - Main.screenPosition, spikeFrame,
                new Color(255, 100, 100, 100) * glowOpacity, Projectile.rotation + (MathHelper.PiOver2 * (Projectile.ai[1] == 1 ? -1 : 1)), spikeOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(Projectile.ai[1] == 1 ? bottomSpikes : topSpikes, Projectile.Center + spikeOffset - Main.screenPosition, spikeFrame, lightColor, Projectile.rotation + (MathHelper.PiOver2 * (Projectile.ai[1] == 1 ? -1 : 1)), spikeOrigin, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Projectile.ai[1] == 1 ? bottomMask : topMask, Projectile.Center + spikeOffset - Main.screenPosition, spikeFrame, Color.White, Projectile.rotation + (MathHelper.PiOver2 * (Projectile.ai[1] == 1 ? -1 : 1)), spikeOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
