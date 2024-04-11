namespace GloryMod.NPCs.Sightseer
{
    public class SeersTag : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;  // Is it a debuff?
            Main.buffNoSave[Type] = true; // Causes this buff not to persist when exiting and rejoining the world
            Main.buffNoTimeDisplay[Type] = true;
        }


        // Allows you to make this buff give certain effects to the given player
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<SeersTagPlayer>().Tagged = true;
        }
    }

    public class SeersTagPlayer : ModPlayer
    {
        // Flag checking when life regen debuff should be activated
        public bool Tagged;

        public override void ResetEffects()
        {
            Tagged = false;
        }
    }
}

