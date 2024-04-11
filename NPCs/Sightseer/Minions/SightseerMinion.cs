using Terraria.GameContent.Bestiary;
using System.IO;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;

namespace GloryMod.NPCs.Sightseer.Minions
{
    internal class SightseerMinion : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Rain,
                new FlavorTextBestiaryInfoElement("These flying jellyfish seem to hail from a land far beyond our own world!" +
                " While possessing greater awareness and intelligence than their seabound relatives, they have a natural instinct to follow larger members of their kind.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(26);
            DrawOffsetY = -4f;

            NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = Main.getGoodWorld ? 50 : 40;
            NPC.knockBackResist = 0.5f;
            NPC.value = Item.buyPrice(0, 0, 0, 5);
            NPC.npcSlots = 1f;
            NPC.chaseable = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.DeathSound = SoundID.DD2_LightningBugHurt;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;

            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;
            }

            if (NPC.frame.Y >= frameHeight * 4) NPC.frame.Y = 0;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public float AIState;
        public ref float AITimer => ref NPC.ai[1];
        public ref float AILifeTimer => ref NPC.ai[2];
        public ref float AIRandomizer => ref NPC.ai[3];


        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(AIState);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                AIState = reader.ReadUInt16();
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (!target.HasBuff(BuffType<SeersTag>()))
            {
                CombatText.NewText(target.getRect(), new Color(255, 186, 101), "Tagged!", true, false);
                SoundEngine.PlaySound(SoundID.Zombie105, target.Center);
            }

            target.AddBuff(BuffType<SeersTag>(), 600, true);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0) for (int i = 0; i < 10; i++) Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 33, Scale: 2f)].noGravity = true;
            else Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 33, Scale: 2f)].noGravity = true;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];
            if (!owner.active || owner.type != NPCType<Sightseer>())
            {
                NPC.life = 0;
            }

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            switch (AIState)
            {
                case 0:

                    if (visibility < 1) visibility = MathHelper.Lerp(visibility, 1, 0.025f);
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, 0.1f);
                    AITimer++;
                    NPC.dontTakeDamage = true;

                    if (AITimer >= 60)
                    {
                        AITimer = 0;
                        AIRandomizer = Main.rand.Next(60, 81);
                        NPC.dontTakeDamage = false;
                        NPC.damage = 75;
                        AIState = 1;
                        NPC.netUpdate = true;
                    }

                    break;

                case 1:

                    AITimer++;
                    AILifeTimer++;
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);

                    if (NPC.Distance(target.Center) > 10)
                    {
                        NPC.velocity += NPC.HasBuff(BuffID.Confused) ? NPC.DirectionFrom(target.Center) * 0.25f : NPC.DirectionTo(target.Center) * 0.4f;
                        NPC.velocity *= AITimer >= AIState && AITimer <= AIState + 20 ? 0.995f : 0.95f;
                    }

                    if (AITimer > AIRandomizer + 20)
                    {
                        AITimer = 0;
                        AIRandomizer = Main.rand.Next(60, 81);
                        NPC.netUpdate = true;
                    }

                    if (AILifeTimer > 300)
                    {
                        AITimer = 0;
                        AIState = 2;
                        AIRandomizer = Main.rand.Next(60, 81);
                        NPC.dontTakeDamage = true;
                        NPC.netUpdate = true;
                    }

                    break;

                case 2:

                    if (visibility > 0) visibility = MathHelper.Lerp(visibility, 0, 0.025f);
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
                    AITimer++;
                    AILifeTimer++;

                    NPC.velocity += NPC.DirectionFrom(target.Center) * 0.4f;
                    NPC.velocity *= AITimer >= AIState && AITimer <= AIState + 20 ? 0.995f : 0.95f;

                    if (AITimer > AIRandomizer + 20)
                    {
                        AITimer = 0;
                        AIRandomizer = Main.rand.Next(60, 81);
                        NPC.netUpdate = true;
                    }

                    if (AILifeTimer >= 400)
                    {
                        NPC.active = false;
                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
                        NPC.netUpdate = true;
                    }

                    break;
            }
        }

        float timer;
        float visibility;
        bool useSilhouette = false;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D silhouette = Request<Texture2D>(Texture + "Silhouette").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.Draw(useSilhouette ? silhouette : texture, drawPos, NPC.frame, useSilhouette ? new Color(0, 0, 0, 255) * visibility : drawColor * visibility, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(mask, drawPos, NPC.frame, new Color(255, 255, 255) * visibility, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
