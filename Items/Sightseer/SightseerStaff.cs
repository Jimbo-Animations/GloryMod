using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.Items.Sightseer
{
    class SightseerStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f; // The default value is 1, but other values are supported. See the docs for more guidance. 
        }

        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.knockBack = 3f;
            Item.mana = 10;
            Item.width = 38;
            Item.height = 46;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(gold: 2, silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.autoReuse = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = BuffType<SightseerMinionBuff>();
            Item.shoot = ProjectileType<SightseerStaffMinion>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            return false;
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

    class SightseerMinionBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
            Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // If the minions exist reset the buff time, otherwise remove the buff from the player
            if (player.ownedProjectileCounts[ProjectileType<SightseerStaffMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    class SightseerStaffMinion : ModProjectile
    {
        public override string Texture => "GloryMod/NPCs/Sightseer/Minions/SightseerMinion";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            Main.projPet[Projectile.type] = true;

            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            Projectile.Size = new Vector2(34);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        // Here you can decide if your minion breaks things like grass or pots
        public override bool? CanCutTiles()
        {
            return false;
        }

        // This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
        public override bool MinionContactDamage()
        {
            return true;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<SightseerMinionBuff>());

                return false;
            }

            if (owner.HasBuff(BuffType<SightseerMinionBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, Projectile.Center);

            int numDusts = 12;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 111, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3)) target.AddBuff(BuffID.Confused, 150);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.rand.NextBool(3)) target.AddBuff(BuffID.Confused, 150);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.ai[1]++;
            Projectile.ai[2]++;

            int index = 1;
            int ownedProjectiles = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                {
                    ownedProjectiles++;
                    if (i < Projectile.whoAmI)
                    {
                        index++;
                    }
                }
            }

            if (!CheckActive(owner))
            {
                return;
            }

            if (Projectile.ai[1] % 5 == 1)
            {
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Vector2 IdealPos = owner.Center + new Vector2(-34 * owner.direction * ownedProjectiles / index, -24);
            opacity = MathHelper.SmoothStep(opacity, 1, 0.2f);

            switch (Projectile.ai[0])
            {
                case 0: //Passive following

                    if (Projectile.Distance(IdealPos) > 10) Projectile.velocity += Projectile.DirectionTo(IdealPos) * 0.3f;
                    Projectile.velocity *= 0.95f;

                    Projectile.rotation = Projectile.rotation.AngleTowards(MathHelper.Clamp(Projectile.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);

                    if (Projectile.Distance(IdealPos) > 2000)
                    {
                        Projectile.position = IdealPos;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.netUpdate = true;
                    }

                    if (Projectile.ai[2] >= 60 && foundTarget)
                    {
                        Projectile.ai[0]++;
                        Projectile.ai[2] = 0;
                        opacity = 0;

                        Projectile.netUpdate = true;
                    }

                    break;

                case 1:

                    if (Projectile.ai[2] == 1)
                    {
                        Projectile.position = targetCenter + new Vector2(Main.rand.NextFloat(150, 201), 0).RotatedByRandom(MathHelper.TwoPi);
                        Projectile.velocity = new Vector2(20, 0).RotatedBy(Projectile.DirectionTo(targetCenter).ToRotation());
                        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                        int numDusts = 12;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 0, 0, 111, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Vector2 trueVelocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                            trueVelocity.X *= 0.5f;
                            trueVelocity = trueVelocity.RotatedBy(Projectile.velocity.ToRotation());
                            Main.dust[dust].velocity = trueVelocity;
                        }

                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, Projectile.Center);
                        Projectile.netUpdate = true;
                    }

                    Projectile.velocity *= 0.96f;

                    if (Projectile.ai[2] >= 30)
                    {
                        Projectile.position = IdealPos;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.rotation = 0;
                        opacity = 0;

                        Projectile.ai[0]--;
                        Projectile.ai[2] = 0;

                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, Projectile.Center);

                        int numDusts = 12;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 0, 0, 111, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;                        
                            Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }

                        Projectile.netUpdate = true;
                    }

                    break;
            }
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            // Starting search distance
            distanceFromTarget = 700f;
            targetCenter = Projectile.position;
            foundTarget = false;

            // This code is required if your minion weapon has the targeting feature
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);

                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 2000f)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                // This code is required either way, used for finding a target
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                        // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                        bool closeThroughWall = between < 100f;

                        if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }

            Projectile.friendly = foundTarget && Projectile.ai[0] == 1;
        }


        float opacity;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, lightColor * opacity, Projectile.rotation, origin, new Vector2(opacity, 1), SpriteEffects.None, 0);

            texture = Request<Texture2D>(Texture + "Mask").Value;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White * opacity, Projectile.rotation, origin, new Vector2(opacity, 1), SpriteEffects.None, 0);

            return false;
        }
    }
}
