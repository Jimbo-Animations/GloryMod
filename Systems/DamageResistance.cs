using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace GloryMod.Systems
{
    class DamageResistance : GlobalNPC
    {
        public float DR = 1f;

        public override bool InstancePerEntity => true;

        public static DamageResistance modNPC(NPC npc)
        {
            return npc.GetGlobalNPC<DamageResistance>();
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (DR != 1f) //So that this code doesn't run when it won't do anything. Doesn't do much but it's a good habit for optimization reasons.
            {
                modifiers.FinalDamage *= DR;
            }
        }
    }
}

