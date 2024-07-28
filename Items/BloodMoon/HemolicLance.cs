using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using System.IO;

namespace GloryMod.Items.BloodMoon
{
    internal class HemolicLance : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public int attackType = 0; // keeps track of which attack it is
        public int comboExpireTimer = 0; // we want the attack pattern to reset if the weapon is not used for certain period of time

        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Melee;
            Item.width = 62;
            Item.height = 62;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 8;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item15;
            Item.value = Item.sellPrice(gold: 10);
            Item.shoot = ProjectileType<HemolicLanceProj>();
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, attackType);
            attackType = (attackType + 1) >= 5 ? 1 : attackType + 1;
            comboExpireTimer = 0;

            return false;
        }

        public override void UpdateInventory(Player player)
        {
            if (comboExpireTimer++ == 120)
            {
                SoundEngine.PlaySound(SoundID.Item131 with { Pitch = -0.5f }, player.Center);

                for (int i = 0; i < 24; i++)
                {
                    int dust = Dust.NewDust(player.Center, 0, 0, 114, Scale: 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / 24);
                }

                attackType = 0;
            }
        }

        public override bool MeleePrefix()
        {
            return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)
        }
    }

    class HemolicLanceProj : ModProjectile
    {
        private const float SWINGRANGE = 1.67f * (float)Math.PI;
        private const float HALFSWING = 0.45f;
        private const float SPINRANGE = 3.34f * (float)Math.PI;
        private const float WINDUP = 0.15f;
        private const float UNWIND = 0.4f; 
        private const float SPINTIME = 2.5f;

        private enum AttackType // Which attack is being performed
        {
            Thrust,
            Swing,
            Swing2,
            Spin,
            EnergySpin
        }

        private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
        {
            Prepare,
            Execute,
            Unwind
        }

        // These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
        private AttackType CurrentAttack
        {
            get => (AttackType)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private AttackStage CurrentStage
        {
            get => (AttackStage)Projectile.localAI[0];
            set
            {
                Projectile.localAI[0] = (float)value;
                Timer = 0; // reset the timer when the projectile switches states
            }
        }

        // Variables to keep track of during runtime
        private ref float InitialAngle => ref Projectile.ai[1];
        private ref float Timer => ref Projectile.ai[2];
        private ref float Progress => ref Projectile.localAI[1];
        private ref float Size => ref Projectile.localAI[2];


        private float prepTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float execTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float hideTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

        public override string Texture => "GloryMod/Items/BloodMoon/HemolicLance";
        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 62; 
            Projectile.friendly = true;
            Projectile.timeLeft = 10000; 
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; 
            Projectile.localNPCHitCooldown = -1; 
            Projectile.ownerHitCheck = true; 
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

            if (CurrentAttack == AttackType.Spin)
            {
                InitialAngle = (float)(-Math.PI / 2 - Math.PI * 1 / 3 * Projectile.spriteDirection); // For the spin, starting angle is designated based on direction of hit
            }
            else
            {
                if (Projectile.spriteDirection == 1)
                {
                    // However, we limit the rangle of possible directions so it does not look too ridiculous
                    targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI * 1 / 3, (float)Math.PI * 1 / 6);
                }
                else
                {
                    if (targetAngle < 0)
                    {
                        targetAngle += 2 * (float)Math.PI; // This makes the range continuous for easier operations
                    }

                    targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
                }

                if (CurrentAttack == AttackType.Thrust)
                {
                    InitialAngle = targetAngle;
                }
                if (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.Spin)
                {
                    InitialAngle = targetAngle - HALFSWING * SWINGRANGE * Projectile.spriteDirection;
                }
                if (CurrentAttack == AttackType.Swing2 || CurrentAttack == AttackType.EnergySpin)
                {
                    InitialAngle = targetAngle + HALFSWING * SWINGRANGE * Projectile.spriteDirection;
                }
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
            writer.Write((sbyte)Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
        }

        public override void AI()
        {
            // Extend use animation until projectile is killed
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            // Kill the projectile if the player dies or gets crowd controlled
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            // AI depends on stage and attack
            // Note that these stages are to facilitate the scaling effect at the beginning and end
            // If this is not desirable for you, feel free to simplify
            switch (CurrentStage)
            {
                case AttackStage.Prepare:
                    PrepareStrike();
                    break;
                case AttackStage.Execute:
                    ExecuteStrike();
                    break;
                default:
                    UnwindStrike();
                    break;
            }

            SetSwordPosition();
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
            Vector2 origin;
            float rotationOffset;
            SpriteEffects effects;

            if (Projectile.spriteDirection > 0)
            {
                origin = new Vector2(0, Projectile.height);
                rotationOffset = MathHelper.ToRadians(45f);
                effects = SpriteEffects.None;
            }
            else
            {
                origin = new Vector2(Projectile.width, Projectile.height);
                rotationOffset = MathHelper.ToRadians(135f);
                effects = SpriteEffects.FlipHorizontally;
            }

            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(mask, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, default, Color.Red * Projectile.Opacity * (1 - i / (float)Projectile.oldPos.Length) * 0.25f, Projectile.oldRot[i] + rotationOffset, origin, Projectile.scale, effects, 0);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(mask, Projectile.Center - Main.screenPosition, default, Color.White * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);

            // Since we are doing a custom draw, prevent it from normally drawing
            return false;
        }

        // Find the start and end of the sword and use a line collider to check for collision with enemies
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
        }

        // Do a similar collision check for tiles
        public override void CutTiles()
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
            Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
        }

        // We make it so that the projectile can only do damage in its release and unwind phases
        public override bool? CanDamage()
        {
            if (CurrentStage == AttackStage.Prepare)
                return false;
            return base.CanDamage();
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Make knockback go away from player
            modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

            if (CurrentAttack == AttackType.Thrust)
                modifiers.FinalDamage *= 2;
            
            if (CurrentAttack == AttackType.Spin || CurrentAttack == AttackType.EnergySpin)
                modifiers.Knockback += 2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentAttack == AttackType.Thrust)
            {
                Owner.velocity /= 2;

                Owner.immune = true;
                Owner.immuneTime = 30;
            }
        }

        // Function to easily set projectile and arm position
        public void SetSwordPosition()
        {
            Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

            // Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
            Vector2 armPosition = Owner.Center;

            armPosition.Y += Owner.gfxOffY;
            Projectile.Center = armPosition;
            Projectile.scale = Size * 1.25f * Owner.GetAdjustedItemScale(Owner.HeldItem);

            Owner.heldProj = Projectile.whoAmI;
        }

        // Function facilitating the taking out of the sword
        private void PrepareStrike()
        {
            if (CurrentAttack == AttackType.Swing || CurrentAttack == AttackType.Spin)
            {
                Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime);
            }
            if (CurrentAttack == AttackType.Swing2 || CurrentAttack == AttackType.EnergySpin)
            {
                Progress = WINDUP * -SWINGRANGE * (1f - Timer / prepTime);
            }

            Size = MathHelper.SmoothStep(0, 1, Timer / prepTime);

            if (Timer >= prepTime)
            {
                SoundEngine.PlaySound(SoundID.Item15);
                CurrentStage = AttackStage.Execute;

                if (CurrentAttack == AttackType.Thrust) 
                {
                    Owner.velocity = new Vector2(20, 0).RotatedBy(Projectile.rotation);
                    SoundEngine.PlaySound(SoundID.Item131, Projectile.Center);

                    int numDusts = 16;
                    for (int i = 0; i < numDusts; i++)
                    {
                        int dust = Dust.NewDust(Projectile.Center, 0, 0, 114, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Vector2 trueVelocity = new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        trueVelocity.X *= 0.5f;
                        trueVelocity = trueVelocity.RotatedBy(Projectile.rotation);
                        Main.dust[dust].velocity = trueVelocity;
                    }
                }
            }
        }

        // Function facilitating the first half of the swing
        private void ExecuteStrike()
        {
            if (CurrentAttack == AttackType.Thrust)
            {
                if (Timer >= execTime)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            if (CurrentAttack == AttackType.Swing)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);

                if (Timer >= execTime)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            if (CurrentAttack == AttackType.Swing2)
            {
                Progress = MathHelper.SmoothStep(0, -SWINGRANGE, (1f - UNWIND) * Timer / execTime);

                if (Timer >= execTime)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            if (CurrentAttack == AttackType.Spin)
            {
                Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) * Timer / (execTime * SPINTIME));

                if (Timer == (int)(execTime * SPINTIME * 3 / 4))
                {
                    SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
                    Projectile.ResetLocalNPCHitImmunity();
                }

                if (Timer >= execTime * SPINTIME)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            if (CurrentAttack == AttackType.EnergySpin)
            {
                Progress = MathHelper.SmoothStep(0, -SPINRANGE, (1f - UNWIND / 2) * Timer / (execTime * SPINTIME));

                if (Timer == (int)(execTime * SPINTIME * 3 / 4))
                {
                    SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
                    Projectile.ResetLocalNPCHitImmunity();

                    SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, Projectile.Center);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(2, 0).RotatedBy(Projectile.DirectionTo(Main.MouseWorld).ToRotation()), ProjectileType<EnergySpinProj>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                }

                if (Timer >= execTime * SPINTIME)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            int dust = Dust.NewDust(Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 0.9f), 0, 0, 114, Scale: 1f);
            Main.dust[dust].noGravity = false;
            Main.dust[dust].velocity = new Vector2(CurrentAttack == AttackType.Thrust ? Main.rand.NextFloat(3, 6) : Main.rand.NextFloat(1, 3), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.PiOver4);
        }

        // Function facilitating the latter half of the swing where the sword disappears
        private void UnwindStrike()
        {
            Size = 1f - (CurrentAttack == AttackType.Spin ? MathHelper.SmoothStep(0, 1, Timer / (hideTime * SPINTIME / 2)) : MathHelper.SmoothStep(0, 1, Timer / hideTime));

            if (CurrentAttack == AttackType.Thrust)
            {
                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }

            if (CurrentAttack == AttackType.Swing)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);

                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }

            if (CurrentAttack == AttackType.Swing2)
            {
                Progress = MathHelper.SmoothStep(0, -SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);

                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }

            if (CurrentAttack == AttackType.Spin)
            {
                Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) + UNWIND / 2 * Timer / (hideTime * SPINTIME / 2));

                if (Timer >= hideTime * SPINTIME / 2)
                {
                    Projectile.Kill();
                }
            }

            if (CurrentAttack == AttackType.EnergySpin)
            {
                Progress = MathHelper.SmoothStep(0, -SPINRANGE, (1f - UNWIND / 2) + UNWIND / 2 * Timer / (hideTime * SPINTIME / 2));

                if (Timer >= hideTime * SPINTIME / 2)
                {
                    Projectile.Kill();
                }
            }
        }
    }

    class EnergySpinProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override string Texture => "GloryMod/CoolEffects/Textures/twirl";
        private Player Owner => Main.player[Projectile.owner];
        float opacity;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
        }

        public override void AI()
        {
            Projectile.rotation -= Projectile.spriteDirection * 0.5f;
            opacity = Projectile.timeLeft <= 30 ? MathHelper.Lerp(opacity, 0, 0.1f) : MathHelper.Lerp(opacity, 1, 0.15f);

            if (Projectile.timeLeft <= 60)
            {
                Projectile.velocity *= 1.02f;
                Projectile.scale *= 1.02f;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
            writer.Write((sbyte)Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.Distance(target.Center) <= 100 && !target.friendly;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = Projectile.spriteDirection > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = Request<Texture2D>(Texture).Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, Color.Red * opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale / 2, effects, 0);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, Color.Red * (opacity / 2), Projectile.rotation, texture.Size() / 2, Projectile.scale / 4, effects, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}

