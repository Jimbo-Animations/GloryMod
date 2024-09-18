using GloryMod.Systems;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using System.Collections.Generic;
using GloryMod.Systems.BossBars;
using GloryMod.Items.NeonBoss;
using System.IO;
using Terraria;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace GloryMod.NPCs.NeonBoss
{
    [AutoloadBossHead]
    internal class NeonTyrant : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("This gargantuan slime has the ability to light up in all sorts of fantastic colors, thanks to its powerful sky magic. Leading an army of elemenal minions; the Neon Tyrant is a formidable monster for even seasoned adventurers.")
            });
        }

        public override void BossHeadSlot(ref int index)
        {
            if (NPC.hide) index = -1;
        }


        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 162;
            NPC.height = 124;
            DrawOffsetY = -2f;
            NPC.scale = 2f;
            NPC.alpha = 255;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = Main.getGoodWorld ? 4000 : 3000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.hide = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.BossBar = GetInstance<NeonBossBar>();
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 1;

            if (projectile.penetrate != 1 && projectile.DamageType != DamageClass.Melee) modNPC.DR = 0.5f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.ai[0] == 0)
                {
                    NPC.frame.Y = frameHeight * 3;
                }
                else
                {
                    NPC.frameCounter += 1;
                    if (NPC.frameCounter > 5)
                    {
                        NPC.frame.Y = NPC.frame.Y + frameHeight;
                        NPC.frameCounter = 0.0;
                    }
                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
            else
            {
                NPC.frameCounter += 1;
                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public Player player
        {
            get => Main.player[NPC.target];
        }


        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(WhatPhase);
                writer.Write(Hasphase2occured);
                writer.Write(Hasphase3occured);
                writer.Write(Hasphase4occured);
                writer.Write(canfall);
                writer.Write(Speed);

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
                WhatPhase = reader.ReadInt32();
                Speed = reader.ReadInt32();
                Hasphase2occured = reader.ReadBoolean();
                Hasphase3occured = reader.ReadBoolean();
                Hasphase4occured = reader.ReadBoolean();
                canfall = reader.ReadBoolean();

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

        private int WhatPhase = 1;
        private bool Hasphase2occured = false;
        private bool Hasphase3occured = false;
        private bool Hasphase4occured = false;
        private bool canfall;
        private float Speed = 1;

        public override void OnKill()
        {
            int numDusts = 60;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 138, Scale: 4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(30), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            int gore1 = Mod.Find<ModGore>("NeonTyrantGore").Type;
            Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, NPC.velocity, gore1);
            NPC.downedSlimeKing = true;
            Main.StopSlimeRain();
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            var SpawnRule = Main.ItemDropsDB.GetRulesForNPCID(NPCID.KingSlime, false);
            foreach (var dropRule in SpawnRule)
            {
                npcLoot.Add(dropRule);
            }
            npcLoot.Add(ItemDropRule.OneFromOptions(1, ItemType<ArgonBasher>(), ItemType<NeonSniper>(), ItemType<KryptonScepter>(), ItemType<XenonChain>()));
            npcLoot.Add(ItemDropRule.Common(ItemID.Gel, minimumDropped: 100, maximumDropped: 150));
        }

        public override void AI()
        {
            Vector2 groundPosition = NPC.Center.findGroundUnder();
            int goalDirectionX = player.Center.X < NPC.Center.X ? -1 : 1;

            canfall = NPC.Bottom.Y < player.Top.Y ? true : false;
            NPC.damage = NPC.velocity.Y > 1 && NPC.ai[0] == 0 ? 120 : 0;

            float Phase2;
            float Phase3;
            float Phase4;

            if (Main.getGoodWorld)
            {
                Phase2 = NPC.lifeMax * 0.9f;
                Phase3 = NPC.lifeMax * 0.8f;
                Phase4 = NPC.lifeMax * 0.7f;
            }
            else
            {
                Phase2 = NPC.lifeMax * 0.85f;
                Phase3 = NPC.lifeMax * 0.7f;
                Phase4 = NPC.lifeMax * 0.55f;
            }

            if (NPC.life <= Phase2 && Hasphase2occured == false)
            {
                WhatPhase = 2;
                NPC.ai[0] = 1;
                NPC.ai[1] = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                Hasphase2occured = true;
            }
            if (NPC.life <= Phase3 && Hasphase3occured == false)
            {
                WhatPhase = 3;
                NPC.ai[0] = 1;
                NPC.ai[1] = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                Hasphase3occured = true;
            }
            if (NPC.life <= Phase4 && Hasphase4occured == false)
            {
                WhatPhase = 4;
                NPC.ai[0] = 1;
                NPC.ai[1] = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                Hasphase4occured = true;
            }

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;

            if (Hasphase2occured)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Music/FOR_GLORY_p2_3");
                Main.musicFade[Main.curMusic] = MathHelper.Lerp(0, 1, 1);

                if (Hasphase3occured)
                {
                    Music = MusicLoader.GetMusicSlot(Mod, "Music/FOR_GLORY_p3_3");
                    Main.musicFade[Main.curMusic] = MathHelper.Lerp(0, 1, 1);

                    if (Hasphase4occured)
                    {
                        if (NPC.ai[1] == 0)
                        {
                            Music = MusicLoader.GetMusicSlot(Mod, "Music/FOR_GLORY_p4_minion_3");
                            Main.musicFade[Main.curMusic] = MathHelper.Lerp(0, 1, 1);
                        }
                        else
                        {
                            Music = MusicLoader.GetMusicSlot(Mod, "Music/FOR_GLORY_p4_3");
                            Main.musicFade[Main.curMusic] = MathHelper.Lerp(0, 1, 1);
                        }
                    }
                }
            }
            else
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Music/FOR_GLORY_p1_3");
            }

            //Should the enemy leave?
            if (!player.active || player.dead)
            {
                Vector2 goalPos = player.Center + new Vector2(0, -9999);
                Vector2 goalVel = goalPos - NPC.Center;
                Retreat();
            }

            void Retreat()
            {
                NPC.TargetClosest();

                if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                if (NPC.velocity.Y < 10) NPC.velocity.Y -= 1f;

                NPC.dontTakeDamage = true;
                NPC.chaseable = false;
                return;
            }

            switch (NPC.ai[1])
            {
                case 0:
                    //Go invisble and summon minions.
                    NPC.dontTakeDamage = true;
                    NPC.damage = 0;

                    if (NPC.alpha < 255 && NPC.ai[2] < 60) NPC.alpha += 5;
                    if (NPC.ai[2] == 0 && WhatPhase != 1) SoundEngine.PlaySound(SoundID.Zombie105, NPC.Center);
                    NPC.ai[2]++;

                    if (NPC.ai[2] < 60)
                    {
                        Hopping(0, 0, 1000);
                    }

                    //Spawning the correct minion.

                    if (NPC.ai[2] > 60)
                    {
                        if ((NPC.AnyNPCs(NPCType<NobleSlime1>()) && WhatPhase == 1) || (NPC.AnyNPCs(NPCType<NobleSlime2>()) && WhatPhase == 2) || (NPC.AnyNPCs(NPCType<NobleSlime3>()) && WhatPhase == 3) || (NPC.AnyNPCs(NPCType<NobleSlime4>()) && WhatPhase == 4))
                        {
                            Vector2 goalPos = player.Center + new Vector2(0, -300);
                            Vector2 goalVel = goalPos - NPC.Center;
                            if (goalVel.Length() > 10)
                            {
                                Speed = MathHelper.Lerp(Speed, 13f, 0.06f);
                                goalVel.Normalize();
                                goalVel *= Speed;
                            }
                            else
                            {
                                Speed = 1f;
                            }

                            NPC.velocity = Vector2.Lerp(NPC.velocity, goalVel + player.position - player.oldPosition, 0.06f);
                            NPC.hide = true;
                        }
                        else
                        {
                            NPC.hide = false;
                            if (NPC.alpha > 0) NPC.alpha -= 5;

                            Hopping(0, 0, 1000);
                            NPC.ai[3]++;

                            if (NPC.ai[3] == 100)
                            {
                                NPC.dontTakeDamage = false;
                                NPC.damage = 150;
                                NPC.ai[3] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[0] = 1;

                                if (WhatPhase == 2) NPC.life = (int)(Phase2);
                                if (WhatPhase == 3) NPC.life = (int)(Phase3);
                                if (WhatPhase == 4) NPC.life = (int)(Phase4);

                                ChangeAttacks();
                            }
                        }
                    }

                    if (NPC.ai[2] == 60)
                    {
                        if (WhatPhase == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Top.X, (int)NPC.Top.Y, NPCType<NobleSlime1>(), NPC.whoAmI);
                        if (WhatPhase == 2 && Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Top.X, (int)NPC.Center.Y, NPCType<NobleSlime2>(), NPC.whoAmI);
                        if (WhatPhase == 3 && Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Top.X, (int)NPC.Center.Y, NPCType<NobleSlime3>(), NPC.whoAmI);
                        if (WhatPhase == 4 && Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Top.X, (int)NPC.Center.Y, NPCType<NobleSlime4>(), NPC.whoAmI);

                        NPC.netUpdate = true;
                    }

                    break;

                case 1:
                    //Turn orange and chase the player while summoning lightning.

                    Hopping(Main.getGoodWorld ? 8 : 6, 20, 30, false, true);
                    NPC.ai[2]++;

                    if (NPC.ai[2] == 100 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.ai[2] = 40;
                        NPC.ai[3]++;

                        for (int i = 0; i < (int)(NPC.ai[3]); i++)
                        {
                            Vector2 spawninsky = player.Center + new Vector2(0, -500);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), spawninsky, Vector2.Zero, ProjectileType<NeonVortex>(), 60, 1, player.whoAmI, 0, (int)NPC.ai[3]);
                            NPC.netUpdate = true;
                        }

                        if (NPC.ai[3] == 3)
                        {
                            NPC.ai[3] = 0;
                            NPC.ai[2] = 0;
                            NPC.ai[0] = 0;
                            ChangeAttacks();
                        }
                    }

                    break;

                case 2:
                    //Turn silver and jump back, then create and chuck debris.

                    NPC.velocity.Y += 0.2f;
                    if (NPC.velocity.Y >= 16) NPC.velocity.Y = 16;

                    if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height) != new Vector2(0, NPC.velocity.Y))
                    {
                        NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height);
                        NPC.position.Y += NPC.velocity.Y;
                        NPC.velocity.Y = 0;
                        NPC.rotation = 0;

                        if (NPC.ai[0] == 0)
                        {
                            if (NPC.ai[2] == 1)
                            {
                                for (int i = 0; i < 40; i++)
                                {
                                    int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-162, 162), 0), 0, 0, 0, Scale: 2f);
                                    Main.dust[dust].noGravity = false;
                                    Main.dust[dust].noLight = true;
                                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(7), 0).RotatedByRandom(MathHelper.TwoPi);
                                }
                                SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
                                SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton, NPC.Center);

                                int projCount = Main.getGoodWorld ? 20 : 15;

                                for (int i = 0; i < projCount; i++)
                                {
                                    Vector2 spawnonground = new Vector2(Main.rand.NextFloat(-330, 330), 0) + groundPosition;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnonground, new Vector2(0, Main.rand.NextFloat(-0.3334f * projCount, (-1 * projCount) - 2)).RotatedByRandom(MathHelper.ToRadians(15)), ProjectileType<NeonTyrantDebris>(), 75, 1, player.whoAmI);
                                }

                                NPC.netUpdate = true;
                            }
                            else
                            {
                                for (int i = 0; i < 30; i++)
                                {
                                    int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-162, 162), 0), 0, 0, 0, Scale: 2f);
                                    Main.dust[dust].noGravity = false;
                                    Main.dust[dust].noLight = true;
                                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi);
                                }
                                SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
                            }
                        }

                        if (NPC.ai[0] == 120)
                        {
                            NPC.ai[3] = 0;
                            NPC.ai[2] = 0;
                            NPC.ai[0] = 0;
                            ChangeAttacks();
                        }

                        NPC.ai[0]++;
                    }
                    else
                    {
                        if (NPC.velocity.Y < 0)
                        {
                            NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.04f, 0.09f);
                        }
                        else
                        {
                            NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.06f, 0.01f);
                            NPC.velocity.Y += 0.5f;
                        }
                    }

                    if (NPC.ai[0] >= 30 && NPC.ai[2] == 0)
                    {
                        NPC.ai[0] = 0;
                        NPC.ai[2] = 1;
                        NPC.velocity.X += goalDirectionX * -14 * Main.rand.NextFloat(0.8f, 1.2f) + (player.Center.X - NPC.Center.X) / 75;
                        NPC.velocity.Y -= 16 * Main.rand.NextFloat(0.8f, 1.2f);
                        NPC.spriteDirection = -1;
                        if (player.Center.X < NPC.Center.X)
                        {
                            NPC.spriteDirection = 1;
                        }
                    }

                    NPC.velocity *= 0.99f;

                    break;

                case 3:
                    //Try and get above the player, then slam down on them.

                    if (NPC.ai[0] == 0 && NPC.ai[2] == 0)
                    {
                        Hopping(0, 0, 30, false, false);
                    }
                    else
                    {
                        NPC.velocity.Y += Main.getGoodWorld ? 0.25f : 0.2f;
                        NPC.velocity.X += Main.getGoodWorld ? 0.3f * goalDirectionX : 0.2f * goalDirectionX;
                        if (NPC.velocity.Y >= 16) NPC.velocity.Y = 16;

                        if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, canfall, canfall) != new Vector2(0, NPC.velocity.Y))
                        {
                            NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, canfall);
                            NPC.position.Y += NPC.velocity.Y;
                            NPC.velocity.Y = 0;
                            NPC.rotation = 0;

                            if (NPC.ai[0] == 0)
                            {
                                if (NPC.ai[2] == 1)
                                {
                                    for (int i = 0; i < 40; i++)
                                    {
                                        int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-162, 162), 0), 0, 0, 0, Scale: 2f);
                                        Main.dust[dust].noGravity = false;
                                        Main.dust[dust].noLight = true;
                                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(7), 0).RotatedByRandom(MathHelper.TwoPi);
                                    }
                                    SoundEngine.PlaySound(SoundID.Item62, NPC.Center);

                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), groundPosition + new Vector2(0, -50), Vector2.Zero, ProjectileType<NeonTyrantShockwave>(), 50, 1, player.whoAmI, 0, 0);
                                    NPC.netUpdate = true;
                                }
                            }

                            if (NPC.ai[0] == 40)
                            {
                                NPC.ai[3] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[0] = 0;
                                ChangeAttacks();
                            }

                            NPC.ai[0]++;
                        }
                        else
                        {
                            if (NPC.velocity.Y < 0)
                            {
                                NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.05f, 0.09f);
                            }
                            else
                            {
                                NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.05f, 0.01f);
                                NPC.velocity.Y += 1f;
                            }
                            NPC.ai[0] = 0;
                        }

                        if (NPC.ai[0] >= 20 && NPC.ai[2] == 0)
                        {
                            NPC.ai[0] = 0;
                            NPC.ai[2] = 1;
                            NPC.velocity.X += goalDirectionX * 10 * Main.rand.NextFloat(0.8f, 1.2f);
                            NPC.velocity.Y -= 30 * Main.rand.NextFloat(0.8f, 1.2f);
                            NPC.spriteDirection = -1;
                            if (player.Center.X < NPC.Center.X)
                            {
                                NPC.spriteDirection = 1;
                            }
                        }

                        NPC.velocity *= 0.99f;
                    }

                    break;

                case 4:
                    //Perform low, short hops while laying mines.

                    Hopping(10, 13, Main.getGoodWorld ? 30 : 40, true, false);
                    NPC.ai[2]++;
                    float repeat = Main.getGoodWorld ? 4 : 3;

                    if (NPC.ai[2] == 100 && NPC.ai[3] < repeat)
                    {
                        Vector2 spawnonground = new Vector2(650, 0) + groundPosition;
                        Vector2 spawnongroundnegative = new Vector2(-650, 0) + groundPosition;
                        NPC.ai[2] = 0;
                        NPC.ai[3]++;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnonground, new Vector2(0, Main.rand.NextFloat(-14, -18)), ProjectileType<NeonTyrantMine>(), 60, 1, player.whoAmI);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnongroundnegative, new Vector2(0, Main.rand.NextFloat(-14, -18)), ProjectileType<NeonTyrantMine>(), 60, 1, player.whoAmI);
                        NPC.netUpdate = true;
                    }

                    if (NPC.ai[2] >= 120)
                    {
                        NPC.ai[3] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[0] = 0;

                        ChangeAttacks();
                    }

                    break;
            }

            void Hopping(float speedX, float speedY, int canjump = 60, bool compensateX = false, bool compensateY = false)
            {
                NPC.velocity.Y += 0.3f;
                if (NPC.velocity.Y >= 16) NPC.velocity.Y = 16;

                if (compensateX == true) speedX += (player.Center.X - NPC.Center.X) / 75 * goalDirectionX;

                if (compensateY == true) speedY -= (player.Center.Y - NPC.Center.Y) / 50;


                if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y))
                {
                    NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true);
                    NPC.position.Y += NPC.velocity.Y;
                    NPC.velocity.Y = 0;
                    NPC.rotation = 0;

                    if (NPC.ai[0] == 0 && !NPC.hide)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-162, 162), 0), 0, 0, 0, Scale: 2f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi);
                        }
                        SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
                    }

                    NPC.ai[0]++;
                }
                else
                {
                    if (NPC.velocity.Y < 0) NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.04f, 0.09f);
                    else
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.06f, 0.01f);
                        NPC.velocity.Y += 0.3f;
                    }

                    NPC.ai[0] = 0;
                }

                if (NPC.ai[0] >= canjump)
                {
                    NPC.ai[0] = 0;
                    NPC.velocity.X += goalDirectionX * speedX * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.velocity.Y -= speedY * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.spriteDirection = -1;
                    if (player.Center.X < NPC.Center.X)
                    {
                        NPC.spriteDirection = 1;
                    }
                }

                NPC.velocity *= 0.99f;
            }

            void ChangeAttacks()
            {
                if (NPC.ai[1] <= 1) NPC.ai[1] = WhatPhase;
                else NPC.ai[1]--;

                NPC.netUpdate = true;
            }
        }

        private float alpha1;
        private float alpha2;
        private float alpha3;
        private float alpha4;
        private float auraAlpha;
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D texture2 = Request<Texture2D>(Texture + "2").Value;
            Texture2D texture3 = Request<Texture2D>(Texture + "3").Value;
            Texture2D texture4 = Request<Texture2D>(Texture + "4").Value;
            Texture2D crowntexture = Request<Texture2D>(Texture + "Crown").Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            if (!NPC.IsABestiaryIconDummy)
            {
                alpha1 = MathHelper.Lerp(alpha1, NPC.ai[1] == 1 || NPC.ai[1] == 0 ? 1 : 0, 0.05f);
                alpha2 = MathHelper.Lerp(alpha2, NPC.ai[1] == 2 ? 1 : 0, 0.05f);
                alpha3 = MathHelper.Lerp(alpha3, NPC.ai[1] == 3 ? 1 : 0, 0.05f);
                alpha4 = MathHelper.Lerp(alpha4, NPC.ai[1] == 4 ? 1 : 0, 0.05f);
                auraAlpha = MathHelper.Lerp(auraAlpha, NPC.damage > 0 ? 1 : 0, 0.15f);

                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha1 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                    Main.EntitySpriteDraw(texture2, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha2 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                    Main.EntitySpriteDraw(texture3, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha3 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                    Main.EntitySpriteDraw(texture4, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha4 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }

                spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(Color.White * alpha1), NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                spriteBatch.Draw(texture2, drawPos, NPC.frame, NPC.GetAlpha(Color.White * alpha2), NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                spriteBatch.Draw(texture3, drawPos, NPC.frame, NPC.GetAlpha(Color.White * alpha3), NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                spriteBatch.Draw(texture4, drawPos, NPC.frame, NPC.GetAlpha(Color.White * alpha4), NPC.rotation, drawOrigin, NPC.scale, effects, 0f);              

                spriteBatch.Draw(crowntexture, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale * 0.5f, effects, 0f);
                spriteBatch.Draw(crowntexture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale * 0.5f, effects, 0f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }
    }

    [AutoloadBossHead]
    class NobleSlime1 : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 68;
            NPC.height = 54;
            DrawOffsetY = -2f;
            NPC.scale = 2f;
            NPC.alpha = 255;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = Main.getGoodWorld ? 1000 : 500;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.npcSlots = 15f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.BossBar = GetInstance<NullBossBar>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = NPCType<NeonTyrant>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("One of the Neon Tyrant's elemental cohorts. This slime has learned to mimic its master's plasma controlling techniques, but it lacks the skills to aim its plasma without telegraphing its direction.")
            });
        }

        private float alpha1 = 0;
        private float auraAlpha;
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            if (!NPC.IsABestiaryIconDummy)
            {
                auraAlpha = MathHelper.Lerp(auraAlpha, NPC.damage > 0 ? 1 : 0, 0.15f);
                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    if (alpha1 > 0) Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha1 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);                    
                }
                
                spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(Color.White) * alpha1, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            } 
            else spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale * 0.5f, effects, 0f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 1;

            if (projectile.penetrate != 1 && projectile.DamageType != DamageClass.Melee) modNPC.DR = 0.5f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.ai[0] == 0)
                {
                    NPC.frame.Y = frameHeight * 3;
                }
                else
                {
                    NPC.frameCounter += 1;
                    if (NPC.frameCounter > 5)
                    {
                        NPC.frame.Y = NPC.frame.Y + frameHeight;
                        NPC.frameCounter = 0.0;
                    }
                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
            else
            {
                NPC.frameCounter += 1;
                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        private Vector2 targetPosition;
        private bool canfall;

        void Retreat()
        {
            NPC.TargetClosest(false);

            if (NPC.timeLeft > 10)
            {
                NPC.timeLeft = 10;
            }
            if (NPC.velocity.Y < 10)
            {
                NPC.velocity.Y -= 1f;
            }

            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            return;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            Vector2 groundPosition = NPC.Center.findGroundUnder();
            int goalDirectionX = player.Center.X < NPC.Center.X ? -1 : 1;

            if (NPC.target < 0 || NPC.target == 255 || player.dead || !player.active)
                NPC.TargetClosest();

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;
            NPC.damage = NPC.velocity.Y > .5f && NPC.ai[0] == 0 ? 80 : 0;
            alpha1 = MathHelper.Lerp(alpha1, 1, 0.1f);

            if (NPC.alpha > 0) NPC.alpha -= 17;

            canfall = NPC.Bottom.Y < player.Top.Y ? true : false;

            //Should the enemy leave?
            if (!player.active || player.dead)
            {
                Retreat();
            }

            Hopping(6, 14, 40, false, false);
            NPC.ai[2]++;

            if (NPC.ai[2] == 100)
            {
                NPC.ai[2] = 0;
                NPC.ai[3]++;

                for (int i = 0; i < (int)NPC.ai[3]; i++)
                {
                    Vector2 spawninsky = player.Center + new Vector2(0, -500);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawninsky, Vector2.Zero, Main.getGoodWorld ? ProjectileType<NeonVortex>() : ProjectileType<NobleVortex>(), 60, 1, player.whoAmI, 0, (int)NPC.ai[3]);
                }

                if (NPC.ai[3] == 3)
                {
                    NPC.ai[3] = 0;
                    NPC.ai[2] = 0;
                    NPC.ai[1] = 0;
                    NPC.ai[0] = 0;
                }
                NPC.netUpdate = true;
            }

            void Hopping(float speedX, float speedY, int canjump = 60, bool compensateX = false, bool compensateY = false)
            {
                NPC.velocity.Y += 0.3f;
                if (NPC.velocity.Y >= 16) NPC.velocity.Y = 16;

                if (compensateX == true) speedX += (player.Center.X - NPC.Center.X) / 75 * goalDirectionX;

                if (compensateY == true) speedY -= (player.Center.Y - NPC.Center.Y) / 50;


                if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y))
                {
                    NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true);
                    NPC.position.Y += NPC.velocity.Y;
                    NPC.velocity.Y = 0;
                    NPC.rotation = 0;

                    if (NPC.ai[0] == 0 && !NPC.hide)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-68, 68), 0), 0, 0, 0, Scale: 2f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi);
                        }
                        SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
                    }

                    NPC.ai[0]++;
                }
                else
                {
                    if (NPC.velocity.Y < 0) NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.04f, 0.09f);
                    else
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.06f, 0.01f);
                        NPC.velocity.Y += 0.3f;
                    }

                    NPC.ai[0] = 0;
                }

                if (NPC.ai[0] >= canjump)
                {
                    NPC.ai[0] = 0;
                    NPC.velocity.X += goalDirectionX * speedX * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.velocity.Y -= speedY * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.spriteDirection = -1;
                    if (player.Center.X < NPC.Center.X)
                    {
                        NPC.spriteDirection = 1;
                    }
                }

                NPC.velocity *= 0.99f;
            }
        }

        public override void OnKill()
        {
            int numDusts = 45;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 138, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }
    }

    [AutoloadBossHead]
    class NobleSlime2 : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 68;
            NPC.height = 54;
            DrawOffsetY = -2f;
            NPC.scale = 2f;
            NPC.alpha = 255;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = Main.getGoodWorld ? 1000 : 500;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.npcSlots = 15f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.BossBar = GetInstance<NullBossBar>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = NPCType<NeonTyrant>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("One of the Neon Tyrant's elemental cohorts. This slime holds a special connection to stone, and uses telekinetic powers to control rocks.")
            });
        }

        private float alpha1 = 0;
        private float auraAlpha;
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            if (!NPC.IsABestiaryIconDummy)
            {
                auraAlpha = MathHelper.Lerp(auraAlpha, NPC.damage > 0 ? 1 : 0, 0.15f);
                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    if (alpha1 > 0) Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha1 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }

                spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(Color.White) * alpha1, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }
            else spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale * 0.5f, effects, 0f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 1;

            if (projectile.penetrate != 1 && projectile.DamageType != DamageClass.Melee) modNPC.DR = 0.5f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.ai[0] == 0)
                {
                    NPC.frame.Y = frameHeight * 3;
                }
                else
                {
                    NPC.frameCounter += 1;
                    if (NPC.frameCounter > 5)
                    {
                        NPC.frame.Y = NPC.frame.Y + frameHeight;
                        NPC.frameCounter = 0.0;
                    }
                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
            else
            {
                NPC.frameCounter += 1;
                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        void Retreat()
        {
            NPC.TargetClosest(false);

            if (NPC.timeLeft > 10)
            {
                NPC.timeLeft = 10;
            }
            if (NPC.velocity.Y < 10)
            {
                NPC.velocity.Y -= 1f;
            }

            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            return;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            Vector2 groundPosition = NPC.Center.findGroundUnder();
            int goalDirectionX = player.Center.X < NPC.Center.X ? -1 : 1;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;
            NPC.damage = NPC.velocity.Y > .5f && NPC.ai[0] == 0 ? 80 : 0;
            alpha1 = MathHelper.Lerp(alpha1, 1, 0.1f);

            if (NPC.alpha > 0) NPC.alpha -= 17;     

            //Should the enemy leave?
            if (!player.active || player.dead)
            {
                Retreat();
            }

            switch (NPC.ai[1])
            {
                case 0:
                    //Hop away while creating chunks of debris.

                    Hopping(-10, 15, 60, true, false);

                    if (NPC.ai[3] == 3)
                    {
                        NPC.ai[3] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 1;
                        NPC.ai[0] = 1;
                    }

                    break;

                case 1:
                    //Ready up before doing a big leap at the player.

                    Hopping(Main.getGoodWorld ? 15 : 10, 15, 100, false, true);

                    if (NPC.ai[3] == 1)
                    {
                        NPC.ai[3] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }

                    break;
            }

            void Hopping(float speedX, float speedY, int canjump = 60, bool compensateX = false, bool compensateY = false)
            {
                NPC.velocity.Y += 0.3f;
                if (NPC.velocity.Y >= 16) NPC.velocity.Y = 16;

                if (compensateX == true) speedX += (player.Center.X - NPC.Center.X) / 75 * goalDirectionX;

                if (compensateY == true) speedY -= (player.Center.Y - NPC.Center.Y) / 50;


                if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y))
                {
                    NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true);
                    NPC.position.Y += NPC.velocity.Y;
                    NPC.velocity.Y = 0;
                    NPC.rotation = 0;

                    if (NPC.ai[0] == 0 && !NPC.hide)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-68, 68), 0), 0, 0, 0, Scale: 2f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi);
                        }
                        SoundEngine.PlaySound(SoundID.Item167, NPC.Center);

                        int projCount = Main.getGoodWorld ? 4 : 3;

                        for (int i = 0; i < projCount; i++)
                        {
                            Vector2 spawnonground = new Vector2(Main.rand.NextFloat(-162, 162), 0) + groundPosition;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnonground, new Vector2(0, Main.rand.NextFloat(-0.3334f * projCount, (-2 * projCount) - 2)).RotatedByRandom(MathHelper.ToRadians(15)), ProjectileType<NeonTyrantDebris>(), NPC.damage / 2, 1, player.whoAmI);
                        }

                        NPC.ai[3]++;
                    }

                    NPC.ai[0]++;
                }
                else
                {
                    if (NPC.velocity.Y < 0) NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.04f, 0.09f);
                    else
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.06f, 0.01f);
                        NPC.velocity.Y += 0.3f;
                    }

                    NPC.ai[0] = 0;
                }

                if (NPC.ai[0] >= canjump)
                {
                    NPC.ai[0] = 0;
                    NPC.velocity.X += goalDirectionX * speedX * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.velocity.Y -= speedY * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.spriteDirection = -1;
                    if (player.Center.X < NPC.Center.X)
                    {
                        NPC.spriteDirection = 1;
                    }
                }

                NPC.velocity *= 0.99f;
            }
        }

        public override void OnKill()
        {
            int numDusts = 45;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 212, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }
    }

    [AutoloadBossHead]
    class NobleSlime3 : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 72;
            NPC.height = 60;
            DrawOffsetY = -2f;
            NPC.scale = 2f;
            NPC.alpha = 255;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = Main.getGoodWorld ? 1000 : 500;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.npcSlots = 15f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.BossBar = GetInstance<NullBossBar>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = NPCType<NeonTyrant>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("One of the Neon Tyrant's elemental cohorts. This slime's thicker, more square body is a result of a rare mutation that gives it greater smashing capabilities.")
            });
        }

        private float alpha1 = 0;
        private float auraAlpha;
        float timer;
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D arrow = Request<Texture2D>("GloryMod/NPCs/NeonBoss/JumpIndicator").Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            if (!NPC.IsABestiaryIconDummy)
            {
                auraAlpha = MathHelper.Lerp(auraAlpha, NPC.damage > 0 ? 1 : 0, 0.15f);
                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    if (alpha1 > 0) Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha1 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }

                spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(Color.White) * alpha1, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

                Player player = Main.player[NPC.target];
                if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                    NPC.TargetClosest();

                if (timer >= MathHelper.Pi) timer = 0f;
                timer += 0.1f;

                for (int i = 0; i < 4; i++)
                {
                    Main.EntitySpriteDraw(arrow, player.Center - new Vector2(0, 150) + new Vector2(4 * indicatorAlpha, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4) - screenPos, null, NPC.GetAlpha(Color.Blue) * indicatorAlpha, 0, arrow.Size() / 2, 1, effects, 0);
                }

                spriteBatch.Draw(arrow, player.Center - new Vector2(0, 150) - screenPos, null, NPC.GetAlpha(Color.White) * indicatorAlpha, 0, arrow.Size() / 2, 1, effects, 0f);
            }
            else spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale * 0.5f, effects, 0f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 1;

            if (projectile.penetrate != 1 && projectile.DamageType != DamageClass.Melee) modNPC.DR = 0.5f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.ai[0] == 0)
                {
                    NPC.frame.Y = frameHeight * 3;
                }
                else
                {
                    NPC.frameCounter += 1;
                    if (NPC.frameCounter > 5)
                    {
                        NPC.frame.Y = NPC.frame.Y + frameHeight;
                        NPC.frameCounter = 0.0;
                    }
                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
            else
            {
                NPC.frameCounter += 1;
                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        private bool canfall;
        private float indicatorAlpha = 0;

        void Retreat()
        {
            NPC.TargetClosest(false);

            if (NPC.timeLeft > 10)
            {
                NPC.timeLeft = 10;
            }
            if (NPC.velocity.Y < 10)
            {
                NPC.velocity.Y -= 1f;
            }

            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            return;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            Vector2 groundPosition = NPC.Center.findGroundUnder();
            int goalDirectionX = player.Center.X < NPC.Center.X ? -1 : 1;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;
            NPC.damage = NPC.velocity.Y > .5f && NPC.ai[0] == 0 ? 80 : 0;
            alpha1 = MathHelper.Lerp(alpha1, 1, 0.1f);

            if (NPC.alpha > 0) NPC.alpha -= 17;

            canfall = NPC.Bottom.Y < player.Top.Y ? true : false;

            indicatorAlpha = MathHelper.Lerp(indicatorAlpha, NPC.ai[1] == 0 && NPC.ai[0] == 0 ? 1 : 0, 0.1f);

            //Should the enemy leave? 
            if (!player.active || player.dead) Retreat();

            switch (NPC.ai[1])
            {
                case 0:
                    //Nerfed blue attack from NT.

                    if (NPC.ai[0] == 0 && NPC.ai[2] == 0)
                    {
                        Hopping(0, 0, 30, false, false);
                    }
                    else
                    {
                        NPC.velocity.Y += Main.getGoodWorld ? 0.25f : 0.2f;
                        NPC.velocity.X += Main.getGoodWorld ? 0.325f * goalDirectionX : 0.25f * goalDirectionX;
                        if (NPC.velocity.Y >= 16)
                        {
                            NPC.velocity.Y = 16;
                        }

                        if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, canfall, canfall) != new Vector2(0, NPC.velocity.Y))
                        {
                            NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, canfall, canfall);
                            NPC.position.Y += NPC.velocity.Y;
                            NPC.velocity.Y = 0;
                            NPC.rotation = 0;

                            if (NPC.ai[0] == 0)
                            {
                                if (NPC.ai[2] == 1)
                                {
                                    for (int i = 0; i < 30; i++)
                                    {
                                        int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-72, 72), 0), 0, 0, 0, Scale: 2f);
                                        Main.dust[dust].noGravity = false;
                                        Main.dust[dust].noLight = true;
                                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(7), 0).RotatedByRandom(MathHelper.TwoPi);
                                    }
                                    SoundEngine.PlaySound(SoundID.Item62, NPC.Center);

                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), groundPosition + new Vector2(0, -50), Vector2.Zero, ProjectileType<NeonTyrantShockwave>(), 50, 1, player.whoAmI, 0, 0);
                                }
                                NPC.netUpdate = true;
                            }

                            if (NPC.ai[0] == 40)
                            {
                                NPC.ai[3] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[1] = 1;
                                NPC.ai[0] = 1;
                            }

                            NPC.ai[0]++;
                        }
                        else
                        {
                            if (NPC.velocity.Y < 0) NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.05f, 0.09f);
                            else
                            {
                                NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.05f, 0.01f);
                                NPC.velocity.Y += 1f;
                            }
                            NPC.ai[0] = 0;
                        }

                        if (NPC.ai[0] >= 20 && NPC.ai[2] == 0)
                        {
                            NPC.ai[0] = 0;
                            NPC.ai[2] = 1;
                            NPC.velocity.X += goalDirectionX * 10 * Main.rand.NextFloat(0.8f, 1.2f);
                            NPC.velocity.Y -= 25 * Main.rand.NextFloat(0.8f, 1.2f);
                            NPC.spriteDirection = -1;
                            if (player.Center.X < NPC.Center.X)
                            {
                                NPC.spriteDirection = 1;
                            }
                        }

                        NPC.velocity *= 0.99f;
                    }

                    break;

                case 1:
                    //Short rest.

                    Hopping(0, 0, 3000, false, false);
                    NPC.ai[2]++;

                    if (NPC.ai[2] == 70)
                    {
                        NPC.ai[3] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 1;
                    }

                    break;
            }

            void Hopping(float speedX, float speedY, int canjump = 60, bool compensateX = false, bool compensateY = false)
            {
                NPC.velocity.Y += 0.3f;
                if (NPC.velocity.Y >= 16) NPC.velocity.Y = 16;

                if (compensateX == true) speedX += (player.Center.X - NPC.Center.X) / 75 * goalDirectionX;

                if (compensateY == true) speedY -= (player.Center.Y - NPC.Center.Y) / 50;


                if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y))
                {
                    NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true);
                    NPC.position.Y += NPC.velocity.Y;
                    NPC.velocity.Y = 0;
                    NPC.rotation = 0;

                    if (NPC.ai[0] == 0 && !NPC.hide)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-68, 68), 0), 0, 0, 0, Scale: 2f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi);
                        }
                        SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
                    }

                    NPC.ai[0]++;
                }
                else
                {
                    if (NPC.velocity.Y < 0) NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.04f, 0.09f);
                    else
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.06f, 0.01f);
                        NPC.velocity.Y += 0.3f;
                    }

                    NPC.ai[0] = 0;
                }

                if (NPC.ai[0] >= canjump)
                {
                    NPC.ai[0] = 0;
                    NPC.velocity.X += goalDirectionX * speedX * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.velocity.Y -= speedY * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.spriteDirection = -1;
                    if (player.Center.X < NPC.Center.X)
                    {
                        NPC.spriteDirection = 1;
                    }
                }

                NPC.velocity *= 0.99f;
            }
        }

        public override void OnKill()
        {
            int numDusts = 45;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 176, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }
    }

    [AutoloadBossHead]
    class NobleSlime4 : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 76;
            NPC.height = 66;
            DrawOffsetY = -2f;
            NPC.scale = 2f;
            NPC.alpha = 255;

            NPC.damage = 0;
            NPC.defense = 16;
            NPC.lifeMax = Main.getGoodWorld ? 1300 : 650;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.npcSlots = 15f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.BossBar = GetInstance<NullBossBar>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = NPCType<NeonTyrant>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("One of the Neon Tyrant's elemental cohorts. This slime uses its sharp spikes and weaker kin in combat, sending in its gelatinous brethren as living explosives.")
            });
        }

        private float alpha1 = 0;
        private float auraAlpha;
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            if (!NPC.IsABestiaryIconDummy)
            {
                auraAlpha = MathHelper.Lerp(auraAlpha, NPC.damage > 0 ? 1 : 0, 0.15f);
                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    if (alpha1 > 0) Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 0, 0) * alpha1 * auraAlpha * (1 - i / (float)NPC.oldPos.Length) * .9f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }

                spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(Color.White) * alpha1, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }
            else spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale * 0.5f, effects, 0f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            DamageResistance modNPC = DamageResistance.modNPC(NPC);
            modNPC.DR = 1;

            if (projectile.penetrate != 1 && projectile.DamageType != DamageClass.Melee) modNPC.DR = 0.5f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.ai[0] == 0)
                {
                    NPC.frame.Y = frameHeight * 3;
                }
                else
                {
                    NPC.frameCounter += 1;
                    if (NPC.frameCounter > 5)
                    {
                        NPC.frame.Y = NPC.frame.Y + frameHeight;
                        NPC.frameCounter = 0.0;
                    }
                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
            else
            {
                NPC.frameCounter += 1;
                if (NPC.frameCounter > 5)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        void Retreat()
        {
            NPC.TargetClosest(false);

            if (NPC.timeLeft > 10)
            {
                NPC.timeLeft = 10;
            }
            if (NPC.velocity.Y < 10)
            {
                NPC.velocity.Y -= 1f;
            }

            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            return;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            Vector2 groundPosition = NPC.Center.findGroundUnder();
            int goalDirectionX = player.Center.X < NPC.Center.X ? -1 : 1;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;
            NPC.damage = NPC.velocity.Y > .5f && NPC.ai[0] == 0 ? 80 : 0;
            alpha1 = MathHelper.Lerp(alpha1, 1, 0.1f);

            if (NPC.alpha > 0) NPC.alpha -= 17;

            //Should the enemy leave?
            if (!player.active || player.dead)
            {
                Retreat();
            }

            switch (NPC.ai[1])
            {
                case 0:
                    //Summon mines from the ground, then leap at the player.

                    Hopping(15, 15, 200, true, true);
                    NPC.ai[2]++;

                    if (NPC.ai[2] == 40)
                    {
                        Vector2 spawnonground = new Vector2(650, 0) + groundPosition;
                        Vector2 spawnongroundnegative = new Vector2(-650, 0) + groundPosition;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), groundPosition, new Vector2(0, Main.rand.NextFloat(-14, -18)), ProjectileType<NeonTyrantMine>(), 50, 1, player.whoAmI);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnonground, new Vector2(0, Main.rand.NextFloat(-14, -18)), ProjectileType<NeonTyrantMine>(), 50, 1, player.whoAmI);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnongroundnegative, new Vector2(0, Main.rand.NextFloat(-14, -18)), ProjectileType<NeonTyrantMine>(), 50, 1, player.whoAmI);
                        NPC.netUpdate = true;
                    }

                    if (NPC.ai[3] == 2)
                    {
                        NPC.ai[3] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 1;
                        NPC.ai[0] = 1;
                    }

                    break;

                case 1:
                    //Chase player while firing out spikes.

                    Hopping(8, 12, 40, true, false);
                    NPC.ai[2]++;

                    if (NPC.ai[3] == 5)
                    {
                        NPC.ai[3] = 1;
                        NPC.ai[2] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 1;
                    }

                    break;
            }

            void Hopping(float speedX, float speedY, int canjump = 60, bool compensateX = false, bool compensateY = false)
            {
                NPC.velocity.Y += 0.3f;
                if (NPC.velocity.Y >= 16)
                {
                    NPC.velocity.Y = 16;
                }

                if (compensateX == true)
                {
                    speedX += (player.Center.X - NPC.Center.X) / 75 * goalDirectionX;
                }
                if (compensateY == true)
                {
                    speedY -= (player.Center.Y - NPC.Center.Y) / 50;
                }


                if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y))
                {
                    NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true);
                    NPC.position.Y += NPC.velocity.Y;
                    NPC.velocity.Y = 0;
                    NPC.rotation = 0;

                    if (NPC.ai[0] == 0)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            int dust = Dust.NewDust(NPC.Bottom + new Vector2(Main.rand.NextFloat(-76, 76), 0), 0, 0, 0, Scale: 1.5f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi);
                        }
                        SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
                        NPC.ai[3]++;

                        if (NPC.ai[1] == 1 && NPC.ai[2] >= 40)
                        {
                            NPC.ai[2] = 0;
                            float numberProjectiles = Main.getGoodWorld ? 10 : 8;
                            Vector2 targetPosition = new Vector2(0, -1).RotatedBy(0 + NPC.rotation);

                            for (int i = 0; i < numberProjectiles; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, targetPosition.RotatedByRandom(MathHelper.ToRadians(60)) * Main.rand.NextFloat(12, 16), ProjectileType<NeonTyrantSpike>(), 50, 2f, Main.myPlayer);
                            }
                        }
                        NPC.netUpdate = true;
                    }

                    NPC.ai[0]++;
                }
                else
                {
                    if (NPC.velocity.Y < 0)
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * 0.04f, 0.09f);
                    }
                    else
                    {
                        NPC.rotation = NPC.rotation.AngleTowards(0f - NPC.velocity.X * 0.06f, 0.01f);
                        NPC.velocity.Y += 0.3f;
                    }

                    NPC.ai[0] = 0;
                }

                if (NPC.ai[0] >= canjump)
                {
                    NPC.ai[0] = 0;
                    NPC.velocity.X += goalDirectionX * speedX * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.velocity.Y -= speedY * Main.rand.NextFloat(0.8f, 1.2f);
                    NPC.spriteDirection = -1;
                    if (player.Center.X < NPC.Center.X)
                    {
                        NPC.spriteDirection = 1;
                    }
                }

                NPC.velocity *= 0.99f;
            }
        }

        public override void OnKill()
        {
            int numDusts = 45;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.Center, 0, 0, 179, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }
    }

    class NobleVortex : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/NeonBoss/NeonVortex";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 100;
            Projectile.alpha = 255;
        }

        private float Speed = 1;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
            Projectile.rotation += 0.4f;
            Projectile.ai[0]++;

            if (Projectile.ai[0] >= 45)
            {
                Projectile.velocity *= 0.95f;

                if (Projectile.ai[0] == 45)
                {
                    if (Projectile.ai[1] == 1)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(target.Center) * new Vector2(0, 5), ProjectileType<VortexTelegraph>(), 50, 1, Projectile.owner);
                    }
                    else
                    {
                        float numberProjectiles = Projectile.ai[1];
                        float rotation = MathHelper.ToRadians(3.5f * Projectile.ai[1]);
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = Projectile.DirectionTo(target.Center).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1)));
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed * 5, ProjectileType<VortexTelegraph>(), 50, 1, Projectile.owner);
                        }
                    }
                }
                if (Projectile.ai[0] == 65)
                {
                    if (Projectile.ai[1] == 1)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(target.Center) * new Vector2(0, 1), ProjectileType<NeonLightning>(), 50, 1, Projectile.owner);
                    }
                    else
                    {
                        float numberProjectiles = Projectile.ai[1];
                        float rotation = MathHelper.ToRadians(3.5f * Projectile.ai[1]);
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = Projectile.DirectionTo(target.Center).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1)));
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ProjectileType<NeonLightning>(), 50, 1, Projectile.owner);
                        }
                    }
                    SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
                }
                if (Projectile.ai[0] > 80) Projectile.alpha += 12;
            }
            else
            {
                Vector2 goalPos = target.Center + new Vector2(0, -500);
                Vector2 goalVel = goalPos - Projectile.Center;

                if (goalVel.Length() > 5)
                {
                    Speed = MathHelper.Lerp(Speed, 10f, 0.06f);
                    goalVel.Normalize();
                    goalVel *= Speed;
                }
                else
                {
                    Speed = 1f;
                }

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, goalVel + target.position - target.oldPosition, 0.06f);

                if (Projectile.ai[0] <= 20)
                {
                    Projectile.alpha -= 13;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame();
            Vector2 drawOrigin = frame.Center();

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.GetAlpha(new Color(255, 255, 255) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f))), Projectile.rotation, drawOrigin, Projectile.scale * (1.3f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    class VortexTelegraph : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/NeonBoss/NeonLightning";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
            Projectile.alpha = 100;
            Projectile.extraUpdates = 2;
            Projectile.scale = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f;
            Projectile.ai[0]++;
            if (Projectile.timeLeft <= 20)
            {
                Projectile.alpha += 8;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame();
            Vector2 drawOrigin = frame.Center();

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.GetAlpha(new Color(255, 255, 255) * ((1 - i / (float)Projectile.oldPos.Length) * 0.99f))), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    class NeonVortex : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 100;
            Projectile.alpha = 255;
        }

        private float Speed = 1;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
            Projectile.rotation += 0.4f;
            Projectile.ai[0]++;

            if (Projectile.ai[0] >= 60)
            {
                Projectile.velocity *= 0.95f;

                if (Projectile.ai[0] == 60)
                {
                    if (Projectile.ai[1] == 1)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(target.Center) * new Vector2(0, 1), ProjectileType<NeonLightning>(), 50, 1, Projectile.owner);
                    }
                    else
                    {
                        float numberProjectiles = Projectile.ai[1];
                        float rotation = MathHelper.ToRadians(3 * Projectile.ai[1]);
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = Projectile.DirectionTo(target.Center).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1)));
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ProjectileType<NeonLightning>(), 50, 1, Projectile.owner);
                        }
                    }
                    SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
                }
                if (Projectile.ai[0] == 65)
                {
                    if (Projectile.ai[1] == 1)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(target.Center) * new Vector2(0, 1), ProjectileType<NeonLightning>(), 50, 1, Projectile.owner);
                    }
                    else
                    {
                        float numberProjectiles = Projectile.ai[1];
                        float rotation = MathHelper.ToRadians(4 * Projectile.ai[1]);
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = Projectile.DirectionTo(target.Center).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1)));
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ProjectileType<NeonLightning>(), 50, 1, Projectile.owner);
                        }
                    }
                    SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
                }
                if (Projectile.ai[0] > 80)
                {
                    Projectile.alpha += 12;
                }
            }
            else
            {
                Vector2 goalPos = target.Center + new Vector2(0, -500);
                Vector2 goalVel = goalPos - Projectile.Center;

                if (goalVel.Length() > 5)
                {
                    Speed = MathHelper.Lerp(Speed, 10f, 0.06f);
                    goalVel.Normalize();
                    goalVel *= Speed;
                }
                else
                {
                    Speed = 1f;
                }

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, goalVel + target.position - target.oldPosition, 0.06f);

                if (Projectile.ai[0] <= 20)
                {
                    Projectile.alpha -= 13;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame();
            Vector2 drawOrigin = frame.Center();

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.GetAlpha(new Color(255, 255, 255) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f))), Projectile.rotation, drawOrigin, Projectile.scale * (1.3f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    class NeonLightning : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 3;
            Projectile.scale = 1.2f;
        }

        private Vector2 startPosition;
        private Vector2 startVelocity;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {

            if (Projectile.timeLeft == 300)
            {
                startPosition = Projectile.Center;
                startVelocity = Projectile.velocity;
            }

            Vector2 goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * 12 * Projectile.ai[1] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 12 * (float)Math.Sin(Projectile.ai[0] + Projectile.ai[1] / 5f) * (float)Math.Sin(MathHelper.Pi * Projectile.ai[1] / 15f);

            Projectile.velocity = goalPosition - Projectile.Center;
            Projectile.rotation += 0.4f;
            Projectile.ai[1]++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.GetAlpha(new Color(255, 231, 86) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f))), Projectile.rotation, drawOrigin, Projectile.scale * (1.3f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class NeonTyrantDebris : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.alpha = 255;
            Projectile.scale = 1.2f;
        }

        private int rngtime = 0;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.rotation += rngtime * 0.05f;
            Projectile.ai[0]++;

            if (Projectile.ai[0] <= 100 + rngtime)
            {
                Projectile.velocity *= 0.99f;
            }
            else
            {
                if (Projectile.ai[0] == 101 + rngtime)
                {
                    Projectile.velocity = Projectile.DirectionTo(player.Center).RotatedByRandom(MathHelper.ToRadians(15)) * (15 - rngtime / 3);
                }
                else
                {
                    if (Projectile.ai[0] >= 161 + rngtime)
                    {
                        Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.DirectionTo(player.Center).ToRotation(), MathHelper.ToRadians(1f * (Projectile.velocity.Length() / 20))).ToRotationVector2() * Projectile.velocity.Length();
                    }
                    Projectile.velocity *= 1.005f;
                }
                Projectile.tileCollide = true;
            }

            if (Projectile.ai[1] == 0)
            {
                Projectile.frame = Main.rand.Next(0, 3) % Main.projFrames[Projectile.type];
                rngtime = Main.rand.Next(-15, 15);
                Projectile.ai[1] = 1;
            }

            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 4;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D trailtexture = Request<Texture2D>(Texture + "Trail").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trailtexture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 231, 86) * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f)), Projectile.rotation, drawOrigin, Projectile.scale * (1.3f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center + new Vector2(Main.rand.NextFloat(10, -10), Main.rand.NextFloat(10, -10)), 0, 0, 1, Scale: 2f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi);
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, ProjectileType<DebrisExplosion>(), 40, 8f, Projectile.owner, 0f, 0f);
        }
    }

    class DebrisExplosion : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/Glow_1";

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 20;
            Projectile.alpha = 0;
            Projectile.scale = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            if (Projectile.ai[0] >= 1)
            {
                Projectile.alpha += 12;
            }

            Projectile.ai[0]++;
            Projectile.scale *= 1.06f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Center();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            int Texturenumber = 15;
            for (int i = 0; i < Texturenumber; i++)
            {
                Main.EntitySpriteDraw(texture, (Projectile.Center + new Vector2(Main.rand.NextFloat(50) * Projectile.scale, 0).RotatedByRandom(MathHelper.TwoPi)) - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.ai[1] == 1 ? new Color(239, 94, 255) : new Color(255, 231, 86)),
                Projectile.rotation, origin, (Projectile.scale * Main.rand.NextFloat(2)), SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class NeonTyrantShockwave : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/Glow_1";

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40;
            Projectile.alpha = 0;
            Projectile.scale = 1f;
            DrawOriginOffsetY = -4;
        }

        private int timer = 0;
        private float rotation = 0;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 groundPosition = Projectile.Center.findGroundUnder();

            if (timer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
                rings.Add(new Tuple<Vector2, float, float>(Projectile.Center, 0f, 50f));
                rotation = Main.rand.NextFloat(-0.2f, 0.2f);
                ScreenUtils.screenShaking = 10f;

                int numDusts = 30;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 226, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5, 15), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }

            if (player.velocity.Y == 0 && player.immuneTime <= 0 && timer < 10)
            {
                PlayerDeathReason shockwave = new PlayerDeathReason();
                player.Hurt(shockwave, Projectile.damage, 0, false, false, 40, false);
                player.immune = true;
                player.immuneTime = 40;
                player.velocity.Y -= 10;
            }


            timer++;
            Projectile.rotation += rotation;
            float light = Projectile.timeLeft / 4;
            Lighting.AddLight(Projectile.Center, 0.6f * light, 1 * light, 0.9f * light);
        }

        List<Tuple<Vector2, float, float>> rings = new List<Tuple<Vector2, float, float>>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D ringTexture = (Texture2D)Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_5");
            Player player = Main.player[Projectile.owner];

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < rings.Count; i++)
            {
                if (i >= rings.Count)
                {
                    break;
                }

                Main.EntitySpriteDraw(ringTexture, rings[i].Item1 - Main.screenPosition, null, new Color(145, 255, 231, Projectile.timeLeft * 6), Projectile.rotation, ringTexture.Size() / 2, rings[i].Item2 / ringTexture.Width, SpriteEffects.None, 0);

                rings[i] = new Tuple<Vector2, float, float>(rings[i].Item1, rings[i].Item2 + rings[i].Item3, rings[i].Item3);
                if (rings[i].Item2 >= player.Distance(rings[i].Item1) + Main.screenWidth)
                {
                    rings.RemoveAt(i);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class NeonTyrantMine : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
            Projectile.scale = 1f;
        }

        private float rngrotation = 0;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 129)
            {
                Projectile.alpha -= 5;
            }
            else
            {
                Projectile.hostile = true;
            }

            if (Projectile.timeLeft == 180)
            {
                rngrotation = Main.rand.NextFloat(0.55f, -0.55f);
            }

            Projectile.rotation += rngrotation * (Projectile.velocity.Y * 0.01f);
            Projectile.velocity *= 0.98f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            float numberProjectiles = 8;
            Vector2 targetPosition = new Vector2(44, 0).RotatedBy(0 + Projectile.rotation);
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);

            for (int i = 0; i < numberProjectiles; i++)
            {
                Vector2 direction = (targetPosition).RotatedBy(i * MathHelper.TwoPi / numberProjectiles);
                direction.Normalize();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + direction, direction * 12, ProjectileType<NeonTyrantSpike>(), 30, 2f, Main.myPlayer);
            }

            int numDusts = 45;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 179, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(25), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, ProjectileType<DebrisExplosion>(), (Projectile.damage), 8f, Projectile.owner, 0f, 1f);
        }
    }

    class NeonTyrantSpike : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 200;
            Projectile.alpha = 0;
            Projectile.scale = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.1f;
            Projectile.velocity *= 0.99f;

            if (Projectile.timeLeft <= 10)
            {
                Projectile.alpha += 25;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)) * ((1 - i / (float)Projectile.oldPos.Length) * 0.99f), Projectile.rotation, drawOrigin, Projectile.scale * (1f - i / (float)Projectile.oldPos.Length) * 0.99f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
