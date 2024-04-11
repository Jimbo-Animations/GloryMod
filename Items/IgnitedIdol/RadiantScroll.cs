using GloryMod.Systems;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using static Terraria.ModLoader.ModContent;

namespace GloryMod.Items.IgnitedIdol
{
    class RadiantScroll : ModItem
    {
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.damage = 50;
            Item.knockBack = 10f;
            Item.mana = 50;

            Item.shoot = ProjectileType<RadiantFlame>();
            Item.shootSpeed = 1;

            Item.Size = new Vector2(34, 32);
            Item.scale = 1f;

            Item.useTime = Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
        }
    }


    class RadiantFlame : ModProjectile
    {
        Player player => Main.player[Projectile.owner];

        float timer;
        float acceleration;

        public override string Texture => "GloryMod/CoolEffects/Textures/InvisProj";

        public override bool ShouldUpdatePosition() => true;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(54);
            Projectile.scale = 1;
            Projectile.alpha = 0;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                if (player.channel)
                {
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.DirectionTo(Projectile.Center).ToRotation() - MathHelper.PiOver2);
                    player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.DirectionTo(Projectile.Center).ToRotation() - MathHelper.PiOver2);

                    player.direction = (Projectile.Center.X >= player.Center.X ? 1 : -1);
                    Lighting.AddLight(Projectile.Center, 1, 0, 0);

                    float maxDistance = 18f;
                    Vector2 vectorToCursor = Main.MouseWorld - Projectile.Center;
                    float distanceToCursor = vectorToCursor.Length();

                    if (distanceToCursor > maxDistance)
                    {
                        distanceToCursor = maxDistance / distanceToCursor;
                        vectorToCursor *= distanceToCursor;
                        acceleration = MathHelper.Lerp(acceleration, 3, 0.025f);
                    }

                    int velocityXBy1000 = (int)(vectorToCursor.X * 1000f);
                    int oldVelocityXBy1000 = (int)(Projectile.velocity.X * 1000f);
                    int velocityYBy1000 = (int)(vectorToCursor.Y * 1000f);
                    int oldVelocityYBy1000 = (int)(Projectile.velocity.Y * 1000f);

                    if (velocityXBy1000 != oldVelocityXBy1000 || velocityYBy1000 != oldVelocityYBy1000)
                    {
                        Projectile.netUpdate = true;
                    }

                    Projectile.velocity = vectorToCursor * acceleration;
                    acceleration = MathHelper.Lerp(acceleration, 0, 0.025f);
                    
                }
                else
                {
                    Projectile.Kill();
                }
            }

            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 150);

            Player player = Main.player[Projectile.owner];

            if (player.Distance(Projectile.Center) <= 100)
            {
                ScreenUtils.screenShaking = 2.5f;
                player.velocity = new Vector2(15, 0).RotatedBy(player.DirectionFrom(Projectile.Center).ToRotation());
            }
        }

        public override void Kill(int timeLeft)
        {
            int numDusts = 36;

            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(5, 10), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }


            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<RadiantBlast>(), Projectile.damage, 5, Projectile.owner);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 16;

            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player player = Main.player[Projectile.owner];

            if (player.Distance(Projectile.Center) <= 100)
            {
                ScreenUtils.screenShaking = 2.5f;
                player.velocity = new Vector2(15, 0).RotatedBy(player.DirectionFrom(Projectile.Center).ToRotation());
            }

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/Star").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);

            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            Color color = Projectile.GetAlpha(new Color(250, 72, 37, 0));

            Main.EntitySpriteDraw(texture, position, sourceRectangle, color, timer, origin, Projectile.scale + (float)Math.Sin(timer) * 0.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, position, sourceRectangle, color, -timer / 2, origin, (Projectile.scale * 0.6f) + (float)Math.Sin(timer) * 0.5f, SpriteEffects.None, 0);

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }
            
            texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;

            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);

            origin = sourceRectangle.Size() / 2f;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color, Projectile.oldRot[i] + MathHelper.PiOver2, origin, Projectile.scale - (i / (float)Projectile.oldPos.Length), SpriteEffects.None, 0);
            }

            return false;
        }
    }

    class RadiantBlast : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 24;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            int numDusts = 30;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 31, Scale: 1f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= 4)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 7;
                }
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 10 ? 0 : 1, 0.1f);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Rectangle result = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
            int num = (int)Terraria.Utils.Remap(Projectile.ai[0] * 2, 0, 200, 10, 40);
            result.Inflate(num, num);
            hitbox = result;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 200);
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255) * visibility, 0, drawOrigin, Projectile.scale * (Projectile.ai[0] * 0.03f + 1), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(250, 72, 37, 0) * visibility, 0, glow.Size() / 2, Projectile.scale * (Projectile.ai[0] * 0.07f + 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }
}
