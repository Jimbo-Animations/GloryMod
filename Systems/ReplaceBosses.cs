using GloryMod.NPCs.NeonBoss;
using Terraria.Audio;
using GloryMod.NPCs.BloodMoon.Hemolitionist;
using GloryMod.NPCs.Sightseer;
using Terraria.DataStructures;
using GloryMod.NPCs.Geomancer;
using GloryMod.NPCs.Nerd;
using GloryMod.NPCs.BloodMoon.BloodSeekerBeast;

namespace GloryMod.Systems
{
    class ReplaceBosses : GlobalNPC
    {
        public bool ShouldICallReworkCode(bool checkMod = false)
        {
            ModLoader.TryGetMod("WAYFAIRContent", out Mod wayfairContent);

            if (checkMod == true) return wayfairContent == null;
            else
            {
                if (wayfairContent != null)
                {
                    return wayfairContent.Call("GloryReworksEnabled").Equals(true);
                }

                return true;
            }
        }

        public override bool PreAI(NPC npc)
        {

            if (npc.type == NPCID.KingSlime && ShouldICallReworkCode(true))
            {
                npc.hide = true;
            }

            if (npc.type == NPCID.EyeofCthulhu && ShouldICallReworkCode(true))
            {
                npc.hide = true;
            }

            if (npc.type == NPCID.BloodNautilus)
            {
                npc.hide = true;
            }

            if (npc.type == NPCID.Tim || npc.type == NPCID.RuneWizard)
            {
                npc.hide = true;
            }

            return true;
        }

        public override void PostAI(NPC npc)
        {
            Player player = Main.player[npc.target];

            if (npc.type == NPCID.KingSlime && ShouldICallReworkCode(true))
            {
                NPC.NewNPC(npc.GetSource_FromAI(), (int)player.Center.X, (int)player.Center.Y, NPCType<NeonTyrant>(), Target: npc.target);
                npc.damage = 0;
                npc.dontTakeDamage = true;
                npc.active = false;
            }

            if (npc.type == NPCID.EyeofCthulhu && ShouldICallReworkCode(true))
            {
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCType<Sightseer>(), Target: npc.target);
                npc.damage = 0;
                npc.dontTakeDamage = true;
                npc.active = false;
            }

            if (npc.type == NPCID.BloodNautilus)
            {
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCType<Hemolitionist>(), Target: npc.target);
                npc.damage = 0;
                npc.dontTakeDamage = true;
                npc.active = false;
            }

            if (npc.type == NPCID.BloodEelHead)
            {
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCType<BSBHead>(), Target: npc.target);
                npc.damage = 0;
                npc.dontTakeDamage = true;
                npc.active = false;
            }

            if (npc.type == NPCID.Tim || npc.type == NPCID.RuneWizard)
            {
                npc.damage = 0;
                npc.dontTakeDamage = true;
                npc.active = false;
            }
        }
    }

    class ReplaceSummons : GlobalItem
    {
        public bool ShouldICallReworkCode(bool checkMod = false)
        {
            ModLoader.TryGetMod("WAYFAIRContent", out Mod wayfairContent);

            if (checkMod == true) return wayfairContent != null;
            else
            {
                if (wayfairContent != null)
                {
                    return wayfairContent.Call("GloryReworksEnabled").Equals(true);
                }

                return true;
            }
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ItemID.SlimeCrown && ShouldICallReworkCode())
            {
                if (player.whoAmI == Main.myPlayer && !NPC.AnyNPCs(NPCType<NeonTyrant>()))
                {
                    int type = NPCType<NeonTyrant>();
                    item.useStyle = ItemUseStyleID.HoldUp;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // If the player is not in multiplayer, spawn directly
                        NPC.SpawnBoss((int)player.position.X, (int)player.position.Y, type, player.whoAmI);
                    }
                    else
                    {
                        // If the player is in multiplayer, request a spawn
                        // This will only work if NPCID.Sets.MPAllowedEnemies[type] is true, which we set in MinionBossBody
                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                    }

                    SoundEngine.PlaySound(SoundID.Roar, player.position);
                }

                return false;
            }

            if (item.type == ItemID.SuspiciousLookingEye && ShouldICallReworkCode())
            {
                if (player.whoAmI == Main.myPlayer && !NPC.AnyNPCs(NPCType<Sightseer>()) && !Main.dayTime)
                {
                    int type = NPCType<Sightseer>();
                    item.useStyle = ItemUseStyleID.HoldUp;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // If the player is not in multiplayer, spawn directly
                        NPC.SpawnBoss((int)player.position.X, (int)player.position.Y, type, player.whoAmI);
                    }
                    else
                    {
                        // If the player is in multiplayer, request a spawn
                        // This will only work if NPCID.Sets.MPAllowedEnemies[type] is true, which we set in MinionBossBody
                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                    }

                    SoundEngine.PlaySound(SoundID.Roar, player.position);
                }

                return false;
            }

            return true;
        }

        public override bool ConsumeItem(Item item, Player player)
        {
            if (item.type == ItemID.SlimeCrown)
            {
                return false;
            }

            if (item.type == ItemID.SuspiciousLookingEye)
            {
                return false;
            }

            return true;
        }
    }
}
