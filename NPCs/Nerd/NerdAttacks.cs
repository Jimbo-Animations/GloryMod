using Terraria.Audio;

namespace GloryMod.NPCs.Nerd
{
    partial class Nerd : ModNPC
    {
        void ResetValues()
        {
            AITimer = 0;
            RepeatAttack = 0;
            NPC.damage = 0;
            NPC.noTileCollide = true;

            if (Main.getGoodWorld)
            {
                for (int i = 0; i < 1 + AIAggression; i++) NPC.NewNPC(NPC.InheritSource(NPC), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.ServantofCthulhu);
            }
            NPC.netUpdate = true;
        }

        void NextBotChase()
        {
            NPC.velocity += NPC.DirectionTo(target.Center) * (0.25f + (AIAggression * 0.25f));
            NPC.velocity *= 0.98f;

            if (AITimer == 0) target.AddBuff(BuffID.Obstructed, 60);

            NPC.damage = 100;
            AITimer++;

            if (AITimer >= 400)
            {
                ResetValues();
                NPC.ai[0] = 1;
                NPC.netUpdate = true;
            }
        }

        void SpazMyTism()
        {
            NPC.velocity += NPC.DirectionTo(target.Center + new Vector2(300 * NPC.spriteDirection, 0)) * 0.5f;
            NPC.velocity *= 0.95f;
            AITimer++;

            if (AITimer >= 60 && AITimer % (30 - (AIAggression * 5)) == 1)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1, 0).RotatedBy(NPC.rotation), ProjectileID.DemonSickle, 100 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 0, target.whoAmI);
                SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                NPC.netUpdate = true;
            }

            if (AITimer >= 300)
            {
                ResetValues();
                NPC.ai[0] = 2;
                NPC.netUpdate = true;
            }
        }

        void SpookyDashes()
        {
            NPC.velocity *= 0.96f;
            AITimer++;

            if (AITimer > 1 && AITimer % (90 - (AIAggression * 15)) == 1)
            {
                NPC.velocity = new Vector2(30, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()).RotatedByRandom(MathHelper.PiOver4);
                NPC.damage = 100;
                SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
            }

            if (AITimer >= 300)
            {
                ResetValues();
                NPC.ai[0] = 3;
                NPC.netUpdate = true;
            }
        }

        void NextBotChase2()
        {
            NPC.velocity += NPC.DirectionTo(target.Center) * (0.5f + (AIAggression * 0.15f));
            NPC.velocity *= 0.96f;

            NPC.damage = 100;
            AITimer++;

            if (AITimer >= 300)
            {
                ResetValues();
                NPC.ai[0] = 4;
                NPC.netUpdate = true;
            }
        }

        void LaserBarrage()
        {
            NPC.velocity *= 0.95f;
            AITimer++;

            if (AITimer < (150 - (AIAggression * 15)) && AITimer % (45 - (AIAggression * 5)) == 1)
            {
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                int numDusts = 12;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.Center, 0, 0, 114, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }

            if (AITimer >= (180 - (AIAggression * 15)) && AITimer % 5 == 1)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(15, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()).RotatedByRandom(MathHelper.PiOver4), ProjectileID.DeathLaser, 100 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 0, target.whoAmI);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(20, 0).RotatedBy(NPC.DirectionTo(target.Center).ToRotation()).RotatedByRandom(MathHelper.PiOver4), ProjectileID.DeathLaser, 100 / (Main.expertMode ? Main.masterMode ? 6 : 4 : 2), 0, target.whoAmI);
                SoundEngine.PlaySound(SoundID.Item75, NPC.Center);
                NPC.netUpdate = true;
            }

            if (AITimer >= 300)
            {
                ResetValues();
                NPC.ai[0] = 1;
                NPC.netUpdate = true;
            }
        }
    }
}
