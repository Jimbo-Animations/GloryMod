using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Boss
{
    partial class BasaltBarrier : ModNPC
    {
        public override void PostAI()
        {
            AITimer++;

            heat = MathHelper.Lerp(heat, useShield ? 1 : 0, 0.05f);
            shieldOpacity = MathHelper.Lerp(heat, useShield ? 1 : 0, 0.075f);

            NPC.dontTakeDamage = MinionCount > 0;
            useShield = MinionCount > 0;

            if (useShield)
            {
                if (AITimer % 3 == 1)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 65, Scale: 2f * heat);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = false;
                }
            }
        }

        private void UpdateSound()
        {
            if (SoundEngine.TryGetActiveSound(ShieldNoise, out ActiveSound sound) && sound is not null && sound.IsPlaying)
            {
                sound.Position = NPC.Center;

                if (useShield && NPC.active)
                    sound.Volume = MathHelper.Lerp(sound.Volume, 1, 0.1f);
                else
                    sound.Volume = MathHelper.Lerp(sound.Volume, 0, 0.1f);

                if (!NPC.active) sound.Stop();
            }

            else if (sound is null)
                ShieldNoise = SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop with { IsLooped = true }, NPC.Center);
        }

        void WallMovement(Vector2 targetLocation, float rotateTo, int wallDirection, float wallSpeed = 1, float wallAcc = 0.05f, float headSpeed = 0.025f, float rotationSpeed = 0.025f)
        {
            // Wall and head movement.

            NPC.velocity.X = MathHelper.SmoothStep(NPC.velocity.X, wallSpeed * wallDirection, wallAcc);
            NPC.position.Y = MathHelper.SmoothStep(NPC.position.Y, targetLocation.Y - (NPC.height / 2), headSpeed);

            // Head rotation.

            NPC.rotation = NPC.rotation.AngleLerp(rotateTo + (NPC.spriteDirection == 1 ? MathHelper.Pi : -MathHelper.Pi), rotationSpeed);

            if (NPC.direction == 1)
                NPC.rotation = MathHelper.Clamp(NPC.rotation, MathHelper.ToRadians(-45), MathHelper.ToRadians(45));
            else
                NPC.rotation = MathHelper.Clamp(NPC.rotation, MathHelper.ToRadians(45), MathHelper.ToRadians(135));

            // Stop the player from moving past the wall.

            if ((target.Center.X < NPC.Right.X - 70 && NPC.spriteDirection == 1 || target.Center.X > NPC.Left.X + 70 && NPC.spriteDirection == -1) && !target.dead && target.active)
            {
                PlayerDeathReason teleport = PlayerDeathReason.ByCustomReason(target.name + " tried passing the barrier");

                // Teleport the player if they get stuck behind the wall.

                if (target.Center.X < NPC.Left.X - 70 && NPC.spriteDirection == 1 || target.Center.X > NPC.Right.X + 70 && NPC.spriteDirection == -1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, target.Center);

                    target.immune = false;
                    target.Hurt(teleport, 10000, NPC.spriteDirection, false, false, 0, false, 50);

                    int numDusts = 50;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(target.Center, 0, 0, 65, Scale: 3f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = false;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    }
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, target.Center);
                    target.velocity = new Vector2(5 * NPC.spriteDirection, 0);

                    target.RemoveAllGrapplingHooks();
                    target.Hurt(teleport, 100, NPC.spriteDirection, false, false, 0, false, 50);
                    target.AddBuff(BuffID.ShadowFlame, 150);
                    target.immune = true;
                    target.immuneTime = 10;

                    int numDusts = 50;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(target.Center, 0, 0, 65, Scale: 3f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = false;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    }
                }
            }

            // Stop the player from leaving hell.

            if (target.Center.Y < Main.UnderworldLayer && !target.dead && target.active)
            {
                PlayerDeathReason escape = PlayerDeathReason.ByCustomReason(target.name + " tried to escape");

                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, target.Center);

                target.immune = false;
                target.Hurt(escape, 10000, NPC.spriteDirection, false, false, 0, false, 50);

                int numDusts = 50;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(target.Center, 0, 0, 65, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = false;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }
        }
    }
}
