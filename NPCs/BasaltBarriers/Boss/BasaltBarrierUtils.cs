using Terraria.Audio;
using Terraria.DataStructures;
/*
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

        void WallMovement(Vector2 targetLocation, float rotateTo, int wallDirection, float wallSpeed = 1, float wallAcc = 0.05f, float headSpeed = 0.025f, float rotationSpeed = 0.025f)
        {
            // Wall and head movement.

            NPC.velocity.X = MathHelper.SmoothStep(NPC.velocity.X, wallSpeed * wallDirection, wallAcc);
            NPC.position.Y = MathHelper.SmoothStep(NPC.position.Y, targetLocation.Y - (NPC.height / 2), headSpeed);

            // Head rotation.

            NPC.rotation = NPC.rotation.AngleLerp(rotateTo + (NPC.spriteDirection == 1 ? MathHelper.Pi : -MathHelper.Pi), rotationSpeed);

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
*/