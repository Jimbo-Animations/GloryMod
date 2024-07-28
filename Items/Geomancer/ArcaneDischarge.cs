using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.Audio;

namespace GloryMod.Items.Geomancer
{
    internal class ArcaneDischarge : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;

            Item.noMelee = true;
            Item.damage = 80;
            Item.knockBack = 5;
            Item.mana = 40;

            Item.Size = new Vector2(28);
            Item.useTime = Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ProjectileType<DischargeBook>();
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Lime;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }
    }

    class DischargeBook : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        float timer;
        float starOpacity;
        public override string Texture => "GloryMod/Items/Geomancer/ArcaneDischarge";

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(20);
            Projectile.scale = 1;
            Projectile.alpha = 0;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            if (Main.myPlayer == Projectile.owner && player.statMana > 0)
            {
                if (player.channel)
                {
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.DirectionTo(Projectile.Center).ToRotation() - MathHelper.PiOver2);

                    player.direction = (Projectile.Center.X >= player.Center.X ? 1 : -1);
                    Projectile.spriteDirection = Projectile.direction = player.direction;
                    Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();
                    Projectile.Center = player.MountedCenter + new Vector2(16, 0).RotatedBy(Projectile.rotation);

                    Projectile.localAI[0]++;
                    Projectile.timeLeft = 2;
                    player.statMana--;
                    player.manaRegenDelay = 10;
                }

                switch (Projectile.ai[0])
                {
                    case 0:
                        //Charge up to shoot.

                        if (Projectile.localAI[0] == 1) SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, Projectile.Center);

                        if (Projectile.localAI[0] >= 50)
                        {
                            starOpacity = MathHelper.Lerp(starOpacity, 1, 0.1f);
                        }

                        if (Projectile.localAI[0] >= 60)
                        {
                            Projectile.ai[0]++;
                        }                              

                        break; 
                    
                    case 1:
                        //Fire volley.

                        if (Projectile.localAI[0] % 8 == 1)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot, Projectile.Center);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(12, 17), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.ToRadians(10)), ProjectileType<DischargeBolt>(), Projectile.damage, Projectile.knockBack);
                        }

                        if (Projectile.localAI[0] >= 180)
                        {
                            Projectile.ai[0]++;
                        }

                        break;

                    case 2:
                        //Shield dash.

                        if (Projectile.localAI[0] == 181)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<DischargeShield>(), (int)(Projectile.damage * 2.5f), Projectile.knockBack * 2);
                        }

                        if (Projectile.localAI[0] == 200)
                        {
                            int numDusts = 16;
                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(Projectile.Center, 0, 0, 110, Scale: 2f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Vector2 trueVelocity = new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                                trueVelocity.X *= 0.5f;
                                trueVelocity = trueVelocity.RotatedBy(Projectile.velocity.ToRotation());
                                Main.dust[dust].velocity = trueVelocity;
                            }

                            SoundEngine.PlaySound(SoundID.Item131, Projectile.Center);
                            player.velocity = new Vector2(20, 0).RotatedBy(Projectile.rotation);
                            Projectile.Kill();
                        }

                        starOpacity = MathHelper.Lerp(starOpacity, 0, 0.1f);

                        break;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D star = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;
            float rotationOffset;
            SpriteEffects effects;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            int frameHeight = star.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, star.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            if (Projectile.spriteDirection > 0)
            {
                rotationOffset = MathHelper.ToRadians(45f);
                effects = SpriteEffects.None;
            }
            else
            {
                rotationOffset = MathHelper.ToRadians(135f);
                effects = SpriteEffects.FlipHorizontally;
            }

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, drawOrigin, Projectile.scale, effects, 0);

            Vector2 starSize = new Vector2(0.1f + (float)Math.Sin(timer) * 0.5f, 0.4f + (float)Math.Sin(timer) * 0.5f);
            Main.spriteBatch.Draw(star, position + new Vector2(5, 0).RotatedBy(Projectile.rotation), sourceRectangle, new Color (100, 255, 100) * starOpacity, Projectile.rotation + rotationOffset + timer, origin, starSize, effects, 0);
            Main.spriteBatch.Draw(star, position + new Vector2(5, 0).RotatedBy(Projectile.rotation), sourceRectangle, new Color(100, 255, 100) * starOpacity, Projectile.rotation + rotationOffset + (timer + MathHelper.PiOver2), origin, starSize, effects, 0);

            return false;
        }
    }

    class DischargeBolt : ModProjectile
    {
        float timer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(30);
            Projectile.scale = 1;
            Projectile.alpha = 0;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;

            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            // Trying to find NPC closest to the projectile
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Vector2 moveTo = targetCenter;

            if (foundTarget)
            {
                Projectile.velocity += Projectile.DirectionTo(moveTo) * 1.33f;
                Projectile.velocity *= 0.95f;
            }         

            if (Main.rand.NextBool(20)) Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 110, Scale: 1f)].noGravity = true;
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }

        //Actual targeting code.
        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            // Starting search distance
            distanceFromTarget = 300f;
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

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, Projectile.Center);
            int numDusts = 16;

            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 110, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(1, 5), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);       

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(40 / i, 200 / i, 60 / i, 0));
                Vector2 trailSize = new Vector2(0.5f - (i / (float)Projectile.oldPos.Length) + ((float)Math.Sin(timer) * 0.5f), 1f - (i / (float)Projectile.oldPos.Length));
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color, Projectile.oldRot[i] + MathHelper.PiOver2, origin, trailSize, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>(Texture).Value;

            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White, Projectile.rotation, origin, Projectile.scale + (float)Math.Sin(timer) * 0.5f, SpriteEffects.None, 0);

            return false;
        }
    }

    class DischargeShield : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        float visibility = 0;
        float timer;
        bool hasStruckEnemy = false;

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(8);
            Projectile.scale = 1;
            Projectile.alpha = 0;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 90;
        }

        public override void AI()
        {          
            if (Projectile.timeLeft > 30)
            {
                Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();
                Projectile.Center = player.MountedCenter + new Vector2(32, 0).RotatedBy(Projectile.rotation);

                visibility = MathHelper.Lerp(visibility, 1, 0.05f);
            }
            else
            {
                Projectile.velocity *= 0.98f;
                if (Projectile.timeLeft == 30)
                {
                    SoundEngine.PlaySound(SoundID.Item131, Projectile.Center);
                    Projectile.velocity = new Vector2(15, 0).RotatedBy(Projectile.rotation);
                }

                visibility = MathHelper.Lerp(visibility, 0, 0.05f);
                if (Projectile.timeLeft <= 10) Projectile.friendly = false;
            }

            if (Main.rand.NextBool(20)) Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 110, Scale: 1f)].noGravity = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + new Vector2(0, 21).RotatedBy(Projectile.rotation), Projectile.Center - new Vector2(0, 21).RotatedBy(Projectile.rotation));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft >= 30 && hasStruckEnemy == false)
            {
                hasStruckEnemy = true;
                player.immune = true;
                player.immuneTime = 30;
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost, Projectile.Center);
            int numDusts = 20;

            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 110, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(1, 9), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, position + new Vector2(6 - (4 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), sourceRectangle, new Color (0, 255, 0, 100) * visibility, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(texture, position, sourceRectangle, Color.White * visibility, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
