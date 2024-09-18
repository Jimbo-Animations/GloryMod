using System.Collections.Generic;
using System.IO;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;

namespace GloryMod.NPCs.Sightseer.Minions
{
    internal class SightseerClone : ModNPC
    {
        public override string Texture => "GloryMod/NPCs/Sightseer/Sightseer";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.ProjectileNPC[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            NPCID.Sets.NPCBestiaryDrawOffset[NPC.type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(100);
            DrawOffsetY = -4f;

            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.knockBackResist = 0;
            NPC.npcSlots = 1f;
            NPC.chaseable = false;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public ref float IsReal => ref NPC.ai[1];
        public ref float AITimer => ref NPC.ai[2];
        public ref float AIState => ref NPC.ai[3];

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (!target.HasBuff(BuffType<SeersTag>()))
            {
                CombatText.NewText(target.getRect(), new Color(255, 186, 101), "Tagged!", true, false);
                SoundEngine.PlaySound(SoundID.Zombie105, target.Center);
            }

            target.AddBuff(BuffType<SeersTag>(), 600, true);
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return target.Distance(NPC.Center) <= NPC.width / 2;
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        private int animState = 0;
        private int animSpeed = 6;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 3;
            NPC.frameCounter += 1;

            if (NPC.frameCounter > animSpeed)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0f;
            }

            switch (animState)
            {
                case 0:

                    if (NPC.frame.Y >= frameHeight * 4) NPC.frame.Y = 0;
                    NPC.frame.X = 0;

                    break;

                case 1:

                    if (NPC.frame.Y >= frameHeight * 3)
                    {
                        NPC.frame.Y = 0;
                        animState = 2;
                    }

                    NPC.frame.X = NPC.frame.Width;

                    break;

                case 2:

                    if (NPC.frame.Y >= frameHeight * 4) NPC.frame.Y = 0;
                    NPC.frame.X = NPC.frame.Width * 2;

                    break;

                case 3:

                    if (NPC.frame.Y >= frameHeight * 8 || NPC.frame.Y < frameHeight * 4) NPC.frame.Y = frameHeight * 4;
                    NPC.frame.X = 0;

                    break;

                case 4:

                    if (NPC.frame.Y >= frameHeight * 7 || NPC.frame.Y < frameHeight * 4)
                    {
                        if (NPC.frame.Y >= frameHeight * 7) animState = 5;
                        NPC.frame.Y = frameHeight * 4;
                    }

                    NPC.frame.X = NPC.frame.Width;

                    break;

                case 5:

                    if (NPC.frame.Y >= frameHeight * 8 || NPC.frame.Y < frameHeight * 4) NPC.frame.Y = frameHeight * 4;
                    NPC.frame.X = NPC.frame.Width * 2;

                    break;
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(AIState);
                writer.Write(IsReal);
                writer.Write(AITimer);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                AIState = reader.ReadByte();
                IsReal = reader.ReadByte();
                AITimer = reader.ReadUInt16();              
            }
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

                    if (AITimer == 0)
                    {
                        NPC.rotation = NPC.DirectionTo(target.Center).ToRotation();                     

                        if (IsReal == 0) 
                        {
                            SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt with { Pitch = -0.35f, Volume = 1.25f }, NPC.Center);
                            flash.Add(new Tuple<Vector2, float, float>(NPC.Center, 0, 1));
                        }

                        animState = 4;
                        animSpeed = 7;
                    }

                    visibility = MathHelper.Lerp(visibility, 1, 0.05f);
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2, 0.15f);
                    AITimer++;

                    if (AITimer >= 20)
                    {
                        AITimer = 0;
                        AIState = 1;
                        NPC.damage = IsReal == 0 ? 75 : 0;
                        NPC.netUpdate = true;
                    }

                    break;

                case 1:

                    if (AITimer == 0)
                    {
                        NPC.velocity = NPC.DirectionTo(target.Center + target.velocity * (NPC.Distance(target.Center) / 10)) * 25;
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

                        SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, NPC.Center);
                      
                        NPC.netUpdate = true;
                    }

                    NPC.velocity += NPC.DirectionTo(target.Center) * 0.25f;
                    NPC.velocity *= 0.99f;

                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.15f);
                    AITimer++;

                    if (AITimer >= 20)
                    {
                        visibility = MathHelper.Lerp(visibility, 0, 0.05f);

                        if (AITimer > 40)
                        {
                            NPC.active = false;
                            NPC.netUpdate = true;
                        }
                    }

                    break;
            }
        }

        float timer;
        float spriteWidth;
        float visibility;
        List<Tuple<Vector2, float, float>> flash = new List<Tuple<Vector2, float, float>>();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D mask2 = Request<Texture2D>(Texture + "Mask2").Value;
            Texture2D silhouette = Request<Texture2D>(Texture + "Silhouette").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Color mirageColor = Systems.Utils.ColorLerpCycle(Main.GlobalTimeWrappedHourly, 3, new Color[] { new Color(200, 200, 100), new Color(100, 200, 200) });

            spriteWidth = MathHelper.SmoothStep(spriteWidth, 1, 0.2f);

            spriteBatch.Draw(silhouette, drawPos, NPC.frame, Color.Black * visibility, NPC.rotation, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0f);
            spriteBatch.Draw(mask, drawPos, NPC.frame, (IsReal == 0 ? Color.White : mirageColor) * visibility, NPC.rotation, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0f);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(mask2, drawPos + new Vector2(8 - (4 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(255, 255, 255) * visibility * 0.5f,
                NPC.rotation, drawOrigin, new Vector2(spriteWidth, 1), SpriteEffects.None, 0);
            }

            for (int i = 0; i < flash.Count; i++)
            {
                if (i >= flash.Count)
                {
                    break;
                }

                Texture2D star = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;
                Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
                flash[i] = new Tuple<Vector2, float, float>(flash[i].Item1, flash[i].Item2 + flash[i].Item3, flash[i].Item3);

                Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White * (1 - visibility), Main.GameUpdateCount * 0.033f + NPC.rotation, star.Size() / 2, new Vector2(0.1f, 1) * (1 + visibility), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White, Main.GameUpdateCount * -0.033f + NPC.rotation + MathHelper.PiOver2, star.Size() / 2, new Vector2(0.1f, 1) * (1 + visibility), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White, Main.GameUpdateCount * 0.025f + NPC.rotation, star.Size() / 2, new Vector2(0.2f, 2) * (1 + visibility * 2), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(star, flash[i].Item1 - Main.screenPosition, null, Color.White, Main.GameUpdateCount * 0.025f + NPC.rotation + MathHelper.PiOver2, star.Size() / 2, new Vector2(0.2f, 2) * (1 + visibility * 2), SpriteEffects.None, 0);

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

            return false;
        }
    }
}
