using Terraria.Audio;
using GloryMod.Systems;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace GloryMod.Items.NeonBoss
{
    class ArgonBasher : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Melee;
            Item.Size = new Vector2(36);
            Item.useTime = 30; 
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 10;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.value = Item.sellPrice(gold: 2);
            Item.shoot = ProjectileType<ArgonBasherProj>();
            Item.shootSpeed = 0;
            Item.channel = true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool MeleePrefix()
        {
            return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)
        }
    }

    class ArgonBasherProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(32);
            Projectile.netImportant = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 3600;
            Projectile.alpha = 0;            
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        Vector2 pullBackTo;
        float pullProgress;
        float glowStrength;
        float superGlow;
        float timer;
        bool fullCharge = false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 mountedCenter2 = Main.player[Projectile.owner].MountedCenter;          
            player.heldProj = Projectile.whoAmI;
            int goalDirectionX = player.Center.X < Projectile.Center.X ? 1 : -1;
            Projectile.spriteDirection = goalDirectionX;

            player.direction = goalDirectionX;
            player.heldProj = Projectile.whoAmI;
            Projectile.scale = 1f * player.GetAdjustedItemScale(player.HeldItem);

            // Extend use animation until projectile is killed
            player.itemAnimation = 2;
            player.itemTime = 2;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.DirectionTo(Projectile.Center).ToRotation() - MathHelper.PiOver2);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.DirectionTo(Projectile.Center).ToRotation() - MathHelper.PiOver2);

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            Projectile.ai[1]++;

            switch (Projectile.ai[0])
            {
                case 0:
                    //Charging

                    pullBackTo = mountedCenter2 + new Vector2(150 * player.GetTotalAttackSpeed(Projectile.DamageType) * pullProgress, 0).RotatedBy(player.DirectionFrom(Main.MouseWorld).ToRotation());
                    pullProgress = 1 - (float)Math.Exp(-Projectile.ai[1] * player.GetTotalAttackSpeed(Projectile.DamageType) / 12);
                   
                    if (pullProgress >= 0.99f) fullCharge = true;

                    if (player.channel) Projectile.position = pullBackTo - Projectile.Size / 2;
                    else
                    {
                        Projectile.ai[0] = 3;
                        Projectile.ai[1] = 0;
                    }

                    if (fullCharge)
                    {
                        Projectile.ai[0] = 1;
                        Projectile.ai[1] = 0;
                    }

                    break;

                case 1:
                    //Releasing

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * (20 * player.GetTotalAttackSpeed(Projectile.DamageType));
                    Projectile.ai[0] = 2;

                    SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, Projectile.Center);

                    break;


                case 2:
                    //Flying

                    Projectile.friendly = true;
                    if (Projectile.Distance(mountedCenter2) >= 300 * player.GetTotalAttackSpeed(Projectile.DamageType)) Projectile.ai[0] = 3;

                    break;

                case 3:
                    //Returning

                    pullProgress = 0;
                    fullCharge = false;

                    if (Projectile.ai[2] <= 0)
                    {
                        Projectile.velocity += Projectile.DirectionTo(mountedCenter2) * player.GetTotalAttackSpeed(Projectile.DamageType) * 2f;
                    }

                    Projectile.ai[2]--;
                    Projectile.velocity *= 0.9f;
                    if (Projectile.Distance(mountedCenter2) <= 16) Projectile.active = false;

                    break;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0] = 3;
            Projectile.friendly = false;
            Projectile.ai[2] = 10;
            glowStrength = 0;

            int numDusts = 25;

            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 176, Scale: 2f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(1, Projectile.ai[0] == 1 ? 8 : 5), 0).RotatedBy(i * MathHelper.TwoPi / numDusts) + (Projectile.velocity * 0.1f);
            }

            ScreenUtils.screenShaking = 2.5f;
            Projectile.velocity = -Projectile.velocity * 0.2f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<ArgonBasherShockwave>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 1);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D ChainTexture = Request<Texture2D>("GloryMod/Items/NeonBoss/ArgonBasherChain").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            SpriteEffects effects = new SpriteEffects();
            if (Projectile.spriteDirection == 1)
            {
                effects = SpriteEffects.FlipVertically;
            }

            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation();
            float distance = distToProj.Length();
            while (distance > 14f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 14f;                      //speed = 24
                center += distToProj;                   //update draw position
                distToProj = playerCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = Projectile.GetAlpha(new Color(255, 255, 255));

                //Draw chain
                Main.spriteBatch.Draw(ChainTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 14, 10), drawColor, projRotation,
                    new Vector2(14 * 0.5f, 10 * 0.5f), 1f, effects, 0f);
            }


            if (pullProgress >= 0.8f) glowStrength = MathHelper.Lerp(glowStrength, 1, 0.025f);
            else glowStrength = MathHelper.Lerp(glowStrength, 0, 0.025f);

            if (fullCharge)
            {
                superGlow = MathHelper.Lerp(superGlow, 1, 0.05f);
            }
            else superGlow = MathHelper.Lerp(superGlow, 0, 0.05f);

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(100 / i, 150 / i, 250 / i, 0));
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(Projectile.GetAlpha(color * ((1 - i / (float)Projectile.oldPos.Length) * 0.95f))) * glowStrength, Projectile.rotation, drawOrigin, Projectile.scale * (1.25f - i / (float)Projectile.oldPos.Length) * 0.95f, SpriteEffects.None, 0);
            }

            for (int j = 0; j < 4; j++)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center + new Vector2(6 * superGlow, 0).RotatedBy(timer + j * MathHelper.TwoPi / 4) - Main.screenPosition, frame, new Color(100, 150, 250, 0) * superGlow, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(new Color(255, 255, 255)), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    class ArgonBasherShockwave : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/InvisProj";

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.netImportant = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40;
            Projectile.alpha = 0;
            Projectile.scale = 1f;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/GroundSlam"), Projectile.Center);
            SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 1.25f }, Projectile.Center);
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.Distance(target.Center) <= 75 && !target.friendly;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
