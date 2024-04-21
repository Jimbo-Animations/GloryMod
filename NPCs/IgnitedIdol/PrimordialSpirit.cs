using Terraria.Audio;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace GloryMod.NPCs.IgnitedIdol
{
    internal class PrimordialSpirit : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 130;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Volume = 1.25f}, Projectile.Center);
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            Projectile.localAI[1]++;

            if (Projectile.localAI[1] >= 5)
            {
                Projectile.frame++;
                Projectile.localAI[1] = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            switch (Projectile.ai[1])
            {
                case 0:

                    if (Main.rand.NextBool(25))
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(0, Projectile.width / 2), 0).RotatedByRandom(MathHelper.TwoPi), 6, Projectile.DirectionFrom(Projectile.Center) * Main.rand.NextFloat(1, 3), 0, default, 1.5f);
                        dust.noGravity = true;
                    }

                    if (Projectile.Distance(owner.Center) > 200 + (owner.ai[1] * 20))
                    {
                        Projectile.position += new Vector2(25, 0).RotatedBy(Projectile.DirectionTo(owner.Center).ToRotation());
                    }

                    if (Projectile.timeLeft == 30)
                    {
                        Vector2 directionTo = Projectile.DirectionTo(owner.Center);
                        float fanSize = 160;
                        for (int i = -7; i < 8; i++)
                        {
                            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(6, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 15 + directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 10, 3f, Main.myPlayer);
                            Main.projectile[proj].ai[0] = Projectile.ai[0];
                            Main.projectile[proj].ai[1] = 1;
                        }

                        int numDusts = 50;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        flash.Add(new Tuple<Vector2, float, float>(Projectile.Center, 0f, 100f));
                        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
                    }

                    break;

                case 1:

                    if (Main.rand.NextBool(25))
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(0, Projectile.width / 2), 0).RotatedByRandom(MathHelper.TwoPi), 59, Projectile.DirectionFrom(Projectile.Center) * Main.rand.NextFloat(1, 3), 0, default, 1.5f);
                        dust.noGravity = true;
                    }

                    if (Projectile.Distance(owner.Center) > 200 + (owner.ai[1] * 20))
                    {
                        Projectile.position += new Vector2(25, 0).RotatedBy(Projectile.DirectionTo(owner.Center).ToRotation());
                    }

                    if (Projectile.timeLeft == 30)
                    {
                        Vector2 directionTo = Projectile.DirectionTo(owner.Center);
                        float fanSize = 160;
                        for (int i = -6; i < 7; i++)
                        {
                            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(6, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 13 + directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 12, 3f, Main.myPlayer);
                            Main.projectile[proj].ai[0] = Projectile.ai[0];
                            Main.projectile[proj].ai[1] = 3;
                        }

                        int numDusts = 50;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 0, 0, 59, Scale: 3f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        flash.Add(new Tuple<Vector2, float, float>(Projectile.Center, 0f, 100f));
                        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
                    }

                    break;
            }          
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[1] == 0)
            {
                target.AddBuff(BuffType<FlamesOfJudgement>(), 200, true);
            }
            if (Projectile.ai[1] == 1)
            {
                target.AddBuff(BuffType<FlamesOfRetribution>(), 200, true);
            }
        }

        private float expand = 0;
        List<Tuple<Vector2, float, float>> flash = new List<Tuple<Vector2, float, float>>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D texture2 = Request<Texture2D>(Texture + "2").Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Texture2D pulse = Request<Texture2D>("GloryMod/CoolEffects/Textures/PulseCircle").Value;
            float mult = 0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            if (Projectile.timeLeft <= 30)

            {
                expand = MathHelper.Lerp(expand, 0, 0.1f);
            }
            else
            {
                expand = MathHelper.Lerp(expand, 1, 0.1f);
            }

            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            float scale = (Projectile.scale * mult) * expand;
            NPC owner = Main.npc[(int)Projectile.ai[0]];

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            if (Projectile.ai[1] == 1)
            {
                Utils.DrawLine(Main.spriteBatch, owner.Center, Projectile.Center, new Color(200, 175, 255) * expand);
                Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 175, 255) * expand, 0, glow.Size() / 2, scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * expand, Main.GameUpdateCount * 0.015f, drawOrigin, scale, SpriteEffects.None, 0);
            }
            else
            {
                Utils.DrawLine(Main.spriteBatch, owner.Center, Projectile.Center, new Color(250, 200, 175) * expand);
                Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(255, 200, 175) * expand, 0, glow.Size() / 2, scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * expand, Main.GameUpdateCount * 0.015f, drawOrigin, scale, SpriteEffects.None, 0);
            }           

            Player target = Main.player[Projectile.owner];

            for (int i = 0; i < flash.Count; i++)
            {
                if (i >= flash.Count)
                {
                    break;
                }

                flash[i] = new Tuple<Vector2, float, float>(flash[i].Item1, flash[i].Item2 + flash[i].Item3, flash[i].Item3);

                if (Projectile.ai[1] == 1)
                {
                    Main.EntitySpriteDraw(pulse, flash[i].Item1 - Main.screenPosition, null, new Color(200, 175, 255, 255) * expand, 0, pulse.Size() / 2, flash[i].Item2 / pulse.Width, SpriteEffects.None, 0);
                }
                else
                {
                    Main.EntitySpriteDraw(pulse, flash[i].Item1 - Main.screenPosition, null, new Color(250, 200, 175, 255) * expand, 0, pulse.Size() / 2, flash[i].Item2 / pulse.Width, SpriteEffects.None, 0);
                }
                   
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
