using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.WorldBuilding;
using MonoMod.Cil;
using System.Reflection;
using Mono.Cecil.Cil;
using Terraria.GameContent;
using ReLogic.Graphics;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.GameContent.UI.BigProgressBar;
using System.Threading;

namespace GloryMod.Systems
{
    static class Utils
    {
        public static Vector2 findGroundUnder(this Vector2 position)
        {
            Vector2 returned = position;
            while (!WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Down(1), new GenCondition[]
                {
                new Conditions.IsSolid()
                }), out _))
            {
                returned.Y++;
            }

            return returned;
        }

        public static Vector2 findSurfaceAbove(this Vector2 position)
        {
            Vector2 returned = position;
            while (WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Up(1), new GenCondition[]
                {
                new Conditions.IsSolid()
                }), out _))
            {
                returned.Y++;
            }

            return returned;
        }

        public static Vector2 findCeilingAbove(this Vector2 position)
        {
            Vector2 returned = position;
            while (!WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Up(1), new GenCondition[]
                {
        new Conditions.IsSolid()
                }), out _))
            {
                returned.Y--;
            }

            return returned;
        }

        public static Vector2 findWallLeft(this Vector2 position)
        {
            Vector2 returned = position;
            while (!WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Left(1), new GenCondition[]
                {
        new Conditions.IsSolid()
                }), out _))
            {
                returned.X--;
            }

            return returned;
        }

        public static Vector2 findWallRight(this Vector2 position)
        {
            Vector2 returned = position;
            while (!WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Right(1), new GenCondition[]
                {
        new Conditions.IsSolid()
                }), out _))
            {
                returned.X++;
            }

            return returned;
        }

        public static bool CloseTo(this float f, float target, float range = 1f)
        {
            return f > target - range && f < target + range;
        }
    }

    class ScreenUtils : ModSystem
    {
        public static float screenShaking = 0f;

        public static void ScreenShake(float Intensity)
        {
            if (screenShaking < Intensity)
            {
                screenShaking = Intensity;
            }
        }

        public override void PostUpdateEverything()
        {
            screenShaking *= 0.95f;
            if ((int)Math.Round(screenShaking) == 0)
            {
                screenShaking = 0;
            }
        }

        float zoomBefore;
        public static float zoomAmount;
        public static Vector2 cameraChangeStartPoint;
        public static Vector2 CameraChangePos;
        public static float CameraChangeTransition;
        public static int CameraChangeLength;
        public static bool isChangingCameraPos;

        public static void ChangeCameraPos(Vector2 pos, int length, float zoom = 1.65f)
        {
            cameraChangeStartPoint = Main.screenPosition;
            CameraChangeLength = length;
            CameraChangePos = pos - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            isChangingCameraPos = true;
            CameraChangeTransition = 0;
            if (Main.GameZoomTarget < zoom)
                zoomAmount = zoom;
        }

        public override void ModifyScreenPosition()
        {
            Player player = Main.LocalPlayer;
            if (!isChangingCameraPos)
            {
                zoomBefore = Main.GameZoomTarget;
            }
            if (isChangingCameraPos)
            {
                if (CameraChangeLength > 0)
                {
                    if (zoomAmount != 1 && zoomAmount != zoomBefore)
                    {
                        Main.GameZoomTarget = Terraria.Utils.Clamp(Main.GameZoomTarget + 0.05f, 1f, zoomAmount);
                    }
                    if (CameraChangeTransition <= 1f)
                    {
                        Main.screenPosition = Vector2.SmoothStep(cameraChangeStartPoint, CameraChangePos, CameraChangeTransition += 0.025f);
                    }
                    else
                    {
                        Main.screenPosition = CameraChangePos;
                    }
                    CameraChangeLength--;
                }
                else if (CameraChangeTransition >= 0)
                {
                    if (Main.GameZoomTarget != zoomBefore)
                    {
                        Main.GameZoomTarget -= 0.05f;
                    }
                    Main.screenPosition = Vector2.SmoothStep(player.Center - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), CameraChangePos, CameraChangeTransition -= 0.05f);
                }
                else
                {
                    isChangingCameraPos = false;
                }
            }

            Vector2 ScreenOffset = new Vector2(Main.rand.NextFloat(-screenShaking, screenShaking), Main.rand.NextFloat(-screenShaking, screenShaking));
            Main.screenPosition += ScreenOffset;
        }
    }

    static class General
    {
        public static void InvokeOnMainThread(Action action)
        {
            if (!AssetRepository.IsMainThread)
            {
                ManualResetEvent evt = new(false);

                Main.QueueMainThreadAction(() =>
                {
                    action();
                    evt.Set();
                });

                evt.WaitOne();
            }
            else
                action();
        }
    }
}
