global using Terraria.ModLoader;
global using System;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.Graphics.Effects;
global using Terraria.ID;
global using static Terraria.ModLoader.ModContent;
using ReLogic.Content;
using Microsoft.Xna.Framework.Audio;
using System.Reflection;
using Terraria.Audio;
using Terraria.Graphics.Shaders;

namespace GloryMod
{
    public class GloryMod : Mod
    {
        public static Vector2 screenFollowPosition;
        public static float maxScreenLerp;
        public static float currentScreenLerp;
        public static int screenTransitionTime;
        public static int screenFollowTime;

        public class TemporaryFix : PreJITFilter
        {
            public override bool ShouldJIT(MemberInfo member) => false;
        }

        // Reference to the main instance of the mod
        public static GloryMod Mod { get; private set; }

        public static GloryMod Instance { get; set; }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                Instance = null;
            }
        }

        public static void SetScreenToPosition(int duration, int transitionTime, Vector2 position, float maxLerp = 1f)
        {
            screenFollowTime = duration;
            screenTransitionTime = transitionTime;
            currentScreenLerp = 0f;
            maxScreenLerp = maxLerp;
            screenFollowPosition = position;
        }

        public override void Load()
        {
            /*Systems.General.InvokeOnMainThread(() =>
            {
                Directory.CreateDirectory(savingFolder);
                string path = Path.Combine(savingFolder, "Shine.png");
                using (Stream stream = File.OpenWrite(path))
                {
                    CreateImage(1000, 1000).SaveAsPng(stream, 1000, 1000);
                }
            });*/

            Request<SoundEffect>("GloryMod/Music/Nap_Time", AssetRequestMode.ImmediateLoad);

            Request<SoundEffect>("GloryMod/Music/GroundSlam", AssetRequestMode.ImmediateLoad);

            Request<SoundEffect>("GloryMod/Music/IgnitedIdolIntro", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("GloryMod/Music/IgnitedIdolGroan", AssetRequestMode.ImmediateLoad);

            Request<SoundEffect>("GloryMod/Music/SightseerAttack", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("GloryMod/Music/SightseerShriek", AssetRequestMode.ImmediateLoad);

            Request<SoundEffect>("GloryMod/Music/HemolitionistRoar", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("GloryMod/Music/HemolitionistRoarAlt", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("GloryMod/Music/HemolitionistDeathray", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("GloryMod/Music/HemolitionistPulseIndicator", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("GloryMod/Music/HemolitionistEnergyScream", AssetRequestMode.ImmediateLoad);
            new SoundStyle("GloryMod/Music/HemoHit", 4, SoundType.Sound);

            if (Main.netMode != NetmodeID.Server)
            {
                Ref<Effect> SightseerRef = new(Request<Effect>("GloryMod/CoolEffects/SightseerScreen", AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene["GloryMod:SightseerScreen"] = new Filter(new ScreenShaderData(SightseerRef, "Sightseer"), EffectPriority.VeryHigh);
                Filters.Scene["GloryMod:SightseerScreen"].Load();
            }
        }
    }
}