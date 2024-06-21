using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.DataStructures;
using GloryMod.NPCs.BloodMoon.BloodSeekerBeast;
using GloryMod.NPCs.BloodMoon.Hemolitionist;

namespace GloryMod.Systems.BossBars
{
    public class NullBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;
        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            return false;
        }
    }

    public class NeonBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            // Display the previously assigned head index
            if (bossHeadIndex != -1)
            {
                return TextureAssets.NpcHeadBoss[bossHeadIndex];
            }
            return null;
        }
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            // Here the game wants to know if to draw the boss bar or not. Return false whenever the conditions don't apply.
            // If there is no possibility of returning false (or null) the bar will get drawn at times when it shouldn't, so write defensive code!
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (!npc.active || npc.hide)
                return false;

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            life = npc.life;
            lifeMax = npc.lifeMax;

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            drawParams.IconScale = 0.9f;

            // Make the bar shake as it loses health in its final phase.
            float lifePercent = drawParams.Life / drawParams.LifeMax;
            float shakeIntensity = drawParams.Life <= drawParams.LifeMax * (Main.getGoodWorld ? 0.7f : 0.55f) && npc.dontTakeDamage != true ? Terraria.Utils.Clamp(1f - lifePercent - 0.2f, 0f, 1f) : 0;
            drawParams.BarCenter.Y -= 20f;
            drawParams.BarCenter += Main.rand.NextVector2Circular(0.5f, 0.5f) * shakeIntensity * 5f;

            return true;
        }
    }

    public class HemolitionistBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            if (bossHeadIndex != -1)
            {
                return TextureAssets.NpcHeadBoss[bossHeadIndex];
            }
            return null;
        }
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (!npc.active || npc.ai[0] == 0)
                return false;

            if (npc.type == NPCType<BSBHead>() && npc.ai[2] == 0)
                return false;

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            life = npc.life;
            lifeMax = npc.lifeMax;

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            drawParams.IconScale = 0.9f;        

            if (npc.type == NPCType<Hemolitionist>())
            {
                // Make the bar shake as it loses health in its final phase.
                float lifePercent = 1;

                if (drawParams.Life < (drawParams.LifeMax / 3) * 2)
                {
                    lifePercent = 0.75f;

                    if (drawParams.Life < (drawParams.LifeMax / 3))
                    {
                        lifePercent = 0.25f;
                    }
                }

                float shakeIntensity = Terraria.Utils.Clamp(1f - lifePercent - 0.2f, 0f, 1f);

                drawParams.BarCenter.Y -= 20f;
                drawParams.BarCenter += Main.rand.NextVector2Circular(0.5f, 0.5f) * shakeIntensity * 5;
            }

            return true;
        }
    }

    public class IgnitedIdolBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            if (bossHeadIndex != -1)
            {
                return TextureAssets.NpcHeadBoss[bossHeadIndex];
            }
            return null;
        }
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (!npc.active || npc.ai[0] == 0 || npc.ai[0] == 5 || npc.ai[0] == 10)
                return false;

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            life = npc.life;
            lifeMax = npc.lifeMax;

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            drawParams.IconScale = 0.9f;

            if (npc.ai[0] > 5)
            {
                drawParams.BarTexture = Request<Texture2D>(Texture + "2").Value;
            }


            // Make the bar shake when it is on 0 health.
            float lifePercent = 1;

            if (npc.life <= 1)
            {
                lifePercent = 0.5f;
            }

            float shakeIntensity = Terraria.Utils.Clamp(1f - lifePercent - 0.2f, 0f, 1f);

            drawParams.BarCenter.Y -= 20f;
            drawParams.BarCenter += Main.rand.NextVector2Circular(0.5f, 0.5f) * shakeIntensity * 5;

            return true;
        }
    }

    public class GeomancerBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            if (bossHeadIndex != -1)
            {
                return TextureAssets.NpcHeadBoss[bossHeadIndex];
            }
            return null;
        }

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (!npc.active || npc.dontTakeDamage && npc.ai[0] == 7)
                return false;

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            life = npc.life;
            lifeMax = npc.lifeMax;

            return true;
        }
    }

    public class SightseerBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            if (bossHeadIndex != -1)
            {
                return TextureAssets.NpcHeadBoss[bossHeadIndex];
            }
            return null;
        }

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (!npc.active || npc.ai[0] == 0 || npc.ai[0] == 6)
                return false;

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            life = npc.life;
            lifeMax = npc.lifeMax;
            shield = !npc.dontTakeDamage ? 0 : life;
            shieldMax = npc.lifeMax;

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            drawParams.IconScale = 0.9f;

            // Make the bar shake as it loses health in its final phase.
            float lifePercent = drawParams.Life / drawParams.LifeMax;
            float shakeIntensity = drawParams.Life <= drawParams.LifeMax * 0.3f ? Terraria.Utils.Clamp(1f - lifePercent - 0.2f, 0f, 1f) : 0;
            drawParams.BarCenter.Y -= 20f;
            drawParams.BarCenter += Main.rand.NextVector2Circular(0.5f, 0.5f) * shakeIntensity * 5f;

            return true;
        }
    }
}
