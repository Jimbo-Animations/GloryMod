using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.IgnitedIdol
{
    internal class AwakenedLight : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override string Texture => "GloryMod/CoolEffects/Textures/InvisProj";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 11;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            NPC owner = Main.npc[(int)Projectile.ai[0]];

            switch (Projectile.ai[1])
            {
                case 0:

                    //Projectile that bounces around the arena.

                    //Controls the bouncing.
                    if (Projectile.Distance(owner.Center) >= 200 + (25 * owner.ai[1]))
                    {
                        Projectile.position = Projectile.oldPosition;
                        Projectile.velocity = -Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45));
                        SoundEngine.PlaySound(SoundID.Item42, Projectile.Center);
                    }

                    //Failsafe if projectile gets stuck.
                    if (Projectile.Distance(owner.Center) >= 200 + (25 * owner.ai[1]))
                    {
                        Projectile.position += new Vector2(25, 0).RotatedBy(Projectile.DirectionTo(owner.Center).ToRotation());                      
                    }

                    Projectile.localAI[0]++; //Controls dust spawn.

                    if (Projectile.localAI[0] >= 10)
                    {
                        Projectile.localAI[0] = 0;

                        int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                    }

                    //Makes the projectile die.
                    if (owner.ai[2] != 1)
                    {
                        Projectile.timeLeft = 11;
                    }
                    if (Projectile.timeLeft <= 10)
                    {
                        expand = MathHelper.Lerp(expand, 0, 0.1f);
                    }

                    //Extra failsafe if the boss despawns while the projectiles are active.
                    Projectile.localAI[1]++;

                    if (Projectile.localAI[1] >= 600)
                    {
                        Projectile.Kill();
                    }

                    break;

                case 1:

                    //Makes projectile die upon hitting the edge of the arena.

                    Projectile.localAI[0]++; //Controls dust spawn.

                    if (Projectile.localAI[0] >= 10)
                    {
                        Projectile.localAI[0] = 0;

                        int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                    }

                    //Accelerates the projectiles.
                    Projectile.velocity *= 1.01f;

                    //Controls the projectile's death.
                    Projectile.localAI[1]++;

                    if (Projectile.localAI[1] > 5 && Projectile.Distance(owner.Center) >= 200 + (25 * owner.ai[1]))
                    {
                        int numDusts = 12;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(4, 8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
                        Projectile.Kill();
                    }
                    else
                    {
                        Projectile.timeLeft = 11;
                    }

                    break;

                case 2:

                    //Projectile that bounces around the arena.

                    //Controls the bouncing.
                    if (Projectile.Distance(owner.Center) >= 200 + (25 * owner.ai[1]))
                    {
                        Projectile.position = Projectile.oldPosition;
                        Projectile.velocity = -Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45));
                        SoundEngine.PlaySound(SoundID.Item42, Projectile.Center);
                    }

                    //Failsafe if projectile gets stuck.
                    if (Projectile.Distance(owner.Center) >= 200 + (25 * owner.ai[1]))
                    {
                        Projectile.position += new Vector2(25, 0).RotatedBy(Projectile.DirectionTo(owner.Center).ToRotation());
                    }

                    Projectile.localAI[0]++; //Controls dust spawn.

                    if (Projectile.localAI[0] >= 10)
                    {
                        Projectile.localAI[0] = 0;

                        int dust = Dust.NewDust(Projectile.Center, 0, 0, 59, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                    }

                    //Makes the projectile die.
                    if (owner.ai[2] != 1)
                    {
                        Projectile.timeLeft = 11;
                    }
                    if (Projectile.timeLeft <= 10)
                    {
                        expand = MathHelper.Lerp(expand, 0, 0.1f);
                    }

                    //Extra failsafe if the boss despawns while the projectiles are active.
                    Projectile.localAI[1]++;

                    if (Projectile.localAI[1] >= 600)
                    {
                        Projectile.Kill();
                    }

                    break;

                case 3:

                    //Makes projectile die upon hitting the edge of the arena.

                    Projectile.localAI[0]++; //Controls dust spawn.

                    if (Projectile.localAI[0] >= 10)
                    {
                        Projectile.localAI[0] = 0;

                        int dust = Dust.NewDust(Projectile.Center, 0, 0, 59, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                    }

                    //Accelerates the projectiles.
                    Projectile.velocity *= 1.015f;

                    //Controls the projectile's death.
                    Projectile.localAI[1]++;

                    if (Projectile.localAI[1] > 5 && Projectile.Distance(owner.Center) >= 200 + (25 * owner.ai[1]))
                    {
                        int numDusts = 12;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 0, 0, 59, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(4, 8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
                        Projectile.Kill();
                    }
                    else
                    {
                        Projectile.timeLeft = 11;
                    }

                    break;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[1] < 2)
            {
                target.AddBuff(BuffType<FlamesOfJudgement>(), 150, true);
            }
            if (Projectile.ai[1] >= 2)
            {
                target.AddBuff(BuffType<FlamesOfRetribution>(), 150, true);
            }
        }

        private float expand = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = Request<Texture2D>("Terraria/Images/Projectile_644").Value;
            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);
            expand = MathHelper.Lerp(expand, 1, 0.1f);
            float scale = (Projectile.scale * mult) * expand;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.ai[1] > 1)
                {
                    Main.EntitySpriteDraw(glow, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(150 + (i * 5), 200 - (i * 10), 255) * ((1.2f - i / (float)Projectile.oldPos.Length) * 0.99f) * scale, Projectile.velocity.ToRotation() + MathHelper.PiOver2, glow.Size() / 2 + new Vector2(0, 2), Projectile.scale * (1 - i / (float)Projectile.oldPos.Length) * scale, SpriteEffects.None, 0);
                }
                else
                {
                    Main.EntitySpriteDraw(glow, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255, 250 - (i * 10), 200 - (i * 15)) * ((1.2f - i / (float)Projectile.oldPos.Length) * 0.99f) * scale, Projectile.velocity.ToRotation() + MathHelper.PiOver2, glow.Size() / 2 + new Vector2(0, 2), Projectile.scale * (1 - i / (float)Projectile.oldPos.Length) * scale, SpriteEffects.None, 0);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
