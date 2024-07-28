using Terraria.GameContent.Bestiary;
using Terraria.Audio;
using Terraria.GameContent;
using GloryMod.Systems;
using System.IO;

namespace GloryMod.NPCs.IgnitedIdol
{
    internal class AwakenedLantern : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 50;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("These lanterns are believed to be sacred, used in religious ceremonies and sacrifices of the past. " +
                "Even now, traces of their divine energy remain, waiting to be activated once more.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 40;
            NPC.height = 70;
            DrawOffsetY = -4f;

            NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = Main.getGoodWorld ? 200 : 150;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.npcSlots = 1f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath39;

            NPC.dontTakeDamage = true;
        }

        private int animState = 0;
        private bool Extinguish = false;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 3;
            if (Extinguish == true)
            {
                Extinguish = false;           
                NPC.frame.Y = 0;
                animState = 3;
            }
            
            //Anim state 0.

            if (animState == 0)
            {
                NPC.frame.Y = 0;
                NPC.frame.X = 0;
            }

            //Anim state 1.

            if (animState == 1)
            {
                NPC.frameCounter++;

                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0f;
                }

                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;                 
                    animState = 2;
                }
            }

            //Anim state 2.

            if (animState == 2)
            {
                NPC.frame.X = NPC.frame.Width;
                NPC.frameCounter++;

                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0f;
                }

                if (NPC.frame.Y >= frameHeight * 5)
                {
                    NPC.frame.Y = 0;
                }
            }

            //Anim state 3.

            if (animState == 3)
            {
                NPC.frame.X = NPC.frame.Width * 2;
                NPC.frameCounter++;

                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0.0;
                }

                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                    NPC.frame.X = 0;
                    animState = 0;
                }
            }
        }

        private float currentDistance = 8;
        private bool attacking;
        private bool changeAppearance = false;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(currentDistance);
                writer.Write(attacking);
                writer.Write(changeAppearance);

                writer.Write(NPC.localAI[0]);
                writer.Write(NPC.localAI[1]);
                writer.Write(NPC.localAI[2]);
                writer.Write(NPC.localAI[3]);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                currentDistance = reader.ReadInt32();
                attacking = reader.ReadBoolean();
                changeAppearance = reader.ReadBoolean();

                NPC.localAI[0] = reader.ReadInt32();
                NPC.localAI[1] = reader.ReadInt32();
                NPC.localAI[2] = reader.ReadInt32();
                NPC.localAI[3] = reader.ReadInt32();
            }
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];
            if (!owner.active || owner.type != NPCType<IgnitedIdol>())
            {
                NPC.life = 0;
            }

            Player target = Main.player[owner.target];

            if (animState != 0)
            {
                if (changeAppearance == true)
                {
                    for (int i = 0; i < NPC.oldPos.Length; i++)
                    {
                        Vector2 dustPos = new Vector2(Main.rand.NextFloat(NPC.width / (i + 2), 0)).RotatedByRandom(MathHelper.TwoPi);

                        if (Main.rand.NextBool(20 + (i * 10)))
                        {
                            Dust dust = Dust.NewDustPerfect(NPC.oldPos[i] - NPC.position + NPC.Center + dustPos, 59, new Vector2(0, Main.rand.NextFloat(1, 5)).RotatedBy(dustPos.ToRotation()), 0, default, 1.5f - (i * 0.015f));
                            dust.noGravity = true;
                        }
                    }

                    Lighting.AddLight(NPC.Center, 0.5f * bloomAlpha, 0.5f * bloomAlpha, 1 * bloomAlpha);
                }
                else
                {
                    for (int i = 0; i < NPC.oldPos.Length; i++)
                    {
                        Vector2 dustPos = new Vector2(Main.rand.NextFloat(NPC.width / (i + 2), 0)).RotatedByRandom(MathHelper.TwoPi);

                        if (Main.rand.NextBool(20 + (i * 10)))
                        {
                            Dust dust = Dust.NewDustPerfect(NPC.oldPos[i] - NPC.position + NPC.Center + dustPos, 6, new Vector2(0, Main.rand.NextFloat(1, 5)).RotatedBy(dustPos.ToRotation()), 0, default, 1.5f - (i * 0.015f));
                            dust.noGravity = true;
                        }
                    }

                    Lighting.AddLight(NPC.Center, 1 * bloomAlpha, 0.5f * bloomAlpha, 0.5f * bloomAlpha);
                }
            }

            //Orbiting
            float SummonTimeMult = 60f;
            float orbitTimeMult = Main.getGoodWorld ? 10f : 15f;
            float distanceMult = 1 - (float)Math.Exp(-NPC.ai[2] / SummonTimeMult);
            currentDistance = MathHelper.Lerp(currentDistance, owner.ai[1], 0.1f);

            NPC.velocity = -NPC.Center + owner.Center + new Vector2((200 + (25 * currentDistance)) * distanceMult, 0).RotatedBy(0.15f * (Main.GameUpdateCount / orbitTimeMult - distanceMult) + NPC.ai[1] * MathHelper.TwoPi / 8);
            NPC.ai[2]++;

            //Actual AI

            NPC.localAI[0]++;

            if (NPC.localAI[0] <= 100)
            {
                Lighting.AddLight(NPC.Center, 0.2f, 0.1f, 0.1f);
            }

            if (NPC.localAI[0] == 100 && owner.ai[0] != 5)
            {
                animState = 1;
                NPC.life = NPC.lifeMax;
                NPC.ai[3] = 0;
            }

            if (owner.ai[0] != 0 && owner.ai[0] != 5 && owner.ai[0] != 10 && NPC.ai[3] == 0)
            {
                NPC.dontTakeDamage = false;
            }

            //Attacks for the lanterns to perform.

            attacking = false;

            switch (owner.ai[0])
            {
                case 2:

                    //Adds lantern triple shot.

                    if (owner.ai[2] == NPC.ai[1] && owner.localAI[0] > 20 && owner.localAI[0] <= 200 && NPC.ai[3] == 0)
                    {
                        attacking = true;
                        if (owner.localAI[0] == 180)
                        {
                            Vector2 directionTo = NPC.DirectionTo(target.Center);
                            float fanSize = 45;

                            for (int i = -1; i < 2; i++)
                            {
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(6, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 3 + directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 60, 3f, Main.myPlayer);
                                Main.projectile[proj].ai[0] = NPC.ai[0];
                                Main.projectile[proj].ai[1] = 1;
                            }

                            int numDusts = 20;
                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(NPC.Center, 0, 0, 6, Scale: 3f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            }

                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                            NPC.netUpdate = true;
                        }
                    }

                    break;

                case 4:

                    //Makes lanterns do the bullet hell.

                    if (owner.ai[2] == 1 && NPC.ai[3] == 0)
                    {
                        if (owner.localAI[0] <= 150)
                        {
                            attacking = true;
                        }

                        if (owner.localAI[0] == 150)
                        {
                            Vector2 directionTo = NPC.DirectionTo(owner.Center);
                            float fanSize = 180;

                            for (int i = -1; i < 2; i++)
                            {
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(6, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 3 + directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 60, 3f, Main.myPlayer);
                                Main.projectile[proj].ai[0] = NPC.ai[0];
                                Main.projectile[proj].ai[1] = 1;
                            }

                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                            NPC.netUpdate = true;
                        }
                    }

                    break;

                case 5:

                    //For transition scene.

                    if (owner.ai[3] == 1)
                    {
                        NPC.dontTakeDamage = true;
                        NPC.life = NPC.lifeMax;
                        NPC.localAI[0] = -800;
                        NPC.ai[3] = 0;
                        animState = 3;
                        bloomAlpha = 0;
                        attacking = false;
                        telegraphAlpha = 0;
                    }

                    if (owner.ai[2] > NPC.ai[1])
                    {
                        changeAppearance = true;
                        if (NPC.localAI[1] == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item100, NPC.Center);
                            animState = 1;
                            NPC.localAI[1]++;
                            Vector2 directionTo = NPC.DirectionTo(target.Center);
                            float fanSize = 50;

                            for (int i = -1; i < 2; i++)
                            {
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 3 + directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 60, 3f, Main.myPlayer);
                                Main.projectile[proj].ai[0] = NPC.ai[0];
                                Main.projectile[proj].ai[1] = 3;
                            }

                            int numDusts = 20;
                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(NPC.Center, 0, 0, 59, Scale: 3f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            }

                            NPC.netUpdate = true;
                        }
                    }

                    if (owner.ai[3] == 1000)
                    {
                        NPC.dontTakeDamage = false;
                        NPC.netUpdate = true;
                    }

                    break;

                case 6:

                    //Creates large spreads of projectiles.

                    if (owner.ai[3] == NPC.ai[1] || owner.localAI[3] == NPC.ai[1])
                    {
                        if (NPC.ai[3] == 0 && owner.localAI[0] <= 60)
                        {
                            attacking = true;

                            if (owner.localAI[2] == 1)
                            {
                                Vector2 directionTo = NPC.DirectionTo(owner.Center);
                                float fanSize = 110;

                                for (int i = -5; i < 6; i++)
                                {
                                    int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(8, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 11 + directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 72, 3f, Main.myPlayer);
                                    Main.projectile[proj].ai[0] = NPC.ai[0];
                                    Main.projectile[proj].ai[1] = 3;
                                }

                                int numDusts = 20;
                                for (int i = 0; i < numDusts; i++)
                                {
                                    int dust = Dust.NewDust(NPC.Center, 0, 0, 59, Scale: 3f);
                                    Main.dust[dust].noGravity = true;
                                    Main.dust[dust].noLight = true;
                                    Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                                }

                                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                                NPC.netUpdate = true;
                            }
                        }
                    }

                    break;

                case 8:
                    //Extra bullets.

                    if (owner.ai[2] == NPC.ai[1] || owner.localAI[2] == NPC.ai[1])
                    {
                        if (owner.ai[3] > 60 && owner.localAI[0] <= 120 && NPC.ai[3] == 0)
                        {
                            attacking = true;
                        }

                        if (owner.ai[3] >= 120 && NPC.ai[3] == 0)
                        {
                            Vector2 directionTo = NPC.DirectionTo(target.Center);

                            int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(10, 0).RotatedBy(directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 90, 3f, Main.myPlayer);
                            Main.projectile[proj].ai[0] = NPC.ai[0];
                            Main.projectile[proj].ai[1] = 3;


                            int numDusts = 20;
                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(NPC.Center, 0, 0, 59, Scale: 3f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            }

                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                            NPC.netUpdate = true;
                        }
                    }

                    break;

                case 9:
                    //Pincer ring of projectiles.

                    if (NPC.ai[3] == 0 && owner.ai[2] >= 730 && owner.ai[2] <= 830)
                    {
                        attacking = true;

                        if (owner.ai[2] == 830)
                        {
                            Vector2 directionTo = NPC.DirectionTo(target.Center);

                            int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(6, 0).RotatedBy(directionTo.ToRotation()), ProjectileType<AwakenedLight>(), 72, 3f, Main.myPlayer);
                            Main.projectile[proj].ai[0] = NPC.ai[0];
                            Main.projectile[proj].ai[1] = 3;

                            int numDusts = 20;
                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(NPC.Center, 0, 0, 59, Scale: 3f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            }

                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                            NPC.netUpdate = true;
                        }
                    }

                    break;

                case 10:

                    if (owner.ai[2] == 1)
                    {
                        NPC.dontTakeDamage = true;
                        NPC.life = NPC.lifeMax;
                        NPC.localAI[0] = 100;

                        if (animState != 1 || animState != 2)
                        {
                            animState = 1;
                        }
                    }

                    NPC.ai[3] = 0;

                    if (owner.ai[2] > 300)
                    {
                        NPC.dontTakeDamage = false;
                        attacking = true;
                    }

                    break;
            }
        }

        public override void UpdateLifeRegen(ref int damage)
        {
            NPC.lifeRegen = 5;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];

            if (owner.ai[0] == 10)
            {
                DamageResistance modNPC = DamageResistance.modNPC(NPC);
                modNPC.DR = 0.5f;
            }

            if (NPC.life <= 0 && owner.ai[0] != 10 && owner.ai[2] <= 200)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                Extinguish = true;
                NPC.localAI[0] = -500;
                NPC.ai[3] = 1;

                if (Main.getGoodWorld && owner.ai[1] < 5)
                {
                }
                else
                {
                    Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, ItemID.Heart);
                }

                if (owner.ai[1] == 8)
                {
                    Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, ItemID.Heart);
                }
                NPC.netUpdate = true;
            }
        }

        public override void OnKill()
        {
            int numDusts = 30;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 59, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8, 12), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
        }


        private float bloomAlpha;
        public float telegraphAlpha;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D texture2 = Request<Texture2D>(Texture + "2").Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D mask2 = Request<Texture2D>(Texture + "Mask2").Value;
            Texture2D glow1 = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Texture2D glow2 = Request<Texture2D>("Terraria/Images/Projectile_644").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 glowOrigin = glow1.Size() / 2;
            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);
            Color lanternColor;
            if (changeAppearance == true)
            {
                lanternColor = new Color(200, 175, 255);
            }
            else
            {
                lanternColor = new Color(250, 200, 100);
            }

            NPC owner = Main.npc[(int)NPC.ai[0]];
            Player target = Main.player[owner.target];

            if (animState == 1)
            {
                bloomAlpha = MathHelper.Lerp(bloomAlpha, 1, 0.1f);
            }
            if (animState == 3)
            {
                bloomAlpha = MathHelper.Lerp(bloomAlpha, 0, 0.1f);
            }
            if (attacking == true)
            {
                telegraphAlpha = MathHelper.Lerp(telegraphAlpha, 1, 0.1f);
            }
            else
            {
                telegraphAlpha = MathHelper.Lerp(telegraphAlpha, 0, 0.1f);
            }

            float scale = (NPC.scale * mult) * bloomAlpha;

            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(glow1, drawPos, null, lanternColor * scale * 0.8f, NPC.rotation, glowOrigin, scale, SpriteEffects.None, 0f);


                if (owner.ai[0] == 2 && owner.ai[2] == NPC.ai[1])
                {
                    Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(NPC.Distance(target.Center) / 2, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), lanternColor * telegraphAlpha);
                }
                if (owner.ai[0] == 4 && owner.ai[2] == 1)
                {
                    Vector2 directionTo = NPC.DirectionTo(owner.Center);
                    float fanSize = 180;
                    for (int i = -1; i < 2; i++)
                    {
                        Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(400, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 3 + NPC.DirectionTo(owner.Center).ToRotation()), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), lanternColor * telegraphAlpha);
                    }                      
                }

                if (owner.ai[0] == 6 && owner.ai[3] == NPC.ai[1] || owner.ai[0] == 6 && owner.localAI[3] == NPC.ai[1])
                {
                    float fanSize = 110;
                    for (int i = -1; i < 2; i++)
                    {
                        Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(200, 0).RotatedBy(i * MathHelper.ToRadians(fanSize) / 3 + NPC.DirectionTo(owner.Center).ToRotation()), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), lanternColor * telegraphAlpha);
                    }
                }

                if (owner.ai[0] == 8 && owner.ai[2] == NPC.ai[1] || owner.ai[0] == 8 && owner.localAI[2] == NPC.ai[1])
                {
                    Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(NPC.Distance(target.Center), 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), lanternColor * telegraphAlpha);
                }

                if (owner.ai[0] == 9)
                {
                    Terraria.Utils.DrawLine(Main.spriteBatch, NPC.Center + new Vector2(NPC.Distance(target.Center), 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()), lanternColor * telegraphAlpha);
                }

                if (owner.ai[0] == 10)
                {
                    Terraria.Utils.DrawLine(Main.spriteBatch, owner.Center, NPC.Center, lanternColor * telegraphAlpha);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                if (changeAppearance == true)
                {
                    for (int i = 1; i < NPC.oldPos.Length; i++)
                    {
                        Main.EntitySpriteDraw(glow2, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, null, NPC.GetAlpha(new Color(50 + (i * 5), 200 - (i * 10), 255)) * ((1.2f - i / (float)NPC.oldPos.Length) * 0.99f) * scale, NPC.velocity.ToRotation() + MathHelper.PiOver2, glow2.Size() / 2 + new Vector2(2, 2), NPC.scale * (1.2f - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0);
                    }
                }
                else
                {
                    for (int i = 1; i < NPC.oldPos.Length; i++)
                    {
                        Main.EntitySpriteDraw(glow2, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, null, NPC.GetAlpha(new Color(255, 250 - (i * 5), 200 - (i * 10))) * ((1.2f - i / (float)NPC.oldPos.Length) * 0.99f) * scale, NPC.velocity.ToRotation() + MathHelper.PiOver2, glow2.Size() / 2 + new Vector2(2, 2), NPC.scale * (1.2f - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0);
                    }
                }

                if (changeAppearance == true)
                {
                    spriteBatch.Draw(texture2, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin + new Vector2(2, 2), NPC.scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(mask2, drawPos, NPC.frame, new Color(255, 255, 255), NPC.rotation, drawOrigin + new Vector2(2, 2), NPC.scale, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin + new Vector2(2, 2), NPC.scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(mask, drawPos, NPC.frame, new Color(255, 255, 255), NPC.rotation, drawOrigin + new Vector2(2, 2), NPC.scale, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(glow1, drawPos, null, lanternColor * scale, NPC.rotation, glowOrigin, scale * 0.75f, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
            }
            else
            {
                animState = 2;
                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin + new Vector2(2, 2), NPC.scale, SpriteEffects.None, 0f);
            }
          
            return false;
        }
    }
}
