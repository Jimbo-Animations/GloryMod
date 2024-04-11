
namespace GloryMod.NPCs.BasaltBarriers.Boss
{
    partial class BasaltBarrier : ModNPC
    {
        private int animState = 0;
        private int animSpeed = 5;
        private int wallFrameY;
        float timer;
        float heat;
        float shieldOpacity;
        float jawRotation;

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter > animSpeed)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;

                wallFrameY += 38;
                if (wallFrameY >= 38 * 7) wallFrameY = 0;
            }
            if (NPC.frame.Y >= frameHeight * 7)
            {
                NPC.frame.Y = 0;
            }

            jawRotation = MathHelper.SmoothStep(jawRotation, animState, 0.15f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D textureMask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D wallTexture = Request<Texture2D>(Texture + "Wall").Value;
            Texture2D textureJaw = Request<Texture2D>(Texture + "Jaw").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);

            Rectangle wallRect = new Rectangle(0, wallFrameY, wallTexture.Width, wallTexture.Height / 7);
            Vector2 wallOrigin = new Vector2(wallTexture.Width / 2, wallTexture.Height / 14);

            SpriteEffects effects = SpriteEffects.None;
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipVertically;

            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);
            float wallPos = spawnCenterY + (200 * mult);

            Vector2 position = new Vector2(NPC.Center.X - (70 * NPC.spriteDirection), wallPos + 3800);

            // Big thanks to Ebon for helping me figure out a functioning system!

            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j <= 4; j++)
                {
                    wallTexture = Request<Texture2D>(Texture + "Wall" + j).Value;

                    Main.spriteBatch.Draw(wallTexture, position - screenPos, wallRect,
                    Lighting.GetColor((int)position.X / 16, (int)position.Y / 16), 0, wallOrigin, NPC.scale, effects, 0);

                    wallTexture = Request<Texture2D>(Texture + "Wall" + j + "Mask").Value;

                    Main.spriteBatch.Draw(wallTexture, position - screenPos, wallRect,
                    Lighting.Brightness((int)position.X / 16, (int)position.Y / 16) <= 0.1f ? Color.Black : Color.White, 0, wallOrigin, NPC.scale, effects, 0);

                    position.Y -= 38;
                }
            }

            // Shield Visuals.

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            if (shieldOpacity > 0.01f)
            {
                for (int i = 0; i < 4; i++)
                { // Include code for both the jaw and skull.
                    Main.EntitySpriteDraw(textureJaw, NPC.Center + new Vector2(72 - (60 * heat), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - screenPos, NPC.frame,
                    Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : new Color(0, 0, 0, 20) * shieldOpacity, NPC.rotation - jawRotation,
                    drawOrigin - new Vector2(NPC.frame.Width * (jawRotation * .25f) * NPC.spriteDirection, 0), NPC.scale, effects, 0);

                    Main.EntitySpriteDraw(texture, NPC.Center + new Vector2(72 - (60 * heat), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - screenPos, NPC.frame,
                    Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : new Color(0, 0, 0, 20) * shieldOpacity, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }
            }           

            // Drawing the head.

            Main.spriteBatch.Draw(textureJaw, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation - jawRotation,
            drawOrigin - new Vector2(NPC.frame.Width * (jawRotation * .25f) * NPC.spriteDirection, 0), NPC.scale, effects, 0); // Jaw.

            Main.spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            Main.spriteBatch.Draw(textureMask, NPC.Center - screenPos, NPC.frame, Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : Color.White,
            NPC.rotation, drawOrigin, NPC.scale, effects, 0); // Skull.

            // Purple shield overlay.

            if (shieldOpacity > 0.01f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Main.EntitySpriteDraw(textureJaw, NPC.Center + new Vector2(24 - (20 * heat), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - screenPos, NPC.frame,
                    Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : new Color(25, 10, 40, 75) * shieldOpacity, NPC.rotation - jawRotation,
                    drawOrigin - new Vector2(NPC.frame.Width * (jawRotation * .25f) * NPC.spriteDirection, 0), NPC.scale, effects, 0); // Jaw.

                    Main.EntitySpriteDraw(texture, NPC.Center + new Vector2(24 - (20 * heat), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - screenPos, NPC.frame,
                    Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : new Color(25, 10, 40, 75) * shieldOpacity, NPC.rotation, drawOrigin, NPC.scale, effects, 0); // Skull.
                }

                Main.EntitySpriteDraw(textureMask, NPC.Center - screenPos, NPC.frame,
                Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) <= 0.1f ? Color.Black : Color.Purple * heat, NPC.rotation, drawOrigin, NPC.scale, effects, 0); // Extra mask.

                Lighting.AddLight(NPC.Center, new Vector3(.25f, .1f, .4f) * heat);
            }

            return false;
        }
    }
}
