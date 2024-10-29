using System.Drawing.Drawing2D;
using GloryMod.Items.BloodMoon.Hemolitionist;
using GloryMod.NPCs.BloodMoon.BloodDrone;
using GloryMod.NPCs.BloodMoon.Hemolitionist.New.Projectiles;
using GloryMod.Systems;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Utilities;
namespace GloryMod.NPCs.BloodMoon.Hemolitionist.New
{
    partial class  Hemolitionist : ModNPC
    {
        private enum AttackPattern
        {
            RedtideIntro = -1,
            StartBattle = 0,
            Intro = 1,
            DashAttack = 2,
            CannonVolleys = 3,
            CarpetBomb = 4,
            MissileBarrage = 5,
            Deathray = 6,
            Telefrag = 7,
            Phase2 = 8,
            DeathAnim = 9
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float AITimer2 => ref NPC.ai[2];
        public ref float AITimer3 => ref NPC.ai[3];

        bool Phase2Start;
        bool Phase2Started;
        bool readyToDie;
        bool meleeActive;
        bool startMusic;
        bool removeArm1;
        bool removeArm2;

        int bodyAnim;
        int armAnim1;
        int armAnim2;
        int armFrameX;
        int armFrameY;
        int armFrameX2;
        int armFrameY2;
        int jetFrameY;
        int screamTimer;
        int cannonTimer;       

        float cannonTele1;
        float cannonTele2;
        float mouthTele;
        float bodyAlpha;
        float maskAlpha;
        float phase2Haze;
        float timer;
        float deathrayRotation;
        NPC targetNPC;
        Vector2 setPoint;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 2;
            NPC.frameCounter++;

            if (NPC.frameCounter > 5)
            {
                NPC.frame.Y += frameHeight;
                armFrameY += frameHeight;
                armFrameY2 += frameHeight;
                jetFrameY += frameHeight;
                NPC.frameCounter = 0f;

                if (bodyAlpha == 1)
                {
                    int dust = Dust.NewDust(NPC.Center + new Vector2(49, 37).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(0, 5).RotatedBy(NPC.rotation);

                    int dust2 = Dust.NewDust(NPC.Center + new Vector2(-57, 37).RotatedBy(NPC.rotation), 0, 0, 114, Scale: 1f);
                    Main.dust[dust2].noGravity = true;
                    Main.dust[dust2].noLight = true;
                    Main.dust[dust2].velocity = new Vector2(0, 5).RotatedBy(NPC.rotation);
                }
            }

            if (jetFrameY >= frameHeight * 4) jetFrameY = 0;
            if (screamTimer > 0) screamTimer--;
            if (cannonTimer > 0) cannonTimer--;


            switch (bodyAnim)
            {
                case 0:

                    NPC.frame.X = 0;
                    NPC.frame.Y = 0;

                    break;

                case 1:

                    NPC.frame.X = 0;

                    if (NPC.frame.Y >= frameHeight * 8)
                    {
                        NPC.frame.Y = 0;
                        bodyAnim = 0;
                    }

                    break;

                case 2:

                    NPC.frame.X = TextureAssets.Npc[NPC.type].Width() / 2;

                    if (NPC.frame.Y >= frameHeight * 8 && screamTimer > 10) NPC.frame.Y = frameHeight * 6;

                    if (NPC.frame.Y >= frameHeight * 10)
                    {                       
                        NPC.frame.Y = 0;
                        bodyAnim = 0;
                    }

                    break;
            }

            switch (armAnim1)
            {
                case 0:

                    armFrameX = 0;
                    armFrameY = 0;

                    break;

                case 1:

                    armFrameX = 0;

                    if (armFrameY >= frameHeight * 5)
                    {
                        armFrameY = frameHeight * 4;
                    }

                    break;

                case 2:

                    armFrameX = 0;

                    if (armFrameY < frameHeight * 5) armFrameY = frameHeight * 5;

                    if (armFrameY >= frameHeight * 8)
                    {
                        armFrameY = 0;
                        armAnim1 = 0;
                    }

                    break;

                case 3:

                    armFrameX = TextureAssets.Npc[NPC.type].Width() / 2;

                    if (armFrameY >= frameHeight * 4 && cannonTimer > 10) armFrameY = 3;

                    if (armFrameY >= frameHeight * 6)
                    {
                        armFrameY = 0;
                        armAnim1 = 0;
                    }

                    break;
            }

            switch (armAnim2)
            {
                case 0:

                    armFrameX2 = 0;
                    armFrameY2 = 0;

                    break;

                case 1:

                    armFrameX2 = 0;

                    if (armFrameY2 >= frameHeight * 5)
                    {
                        armFrameY2 = frameHeight * 4;
                    }

                    break;

                case 2:

                    armFrameX2 = 0;

                    if (armFrameY2 < frameHeight * 5) armFrameY2 = frameHeight * 5;

                    if (armFrameY2 >= frameHeight * 8)
                    {
                        armFrameY2 = 0;
                        armAnim2 = 0;
                    }

                    break;

                case 3:

                    armFrameX2 = TextureAssets.Npc[NPC.type].Width() / 2;

                    if (armFrameY2 >= frameHeight * 6)
                    {
                        armFrameY2 = 0;
                        armAnim2 = 0;
                    }

                    break;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            Music = MusicLoader.GetMusicSlot(Mod, "Music/Redtide_intro");
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && !readyToDie)
            {
                if (Phase2Started)
                {
                    NPC.ai[0] = 9;
                    NPC.life = 1;
                    NPC.dontTakeDamage = true;
                    bodyAnim = 0;
                    ResetValues();
                }
                else
                {
                    NPC.life = 1;
                }            
            }
        }

        public override bool CheckDead()
        {
            int gore1 = Mod.Find<ModGore>("HemolitionistGore1").Type;
            int gore2 = Mod.Find<ModGore>("HemolitionistGore3").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, new Vector2(-4, -5), gore2);
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, new Vector2(6, -3), gore2);
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, -2), gore1);

            return true;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            var SpawnRule = Main.ItemDropsDB.GetRulesForNPCID(NPCID.BloodNautilus, false);
            foreach (var dropRule in SpawnRule)
            {
                npcLoot.Add(dropRule);
            }
            npcLoot.Add(ItemDropRule.Common(ItemID.ChumBucket, minimumDropped: 12, maximumDropped: 20));
            npcLoot.Add(ItemDropRule.Common(ItemType<HemotechThruster>()));
            npcLoot.Add(ItemDropRule.Common(ItemType<ClawBuster>()));
        }

        public override void PostAI()
        {
            if ((!target.active || target.dead || !Main.bloodMoon) && NPC.ai[0] != 0)
            {
                NPC.active = false;
                ScreenUtils.screenShaking += 5f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<RedRoar>(), 0, 0, target.whoAmI);

                    for (int i = 0; i < 12; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1).RotatedBy(i * MathHelper.TwoPi / 12), ProjectileType<DroneBullet>(), 120, 3f, Main.myPlayer, 0, 1);
                    }
                }

                int numDusts = 45;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                SoundEngine.PlaySound(SoundID.Zombie93, NPC.Center);
                NPC.netUpdate = true;
            }

            if (NPC.AnyNPCs(NPCType<RedtideWarrior>())) Music = MusicLoader.GetMusicSlot(Mod, "Music/Redtide_intro");
            else if (startMusic) 
            {
                Main.musicFade[Main.curMusic] = MathHelper.Lerp(0, 1, 1);
                Music = MusicLoader.GetMusicSlot(Mod, "Music/Anemia_Full_Loop2");
            }
            else 
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Music/Silence");
            }

            if (bodyAlpha >= 1) Lighting.AddLight(NPC.Center, new Vector3(.2f, .2f, .2f));
            NPC.damage = meleeActive ? 180 : 0;

            if (NPC.life < NPC.lifeMax - 7500) Phase2Start = true;

            if (NPC.dontTakeDamage == true)
            {
                //cancel all buffs
                for (int i = 0; i < NPC.buffTime.Length; i++)
                {
                    NPC.buffTime[i] = 0;
                }
            }
        }

        private float[] aiWeights = new float[8];
        private void PickAttack()
        {
            if (Phase2Start && !Phase2Started)
            {
                NPC.ai[0] = 8;
            }
            else
            {
                WeightedRandom<int> aiStatePool = new WeightedRandom<int>();
                for (int state = 2; state < (!Phase2Start ? aiWeights.Length - 2 : aiWeights.Length); state++)
                {
                    //weights are squared to bias more towards attacks that haven't been used in a while
                    aiStatePool.Add(state, Math.Pow(aiWeights[state], 2));
                }
                NPC.ai[0] = aiStatePool;

                for (int state = 2; state < (!Phase2Start ? aiWeights.Length - 2 : aiWeights.Length); state++)
                {
                    if (NPC.ai[0] != state)
                        aiWeights[state] += aiWeights[(int)NPC.ai[0]] / (aiWeights.Length - 1);
                }
                aiWeights[(int)NPC.ai[0]] = 0f;
            }        
        }

        private void InitializeAIStates()
        {
            aiWeights[0] = 0;
            for (int state = 2; state < (!Phase2Start ? aiWeights.Length - 2 : aiWeights.Length); state++)
            {
                aiWeights[state] = 1f;
            }
        }

        private void ResetValues(bool enraged = false)
        {            
            AITimer = 0;
            AITimer2 = 0;
            AITimer3 = 0;
            meleeActive = false;
            mouthTele = 0;
            cannonTele1 = 0;
            cannonTele2 = 0;

            NPC.netUpdate = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D textureMask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D textureArm = Request<Texture2D>(Texture + "Arm").Value;
            Texture2D textureArmMask = Request<Texture2D>(Texture + "ArmMask").Value;
            Texture2D textureArmMask2 = Request<Texture2D>(Texture + "ArmMask2").Value;
            Texture2D textureJet = Request<Texture2D>(Texture + "Jet").Value;
            Texture2D textureJetMask = Request<Texture2D>(Texture + "JetMask").Value;
            Texture2D textureJetMask2 = Request<Texture2D>(Texture + "JetMask2").Value;
            Texture2D textureJetMask3 = Request<Texture2D>(Texture + "JetMask3").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width / 2, NPC.frame.Height / 2);
            Vector2 drawPos = NPC.Center + new Vector2(0, 20).RotatedBy(NPC.rotation) - screenPos;
            Vector2 drawPos2 = NPC.Center + new Vector2(0, 20).RotatedBy(NPC.rotation);
            Rectangle armRect = new Rectangle(armFrameX, armFrameY, textureArm.Width / 2, textureArm.Height / 8);
            Rectangle armRect2 = new Rectangle(armFrameX2, armFrameY2, textureArm.Width / 2, textureArm.Height / 8);
            Rectangle jetRect = new Rectangle(0, jetFrameY, textureJet.Width, textureJet.Height / 4);

            float mult = .85f + ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) * .15f);
            float maskOpacity = 5f * phase2Haze * mult * maskAlpha;
            float maskOpacity2 = mult * .35f * bodyAlpha;
            float bodyOpacity = .5f * phase2Haze * mult * bodyAlpha;
            if (timer >= MathHelper.Pi) timer = 0f;
            timer += .125f;
            cannonTele1 = MathHelper.SmoothStep(cannonTele1, 0, .15f);
            cannonTele2 = MathHelper.SmoothStep(cannonTele2, 0, .15f);


            if (Phase2Started)
            {
                for (int i = 0; i < 4; i++)
                {
                    spriteBatch.Draw(texture, drawPos + new Vector2(5 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, Color.Red * bodyOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(textureJetMask3, drawPos + new Vector2(5 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), jetRect, Color.Red * bodyOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                    if (!removeArm1) spriteBatch.Draw(textureArm, drawPos + new Vector2(5 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), armRect, Color.Red * bodyOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                    if (!removeArm2) spriteBatch.Draw(textureArm, drawPos + new Vector2(5 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), armRect2, Color.Red * bodyOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);
                }
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor * bodyAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(textureJet, drawPos, jetRect, drawColor * bodyAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            if (!removeArm1) spriteBatch.Draw(textureArm, drawPos, armRect, drawColor * bodyAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            if (!removeArm2) spriteBatch.Draw(textureArm, drawPos, armRect2, drawColor * bodyAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);

            if (Phase2Started)
            {
                for (int i = 0; i < 4; i++)
                {
                    spriteBatch.Draw(textureMask, drawPos + new Vector2(2 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, Color.Red * maskOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(textureJetMask, drawPos + new Vector2(2 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), jetRect, Color.Red * bodyOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(textureJetMask2, drawPos + new Vector2(2 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), jetRect, Color.Red * maskOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);

                    if (!removeArm1) spriteBatch.Draw(textureArmMask, drawPos + new Vector2(2 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), armRect, Color.Red * maskOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                    if (!removeArm2) spriteBatch.Draw(textureArmMask, drawPos + new Vector2(2 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), armRect2, Color.Red * maskOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);            
                    if (!removeArm1) spriteBatch.Draw(textureArmMask2, drawPos + new Vector2(2 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), armRect, Color.Red * maskOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
                    if (!removeArm2) spriteBatch.Draw(textureArmMask2, drawPos + new Vector2(2 * phase2Haze, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), armRect2, Color.Red * maskOpacity, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);
                }
            }

            spriteBatch.Draw(textureMask, drawPos, NPC.frame, Color.White * maskAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(textureJetMask, drawPos, jetRect, Color.White * bodyAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            if (!removeArm1) spriteBatch.Draw(textureArmMask, drawPos, armRect, Color.White * maskAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            if (!removeArm2) spriteBatch.Draw(textureArmMask, drawPos, armRect2, Color.White * maskAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);

            spriteBatch.Draw(textureJetMask2, drawPos, jetRect, Color.White * maskOpacity2, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            if (!removeArm1) spriteBatch.Draw(textureArmMask2, drawPos, armRect, Color.White * maskOpacity2, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            if (!removeArm2) spriteBatch.Draw(textureArmMask2, drawPos, armRect2, Color.White * maskOpacity2, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            if (cannonTele1 > 0)
            {
                Color color = Color.White * cannonTele1;
                Terraria.Utils.DrawLine(Main.spriteBatch, drawPos2 + new Vector2(-28, 2053).RotatedBy(NPC.rotation), NPC.Center + new Vector2(-28, 53).RotatedBy(NPC.rotation), color);
            }

            if (cannonTele2 > 0)
            {
                Color color = Color.White * cannonTele2;
                Terraria.Utils.DrawLine(Main.spriteBatch, drawPos2 + new Vector2(26, 2053).RotatedBy(NPC.rotation), NPC.Center + new Vector2(26, 53).RotatedBy(NPC.rotation), color);
            }

            if (mouthTele > 0)
            {
                Color color = Color.Red * mouthTele;
                Terraria.Utils.DrawLine(Main.spriteBatch, drawPos2 + new Vector2(-2, 2027).RotatedBy(NPC.rotation), NPC.Center + new Vector2(-2, 27).RotatedBy(NPC.rotation), color, color, 4);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }       
}
