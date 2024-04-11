using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace GloryMod.Systems
{
	class GloryNPC : GlobalNPC
    {
		public override bool InstancePerEntity
		{
			get
			{
				return true;
			}
		}
	}
} 
