using System.Collections.Generic;

namespace GloryMod.NPCs.Sightseer
{
    internal partial class Sightseer : ModNPC
    {
        float timer;
        float visibility;
        float spriteWidth = 1;
        bool beVisible = false;
        bool useSilhouette = false;
        List<Tuple<Vector2, float, float>> flash = new List<Tuple<Vector2, float, float>>();
        List<Tuple<Vector2, float, float>> eyeFlash = new List<Tuple<Vector2, float, float>>();
        List<Tuple<Vector2, float, float>> rings = new List<Tuple<Vector2, float, float>>();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D mask2 = Request<Texture2D>(Texture + "Mask2").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            //Makes sure it does not draw its normal code for its bestiary entry.
            if (!NPC.IsABestiaryIconDummy)
            {
                spriteWidth = MathHelper.SmoothStep(spriteWidth, 1, 0.2f);
                if (beVisible) visibility = MathHelper.SmoothStep(visibility, 1, 0.2f);

                spriteBatch.Draw(texture, drawPos, NPC.frame, useSilhouette ? new Color(0, 0, 0, 255) * visibility : drawColor * visibility, NPC.rotation, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0f);
                spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White * visibility, NPC.rotation, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0f);

                for (int i = 0; i < 4; i++)
                {
                    Main.EntitySpriteDraw(mask2, drawPos + new Vector2(8 - (4 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, Color.White * visibility * 0.5f,
                    NPC.rotation, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0);
                }

                void MirrorChaseVisuals(int mirageCount)
                {
                    for (int i = 1; i < mirageCount; i++)
                    {
                        Vector2 MirrorPos = target.Center - new Vector2(target.Center.X - NPC.Center.X, target.Center.Y - NPC.Center.Y).RotatedBy(MathHelper.TwoPi * i / mirageCount) - screenPos;

                        spriteBatch.Draw(texture, MirrorPos, NPC.frame, new Color(0, 0, 0, 255) * visibility, NPC.rotation + (MathHelper.TwoPi / mirageCount) * i, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0f);
                        spriteBatch.Draw(mask, MirrorPos, NPC.frame, Color.White * visibility, NPC.rotation + (MathHelper.TwoPi / mirageCount) * i, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0f);

                        for (int j = 0; j < 4; j++)
                        {
                            Main.EntitySpriteDraw(mask2, MirrorPos + new Vector2(8 - (4 * visibility), 0).RotatedBy(timer + j * MathHelper.TwoPi / 4), NPC.frame, Color.White * visibility * 0.5f,
                            NPC.rotation + (MathHelper.TwoPi / mirageCount) * i, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0);
                        }
                    }
                }


                if (AIstate == AttackPattern.MirrorChase && visibility != 0) MirrorChaseVisuals(phase2Started ? 6 : 4);
                if (AIstate == AttackPattern.MirrorChase2 && visibility != 0) MirrorChaseVisuals(8);

                Texture2D star = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;
                Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
                Texture2D ring = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_2").Value;

                for (int i = 0; i < flash.Count; i++)
                {
                    if (i >= flash.Count)
                    {
                        break;
                    }

                    flash[i] = new Tuple<Vector2, float, float>(flash[i].Item1, flash[i].Item2 + flash[i].Item3, flash[i].Item3);

                    Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White * (1 - visibility), Main.GameUpdateCount * -0.033f + NPC.rotation, star.Size() / 2, new Vector2(0.1f, 1) * (1 + visibility), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White * (1 - visibility), Main.GameUpdateCount * -0.033f + NPC.rotation + MathHelper.PiOver2, star.Size() / 2, new Vector2(0.1f, 1) * (1 + visibility), SpriteEffects.None, 0);

                    Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White * (1 - visibility), Main.GameUpdateCount * 0.025f + NPC.rotation, star.Size() / 2, new Vector2(0.2f, 2) * (1 + visibility * 2), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White * (1 - visibility), Main.GameUpdateCount * 0.025f + NPC.rotation + MathHelper.PiOver2, star.Size() / 2, new Vector2(0.2f, 2) * (1 + visibility * 2), SpriteEffects.None, 0);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                    Main.EntitySpriteDraw(glow, flash[i].Item1 - Main.screenPosition, null, Color.White * (1 - visibility), Main.GameUpdateCount * 0.025f + NPC.rotation, glow.Size() / 2, 1 + visibility, SpriteEffects.None, 0);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                    if (flash[i].Item2 >= 20)
                    {
                        flash.RemoveAt(i);
                    }
                }

                for (int i = 0; i < eyeFlash.Count; i++)
                {
                    if (i >= eyeFlash.Count)
                    {
                        break;
                    }

                    eyeFlash[i] = new Tuple<Vector2, float, float>(eyeFlash[i].Item1, eyeFlash[i].Item2 + eyeFlash[i].Item3, eyeFlash[i].Item3);

                    Main.EntitySpriteDraw(star, drawPos + new Vector2(0, phase3Started ? -64 : -42).RotatedBy(NPC.rotation), null, Color.White * ((20 - eyeFlash[i].Item2) / 20), NPC.rotation, star.Size() / 2, new Vector2(0.25f, 2.5f) * (1 + (eyeFlash[i].Item2 / 20)), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(star, drawPos + new Vector2(0, phase3Started ? -64 : -42).RotatedBy(NPC.rotation), null, Color.White * ((20 - eyeFlash[i].Item2) / 20), NPC.rotation + MathHelper.PiOver2, star.Size() / 2, new Vector2(0.25f, 2.5f) * (1 + (eyeFlash[i].Item2 / 20)), SpriteEffects.None, 0);

                    if (eyeFlash[i].Item2 >= 20)
                    {
                        eyeFlash.RemoveAt(i);
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                for (int i = 0; i < rings.Count; i++)
                {
                    if (i >= rings.Count)
                    {
                        break;
                    }

                    Main.EntitySpriteDraw(ring, rings[i].Item1 - Main.screenPosition, null, new Color(47, 250, 255, 100), 0, ring.Size() / 2, rings[i].Item2 / ring.Width, SpriteEffects.None, 0);

                    rings[i] = new Tuple<Vector2, float, float>(rings[i].Item1, rings[i].Item2 + rings[i].Item3, rings[i].Item3);
                    if (rings[i].Item2 >= target.Distance(rings[i].Item1) + Main.screenWidth * 3)
                    {
                        rings.RemoveAt(i);
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
            }
            else
            {
                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, 0, drawOrigin, 0.6f, SpriteEffects.None, 0f);
                spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White, 0, drawOrigin, 0.6f, SpriteEffects.None, 0f);
                animState = 3;
                animSpeed = 6;
            }
            
            return false;
        }
    }
}
