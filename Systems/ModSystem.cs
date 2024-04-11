using System.Collections.Generic;
using Terraria.ModLoader.IO;
using System.IO;

namespace GloryMod.Systems
{
    internal class GlorySystem : ModSystem
    {
        public static bool DownedRedTideWarrior;
        public static bool DownedHemolitionist;

        public override void OnWorldLoad()
        {
            DownedRedTideWarrior = false;
            DownedHemolitionist = false;
        }

        public override void OnWorldUnload()
        {
            DownedRedTideWarrior = false;
            DownedHemolitionist = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var downed = new List<string>();

            if (DownedRedTideWarrior) downed.Add("RedtideWarrior");
            if (DownedHemolitionist) downed.Add("Hemolitionist");

            tag.Add("downed", downed);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            var downed = tag.GetList<string>("downed");

            DownedRedTideWarrior = downed.Contains("RedtideWarrior");
            DownedHemolitionist = downed.Contains("Hemolitionist");
        }

        public override void NetSend(BinaryWriter writer)
        {
            // Order of operations is important and has to match that of NetReceive
            var flags = new BitsByte();
            flags[0] = DownedRedTideWarrior;
            flags[1] = DownedHemolitionist;
            writer.Write(flags);
        }


        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            DownedRedTideWarrior = flags[0];
            DownedHemolitionist = flags[1];
        }
    }
}