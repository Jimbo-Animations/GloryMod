namespace GloryMod.NPCs.IgnitedIdol
{
    public class FlamesOfJudgement : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;  // Is it a debuff?
            Main.pvpBuff[Type] = true; // Players can give other players buffs, which are listed as pvpBuff
            Main.buffNoSave[Type] = true; // Causes this buff not to persist when exiting and rejoining the world
        }


        // Allows you to make this buff give certain effects to the given player
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<IIDebuff1Player>().Burning = true;
        }
    }

    public class IIDebuff1Player : ModPlayer
    {
        // Flag checking when life regen debuff should be activated
        public bool Burning;

        public override void ResetEffects()
        {
            Burning = false;
        }

        public override void PostUpdate()
        {
            if (Burning)
            {
                Player.statDefense *= 0;
                Main.dust[Dust.NewDust(Player.position, Player.width, Player.height, 6, Scale: 1f)].noGravity = true;
            }               
        }

        public override void UpdateBadLifeRegen()
        {
            if (Burning)
            {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 8;
            }
        }
    }
    public class FlamesOfRetribution : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;  // Is it a debuff?
            Main.pvpBuff[Type] = true; // Players can give other players buffs, which are listed as pvpBuff
            Main.buffNoSave[Type] = true; // Causes this buff not to persist when exiting and rejoining the world
        }


        // Allows you to make this buff give certain effects to the given player
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<IIDebuff2Player>().Burning = true;
        }
    }

    public class IIDebuff2Player : ModPlayer
    {
        // Flag checking when life regen debuff should be activated
        public bool Burning;

        public override void ResetEffects()
        {
            Burning = false;
        }

        public override void PostUpdate()
        {
            if (Burning)
            {
                Player.statDefense *= 0;
                Main.dust[Dust.NewDust(Player.position, Player.width, Player.height, 59, Scale: 1f)].noGravity = true;
            }
        }

        public override void UpdateBadLifeRegen()
        {
            if (Burning)
            {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 10;
            }
        }
    }
}
