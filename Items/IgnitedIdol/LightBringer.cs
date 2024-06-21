using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using System.IO;
using System.Collections.Generic;

namespace GloryMod.Items.IgnitedIdol
{
    internal class LightBringer : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public int attackType = 0; // keeps track of which attack it is
        public int comboExpireTimer = 0; // we want the attack pattern to reset if the weapon is not used for certain period of time

        public override void SetDefaults()
        {
            Item.damage = 65;
            Item.DamageType = DamageClass.Melee;
            Item.width = 52;
            Item.height = 52;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 10;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.value = Item.sellPrice(gold: 5);
            Item.shoot = ProjectileType<LightBringerProj>();
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Using the shoot function, we override the swing projectile to set ai[0] (which attack it is)
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, attackType);
            attackType = (attackType + 1) % 3; // Increment attackType to make sure next swing is different
            comboExpireTimer = 0; // Every time the weapon is used, we reset this so the combo does not expire
            return false; // return false to prevent original projectile from being shot
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = new TooltipLine(Mod, "Tooltip#0", "Warning: may attract moths")
            {
                OverrideColor = new Color(255, 0, 0)
            };
            tooltips.Add(line);
        }

        public override void UpdateInventory(Player player)
        {
            if (comboExpireTimer++ >= 120) // after 120 ticks (== 2 seconds) in inventory, reset the attack pattern
                attackType = 0;
        }

        public override bool MeleePrefix()
        {
            return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)
        }
    }

    class LightBringerProj : ModProjectile
    {
        // We define some constants that determine the swing range of the sword
        // Not that we use multipliers here since that simplifies the amount of tweaks for these interactions
        // You could change the values or even replace them entirely, but they are tweaked with looks in mind
        private const float SWINGRANGE = 1f * (float)Math.PI; // The angle a swing attack covers (300 deg)
        private const float HALFSWING = 0.4f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
        private const float WINDUP = 0.2f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
        private const float UNWIND = 0.4f; // When should the sword start disappearing

        private enum AttackType // Which attack is being performed
        {
            Swing,
            Swing2,
            Stab
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
        private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
        private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
        private ref float Size => ref Projectile.localAI[2]; // Size of sword

        // We define timing functions for each stage, taking into account melee attack speed
        // Note that you can change this to suit the need of your projectile
        private float prepTime => 9f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float execTime => 9f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float hideTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true; // Projectile hits enemies
            Projectile.timeLeft = 10000; // Time it takes for projectile to expire
            Projectile.penetrate = -1; // Projectile pierces infinitely
            Projectile.tileCollide = false; // Projectile does not collide with tiles
            Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
            Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
            Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
            Projectile.DamageType = DamageClass.Melee; // Projectile is a melee projectile
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

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

            if (CurrentAttack == AttackType.Swing)
            {
                InitialAngle = targetAngle - HALFSWING * SWINGRANGE * Projectile.spriteDirection;
            }
            if (CurrentAttack == AttackType.Swing2)
            {
                InitialAngle = targetAngle + HALFSWING * SWINGRANGE * Projectile.spriteDirection;
            }
            if (CurrentAttack == AttackType.Stab)
            {
                InitialAngle = targetAngle;
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
            // If this is not desireable for you, feel free to simplify
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
            Texture2D glow = Request<Texture2D>("GloryMod/CoolEffects/Textures/Glow_1").Value;

            Vector2 glowPos = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 0.9f) - Main.screenPosition;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(mask, Projectile.Center - Main.screenPosition, default, Color.White * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
            Main.spriteBatch.Draw(glow, glowPos, default, new Color(250, 200, 100) * Projectile.Opacity * 0.25f, Projectile.rotation + rotationOffset, glow.Size() / 2, Projectile.scale * 0.25f, effects, 0);

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
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentAttack == AttackType.Stab && Timer < execTime)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsySummon, Projectile.Center);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ProjectileType<LightBringerStab>(), Projectile.damage, 0 , Projectile.owner);
            }

            target.AddBuff(BuffID.OnFire3, 300, false);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (CurrentAttack == AttackType.Stab && Timer < execTime)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsySummon, Projectile.Center);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ProjectileType<LightBringerStab>(), Projectile.damage, 0, Projectile.owner);
            }

            target.AddBuff(BuffID.OnFire3, 300, false);
        }

        // Function to easily set projectile and arm position
        public void SetSwordPosition()
        {
            Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

            // Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
            Vector2 armPosition = Owner.Center;

            armPosition.Y += Owner.gfxOffY;
            Projectile.Center = armPosition; // Set projectile to arm position
            Projectile.scale = Size * 1.25f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

            Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
        }

        // Function facilitating the taking out of the sword
        private void PrepareStrike()
        {
            if (CurrentAttack == AttackType.Swing)
            {
                Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime); // Calculates rotation from initial angle
                Size = MathHelper.SmoothStep(0, 1, Timer / prepTime); // Make sword slowly increase in size as we prepare to strike until it reaches max
            }
            if (CurrentAttack == AttackType.Swing2)
            {
                Progress = WINDUP * -SWINGRANGE * (1f - Timer / prepTime); // Calculates rotation from initial angle
                Size = MathHelper.SmoothStep(0, 1, Timer / prepTime); // Make sword slowly increase in size as we prepare to strike until it reaches max
            }
            if (CurrentAttack == AttackType.Stab)
            {
                Size = MathHelper.SmoothStep(0, 1, Timer / prepTime); // Make sword slowly increase in size as we prepare to strike until it reaches max
            }

            if (Timer >= prepTime)
            {
                SoundEngine.PlaySound(SoundID.Item1); // Play sword sound here since playing it on spawn is too early
                
                CurrentStage = AttackStage.Execute; // If attack is over prep time, we go to next stage
            }
        }

        // Function facilitating the first half of the swing
        private void ExecuteStrike()
        {
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
            if (CurrentAttack == AttackType.Stab)
            {
                if (Timer >= execTime * 2 / 3)
                {                   
                    CurrentStage = AttackStage.Unwind;
                }
            }  

            for (int i = 0; i < 3; i++)
            {
                int dust = Dust.NewDust(Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 0.9f), 0, 0, 6, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = new Vector2(CurrentAttack == AttackType.Stab ? Main.rand.NextFloat(3, 7) : Main.rand.NextFloat(1, 4), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.PiOver4);
            }        
        }

        // Function facilitating the latter half of the swing where the sword disappears
        private void UnwindStrike()
        {
            if (CurrentAttack == AttackType.Swing)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
                Size = 1f - MathHelper.SmoothStep(0, 1, Timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation

                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }
            if (CurrentAttack == AttackType.Swing2)
            {
                Progress = MathHelper.SmoothStep(0, -SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
                Size = 1f - MathHelper.SmoothStep(0, 1, Timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation

                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }
            if (CurrentAttack == AttackType.Stab)
            {
                Size = 1f - MathHelper.SmoothStep(0, 1, Timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation

                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }

            int dust = Dust.NewDust(Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 0.9f), 0, 0, 6, Scale: 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(1, 4), 0).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.PiOver4);
        }
    }

    class LightBringerStab : ModProjectile
    {
        public override string Texture => "GloryMod/CoolEffects/Textures/SemiStar";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.friendly = true;
            Projectile.timeLeft = 10;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false; 
            Projectile.usesLocalNPCImmunity = true; 
            Projectile.localNPCHitCooldown = -1; 
            Projectile.ownerHitCheck = true; 
            Projectile.DamageType = DamageClass.Melee;
        }

        private Player Owner => Main.player[Projectile.owner];

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 1f * Owner.GetAdjustedItemScale(Owner.HeldItem);
        }


        float opacity;
        public override void AI()
        {
            // Kill the projectile if the player dies or gets crowd controlled
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }
       
            Projectile.rotation = Projectile.velocity.ToRotation();
            opacity = MathHelper.Lerp(opacity, 1, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300, false);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);

            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * (1 - opacity), MathHelper.PiOver2, origin, new Vector2(.5f, 2) * (Projectile.scale + opacity), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * (1 - opacity), 0, origin, new Vector2(.5f, 2) * (Projectile.scale + opacity), SpriteEffects.None, 0);

            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * (1 - opacity), MathHelper.PiOver2, origin, new Vector2(.3f, 1.2f) * (Projectile.scale + opacity), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * (1 - opacity), 0, origin, new Vector2(.3f, 1.2f) * (Projectile.scale + opacity), SpriteEffects.None, 0);
            return false;
        }
    }
}

