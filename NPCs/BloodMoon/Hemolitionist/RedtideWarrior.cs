using Terraria.Audio;
using static Terraria.ModLoader.ModContent;
using GloryMod.Systems;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace GloryMod.NPCs.BloodMoon.Hemolitionist
{
    internal class RedtideWarrior : ModNPC
    {
        public override string Texture => "GloryMod/NPCs/BloodMoon/Hemolitionist/RedtideWarriorBody";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.ImmuneToAllBuffs[NPC.type] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = NPCType<RedtideWarrior>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                new FlavorTextBestiaryInfoElement("The sanguine seas of a Blood Moon have a corruptive and transformative power on all that remain in contact with it. " +
                "Such effects are disastrous for the peoples of the deep sea, who are forced to flee the oceans before moonrise, or be consumed by its dark energies...")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 66;
            NPC.height = 98;
            DrawOffsetY = 0f;
            NPC.alpha = 255;

            NPC.damage = 0;          
            NPC.defense = 30;
            NPC.lifeMax = Main.getGoodWorld ? 50000 : 40000;
            NPC.spriteDirection = 1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.npcSlots = 1f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
        }

        private float SpearSummonAlpha = 0;
        private float SpearAlpha = 0;

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest(true);

            if (NPC.ai[0] == 0)
            {
                NPC.velocity.Y -= 2.5f;
            }

            if (NPC.alpha > 0)
            {
                NPC.alpha -= 13;

                if (NPC.alpha < 0)
                {
                    NPC.alpha = 0;
                }
            }

            if (NPC.ai[0] == 50)
            {
                CombatText.NewText(NPC.getRect(), new Color(218, 59, 31), "Halt, demon!", true);
                SoundEngine.PlaySound(SoundID.Zombie34, NPC.Center);
            }

            if (NPC.ai[0] == 200)
            {
                CombatText.NewText(NPC.getRect(), new Color(218, 59, 31), "Far too long have your kind terrorized my people.", true);
                SoundEngine.PlaySound(SoundID.Zombie35, NPC.Center);
            }

            if (NPC.ai[0] == 350)
            {
                CombatText.NewText(NPC.getRect(), new Color(218, 59, 31), "Every night, when the sky is lit by a red moon,", true);
                SoundEngine.PlaySound(SoundID.Zombie35, NPC.Center);
            }

            if (NPC.ai[0] == 475)
            {
                CombatText.NewText(NPC.getRect(), new Color(218, 59, 31), "my kin are forced to flee as you lay waste to our homes...", true);
                SoundEngine.PlaySound(SoundID.Zombie34, NPC.Center);
            }

            if (NPC.ai[0] == 600)
            {
                NPC.ai[1] = 1;
            }

            if (NPC.ai[0] == 700)
            {
                CombatText.NewText(NPC.getRect(), new Color(218, 59, 31), "Tonight, I will put an end to this curse.", true);
                SoundEngine.PlaySound(SoundID.Zombie35, NPC.Center);
            }

            if (NPC.ai[0] == 825)
            {
                NPC.ai[2] = 2;
            }


            if (NPC.ai[0] == 900)
            {
                CombatText.NewText(NPC.getRect(), new Color(218, 59, 31), "Prepare yourself, fiend.", true);
                SoundEngine.PlaySound(SoundID.Zombie35, NPC.Center);
            }

            if (NPC.ai[0] == 1050)
            {
                CombatText.NewText(NPC.getRect(), new Color(255, 60, 30), "As the champion of these seas;", true);
                SoundEngine.PlaySound(SoundID.Zombie34, NPC.Center);
            }

            if (NPC.ai[0] == 1125)
            {
                CombatText.NewText(NPC.getRect(), new Color(255, 60, 30), "I will strike-");
                SoundEngine.PlaySound(SoundID.Zombie34, NPC.Center);
            }


            if (NPC.ai[2] == 2 && NPC.ai[1] != 2)
            {
                if (SpearSummonAlpha < 1 && SpearAlpha == 0)
                {
                    SpearSummonAlpha += 0.1f;
                }
                if (SpearSummonAlpha >= 1)
                {
                    SpearAlpha = 1;
                    SoundEngine.PlaySound(SoundID.Item44, NPC.Center);
                }
                if (SpearAlpha == 1)
                {
                    SpearSummonAlpha -= 0.1f;
                }
            }

            if (NPC.ai[1] == 2)
            {
                CombatText.NewText(NPC.getRect(), Color.Red, "KABOOMM!!!", true);
                NPC.SimpleStrikeNPC(100000, 0, true);
                SoundEngine.PlaySound(SoundID.DD2_WyvernScream, NPC.Center);
            }
          
            NPC.direction = NPC.spriteDirection = Main.player[NPC.target].Center.X > NPC.Center.X ? 1 : -1;
            NPC.ai[0]++;
            NPC.velocity *= 0.98f;
        }

        private int ArmsCounter;
        private int BlinkCounter;
        private int ArmsFrameX;
        private int ArmsFrameY;
        private bool JustBlinked = false;

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 2;
            NPC.frameCounter++;
            ArmsCounter++;
            if (NPC.frameCounter >= 6 && NPC.ai[1] != 2)
            {
                if (JustBlinked == true)
                {
                    JustBlinked = false;
                    NPC.frame.X -= NPC.frame.Width;
                }

                BlinkCounter++;
                NPC.frame.Y += frameHeight;
                if (BlinkCounter > 10 && Main.rand.NextBool(5) == true)
                {
                    BlinkCounter = 0;
                    NPC.frame.X += NPC.frame.Width;
                    JustBlinked = true;
                }

                NPC.frameCounter = 0.0;
            }
            if (NPC.frame.Y >= frameHeight * 5)
            {
                NPC.frame.Y = 0;
            }

            if (NPC.ai[1] == 0)
            {
                ArmsFrameX = 0;

                if (ArmsCounter >= 6)
                {
                    ArmsCounter = 0;
                    ArmsFrameY += frameHeight;
                }

                if (ArmsFrameY >= frameHeight * 5)
                {
                    ArmsFrameY = 0;
                }
            }

            if (NPC.ai[1] == 1)
            {
                if (NPC.ai[2] == 0)
                {
                    NPC.ai[2] = 1;
                    ArmsFrameY = 0;
                    ArmsFrameX += NPC.frame.Width;
                }            

                if (ArmsCounter >= 6)
                {
                    ArmsCounter = 0;
                    ArmsFrameY += frameHeight;
                }

                if (ArmsFrameY >= frameHeight * 5)
                {
                    ArmsFrameY = frameHeight * 4;
                }
            }
        }

        public override bool CheckDead()
        {
            int gore1 = Mod.Find<ModGore>("RedtideWarriorGore1").Type;
            int gore2 = Mod.Find<ModGore>("RedtideWarriorGore2").Type;
            int gore3 = Mod.Find<ModGore>("RedtideWarriorGore3").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2 (-7 * NPC.spriteDirection, -3), gore1);
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(6 * NPC.spriteDirection, -4), gore2);
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(3 * NPC.spriteDirection, -7), gore3);
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/NPCs/BloodMoon/Hemolitionist/RedtideWarriorBody").Value;
            Texture2D textureMask = Request<Texture2D>("GloryMod/NPCs/BloodMoon/Hemolitionist/RedtideWarriorMask").Value;
            Texture2D textureArms = Request<Texture2D>("GloryMod/NPCs/BloodMoon/Hemolitionist/RedtideWarriorArms").Value;
            Texture2D textureSpear = Request<Texture2D>("GloryMod/NPCs/BloodMoon/Hemolitionist/RedtideWarriorSpear").Value;
            Texture2D textureSpearSummon = Request<Texture2D>("GloryMod/NPCs/BloodMoon/Hemolitionist/RedtideWarriorSpearSummon").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            Rectangle armsRect = new Rectangle(ArmsFrameX, ArmsFrameY, texture.Width / 2, NPC.height);

            //Makes sure it does not draw its normal code for its bestiary entry.
            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(textureMask, drawPos, NPC.frame, new Color(255, 255, 255), NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(textureArms, drawPos, armsRect, drawColor, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(textureSpear, drawPos, NPC.frame, drawColor * SpearAlpha, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(textureSpearSummon, drawPos, NPC.frame, new Color(255, 255, 255) * SpearSummonAlpha, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, drawPos + new Vector2(0, 2), NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(textureMask, drawPos + new Vector2(0, 2), NPC.frame, new Color(255, 255, 255), NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(textureArms, drawPos + new Vector2(0, 2), armsRect, drawColor, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }
    }
}
