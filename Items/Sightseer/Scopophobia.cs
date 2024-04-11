using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace GloryMod.Items.Sightseer
{
    internal class Scopophobia : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.DefaultToMagicWeapon(ProjectileType<ScopophobiaProj>(), 15, 10, true);
            Item.SetWeaponValues(20, 3f);
            Item.mana = 80;

            Item.width = 28;
            Item.height = 46;
            Item.scale = 1f;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item165;
            Item.useTurn = false;

            Item.value = Item.sellPrice(gold: 2, silver: 50);
            Item.rare = ItemRarityID.Blue;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity += player.velocity;
        }

        public int reUseTimer = 120;
        public override void UpdateInventory(Player player)
        {
            reUseTimer++;

            if (reUseTimer == 120) SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, player.Center);
        }

        public override bool CanUseItem(Player player)
        {
            return reUseTimer >= 120;
        }

        public override bool? UseItem(Player player)
        {
            reUseTimer = 0;
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DemoniteBar, 5);
            recipe.AddIngredient(ItemType<OtherworldlyFlesh>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrimtaneBar, 5);
            recipe.AddIngredient(ItemType<OtherworldlyFlesh>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    class ScopophobiaProj : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/Sightseer/Projectiles/SightseerAnomaly";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(70);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 400;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
            Projectile.scale = 0.5f;
        }

        float opacity;
        float eyeOpacity;
        float timer;
        Vector2 moveTo;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[1]++;
            for (int i = 0; i < 5; i++) Main.dust[Dust.NewDust(target.position, target.width, target.height, 111, Scale: 2f)].noGravity = true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.ai[1]++;
            for (int i = 0; i < 5; i++) Main.dust[Dust.NewDust(target.position, target.width, target.height, 111, Scale: 2f)].noGravity = true;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            Player owner = Main.player[Projectile.owner];
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Projectile.velocity *= 0.95f;

            if (Projectile.ai[1] >= 12 && Projectile.timeLeft > 60) Projectile.timeLeft = 60;
            Projectile.friendly = Projectile.timeLeft > 60 && Projectile.ai[0] > 30 ? true : false;

            if (foundTarget && Projectile.timeLeft > 60 && Projectile.ai[0] > 30)
            {
                Projectile.velocity += Projectile.DirectionTo(moveTo) * 0.75f;
            }

            if (Projectile.ai[0] % 5 == 1)
            {
                Projectile.frame++;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 0;
                }
            }

            Vector2 velocity = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
            int dust = Dust.NewDust(Projectile.Center + (velocity * Main.rand.NextFloat(100, 111) * opacity), 0, 0, 33, Scale: 2f * opacity);
            Main.dust[dust].noGravity = false;
            Main.dust[dust].noLight = true;
            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-3, -5) * opacity, 0).RotatedBy(velocity.ToRotation());

            if (Projectile.ai[0] == 30) SoundEngine.PlaySound(new SoundStyle("GloryMod/Music/SightseerAttack"), Projectile.Center);

            opacity = Projectile.timeLeft > 60 ? MathHelper.SmoothStep(opacity, 1, 0.15f) : MathHelper.SmoothStep(opacity, 0, 0.15f);
            eyeOpacity = Projectile.timeLeft > 60 && Projectile.ai[0] > 30 ? MathHelper.SmoothStep(eyeOpacity, 1, 0.25f) : MathHelper.SmoothStep(eyeOpacity, 0, 0.25f); 
        }

        //Actual targeting code.
        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            // Starting search distance
            distanceFromTarget = 500f;
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

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/Vortex").Value;
            int frameHeight;
            int frameY;
            Rectangle sourceRectangle;
            Vector2 origin;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(0, 100, 250) * opacity * 0.5f, Main.GameUpdateCount * 0.025f, texture.Size() / 2, Projectile.scale * opacity * 2, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(0, 100, 250) * opacity, Main.GameUpdateCount * 0.05f, texture.Size() / 2, Projectile.scale * opacity, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            texture = Request<Texture2D>(Texture).Value;
            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White * opacity, Projectile.rotation, origin, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.05f, SpriteEffects.None, 0);

            texture = Request<Texture2D>(Texture + "Mask").Value;
            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(4 - (2 * opacity), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), sourceRectangle, new Color(47, 250, 255) * opacity * 0.5f,
                Projectile.rotation, origin, Projectile.scale * opacity + (float)Math.Sin(timer) * 0.05f, SpriteEffects.None, 0);
            }

            //Used for looking at enemies
            Player owner = Main.player[Projectile.owner];
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            moveTo.X = MathHelper.Lerp(moveTo.X, targetCenter.X, 0.2f);
            moveTo.Y = MathHelper.Lerp(moveTo.Y, targetCenter.Y, 0.2f);

            texture = Request<Texture2D>("GloryMod/NPCs/Sightseer/Projectiles/WigglyShot").Value;
            Vector2 eyePosition = Projectile.Center + new Vector2(MathHelper.Clamp(Projectile.Distance(moveTo) * 0.02f, 0, 8), 0).RotatedBy(Projectile.DirectionTo(moveTo).ToRotation());

            Main.EntitySpriteDraw(texture, eyePosition - Main.screenPosition, null, Color.White * eyeOpacity, 0, texture.Size() / 2f, new Vector2(1, eyeOpacity) * Projectile.scale, SpriteEffects.None, 0);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, eyePosition - Main.screenPosition + new Vector2(4 - (2 * eyeOpacity), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), null, new Color(47, 250, 255) * eyeOpacity * 0.5f,
                0, texture.Size() / 2f, new Vector2(1, eyeOpacity) * Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
