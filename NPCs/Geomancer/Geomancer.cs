using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using GloryMod.Systems;
using Terraria.GameContent.ItemDropRules;
using GloryMod.Systems.BossBars;
using System.IO;
using GloryMod.Items.Geomancer;

namespace GloryMod.NPCs.Geomancer
{
    [AutoloadBossHead]
    internal class Geomancer : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 16;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
                new FlavorTextBestiaryInfoElement("A lesser magician, fueled by geological activity found deep underground. His rough and dangerous lifestyle has left him weary and hostile to strangers." +
                "Whatever you do, do not call him 'Timothy.'")
            });
        }

        private enum AttackPattern
        {
            StartBattle = 0,
            Idle = 1,
            Teleport = 2,
            RedSpell = 3,
            BlueSpell = 4,
            YellowSpell = 5,
            WinAnim = 6,
            DefeatAnim = 7
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float TimerRand => ref NPC.ai[2];
        public ref float teleTimer => ref NPC.ai[3];

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 58;
            NPC.height = 90;

            NPC.damage = 0;
            NPC.defense = Main.hardMode ? 30 : 10;
            NPC.lifeMax = Main.getGoodWorld ? (Main.hardMode ? 30000 : 2000) : (Main.hardMode ? 22500 : 1500);
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            NPC.npcSlots = 50f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.DD2_GoblinBomberHurt with { Pitch = -0.5f };
            NPC.DeathSound = null;
            NPC.netAlways = true;

            NPC.BossBar = GetInstance<GeomancerBossBar>();
            Music = MusicLoader.GetMusicSlot(Mod, "Music/Wizard_Time");
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;
                capeFrameY += frameHeight;
                if (capeFrameY >= frameHeight * 8) capeFrameY = 0;
            }

            if (attackAnim == 0) if (NPC.frame.Y >= frameHeight * 8) NPC.frame.Y = 0;

            if (attackAnim == 1)
            {
                if (NPC.frame.Y < frameHeight * 8) NPC.frame.Y = frameHeight * 8;
                if (NPC.frame.Y >= frameHeight * 12) NPC.frame.Y = frameHeight * 12;
            }

            if (attackAnim == 2)
            {
                if (NPC.frame.Y < frameHeight * 12) NPC.frame.Y = frameHeight * 12;
                if (NPC.frame.Y >= frameHeight * 16)
                {
                    NPC.frame.Y = 0;
                    attackAnim = 0;
                }
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(attackAnim);
                writer.Write(teleThreshold);
                writer.Write(capeFrameY);
                writer.Write(lastAttack);
                writer.Write(inPhase2);
                writer.Write(hasDied);
                writer.Write(invisible);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                attackAnim = reader.ReadInt32();
                teleThreshold = reader.ReadInt32();
                capeFrameY = reader.ReadInt32();
                lastAttack = reader.ReadInt32();
                inPhase2 = reader.ReadBoolean();
                hasDied = reader.ReadBoolean();
                invisible = reader.ReadBoolean();
            }
        }

        private float teleThreshold = 480;
        private Vector2 goalPosition;
        private int attackAnim;
        private int capeFrameY;
        private int lastAttack;
        private bool inPhase2;
        private bool hasDied;
        private bool invisible;

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && !hasDied)
            {
                hasDied = true;
                NPC.dontTakeDamage = true;
                NPC.noTileCollide = true;
                NPC.life = 1;
                NPC.ai[0] = 7;
                AITimer = 0;
                NPC.netUpdate = true;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule hardMode = new LeadingConditionRule(new Conditions.IsHardmode());

            hardMode.OnSuccess(ItemDropRule.Common(ItemType<ArcaneDischarge>()));
            hardMode.OnSuccess(ItemDropRule.Common(ItemID.RuneRobe));
            hardMode.OnSuccess(ItemDropRule.Common(ItemID.RuneHat));

            npcLoot.Add(ItemDropRule.Common(ItemID.WizardHat));
            npcLoot.Add(ItemDropRule.Common(ItemType<OrbOfPondering>()));
            npcLoot.Add(hardMode);

            for (int i = 0;  i < 10; i++) npcLoot.Add(ItemDropRule.OneFromOptions(1, ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Amber, ItemID.Diamond));

        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = Main.hardMode ? ItemID.HealingPotion : ItemID.LesserHealingPotion;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            teleTimer++;

            if (NPC.target < 0 || NPC.target == 255 || player.dead || !player.active)
                NPC.TargetClosest();

            NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            inPhase2 = Main.getGoodWorld ? (NPC.life <= (NPC.lifeMax / 3) * 2 ? true : false) : (NPC.life <= (NPC.lifeMax / 2) ? true : false);

            switch (AIstate)
            {
                case AttackPattern.StartBattle:

                    TimerRand = 60;
                    teleTimer = teleThreshold;
                    NPC.ai[0] = 1;
                    NPC.netUpdate = true;

                    break;

                case AttackPattern.Idle:

                    AITimer++;
                    attackAnim = 0;

                    if (AITimer >= TimerRand)
                    {
                        ChooseAttack();
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.Teleport:

                    AITimer++;
                    invisible = AITimer >= 20 && AITimer <= 100;
                    NPC.dontTakeDamage = invisible;

                    if (AITimer <= 30) Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 110, Scale: 2f)].noGravity = true;
                    if (AITimer <= 60) visibility = MathHelper.Lerp(visibility, 0, 0.05f);                  
                    if (AITimer >= 60) visibility = MathHelper.Lerp(visibility, 1, 0.05f);

                    if (AITimer == 60)
                    {
                        if (FindTeleportPoint(player))
                        {
                            if (Main.hardMode && inPhase2)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(200, 0).RotatedBy(i * MathHelper.TwoPi / 4), Vector2.Zero, ProjectileType<GeomancerGreenProjectile>(), Main.hardMode ? 150 : 50, 0, player.whoAmI);
                                }
                            }

                            NPC.position = goalPosition;
                            NPC.netUpdate = true;
                            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
                        }
                        else AITimer--;
                    }
                    
                    if (AITimer == 120)
                    {
                        setRand();
                        NPC.ai[0] = 1;
                        NPC.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot, NPC.Center);
                        int numDusts = 24;

                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 110, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            Main.dust[dust].velocity.X *= 0.75f;
                        }
                    }

                    break;

                case AttackPattern.RedSpell:

                    SpellPrep();

                    if (AITimer == 90)
                    {

                        for (int i = Main.hardMode ? -2 : -1; i < (Main.hardMode ? 3 : 2); i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), player.Center + new Vector2(200 + (40 * i), 0), Vector2.Zero, ProjectileType<GeomancerRedProjectile>(), Main.hardMode ? 150 : 50, 0, player.whoAmI, player.whoAmI, i, 1);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), player.Center - new Vector2(200 + (40 * i), 0), Vector2.Zero, ProjectileType<GeomancerRedProjectile>(), Main.hardMode ? 150 : 50, 0, player.whoAmI, player.whoAmI, i, -1);
                        }

                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, NPC.Center);
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.BlueSpell:

                    SpellPrep();

                    if (AITimer == 90)
                    {
                        for (int i = Main.hardMode ? -5 : -4; i < (Main.hardMode ? 6 : 5); i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), player.Center + new Vector2(150 * -NPC.spriteDirection, 0).RotatedBy(i * MathHelper.Pi / 9), Vector2.Zero, ProjectileType<GeomancerBlueProjectile>(), Main.hardMode ? 150 : 50, 0, player.whoAmI, player.whoAmI, i, NPC.spriteDirection);
                        }

                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, NPC.Center);
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.YellowSpell:

                    SpellPrep();

                    if (AITimer == 90)
                    {
                        for (int i = Main.hardMode ? -7 : -5; i < (Main.hardMode ? 8 : 6); i++)
                        {
                            int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), player.Center + new Vector2(40 * i, Main.hardMode ? -200 + Math.Abs(15 * i) : -200), Vector2.Zero, ProjectileType<GeomancerYellowProjectile>(), Main.hardMode ? 150 : 50, 0, player.whoAmI, player.whoAmI, i);
                            Main.projectile[proj].ai[1] = i;
                        }
                        if (Main.hardMode)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), player.Center + new Vector2(0, -200), Vector2.Zero, ProjectileType<GeomancerYellowProjectile>(), Main.hardMode ? 150 : 50, 0, player.whoAmI, player.whoAmI, 0, i);
                            }
                        }

                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, NPC.Center);
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.WinAnim:

                    AITimer++;
                    NPC.dontTakeDamage = true;
                    attackAnim = 0;

                    if (AITimer <= 30) Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 110, Scale: 2f)].noGravity = true;
                    if (AITimer <= 60) visibility = MathHelper.Lerp(visibility, 0, 0.05f);
                    if (AITimer == 60) NPC.active = false;

                    break;

                case AttackPattern.DefeatAnim:

                    AITimer++;
                    NPC.rotation = AITimer * (float)Math.Sin(AITimer / 2f) * 0.005f;

                    if (AITimer == 1)
                    {
                        NPC.NPCLoot();
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileID.DD2ExplosiveTrapT3Explosion, 0, 0, player.whoAmI);
                        NPC.velocity = new Vector2(5 * -NPC.spriteDirection, -10);

                        int gore = Mod.Find<ModGore>("GeomancerStaff").Type;
                        Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, new Vector2(-5 * NPC.spriteDirection, -3), gore);
                        SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DD2_GoblinDeath with { Pitch = -0.5f }, NPC.Center);
                        NPC.netUpdate = true;
                    }

                    if (AITimer >= 200)
                    {
                        SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 1.5f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DD2_GoblinBomberHurt with { Pitch = -0.5f }, NPC.Center);
                        ScreenUtils.screenShaking = 5f;
                        NPC.active = false;
                        NPC.netUpdate = true;
                    }

                    break;
                    
            }

            void SpellPrep()
            {
                AITimer++;

                if (AITimer < 90) 
                {
                    attackAnim = 1;
                    visibility2 = MathHelper.Lerp(visibility2, 1, 0.025f);
                }
                else visibility2 = MathHelper.Lerp(visibility2, 0, 0.025f);
                if (AITimer == 30) SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, NPC.Center);
                if (AITimer == 90) attackAnim = 2;

                if (AITimer == 120)
                {
                    setRand();
                    NPC.ai[0] = 1;
                    NPC.netUpdate = true;
                }
            }
        }

        void setRand()
        {
            TimerRand = Main.getGoodWorld ? (inPhase2 ? 0 : 60) : (inPhase2 ? Main.rand.Next(0, 60) : Main.rand.NextFloat(60, 120));
            AITimer = 0;

            if (inPhase2)
            {
                teleThreshold = 400;
                if (teleTimer >= teleThreshold) TimerRand -= 20;
            }
        }

        private void ChooseAttack()
        {
            int Choice;
            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
                NPC.ai[0] = 6;
            }
            else
            {
                if (teleTimer >= teleThreshold)
                {
                    NPC.ai[0] = Choice = 2;
                    teleTimer = 0;
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
                }
                else
                {
                    Choice = Main.rand.Next(new int[] { 3, 4, 5 });
                    while (Choice == lastAttack) Choice = Main.rand.Next(new int[] { 3, 4, 5 });
                    if (Choice != lastAttack)
                    {
                        NPC.ai[0] = Choice;
                        lastAttack = Choice;
                    }
                }
            }      

            if (Main.rand.NextBool(2)) SoundEngine.PlaySound(SoundID.DD2_GoblinScream with { Pitch = -1f }, NPC.Center);
            AITimer = 0;
        }

        private bool FindTeleportPoint(Player player)
        {
            //try up to 20 times
            for (int i = 0; i < 40; i++)
            {
                float direction = Main.rand.NextBool(2) ? -1 : 1;

                Vector2 tryGoalPoint = player.Center + new Vector2(-NPC.width / 2 + Main.rand.NextFloat(250f, 600f) * direction, Main.rand.NextFloat(-450f, 450f));
                tryGoalPoint.Y = 16 * (int)(tryGoalPoint.Y / 16);
                tryGoalPoint -= new Vector2(0, NPC.height);

                bool viable = true;

                for (int x = (int)((tryGoalPoint.X) / 16); x <= (int)((tryGoalPoint.X + NPC.width) / 16); x++)
                {
                    for (int y = (int)((tryGoalPoint.Y) / 16); y <= (int)((tryGoalPoint.Y + NPC.height) / 16); y++)
                    {
                        if (Main.tile[x, y].HasUnactuatedTile)
                        {
                            viable = false;
                            break;
                        }
                    }

                    if (!viable)
                    {
                        break;
                    }
                }

                if (viable)
                {
                    for (int y = (int)((tryGoalPoint.Y + NPC.height) / 16); y < Main.maxTilesY; y++)
                    {
                        int x = (int)((tryGoalPoint.X + NPC.width / 2) / 16);
                        if (Main.tile[x, y].HasUnactuatedTile && (Main.tileSolid[Main.tile[x, y].TileType] || Main.tileSolidTop[Main.tile[x, y].TileType]))
                        {
                            goalPosition = new Vector2(tryGoalPoint.X, y * 16 - NPC.height);

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        float timer;
        float visibility = 1;
        float visibility2;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D cape = Request<Texture2D>(Texture + "Cape").Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D textureDefeat = Request<Texture2D>(Texture + "Defeated").Value;
            Texture2D capeDefeat = Request<Texture2D>(Texture + "DefeatedCape").Value;
            Texture2D maskDefeat = Request<Texture2D>(Texture + "DefeatedMask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            Rectangle capeRect = new Rectangle(0, capeFrameY, cape.Width, cape.Height / 15);
            Vector2 capeOrigin = new Vector2(cape.Width / 2, (cape.Height / 2) / 15);
            SpriteEffects effects;

            if (NPC.spriteDirection > 0)
            {
                effects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                effects = SpriteEffects.None;
            }

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            //Makes sure it does not draw its normal code for its bestiary entry.
            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.ai[0] == 2 || NPC.ai[0] == 6)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Main.EntitySpriteDraw(texture, drawPos + new Vector2(8 - (8 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(0, 255, 0, 100) * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                        Main.EntitySpriteDraw(cape, drawPos - new Vector2(12 * NPC.spriteDirection, -20) + new Vector2(8 - (8 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), capeRect, new Color(0, 255, 0, 100) * visibility, NPC.rotation, capeOrigin, NPC.scale, effects, 0);
                    }
                }

                if (NPC.ai[0] == 3)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Main.EntitySpriteDraw(texture, drawPos + new Vector2(8 - (8 * visibility2), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(250, 100, 50, 100) * visibility2, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                        Main.EntitySpriteDraw(cape, drawPos - new Vector2(12 * NPC.spriteDirection, -20) + new Vector2(8 - (8 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), capeRect, new Color(255, 0, 0, 100) * visibility, NPC.rotation, capeOrigin, NPC.scale, effects, 0);
                    }
                }

                if (NPC.ai[0] == 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Main.EntitySpriteDraw(texture, drawPos + new Vector2(8 - (8 * visibility2), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(50, 80, 220, 100) * visibility2, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                        Main.EntitySpriteDraw(cape, drawPos - new Vector2(12 * NPC.spriteDirection, -20) + new Vector2(8 - (8 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), capeRect, new Color(0, 0, 255, 100) * visibility, NPC.rotation, capeOrigin, NPC.scale, effects, 0);
                    }
                }

                if (NPC.ai[0] == 5)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Main.EntitySpriteDraw(texture, drawPos + new Vector2(8 - (8 * visibility2), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(200, 200, 50, 100) * visibility2, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                        Main.EntitySpriteDraw(cape, drawPos - new Vector2(12 * NPC.spriteDirection, -20) + new Vector2(8 - (8 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), capeRect, new Color(150, 150, 0, 100) * visibility, NPC.rotation, capeOrigin, NPC.scale, effects, 0);
                    }
                }

                if (!hasDied)
                {
                    spriteBatch.Draw(cape, drawPos - new Vector2(12 * NPC.spriteDirection, -20), capeRect, drawColor * visibility, NPC.rotation, capeOrigin, NPC.scale, effects, 0f);
                    spriteBatch.Draw(texture, drawPos + new Vector2(2 * NPC.spriteDirection, -2), NPC.frame, drawColor * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                    spriteBatch.Draw(mask, drawPos + new Vector2(2 * NPC.spriteDirection, -2), NPC.frame, new Color(255, 255, 255) * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                }
                else
                {
                    spriteBatch.Draw(capeDefeat, drawPos - new Vector2(12 * NPC.spriteDirection, -20), null, drawColor, NPC.rotation, capeDefeat.Size() / 2, NPC.scale, effects, 0f);
                    spriteBatch.Draw(textureDefeat, drawPos + new Vector2(2 * NPC.spriteDirection, -2), null, drawColor, NPC.rotation, textureDefeat.Size() / 2, NPC.scale, effects, 0f);
                    spriteBatch.Draw(maskDefeat, drawPos + new Vector2(2 * NPC.spriteDirection, -2), null, new Color(255, 255, 255), NPC.rotation, maskDefeat.Size() / 2, NPC.scale, effects, 0f);
                }
            }
            else
            {
                spriteBatch.Draw(cape, drawPos - new Vector2(12 * NPC.spriteDirection, -22), capeRect, drawColor, NPC.rotation, capeOrigin, NPC.scale, effects, 0f);
                spriteBatch.Draw(texture, drawPos + new Vector2(2 * NPC.spriteDirection, 4), NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                spriteBatch.Draw(mask, drawPos + new Vector2(2 * NPC.spriteDirection, 4), NPC.frame, new Color(255, 255, 255), NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }        

            return false;
        }
    }
}
