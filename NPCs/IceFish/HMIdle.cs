using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace GloryMod.NPCs.IceFish
{
    internal class HMIdle : ModNPC
    {
        public override string Texture => "GloryMod/NPCs/IceFish/HM";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            NPCID.Sets.TrailCacheLength[NPC.type] = 6;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 54;
            NPC.height = 54;

            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = Main.getGoodWorld ? (Main.hardMode ? 14400 : 2400) : (Main.hardMode ? 12000 : 2000);
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.npcSlots = 50f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.DD2_WitherBeastCrystalImpact;
            NPC.DeathSound = null;
            NPC.coldDamage = true;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float AggroTimer => ref NPC.ai[2];
        public ref float SoundCooldown => ref NPC.ai[3];

        Vector2 patrolZone;

        public override bool CheckActive()
        {
            return NPC.ai[1] >= 3600;
        }

        private int animState = 0;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 4;
            NPC.frameCounter++;

            if (NPC.frameCounter > 4)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;
            }

            switch (animState)
            {
                case 0:

                    NPC.frame.X = 0;

                    if (NPC.frame.Y >= frameHeight * 8)
                    {
                        NPC.frame.Y = 0;
                    }

                    break;

                case 1:

                    NPC.frame.X = NPC.frame.Width;

                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }

                    break;

                case 2:

                    NPC.frame.X = NPC.frame.Width * 2;

                    if (NPC.frame.Y >= frameHeight * 2 && NPC.velocity.Y < 0)
                    {
                        NPC.frame.Y = frameHeight;
                    }

                    if (NPC.frame.Y >= frameHeight * 4 && NPC.velocity.Y > 0)
                    {
                        NPC.frame.Y = frameHeight * 3;
                    }

                    break;

                case 3:

                    NPC.frame.X = NPC.frame.Width * 3;

                    if (NPC.frame.Y >= frameHeight * 6)
                    {
                        NPC.frame.Y = frameHeight;
                    }

                    break;
            }           
        }
        private bool CheckTileCollision()
        {
            int minTilePosX = (int)(NPC.Left.X / 16) - 1;
            int maxTilePosX = (int)(NPC.Right.X / 16) + 2;
            int minTilePosY = (int)(NPC.Top.Y / 16) - 1;
            int maxTilePosY = (int)(NPC.Bottom.Y / 16) + 2;

            // Ensure that the tile range is within the world bounds
            if (minTilePosX < 0)
                minTilePosX = 0;
            if (maxTilePosX > Main.maxTilesX)
                maxTilePosX = Main.maxTilesX;
            if (minTilePosY < 0)
                minTilePosY = 0;
            if (maxTilePosY > Main.maxTilesY)
                maxTilePosY = Main.maxTilesY;

            bool collision = false;

            // This is the initial check for collision with tiles.
            for (int i = minTilePosX; i < maxTilePosX; ++i)
            {
                for (int j = minTilePosY; j < maxTilePosY; ++j)
                {
                    Tile tile = Main.tile[i, j];

                    // If the tile is solid or is a filled liquid, then there's valid collision
                    if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] || tile.LiquidAmount > 64)
                    {
                        Vector2 tileWorld = new Point16(i, j).ToWorldCoordinates(0, 0);

                        if (NPC.Right.X > tileWorld.X && NPC.Left.X < tileWorld.X + 16 && NPC.Bottom.Y > tileWorld.Y && NPC.Top.Y < tileWorld.Y + 16)
                        {
                            // Collision found
                            collision = true;

                            if (Main.rand.NextBool(100))
                                WorldGen.KillTile(i, j, fail: true, effectOnly: true, noItem: false);
                        }
                    }
                }
            }

            return collision;
        }

        private void TunnelAbout(Vector2 targetPos, ref bool collision, bool forceBurrow, float maxSpeedH = 10, float maxSpeedV = 5, float leniency = 250, float depth = 250)
        {
            Vector2 goTo = new(targetPos.X, targetPos.Y + depth);

            float speedH = .4f;
            float speedV = .25f;

            if (collision)
            {
                // This bit is modified code from Star Construct, from the Polarities mod.

                if (Math.Abs(NPC.velocity.X) < maxSpeedH)
                {
                    //only accelerate if going towards the target or if far enough away
                    if ((NPC.velocity.X != 0 && ((goTo.X > NPC.Center.X) == (NPC.velocity.X > 0))) || Math.Abs(NPC.Center.X - goTo.X) > leniency)
                    {
                        NPC.velocity.X += goTo.X > NPC.Center.X ? speedH : -speedH;
                    }
                    else if (Math.Abs(NPC.Center.X - goTo.X) <= leniency && (NPC.velocity.X == 0 || ((goTo.X > NPC.Center.X) != (NPC.velocity.X > 0))))
                    {
                        //if too close and going away, accelerate away
                        NPC.velocity.X += goTo.X > NPC.Center.X ? -speedH : speedH;
                    }
                }
                else
                {
                    //decelerate if going too fast
                    if (Math.Abs(NPC.velocity.X) > speedH)
                    {
                        NPC.velocity.X += NPC.velocity.X > 0 ? -speedH : speedH;
                    }
                    else NPC.velocity.X = 0;
                }

                // Controls Y velocity

                if ((NPC.Distance(Systems.Utils.findSurfaceAbove(NPC.Center)) < 32 || !collision) && forceBurrow && NPC.Distance(target.Center) < 1000) NPC.velocity.Y += .3f;

                if (Math.Abs(NPC.velocity.Y) < maxSpeedV)
                {
                    NPC.velocity.Y += goTo.Y > NPC.Center.Y ? speedV : -speedV;
                }
                else
                {
                    //decelerate if going too fast
                    if (Math.Abs(NPC.velocity.Y) > speedV)
                    {
                        NPC.velocity.Y += NPC.velocity.Y > 0 ? -speedV : speedV;
                    }
                    else NPC.velocity.Y = 0;
                }
            }
            else NPC.velocity.Y += .11f;

            if (NPC.velocity.Y > 8) NPC.velocity.Y = 8;

            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.Y * .075f * NPC.spriteDirection, 0.1f);
            NPC.velocity *= .99f;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            if (AITimer == 0) patrolZone = NPC.Center;

            AITimer++;
            SoundCooldown++;

            bool collision = CheckTileCollision();

            if (Math.Abs(target.Center.X - NPC.Center.X) < 400 && NPC.Distance(target.Center) < 1000)
            {
                AggroTimer++;

                if (AggroTimer == 450 && SoundCooldown > 60) 
                {
                    SoundEngine.PlaySound(SoundID.DD2_DrakinDeath with { Volume = 3, Pitch = -.2f }, NPC.Center);
                    SoundCooldown = 0;
                    NPC.netUpdate = true;
                }

                if (AggroTimer > 600)
                {
                    NPC.Transform(NPCType<HM>());
                    CombatText.NewText(NPC.getRect(), new Color(50, 100, 255), "!", true, false);
                    NPC.netUpdate = true;
                }
            }
            else AggroTimer--;

            if (NPC.soundDelay == 0 && collision)
            {
                // Play sounds quicker the closer the NPC is to the target location
                float num1 = NPC.Distance(target.Center) / 40f;

                if (num1 < 15)
                    num1 = 15f;

                if (num1 > 20)
                    num1 = 20f;

                NPC.soundDelay = (int)num1;

                SoundEngine.PlaySound(SoundID.WormDig, NPC.position);

                SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
            }

            TunnelAbout(Systems.Utils.findGroundUnder(patrolZone), ref collision, true, 4, 3, 300, 300);
            MathHelper.Clamp(AggroTimer, 0, 600);        
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            SoundEngine.PlaySound(SoundID.DD2_DrakinHurt with { Volume = 2, Pitch = -.2f }, NPC.Center);
            NPC.Transform(NPCType<HM>());
            CombatText.NewText(NPC.getRect(), new Color(50, 100, 255), "!", true, false);
            NPC.netUpdate = true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.PlayerSafe) return 0f;
            if (NPC.AnyNPCs(NPCType<HM>()) || NPC.AnyNPCs(NPCType<HMIdle>())) return 0f;
            if (spawnInfo.Player.ZoneSnow && Main.raining)
            {
                if (!Main.dayTime) return 0.015f;
                else return 0.01f;
            }
            else return 0f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2((NPC.frame.Width * .5f) + (NPC.frame.Width * .35f * NPC.spriteDirection), NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects;

            NPC.spriteDirection = NPC.direction = NPC.velocity.X > 0 ? 1 : -1;

            if (NPC.spriteDirection > 0) effects = SpriteEffects.FlipHorizontally;
            else effects = SpriteEffects.None;

            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }    
}
