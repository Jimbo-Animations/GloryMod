using GloryMod.NPCs.BasaltBarriers.Minions;
using GloryMod.NPCs.BasaltBarriers.Projectiles;
using Terraria.Audio;

namespace GloryMod.NPCs.BasaltBarriers.Boss
{
    partial class BasaltBarrier : ModNPC
    {
        private void Intro()
        {
            WallMovement(AITimer < 720 ? NPC.position : target.position, AITimer < 420 ? NPC.velocity.ToRotation() : NPC.DirectionTo(target.Center).ToRotation(), NPC.spriteDirection, AITimer < 420 ? 1.25f : 0 , rotationSpeed : .015f);

            if (AITimer == 540)
            {
                showEye = true;
                animState = 1;
                SoundEngine.PlaySound(SoundID.NPCDeath10 with { Volume = 2, Pitch = -.5f }, NPC.Center);
            }

            if (AITimer > 540 & AITimer <= 720) Systems.ScreenUtils.screenShaking = 5f;

            if (AITimer > 720) 
            {
                animState = 0;
                showEye = false;
            }

            if (AITimer == 750)
            {
                for (int i = -2; i < 3; i++)
                {
                    minion = Main.npc[NPC.NewNPC(NPC.InheritSource(NPC), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Forsaken>())];
                    minion.ai[1] = 180;
                    minion.ai[2] = i;
                    minion.ai[3] = NPC.whoAmI;
                }

                SoundEngine.PlaySound(SoundID.DD2_BetsySummon, NPC.Center);

                AITimer = 0;
                NPC.ai[0]++;
                NPC.netUpdate = true;
            }
        }

        private void SoulBlast()
        {
            WallMovement(target.position, NPC.DirectionTo(target.Center).ToRotation(), NPC.spriteDirection, Systems.Utils.CloseTo(NPC.Center.X, target.Center.X, 700) ? 2 - (MinionCount / 4) : 20, rotationSpeed: .015f);

            if (AITimer == 30)
            {
                showEye = true;
                SoundEngine.PlaySound(SoundID.DD2_DrakinBreathIn with { Pitch = -.33f, Volume = 1.25f }, NPC.Center);
            }

            if (AITimer >= 30 && AITimer < 90)
            {
                if (AITimer % 3 == 1)
                {
                    Vector2 dustRand = new Vector2(Main.rand.NextFloat(30, 50), 0).RotatedByRandom(MathHelper.TwoPi);
                    int dust = Dust.NewDust(NPC.Center + new Vector2(-14, 8 * -NPC.spriteDirection).RotatedBy(NPC.rotation) + dustRand, 0, 0, 6, Scale: AITimer * 0.04f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = false;
                    Main.dust[dust].velocity = -dustRand * 0.1f;
                    Main.dust[dust].velocity += NPC.velocity * .5f;
                    Main.dust[dust].position += NPC.velocity;
                }
            }

            if (AITimer >= 90 && AITimer < 180)
            {
                if (AITimer % 5 == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_DrakinShot, NPC.Center);
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-14, 8 * -NPC.spriteDirection).RotatedBy(NPC.rotation), new Vector2(1, 0).RotatedBy(NPC.rotation + MathHelper.Pi).RotatedByRandom(MathHelper.ToRadians(10)), ProjectileType<BBSpiritBolt>(), 80, 0, target.whoAmI);
                    Main.projectile[proj].ai[1] = Main.rand.NextFloat(-4, 5);
                    Main.projectile[proj].ai[2] = Main.rand.NextFloat(8, 13);
                    NPC.netUpdate = true;
                }
            }
        }
    }
}
