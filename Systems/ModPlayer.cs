using static Terraria.ModLoader.ModContent;
using GloryMod.NPCs.Nerd;

namespace GloryMod.Systems
{
    class GloryModPlayer : ModPlayer
    {
        public static GloryModPlayer GloryPlayer(Player Player)
        {
            return Player.GetModPlayer<GloryModPlayer>();
        }

        public override void PreUpdate()
        {
            if (!Main.dayTime && Main.time == 27000 && Player.ZoneGraveyard) NPC.SpawnOnPlayer(Player.whoAmI, NPCType<Nerd>());
        }
    }
}
