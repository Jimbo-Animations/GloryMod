using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Utilities;

namespace GloryMod.NPCs.IceFish
{
    partial class HM : ModNPC
    {
        private enum AttackPattern
        {
            StartBattle = 0,
            Idle = 1,
            JumpStrike = 2,
            SpikeSkim = 3,
            DebrisShower = 4,
            SuperJump = 5,
            Stun = 6,
            DeathAnim = 7
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float TimerRand => ref NPC.ai[2];
        public ref float AggravationCount => ref NPC.ai[3];

        public bool hasJumped;
        public bool jumping;
        public bool shouldDie;

        public Vector2 moveToZone;
        public NPC minion;

        public float blurAlpha;
        public int enrageTimer;
        public int damageScale;

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

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && !shouldDie)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                AggravationCount++;
                shouldDie = true;

                //cancel all buffs
                for (int i = 0; i < NPC.buffTime.Length; i++)
                {
                    NPC.buffTime[i] = 0;
                }
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = Main.hardMode ? ItemID.GreaterHealingPotion : ItemID.HealingPotion;
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

        private bool platformCheese()
        {
            bool collision = CheckTileCollision();
            bool cheese = false;

            if ((Systems.Utils.findGroundUnder(target.Top).Y + 250 < (collision ? Systems.Utils.findSurfaceAbove(NPC.Center).Y : Systems.Utils.findGroundUnder(NPC.Center).Y))
                || (target.Bottom.Y + 400 < Systems.Utils.findGroundUnder(target.Bottom).Y))
                cheese = true;

            return cheese;
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

        // This is also from Star Construct

        private float[] aiWeights = new float[5];
        private void PickAttack()
        {
            WeightedRandom<int> aiStatePool = new WeightedRandom<int>();
            for (int state = 1; state < aiWeights.Length; state++)
            {
                //weights are squared to bias more towards attacks that haven't been used in a while
                aiStatePool.Add(state, Math.Pow(aiWeights[state], 2));
            }
            NPC.ai[0] = aiStatePool;

            for (int state = 1; state < aiWeights.Length; state++)
            {
                if (NPC.ai[0] != state)
                    aiWeights[state] += aiWeights[(int)NPC.ai[0]] / (aiWeights.Length - 1);
            }
            aiWeights[(int)NPC.ai[0]] = 0f;
        }

        private void InitializeAIStates()
        {
            aiWeights[0] = 0;
            for (int state = 1; state < aiWeights.Length; state++)
            {
                aiWeights[state] = 1f;
            }
        }

        private void ResetValues(bool enraged = false)
        {
            NPC.ai[0] = enraged ? -1 : 1;
            AITimer = 0;
            animState = 0;
            TimerRand = Main.rand.NextFloat(120, 181);
            hasJumped = false;
            jumping = false;

            NPC.netUpdate = true;
        }
    }
}
