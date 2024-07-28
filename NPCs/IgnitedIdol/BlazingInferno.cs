using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using System.Collections.Generic;
using System;

namespace GloryMod.NPCs.IgnitedIdol
{
    internal class BlazingInferno : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 900;
            Projectile.alpha = 0;
        }

        public override string Texture => "GloryMod/CoolEffects/Textures/InvisProj";

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
            if (Projectile.timeLeft >= 100)
            {
                Projectile.localAI[0]++;
                Projectile.localAI[1]++;
            }

            Lighting.AddLight(Projectile.Center, 0.5f * expand, 0.5f * expand, 1 * expand);

            //Spray bullets everywhere.

            if (Projectile.localAI[1] >= 35 && Projectile.timeLeft >= 200)
            {
                int projNumber = 2 + (int)owner.ai[1];
                float projRotation = Projectile.localAI[0] / 100;
                for (int i = 0; i < projNumber; i++)
                {
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Projectile.ai[1] == 1 || Main.getGoodWorld ? 4 : 3, 0).RotatedBy((i * MathHelper.TwoPi / projNumber) + projRotation).RotatedByRandom(MathHelper.ToRadians(10)), ProjectileType<AwakenedLight>(), Projectile.damage, 3f, Main.myPlayer);
                    Main.projectile[proj].ai[0] = Projectile.ai[0];
                    Main.projectile[proj].ai[1] = 3;
                }

                Projectile.localAI[1] = 0;
                SoundEngine.PlaySound(SoundID.DD2_BetsySummon, Projectile.Center);
                Projectile.hostile = true;
            }

            if (Projectile.ai[1] == 2 && owner.ai[3] == 0 && owner.active == true)
            {
                Projectile.timeLeft = 201;
            }
            
            if (Projectile.timeLeft <= 200)
            {
                Projectile.hostile = false;
            }

            if (Projectile.timeLeft == 100)
            {
                flash.Add(new Tuple<Vector2, float, float>(Projectile.Center, 0f, 50f));
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen with { Volume = 1.5f}, Projectile.Center);

                int numDusts = 50;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 59, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }
        }
        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 100 * expand;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffType<FlamesOfRetribution>(), 200, true);
        }

        private float expand = 0;
        private float flashAlpha = 1;
        List<Tuple<Vector2, float, float>> flash = new List<Tuple<Vector2, float, float>>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D inferno = Request<Texture2D>("GloryMod/CoolEffects/Textures/Inferno1").Value;
            Texture2D pulse = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_2").Value;
            Texture2D glow1 = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_7").Value;
            Texture2D glow2 = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            if (Projectile.timeLeft < 200)
            {
                expand = MathHelper.Lerp(expand, 0, 0.02f);
                if (Projectile.timeLeft < 100)
                {
                    flashAlpha = MathHelper.Lerp(flashAlpha, 0, 0.02f);
                }
            }
            else
            {
                expand = MathHelper.Lerp(expand, 1, 0.02f);
            }
            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);
            float scale = (Projectile.scale * mult) * expand;
            float projScale = expand * (float)(5 + Math.Sin(Projectile.localAI[0] / 3f)) / 5f;

            Main.EntitySpriteDraw(glow1, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255), 0, glow1.Size() / 2, scale * 3.5f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(glow2, Projectile.Center - Main.screenPosition, null, new Color(200, 250, 255) * scale, 0, glow2.Size() / 2, scale * 2.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(inferno, Projectile.Center - Main.screenPosition, null, new Color(100, 100, 250) * projScale, Main.GameUpdateCount * 0.02f, inferno.Size() / 2, scale * 1.25f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(inferno, Projectile.Center - Main.screenPosition, null, new Color(100, 100, 250) * projScale, Main.GameUpdateCount * 0.04f, inferno.Size() / 2, scale * 1.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(inferno, Projectile.Center - Main.screenPosition, null, new Color(100, 100, 250) * projScale * 0.75f, Main.GameUpdateCount * 0.06f, inferno.Size() / 2, scale * 1.75f, SpriteEffects.None, 0);
          
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Player target = Main.player[Projectile.owner];
            for (int i = 0; i < flash.Count; i++)
            {
                if (i >= flash.Count)
                {
                    break;
                }

                flash[i] = new Tuple<Vector2, float, float>(flash[i].Item1, flash[i].Item2 + flash[i].Item3, flash[i].Item3);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                Main.EntitySpriteDraw(pulse, flash[i].Item1 - Main.screenPosition, null, new Color(200, 200, 255, 255) * flashAlpha, 0, pulse.Size() / 2, flash[i].Item2 / pulse.Width, SpriteEffects.None, 0);

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
