using GloryMod.NPCs.BasaltBarriers.Minions;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.BasaltBarriers.Boss
{
    partial class BasaltBarrier : ModNPC
    {
        public override void PostAI()
        {
            AITimer++;

            jawRotation = MathHelper.SmoothStep(jawRotation, animState, .15f);
            eyeOpacity = MathHelper.SmoothStep(eyeOpacity, showEye ? 1 : 0, .15f);
        }

        void ManageForsaken()
        {
            MinionCount = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == NPCType<Forsaken>() && Main.npc[i].ai[3] == NPC.whoAmI)
                {
                    MinionCount++;
                }
            }
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

                if (target.Center.X < NPC.Left.X - 70 && NPC.spriteDirection == 1 || target.Center.X > NPC.Right.X + 70 && NPC.spriteDirection == -1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, target.Center);

                    target.immune = false;
                    target.Hurt(teleport, 10000, NPC.spriteDirection, false, false, 0, false, 50);
                    target.dead = true;
                    target.active = false;

                    int numDusts = 50;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(target.Center, 0, 0, 65, Scale: 3f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = false;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 21), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
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

                    int numDusts = 40;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(target.Center, 0, 0, 65, Scale: 3f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = false;
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8, 17), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
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
