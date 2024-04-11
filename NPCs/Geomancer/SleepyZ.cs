using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace GloryMod.NPCs.Geomancer
{
    internal class SleepyZ : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 14, 14);
        }

        public override bool Update(Dust dust)
        {
            // Move the dust based on its velocity and reduce its size to then remove it, as the 'return false;' at the end will prevent vanilla logic.
            dust.velocity.X *= 0.95f;
            dust.position += dust.velocity;
            dust.scale += 0.01f;
            dust.alpha += 6;

            if (dust.scale > 1.5f)
                dust.active = false;

            return false;
        }
    }
}
