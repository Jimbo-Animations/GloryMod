using Terraria.Audio;
using Terraria.DataStructures;

namespace GloryMod.NPCs.Geomancer
{
    class GeomancerRedProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(34);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
        }

        Vector2 formation;
        float opacity;
        public override void OnSpawn(IEntitySource source)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.rotation = target.Center.X < Projectile.Center.X ? MathHelper.Pi : 0;
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.localAI[0]++;
            formation = target.Center + new Vector2(200 + (40 * Projectile.ai[1]), 0) * Projectile.ai[2];

            if (Projectile.localAI[0] < 90 + (15 * Projectile.ai[1]) && Projectile.Distance(formation) > 10)
            {
                Projectile.velocity += Projectile.DirectionTo(formation) * (Main.hardMode ? 1f : 0.5f);
                Projectile.velocity *= 0.9f;
                if (Main.hardMode) Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.DirectionTo(target.Center + target.velocity * (Projectile.Distance(target.Center) / 3)).ToRotation(), 0.25f);
            }

            if (Projectile.localAI[0] == 90 + (15 * Projectile.ai[1]))
            {
                Projectile.velocity = new Vector2(Main.hardMode ? 3 : 2, 0).RotatedBy(Projectile.rotation);
                SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash, Projectile.Center);
            }
            if (Projectile.localAI[0] >= 90 + (15 * Projectile.ai[1])) 
            {
                Projectile.velocity *= 1.025f;
            }

            if (Projectile.timeLeft <= 20) opacity = MathHelper.Lerp(opacity, 0, 0.05f);
            else opacity = MathHelper.Lerp(opacity, 1, 0.1f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(Main.hardMode ? BuffID.OnFire3 : BuffID.OnFire, Main.hardMode ?  200 : 200 / (Main.expertMode ? Main.masterMode ? 3 : 2 : 1));
        }

        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(250 / i, 100 / i, 50 / i, 0)) * opacity;
                Vector2 trailSize = new Vector2(0.5f - (i / (float)Projectile.oldPos.Length) + ((float)Math.Sin(timer) * 0.5f), 1f - (i / (float)Projectile.oldPos.Length));
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color, Projectile.rotation + MathHelper.PiOver2, origin, trailSize, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>(Texture).Value;

            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * opacity, Projectile.rotation, origin, Projectile.scale + (float)Math.Sin(timer) * 0.5f, SpriteEffects.None, 0);

            return false;
        }
    }

    class GeomancerBlueProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
        }

        Vector2 formation;
        float opacity;
        public override void OnSpawn(IEntitySource source)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.rotation = Projectile.DirectionTo(target.Center).ToRotation();
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;           
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.localAI[0]++;
            formation = target.Center + new Vector2(150 * -Projectile.ai[2], 0).RotatedBy(Projectile.ai[1] * MathHelper.Pi / 9);

            if (Projectile.localAI[0] < 90 && Projectile.Distance(formation) > 10)
            {             
                if (Projectile.localAI[0] < 60) Projectile.velocity += Projectile.DirectionTo(formation) * (Main.hardMode ? 1.2f : 0.6f);
                Projectile.velocity *= 0.9f;
            }

            if (Projectile.localAI[0] == 90)
            {
                Projectile.velocity = new Vector2(Main.hardMode ? 4 : 2, 0).RotatedBy(Projectile.rotation);
                SoundEngine.PlaySound(SoundID.DeerclopsIceAttack, Projectile.Center);
            }
            if (Projectile.localAI[0] >= 90)
            {
                Projectile.velocity *= 1.025f;
            }

            if (Projectile.timeLeft <= 20) opacity = MathHelper.Lerp(opacity, 0, 0.05f);
            else opacity = MathHelper.Lerp(opacity, 1, 0.1f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Frozen, 30 / (Main.expertMode ? Main.masterMode ? 3 : 2 : 1));
        }

        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(50 / i, 80 / i, 220 / i, 0)) * opacity;
                Vector2 trailSize = new Vector2(0.5f - (i / (float)Projectile.oldPos.Length) + ((float)Math.Sin(timer) * 0.5f), 1f - (i / (float)Projectile.oldPos.Length));
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color, Projectile.rotation + MathHelper.PiOver2, origin, trailSize, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>(Texture).Value;

            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * opacity, Projectile.rotation, origin, Projectile.scale + (float)Math.Sin(timer) * 0.5f, SpriteEffects.None, 0);

            return false;
        }
    }

    class GeomancerYellowProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
        }

        float opacity;
        Vector2 formation;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = MathHelper.PiOver2;
            if (Projectile.ai[1] == 0) Projectile.localAI[0] = 30 + (Projectile.ai[2] * 30);
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.localAI[0]++;
            Projectile.localAI[1]++;
            formation = target.Center + new Vector2(40 * Projectile.ai[1], Main.hardMode ? -200 + Math.Abs(15 * Projectile.ai[1]) : -200);

            if (Projectile.localAI[0] < 120)
            {
                if (Projectile.Distance(formation) > 10) Projectile.velocity += Projectile.DirectionTo(formation) * (Main.hardMode ? 1f : 0.5f);
                Projectile.velocity *= 0.9f;
            }

            if (Projectile.localAI[0] == 120)
            {
                Projectile.velocity = new Vector2(0, Main.hardMode ? 6 : 4);
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, Projectile.Center);
            }
            if (Projectile.localAI[0] >= 120)
            {
                Projectile.velocity *= 1.025f;
            }

            if (Projectile.timeLeft <= 20) opacity = MathHelper.Lerp(opacity, 0, 0.05f);
            else opacity = MathHelper.Lerp(opacity, 1, 0.1f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.BrokenArmor, 300 / (Main.expertMode ? Main.masterMode ? 3 : 2 : 1));
        }

        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(200 / i, 200 / i, 50 / i, 0)) * opacity;
                Vector2 trailSize = new Vector2(0.5f - (i / (float)Projectile.oldPos.Length) + ((float)Math.Sin(timer) * 0.5f), 1f - (i / (float)Projectile.oldPos.Length));
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color, Projectile.rotation + MathHelper.PiOver2, origin, trailSize, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>(Texture).Value;

            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * opacity, Projectile.rotation, origin, Projectile.scale + (float)Math.Sin(timer) * 0.5f, SpriteEffects.None, 0);

            return false;
        }
    }

    class GeomancerGreenProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
        }

        float opacity;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage /= Main.expertMode ? Main.masterMode ? 6 : 4 : 2;
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Projectile.localAI[0]++;
            if (Projectile.Distance(target.Center) <= 200 && Projectile.localAI[0] >= 60) Projectile.localAI[1]++;

            if (Projectile.localAI[0] < 60)
            {
                Projectile.rotation = Projectile.DirectionTo(target.Center).ToRotation();
                Projectile.hostile = false;
            } 
            else 
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.hostile = true;
            }

            if (Projectile.localAI[0] == 60) 
            {
                Projectile.velocity = new Vector2(5, 0).RotatedBy(Projectile.rotation);
                SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, Projectile.Center);

                int numDusts = 12;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 110, Scale: 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Vector2 trueVelocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                    trueVelocity.X *= 0.5f;
                    trueVelocity = trueVelocity.RotatedBy(Projectile.velocity.ToRotation());
                    Main.dust[dust].velocity = trueVelocity;
                }
            }
            if (Projectile.localAI[0] > 60 && Projectile.localAI[1] < 15)
            {
                Projectile.velocity += Projectile.DirectionTo(target.Center) * 1.05f;
                Projectile.velocity *= 0.95f;
            }
            if (Projectile.localAI[1] >= 15) Projectile.velocity *= 1.025f;

            if (Projectile.timeLeft <= 20) opacity = MathHelper.Lerp(opacity, 0, 0.05f);
            else opacity = MathHelper.Lerp(opacity, 1, 0.1f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.WitheredWeapon, 300);
        }

        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = Projectile.GetAlpha(new Color(50 / i, 250 / i, 50 / i, 0)) * opacity;
                Vector2 trailSize = new Vector2(0.5f - (i / (float)Projectile.oldPos.Length) + ((float)Math.Sin(timer) * 0.5f), 1f - (i / (float)Projectile.oldPos.Length));
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, sourceRectangle, color, Projectile.rotation + MathHelper.PiOver2, origin, trailSize, SpriteEffects.None, 0);
            }

            texture = Request<Texture2D>(Texture).Value;

            frameHeight = texture.Height / Main.projFrames[Projectile.type];
            frameY = frameHeight * Projectile.frame;
            sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            origin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White * opacity, Projectile.rotation, origin, Projectile.scale + (float)Math.Sin(timer) * 0.5f, SpriteEffects.None, 0);

            return false;
        }
    }
}
