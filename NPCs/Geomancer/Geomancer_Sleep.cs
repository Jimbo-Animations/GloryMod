using Terraria.Audio;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using ReLogic.Utilities;

namespace GloryMod.NPCs.Geomancer
{
    internal class Geomancer_Sleep : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 15;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            NPCID.Sets.ImmuneToAllBuffs[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 58;
            NPC.height = 90;

            NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 50f;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.friendly = false;
            NPC.chaseable = false;
            NPC.HitSound = SoundID.NPCHit1;
        }

        SlotId napTime = SlotId.Invalid;

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[NPC.target];
            napTime = SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/Nap_Time") with { IsLooped = true }, NPC.Center);

            if (NPC.target < 0 || NPC.target == 255 || player.dead || !player.active)
                NPC.TargetClosest();

            NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
        }

        private void UpdateSound()
        {
            if (SoundEngine.TryGetActiveSound(napTime, out ActiveSound sound) && sound is not null && sound.IsPlaying)
            {
                sound.Position = NPC.Center;

                if (NPC.ai[0] != 1 && NPC.active)
                    sound.Volume = MathHelper.Lerp(sound.Volume, 0.75f, 0.1f);
                else
                    sound.Volume = MathHelper.Lerp(sound.Volume, 0f, 0.1f);

                if (NPC.ai[1] >= 7200) sound.Stop();
            }
        }

        public override bool CheckActive()
        {
            return NPC.ai[1] >= 7200;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            NPC.ai[1]++;
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            //Snoozing

            if (NPC.ai[0] != 1)
            {
                if (NPC.ai[1] % 20 == 1)
                {
                    Dust.NewDustPerfect(NPC.Center + new Vector2(10 * NPC.spriteDirection, -20), DustType<SleepyZ>(), new Vector2(Main.rand.NextFloat(1, 2) * NPC.spriteDirection, -2), 0, default, 1f);
                }
            }

            if (player.Distance(NPC.Center) <= 100) NPC.ai[2]++;
            else if (NPC.ai[2] > 0) NPC.ai[2]--;

            if (NPC.ai[2] > 300)
            {
                NPC.ai[0] = 1;
                NPC.ai[1] = 0;
                NPC.ai[2] = 0;
                NPC.dontTakeDamage = true;
                SoundEngine.PlaySound(SoundID.DD2_GoblinScream with { Pitch = -1f }, NPC.Center);
                NPC.netUpdate = true;
            }

            UpdateSound();
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                NPC.life = 1;
                NPC.ai[0] = 1;
                NPC.ai[1] = 0;

                NPC.dontTakeDamage = true;
                SoundEngine.PlaySound(SoundID.DD2_GoblinScream with { Pitch = -1f }, NPC.Center);
                NPC.netUpdate = true;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            switch (NPC.ai[0])
            {
                case 0:

                    if (NPC.frameCounter > 6)
                    {
                        NPC.frame.Y += frameHeight;
                        NPC.frameCounter = 0f;
                    }

                    if (NPC.frame.Y >= frameHeight * 7) NPC.frame.Y = 0;

                    break;

                case 1:

                    if (NPC.frame.Y < frameHeight * 7) NPC.frame.Y = frameHeight * 7;

                    if (NPC.frameCounter > 6)
                    {
                        if (NPC.frame.Y >= frameHeight * 14)
                        {                                                                                              
                            if (NPC.ai[3] == 1)
                            {
                                NPC.Transform(NPCType<Geomancer>());                                
                                CombatText.NewText(NPC.getRect(), new Color(100, 255, 100), "!", true, false);
                                NPC.netUpdate = true;
                            }

                            NPC.frameCounter = 0f;
                            NPC.ai[3] = 1;
                        }
                        else
                        {
                            NPC.frame.Y += frameHeight;
                            NPC.frameCounter = 0f;
                        }
                    }

                    break;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.PlayerSafe) return 0f;
            if (spawnInfo.Player.ZoneRockLayerHeight) return 0.01f;
            if (NPC.AnyNPCs(NPCType<Geomancer>()) || NPC.AnyNPCs(NPCType<Geomancer_Sleep>())) return 0f;
            else return 0f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos - new Vector2(0, 4);

            spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(mask, drawPos, NPC.frame, NPC.GetAlpha(new Color(255, 255, 255)), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
