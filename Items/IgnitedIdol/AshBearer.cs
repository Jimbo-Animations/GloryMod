using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace GloryMod.Items.IgnitedIdol
{
    internal class AshBearer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;
            Item.damage = 10;
            Item.knockBack = 5f;
            Item.ArmorPenetration = 10;

            Item.mana = 10;
            Item.shoot = ProjectileType<MiniLamp>();
            Item.shootSpeed = 0f;

            Item.width = 34;
            Item.height = 48;
            Item.scale = 1f;

            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.DD2_BetsySummon;
            Item.autoReuse = true;
            Item.useTurn = false;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
                if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
                player.itemAnimation = player.itemAnimationMax;

                if (Main.rand.NextBool(player.itemAnimation > 0 ? 40 : 80))
                {
                    Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, 6);
                }

                Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
                Lighting.AddLight(position, 1f, 0.5f, 0.5f);
            }
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation += new Vector2(0, 8);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < player.maxMinions + 1; i++)
            {
                var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
                projectile.originalDamage = Item.damage;
                projectile.ai[0] = i;
            }

            return false;
        }
    }

    public class MiniLamp : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[Projectile.type] = 12;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.tileCollide = false;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
        }

        // Here you can decide if your minion breaks things like grass or pots
        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        private float currentNumber = 1;
        private float currentDistance = 0;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.localAI[0]++;
            Projectile.timeLeft = 2;

            if (owner.dead || !owner.active || !owner.channel) Projectile.Kill();

            //Animation.
            if (Projectile.localAI[0] % 5 == 1 && Projectile.localAI[0] >= 60)
            {
                Projectile.frame++;

                if (Projectile.frame >= 8)
                {
                    Projectile.frame = 5;
                }
            }

            //Orbiting
            float SummonTimeMult = 50f;
            float orbitTimeMult = 10f;
            float distanceMult = 1 - (float)Math.Exp(-Projectile.ai[1] / SummonTimeMult);
            currentNumber = MathHelper.Lerp(currentNumber, owner.maxMinions + 1, 0.1f);
            currentDistance = (200 + (40 * currentNumber)) * distanceMult;

            Projectile.velocity = -Projectile.Center + owner.Center + new Vector2(currentDistance, 0).RotatedBy(0.15f * (Main.GameUpdateCount / orbitTimeMult - distanceMult) + Projectile.ai[0] * MathHelper.TwoPi / (owner.maxMinions + 1));
            Projectile.ai[1]++;

            //Look for targets.
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

            //Attack targetted enemies.
            if (foundTarget && Projectile.localAI[0] > 60)
            {
                Projectile.localAI[1]++;

                if (Projectile.localAI[1] >= 45)
                {
                    Projectile.localAI[1] = 0;
                    SoundEngine.PlaySound(SoundID.DD2_BetsySummon, Projectile.Center);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(1, 0).RotatedBy(Projectile.DirectionTo(targetCenter).ToRotation()), ProjectileType<LanternFireball>(), Projectile.damage, 5);
                }
            }
            else
            {
                Projectile.localAI[1] = 0;
            }

            //Extras.
            if (Projectile.localAI[0] > 45)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Main.rand.NextBool(30 + (i * 15)))
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.oldPos[i] - Projectile.position + Projectile.Center, 6, new Vector2(0, Main.rand.NextFloat(1, 5)), 0, default, 1.5f - (i * 0.015f));
                        dust.noGravity = true;
                    }
                }
            }          

            Lighting.AddLight(Projectile.Center, 1 * bloomAlpha, 0.5f * bloomAlpha, 0.5f * bloomAlpha);
        }

        //Actual targeting code.
        public void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = currentDistance;
            targetCenter = Projectile.position;
            foundTarget = false;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, owner.Center);

                // Checks to see if target is in range
                if (between < currentDistance)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, owner.Center);
                        bool inRange = between < distanceFromTarget;

                        if (inRange)
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            int numDusts = 20;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(4, 8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
        }

        //Visuals and whatnot.
        private float bloomAlpha;
        private float telegraphAlpha;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Texture2D glow1 = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Texture2D glow2 = Request<Texture2D>("Terraria/Images/Projectile_644").Value;
            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);           
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            float scale = (Projectile.scale * mult) * bloomAlpha;

            if (Projectile.localAI[0] >= 60)
            {
                bloomAlpha = MathHelper.Lerp(bloomAlpha, 1, 0.1f);
            }
            else
            {
                bloomAlpha = MathHelper.Lerp(bloomAlpha, 0, 0.1f);
            }

            Player owner = Main.player[Projectile.owner];
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(glow1, Projectile.Center - Main.screenPosition, null, new Color(250, 200, 100) * scale, Projectile.rotation, glow1.Size() / 2, scale * 0.3f, SpriteEffects.None, 0f);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(glow2, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255, 250 - (i * 10), 200 - (i * 12)) * (1f - i / (float)Projectile.oldPos.Length) * scale, Projectile.velocity.ToRotation() + MathHelper.PiOver2, glow2.Size() / 2, Projectile.scale * (0.75f - i / (float)Projectile.oldPos.Length) * scale, SpriteEffects.None, 0);
            }

            if (foundTarget && Projectile.localAI[0] > 60)
            {
                telegraphAlpha = MathHelper.Lerp(telegraphAlpha, 1, 0.1f);
            }
            else
            {
                telegraphAlpha = MathHelper.Lerp(telegraphAlpha, 0, 0.1f);
            }

            Utils.DrawLine(Main.spriteBatch, Projectile.Center + new Vector2(Projectile.Distance(targetCenter), 0).RotatedBy(Projectile.DirectionTo(targetCenter).ToRotation()), Projectile.Center, Color.Black, new Color(250, 200, 100) * telegraphAlpha, 1);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(mask, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(glow1, Projectile.Center - Main.screenPosition, null, new Color(250, 200, 100) * scale * 0.8f, Projectile.rotation, glow1.Size() / 2, scale * 0.15f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    class LanternFireball : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/InvisProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(12);
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.localAI[0]++; //Controls dust spawn.

            if (Projectile.localAI[0] >= 10)
            {
                Projectile.localAI[0] = 0;

                int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 2f);
                Main.dust[dust].noGravity = true;
            }
            if (Projectile.timeLeft <= 10)
            {
                expand = MathHelper.Lerp(expand, 0, 0.1f);
            }

            // Trying to find NPC closest to the projectile
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Vector2 moveTo = targetCenter;
            if (foundTarget)
            {
                Projectile.velocity += Projectile.DirectionTo(moveTo) * 1.33f;
                Projectile.velocity *= 0.95f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300, false);
        }

        public override void Kill(int timeLeft)
        {
            int numDusts = 20;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(4, 8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
        }

        //Actual targeting code.
        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            // Starting search distance
            distanceFromTarget = 1000f;
            targetCenter = Projectile.position;
            foundTarget = false;

            if (!foundTarget)
            {
                // This code is required either way, used for finding a target
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.CanBeChasedBy() && npc.Distance(Projectile.Center) <= distanceFromTarget)
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;

                        if ((closest && inRange) || !foundTarget)
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
        }

        private float expand = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = Request<Texture2D>("Terraria/Images/Projectile_644").Value;
            float mult = (0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f);
            expand = MathHelper.Lerp(expand, 1, 0.1f);
            float scale = (Projectile.scale * mult) * expand;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(glow, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, new Color(255, 250 - (i * 10), 200 - (i * 15)) * ((1.2f - i / (float)Projectile.oldPos.Length) * 0.99f) * scale, Projectile.velocity.ToRotation() + MathHelper.PiOver2, glow.Size() / 2 + new Vector2(0, 2), Projectile.scale * (1 - i / (float)Projectile.oldPos.Length) * scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
