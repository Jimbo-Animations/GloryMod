namespace GloryMod.CoolEffects
{
    internal class ApplyShaders : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player)
        {
            return true;
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override void SpecialVisuals(Player player, bool isActive)
        {
           player.ManageSpecialBiomeVisuals("GloryMod:SightseerScreen", NPC.AnyNPCs(NPCType<NPCs.Sightseer.Sightseer>()));
        }
    }
}
