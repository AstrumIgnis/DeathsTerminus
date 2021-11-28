﻿using System;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using DeathsTerminus.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria.Graphics.Effects;
using DeathsTerminus.Assets;

namespace DeathsTerminus.NPCs.CataBoss
{
    [AutoloadBossHead]
    public class CataBoss : ModNPC
    {
        //ai[0] is attack type
        //ai[1] is attack timer
        //ai[2] and ai[3] are secondary values for attacks

        //localAI[0] is 0 if shadow cata and 1 if regular cata

        private bool canShieldBonk;
        private bool holdingShield;
        private bool onSlimeMount;
        private int iceShieldCooldown;
        private int hitDialogueCooldown;
        private int rodAnticheeseCooldown;
        private bool drawSpawnTransitionRing;
        private Color spawnTransitionColor = Color.Purple;
        private Color auraColor = Color.Purple;
        private bool useRainbowColorAura = false;
        private bool useRainbowColorTransition = false;
        private bool drawEyeTrail = false;
        private bool drawAura = false;
        private int auraCounter = 0;
        private bool killable = false;
        private float teleportTime;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cataclysmic Armageddon");
            Main.npcFrameCount[npc.type] = 6;

            NPCID.Sets.TrailCacheLength[npc.type] = 26;
            NPCID.Sets.TrailingMode[npc.type] = 3;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = (int)AIStyles.CustomAI;
            npc.width = 18;
            npc.height = 40;
            drawOffsetY = -5;

            npc.defense = 0;
            npc.lifeMax = 250;
            npc.chaseable = false;
            npc.HitSound = SoundID.NPCHit5;
            npc.DeathSound = SoundID.NPCDeath59;

            npc.damage = 160;
            npc.knockBackResist = 0f;

            npc.value = Item.buyPrice(platinum: 1);

            npc.npcSlots = 15f;
            npc.boss = true;
            //bossBag = ItemType<CataBossBag>();

            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;

            for (int i = 0; i < Main.maxBuffTypes; i++)
            {
                npc.buffImmune[i] = true;
            }

            music = MusicID.Boss4;

            Mod modMusic = ModLoader.GetMod("DeathsTerminusMusic");
            if (modMusic != null)
            {
                music = modMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/Lights_Aerial_Veil");
            }
        }

        public override void AI()
        {
            Player player = Main.player[npc.target];
            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!player.active || player.dead)
                {
                    if (npc.localAI[0] == 0)
                    {
                        ShadowCataFleeAnimation();
                    }
                    else
                    {
                        npc.Transform(ModContent.NPCType<CataclysmicArmageddon>());
                    }

                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].hostile)
                        {
                            Main.projectile[i].active = false;
                        }
                    }
                    return;
                }
            }

            holdingShield = false;
            drawSpawnTransitionRing = false;
            if (iceShieldCooldown > 0)
            {
                iceShieldCooldown--;
            }
            if (hitDialogueCooldown > 0)
            {
                hitDialogueCooldown--;
            }
            if (rodAnticheeseCooldown > 0)
            {
                rodAnticheeseCooldown--;
            }
            if (drawAura)
            {
                auraCounter++;
            }

            //RoD arrival dusts
            if (teleportTime > 0)
            {
                if ((float)Main.rand.Next(100) <= 100f * teleportTime)
                {
                    int num2 = Dust.NewDust(npc.position, npc.width, npc.height, 164);
                    Main.dust[num2].scale = teleportTime * 1.5f;
                    Main.dust[num2].noGravity = true;
                    Dust obj2 = Main.dust[num2];
                    obj2.velocity *= 1.1f;
                }
                teleportTime -= 0.005f;
            }

            npc.life = npc.lifeMax;

            //RoD anticheese
            if (npc.ai[0] < 23 && rodAnticheeseCooldown == 0 && player.HeldItem.type == ItemID.RodofDiscord && player.itemTime > 0)
            {
                rodAnticheeseCooldown = player.itemTime;
                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBossRod>(), 80, 0f, Main.myPlayer, player.whoAmI);
                }
            }

            switch (npc.ai[0])
            {
                case 0:
                    //5 secs each
                    SpawnAnimation();
                    break;
                case 1:
                    //3 secs each
                    SideScythesAttack();
                    break;
                case 2:
                case 7:
                    //7.3333 secs each
                    SideScythesAttackSpin();
                    break;
                case 3:
                case 8:
                    //4.3333 secs each
                    SideBlastsAttack();
                    break;
                case 4:
                    //8 secs each
                    IceSpiralAttack();
                    break;
                case 5:
                    //2.8 secs each
                    ShieldBonk();
                    break;
                case 6:
                    //2 secs each
                    SlimeBonk();
                    break;
                case 9:
                    //10 secs each
                    MothsAndLampAttack();
                    break;
                case 10:
                    //10 secs each
                    HeavenPetAttack();
                    break;
                case 11:
                    //5 secs each
                    Phase1To2Animation();
                    break;
                case 12:
                case 19:
                    //7.3333 secs each
                    SideScythesAttackHard();
                    break;
                case 13:
                case 21:
                    //5 secs each
                    SideBlastsAttackHard();
                    break;
                case 14:
                    //12 secs each
                    IceSpiralAttackHard();
                    break;
                case 15:
                    //2.5 secs each
                    SideSuperScythesAttack();
                    break;
                case 16:
                    //9 secs each
                    AncientDoomMinefield();
                    break;
                case 17:
                    //6 secs each
                    ShieldBonkHard();
                    break;
                case 18:
                    //10 secs each
                    SlimeBonkHard();
                    break;
                case 20:
                    //10 secs each
                    MothronsAndLampAttack();
                    break;
                case 22:
                    //10 secs each
                    HeavenPetAttackHard();
                    break;
                case 23:
                    //5 secs each
                    Phase2To3Animation();
                    break;
                case 24:
                    //15 secs each
                    IceScythesAttack();
                    break;
                case 25:
                    //12 secs each
                    AncientDoomMinefieldHard();
                    break;
                case 26:
                    //13 secs each
                    FishronsMothsAttack();
                    break;
                case 27:
                    //29 secs each
                    MothronsAndLampCircularAttack();
                    break;
                case 28:
                    if (Main.expertMode)
                    {
                        //5 secs each
                        Phase3To4Animation();
                    }
                    else
                    {
                        //4 secs each
                        DeathAnimation();
                    }
                    break;
                case 29:
                    if (Main.expertMode)
                    {
                        //32 secs each
                        MegaSprocketVsMegaBaddySuperCinematicDesperationAttack();
                    }
                    else
                    {
                        killable = true;
                        npc.life = 0;
                        npc.checkDead();
                    }
                    break;
                case 30:
                    //4 secs each
                    DeathAnimation();
                    break;
                case 31:
                    killable = true;
                    npc.life = 0;
                    npc.checkDead();
                    break;
            }
        }

        private void FlyToPoint(Vector2 goalPoint, Vector2 goalVelocity, float maxXAcc = 0.5f, float maxYAcc = 0.5f)
        {
            Vector2 goalOffset = goalPoint - goalVelocity - npc.Center;
            Vector2 relativeVelocity = npc.velocity - goalVelocity;

            //compute whether we'll overshoot or undershoot our X goal at our current velocity
            if (relativeVelocity.X * relativeVelocity.X / 2 / maxXAcc > Math.Abs(goalOffset.X) && (goalOffset.X > 0 ^ relativeVelocity.X < 0))
            {
                //overshoot
                npc.velocity.X += maxXAcc * (goalOffset.X > 0 ? -1 : 1);
            }
            else
            {
                //undershoot
                npc.velocity.X += maxXAcc * (goalOffset.X > 0 ? 1 : -1);
            }
            //compute whether we'll overshoot or undershoot our X goal at our current velocity
            if (relativeVelocity.Y * relativeVelocity.Y / 2 / maxYAcc > Math.Abs(goalOffset.Y) && (goalOffset.Y > 0 ^ relativeVelocity.Y < 0))
            {
                //overshoot
                npc.velocity.Y += maxYAcc * (goalOffset.Y > 0 ? -1 : 1);
            }
            else
            {
                //undershoot
                npc.velocity.Y += maxYAcc * (goalOffset.Y > 0 ? 1 : -1);
            }
        }

        //1 sec
        private void SpawnAnimation()
        {
            Player player = Main.player[npc.target];

            if (npc.localAI[0] == 0 && npc.ai[1] == 0)
            {
                npc.Center = player.Center + new Vector2(240 * (Main.rand.NextBool() ? 1 : -1), -240);
                npc.velocity = Vector2.Zero;

                for (int i = 0; i < 128; i++)
                {
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<ShadowDust>(), Scale: 2).velocity = new Vector2((float)Math.Sin(Main.rand.NextFloat() * MathHelper.Pi / 2f) * 6f,0).RotatedByRandom(MathHelper.TwoPi);
                }

                Main.LocalPlayer.GetModPlayer<DTPlayer>().screenShakeTime = 60;
                Main.PlaySound(SoundID.DoubleJump, npc.Center);
                Main.PlaySound(SoundID.Item122, npc.Center);

                CombatText.NewText(npc.getRect(), new Color(0, 76, 153), "So, here we are... it's about time you died!", true);

                //initialize custom death sound
                npc.DeathSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ShadowCataDeath").WithVolume(4f);
            }

            if ((npc.Center - player.Center).Length() > 1000 && npc.ai[1] == 0)
            {
                npc.Center = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 1000;
            }

            spawnTransitionColor = Color.Purple;

            CataBossSky.celestialObject = 0;

            if (npc.ai[1] == 120)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), new Color(0, 76, 153), "Don't waste your time attacking.", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), new Color(0, 76, 153), "So you think you can defeat me?", true);
                }
            }
            else if (npc.ai[1] == 180)
            {
                Main.PlaySound(SoundID.Zombie, npc.Center, 105);
            }
            else if (npc.ai[1] == 299)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), Color.Purple, "This shadow form can't be damaged!", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), Color.Purple, "Well then, let's see what you can do.", true);
                }
            }

            //transition animation
            if (npc.ai[1] >= 180 && npc.ai[1] < 299)
            {
                npc.velocity = Vector2.Zero;

                drawSpawnTransitionRing = true;
            }
            else
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

                if (npc.localAI[0] == 0)
                {
                    goalPosition = player.Center + new Vector2(-npc.direction, -1) * 240;
                }

                FlyToPoint(goalPosition, Vector2.Zero);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 300)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void Phase1To2Animation()
        {
            //Pre-animation and text
            Player player = Main.player[npc.target];

            spawnTransitionColor = Color.Orange;

            if (npc.ai[1] == 120)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), Color.Purple, "This is only the beginning, you insect!", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), Color.Purple, "Not bad, you've survived a minute.", true);
                }
            }
            else if (npc.ai[1] == 180)
            {
                Main.PlaySound(SoundID.Zombie, npc.Center, 105);
            }
            else if (npc.ai[1] == 299)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), Color.Orange, "You'll soon be reunited with your family!", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), Color.Orange, "But it will only get harder from here.", true);
                }

                drawEyeTrail = true;

                auraColor = spawnTransitionColor;

                CataBossSky.celestialObject = 1;
            }

            //transition animation
            if (npc.ai[1] >= 180 && npc.ai[1] < 299)
            {
                npc.velocity = Vector2.Zero;

                drawSpawnTransitionRing = true;
            }
            else
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

                FlyToPoint(goalPosition, Vector2.Zero);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 300)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void Phase2To3Animation()
        {
            //Pre-animation and text
            Player player = Main.player[npc.target];

            spawnTransitionColor = Color.LightBlue;

            if (npc.ai[1] == 120)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), Color.Orange, "Why do you insist on prolonging this?", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), Color.Orange, "You're making good progress so far.", true);
                }
            }
            else if (npc.ai[1] == 180)
            {
                Main.PlaySound(SoundID.Zombie, npc.Center, 105);
            }
            else if (npc.ai[1] == 299)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), Color.LightBlue, "You weren't meant to retaliate!", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), Color.LightBlue, "Let's see if you've got what it takes.", true);
                }

                drawAura = true;

                auraColor = spawnTransitionColor;

                CataBossSky.celestialObject = 2;
            }

            //transition animation
            if (npc.ai[1] >= 180 && npc.ai[1] < 299)
            {
                npc.velocity = Vector2.Zero;

                drawSpawnTransitionRing = true;
            }
            else
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

                FlyToPoint(goalPosition, Vector2.Zero);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 300)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void Phase3To4Animation()
        {
            //Pre-animation and text
            Player player = Main.player[npc.target];

            npc.dontTakeDamage = true;
            useRainbowColorTransition = true;

            if (npc.ai[1] >= 240 && npc.localAI[0] != 0)
            {
                iceShieldCooldown += 2;
            }

            if (npc.ai[1] == 120)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), Color.LightBlue, "This isn't right, you're supposed to be dead!", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), Color.LightBlue, "That's it, this has taken long enough.", true);
                }
            }
            else if (npc.ai[1] == 180)
            {
                Main.PlaySound(SoundID.Zombie, npc.Center, 105);
            }
            else if (npc.ai[1] == 299)
            {
                if (npc.localAI[0] == 0)
                {
                    CombatText.NewText(npc.getRect(), Color.White, "I can't keep this form up much longer!", true);

                    for (int i = 0; i < 64; i++)
                    {
                        Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<ShadowDust>(), Scale: 2).velocity = new Vector2((float)Math.Sin(Main.rand.NextFloat() * MathHelper.Pi / 2f) * 4f, 0).RotatedByRandom(MathHelper.TwoPi);
                    }

                    Main.PlaySound(SoundID.DoubleJump, npc.Center);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), Color.White, "That wasn't meant to happen. Shadow, assistance?", true);

                    Main.PlaySound(SoundID.Item27, npc.Center);

                    for (int i = 0; i < 6; i++)
                    {
                        Gore.NewGorePerfect(npc.Center - new Vector2(12, 35), new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / 6), mod.GetGoreSlot("Gores/CataBossIceShard"));
                        Gore.NewGorePerfect(npc.Center - new Vector2(12, 35), new Vector2(3, 0).RotatedBy((i + 0.5f) * MathHelper.TwoPi / 6), mod.GetGoreSlot("Gores/CataBossIceShard"));
                    }
                }

                useRainbowColorAura = true;
                iceShieldCooldown = 0;

                CataBossSky.celestialObject = 3;
            }

            //transition animation
            if (npc.ai[1] >= 180 && npc.ai[1] < 299)
            {
                npc.velocity = Vector2.Zero;

                drawSpawnTransitionRing = true;
            }
            else
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

                FlyToPoint(goalPosition, Vector2.Zero);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 300)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void DeathAnimation()
        {
            if (npc.localAI[0] == 0)
            {
                //Pre-animation and text
                Player player = Main.player[npc.target];

                Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<ShadowDust>());

                if (npc.ai[1] == 120)
                {
                    CombatText.NewText(npc.getRect(), Color.White, "How... how did this happen...", true);
                }
                else if (npc.ai[1] == 240)
                {
                    CombatText.NewText(npc.getRect(), Color.White, "I can't contain this form any longer...", true);
                }
                else if (npc.ai[1] == 360)
                {
                    CombatText.NewText(npc.getRect(), Color.White, "I guess it's time we fight for real.", true);
                }
                else if (npc.ai[1] == 480)
                {
                    CombatText.NewText(npc.getRect(), Color.White, "I'll be waiting.", true);
                }

                //death animation
                if (npc.ai[1] >= 60)
                {
                    npc.velocity = Vector2.Zero;
                }
                else
                {
                    if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                        npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction;
                    Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

                    FlyToPoint(goalPosition, Vector2.Zero);
                }

                npc.ai[1]++;
                if (npc.ai[1] == 600)
                {
                    for (int i = 0; i < 256; i++)
                    {
                        Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<ShadowDust>(), Scale: 2).velocity = new Vector2((float)Math.Sin(Main.rand.NextFloat() * MathHelper.Pi / 2f) * 8f, 0).RotatedByRandom(MathHelper.TwoPi);
                    }

                    Main.LocalPlayer.GetModPlayer<DTPlayer>().screenShakeTime = 60;
                    Main.PlaySound(SoundID.DoubleJump, npc.Center);

                    npc.ai[1] = 0;
                    npc.ai[0]++;
                }
            }
            else
            {
                //Pre-animation and text
                Player player = Main.player[npc.target];

                if (npc.ai[1] == 60 && Main.netMode != 1)
                {
                    Vector2 shotSpeed = new Vector2(0, 12).RotatedByRandom(0.5f);
                    Projectile.NewProjectile(npc.Center - 180 * shotSpeed, shotSpeed, ModContent.ProjectileType<CataBossStar>(), 0, 0f, Main.myPlayer);
                }

                if (npc.ai[1] == 120)
                {
                    CombatText.NewText(npc.getRect(), Color.White, "Well, that was fun. I sure hope my current vulnerability won't suddenly become an issue.", true);
                }

                //death animation
                if (npc.ai[1] >= 60)
                {
                    npc.velocity = Vector2.Zero;
                }
                else
                {
                    if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                        npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction;
                    Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

                    FlyToPoint(goalPosition, Vector2.Zero);
                }

                npc.ai[1]++;
                if (npc.ai[1] == 240)
                {
                    npc.ai[1] = 0;
                    npc.ai[0]++;
                }
            }
        }

        private void ShadowCataFleeAnimation()
        {
            Player player = Main.player[npc.target];

            if (npc.localAI[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

                FlyToPoint(goalPosition, Vector2.Zero);
            }
            else if (npc.localAI[1] < 120)
            {
                npc.velocity = Vector2.Zero;
                if (npc.localAI[1] == 60)
                {
                    Main.PlaySound(SoundID.Zombie, npc.Center, 105);
                }
            }
            else
            {
                npc.velocity.Y -= 0.3f;
            }

            npc.localAI[1]++;
            if (npc.localAI[1] == 240)
            {
                npc.active = false;
            }
        }

        //3 secs
        private void SideScythesAttack()
        {
            Player player = Main.player[npc.target];
            if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] >= 60 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
            {
                int numShots = 8;
                for (int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);

                    Vector2 targetPoint = npc.Center + new Vector2(800, 0).RotatedBy(i * MathHelper.TwoPi / numShots);
                    Vector2 launchPoint = targetPoint + new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2);
                    Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f);
                    launchPoint = targetPoint - new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2);
                    Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f);
                }
            }

            if (npc.ai[1] >= 60 && npc.ai[1] % 10 == 0)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 180)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //7 secs 20 ticks
        private void SideScythesAttackSpin()
        {
            Player player = Main.player[npc.target];

            if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] >= 60 && npc.ai[1] < 220 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
            {
                int numShots = 8;
                for (int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots + npc.direction * (npc.ai[1] - 60) / 100f) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);

                    Vector2 targetPoint = npc.Center + new Vector2(800, 0).RotatedBy(i * MathHelper.TwoPi / numShots + npc.direction * (npc.ai[1] - 60) / 100f);
                    Vector2 launchPoint = targetPoint + new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2 + npc.direction * (npc.ai[1] - 60) / 100f);
                    Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f);
                    launchPoint = targetPoint - new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2 + npc.direction * (npc.ai[1] - 60) / 100f);
                    Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f);
                }
            }
            else if (npc.ai[1] >= 280 && npc.ai[1] < 440 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
            {
                int numShots = 8;
                for (int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots - npc.direction * (npc.ai[1] - 280) / 100f) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);

                    Vector2 targetPoint = npc.Center + new Vector2(800, 0).RotatedBy(i * MathHelper.TwoPi / numShots - npc.direction * (npc.ai[1] - 280) / 100f);
                    Vector2 launchPoint = targetPoint + new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2 - npc.direction * (npc.ai[1] - 280) / 100f);
                    Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f);
                    launchPoint = targetPoint - new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2 - npc.direction * (npc.ai[1] - 280) / 100f);
                    Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f);
                }
            }

            if (((npc.ai[1] >= 60 && npc.ai[1] < 220) || (npc.ai[1] >= 280 && npc.ai[1] < 440)) && npc.ai[1] % 10 == 0)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 440)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //4 secs 20 ticks
        private void SideBlastsAttack()
        {
            Player player = Main.player[npc.target];

            if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 480;

            FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.4f, maxYAcc: 0.4f);

            int shotPeriod = 50;
            int numShots = 4;
            float shotSpeed = 0.5f;
            float shotDistanceFromPlayer = 200;

            if (npc.ai[1] >= 60 && npc.ai[1] % shotPeriod == 60 % shotPeriod && Main.netMode != 1)
            {
                float angleRatio = shotDistanceFromPlayer / (player.Center - npc.Center).Length();
                if (angleRatio > 1)
                {
                    angleRatio = 1;
                }

                int direction = npc.ai[1] % (shotPeriod * 2) == 60 % (shotPeriod * 2) ? 1 : -1;
                Vector2 shotVelocity = (player.Center - npc.Center).SafeNormalize(Vector2.Zero).RotatedBy(direction * Math.Asin(angleRatio)) * shotSpeed;

                Projectile.NewProjectile(npc.Center, shotVelocity, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(npc.Center, -shotVelocity, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
            }

            if (npc.ai[1] >= 90 && npc.ai[1] % shotPeriod == 90 % shotPeriod)
            {
                Main.PlaySound(SoundID.Item71, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 60 + shotPeriod * numShots)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //8 secs
        private void IceSpiralAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else
            {
                npc.velocity = Vector2.Zero;

                if (npc.ai[1] == 60 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShield>(), 80, 0f, Main.myPlayer);
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<RotatingIceShards>(), 80, 0f, Main.myPlayer, ai0: i);
                    }
                }

                if (npc.ai[1] == 60)
                {
                    Main.PlaySound(29, npc.Center, 88);
                }
                if (npc.ai[1] == 120)
                {
                    Main.PlaySound(SoundID.Item120, npc.Center);
                }

                if (npc.ai[1] % 10 == 0 && npc.ai[1] < 360 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShardArena>(), 80, 0f, Main.myPlayer, ai0: 1);
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShardArena>(), 80, 0f, Main.myPlayer, ai0: -1);
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 480)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //2 secs 48 ticks
        private void ShieldBonk()
        {
            Player player = Main.player[npc.target];

            holdingShield = true;

            if (npc.ai[1] % 84 < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 180;

                FlyToPoint(goalPosition, player.velocity, 0.8f, 0.8f);
            }
            else if (npc.ai[1] % 84 == 60)
            {
                canShieldBonk = true;

                npc.width = 40;
                npc.position.X -= 11;

                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                npc.velocity.X += npc.direction * 15;
                npc.velocity.Y /= 2;
            }
            else if (npc.ai[1] % 84 == 83)
            {
                if (canShieldBonk)
                {
                    npc.width = 18;
                    npc.position.X += 11;
                }

                canShieldBonk = false;

                npc.velocity.X -= npc.direction * 15;
            }

            //custom stuff for player EoC shield bonks
            //adapted from how the player detects SoC collision
            if (canShieldBonk)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && Main.player[i].dash == 2 && Main.player[i].eocDash > 0 && Main.player[i].eocHit < 0)
                    {
                        Rectangle shieldHitbox = new Rectangle((int)((double)Main.player[i].position.X + (double)Main.player[i].velocity.X * 0.5 - 4.0), (int)((double)Main.player[i].position.Y + (double)Main.player[i].velocity.Y * 0.5 - 4.0), Main.player[i].width + 8, Main.player[i].height + 8);
                        if (shieldHitbox.Intersects(npc.getRect()))
                        {
                            //custom stuff for player EoC shield bonks
                            //adapted from how the player detects SoC collision
                            npc.width = 18;
                            npc.position.X += 11;

                            npc.direction *= -1;
                            npc.velocity.X += npc.direction * 30;
                            canShieldBonk = false;

                            Main.PlaySound(SoundID.NPCHit4, npc.Center);

                            //redo the player's SoC bounce motion
                            int num40 = Main.player[i].direction;
                            if (Main.player[i].velocity.X < 0f)
                            {
                                num40 = -1;
                            }
                            if (Main.player[i].velocity.X > 0f)
                            {
                                num40 = 1;
                            }
                            Main.player[i].eocDash = 10;
                            Main.player[i].dashDelay = 30;
                            Main.player[i].velocity.X = -num40 * 9;
                            Main.player[i].velocity.Y = -4f;
                            Main.player[i].immune = true;
                            Main.player[i].immuneNoBlink = true;
                            Main.player[i].immuneTime = 4;
                            Main.player[i].eocHit = i;

                            break;
                        }
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 168)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SlimeBonk()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(0, -1) * 360;

                FlyToPoint(goalPosition, player.velocity, 3f, 1f);
            }
            else
            {
                if (npc.ai[1] == 60)
                {
                    Main.PlaySound(SoundID.Item81, npc.Center);

                    onSlimeMount = true;

                    if (npc.velocity.Y < 0) npc.velocity.Y /= 2;
                    npc.velocity.X = player.velocity.X;

                    npc.width = 40;
                    npc.position.X -= 11;
                    npc.height = 64;
                    drawOffsetY = -15;

                    //mount dusts from slime mount
                    for (int i = 0; i < 100; i++)
                    {
                        int num2 = Dust.NewDust(new Vector2(npc.position.X - 20f, npc.position.Y), npc.width + 40, npc.height, 56);
                        Main.dust[num2].scale += (float)Main.rand.Next(-10, 21) * 0.01f;
                        Main.dust[num2].noGravity = true;
                        Dust obj2 = Main.dust[num2];
                        obj2.velocity += npc.velocity * 0.8f;
                    }
                }
                else if (onSlimeMount)
                {
                    if (npc.ai[1] < 119 && (npc.velocity.Y < 0 || npc.Hitbox.Top - 300 < player.Hitbox.Bottom))
                    {
                        npc.velocity.Y += 0.9f;

                        FlyToPoint(player.Center, player.velocity, 0.05f, 0f);
                    }
                    else
                    {
                        npc.velocity = player.velocity;
                        onSlimeMount = false;

                        npc.width = 18;
                        npc.position.X += 11;
                        npc.height = 40;
                        drawOffsetY = -5;
                    }
                }
                else
                {
                    npc.velocity *= 0.98f;
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 120)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void MothsAndLampAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 1080;

                FlyToPoint(goalPosition, player.velocity, 0.8f, 0.8f);
            }
            else
            {
                if (npc.ai[1] == 60)
                {
                    npc.ai[2] = npc.Center.X;
                    npc.ai[3] = npc.Center.Y;
                    
                    Main.PlaySound(SoundID.Zombie, npc.Center + new Vector2(npc.direction, 0) * 1500, 104);
                    Main.LocalPlayer.GetModPlayer<DTPlayer>().screenShakeTime = 60;

                    if (Main.netMode != 1)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<SigilArena>(), 80, 0f, Main.myPlayer, ai0: 600);
                        Projectile.NewProjectile(npc.Center + new Vector2(npc.direction, 0) * 1500, new Vector2(-npc.direction * 5, 0), ModContent.ProjectileType<SunLamp>(), 80, 0f, Main.myPlayer);
                    }
                }

                if (Main.netMode != 1 && npc.ai[1] >= 90 && npc.ai[1] <= 480 && npc.ai[1] % 4 == 0)
                {
                    Vector2 arenaCenter = new Vector2(npc.ai[2], npc.ai[3]);

                    //ray X position
                    float relativeRayPosition = npc.direction * 1500 - npc.direction * 5 * (npc.ai[1] + 30 - 60);

                    //angle of the arena still available
                    float availableAngle = (float)Math.Acos(relativeRayPosition / 1200f);
                    if (npc.direction == 1)
                    {
                        availableAngle = MathHelper.Pi - availableAngle;
                    }
                    if (availableAngle > MathHelper.Pi / 2)
                    {
                        availableAngle = MathHelper.Pi / 2;
                    }

                    float availableHeight = (float)Math.Sqrt(1200f * 1200f - relativeRayPosition * relativeRayPosition);

                    float shotAngle = -npc.direction * availableAngle * ((npc.ai[1] / 1.618f / 4 % 1) * 2 - 1);
                    float goalHeight = arenaCenter.Y + -npc.direction * shotAngle / availableAngle * availableHeight;

                    Projectile.NewProjectile(arenaCenter + new Vector2(-1800f * npc.direction, 0f).RotatedBy(shotAngle), new Vector2(32f * npc.direction, 0).RotatedBy(shotAngle), ModContent.ProjectileType<BabyMothronProjectile>(), 80, 0f, Main.myPlayer, ai0: goalHeight, ai1: npc.direction * 1500 - npc.direction * 5 * (npc.ai[1] - 60) + arenaCenter.X);
                }

                Vector2 goalPosition = new Vector2(npc.ai[2], npc.ai[3]) + new Vector2(-npc.direction, 0) * 1080;

                FlyToPoint(goalPosition, Vector2.Zero, 0.25f, 0.25f);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void HeavenPetAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] == 0 && Main.netMode != 1)
            {
                int proj = Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * -6f, ModContent.ProjectileType<HeavenPetProjectile>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI);
                Main.projectile[proj].localAI[1] = npc.localAI[0];
            }

            if (npc.ai[1] < 600)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }

            if (npc.ai[1] % 15 == 0 && npc.ai[1] > 60)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);

                if (Main.netMode != 1)
                    Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //7 secs 20 ticks
        private void SideScythesAttackHard()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 360;

                FlyToPoint(goalPosition, Vector2.Zero);
            }
            else
            {
                npc.spriteDirection = player.Center.X > npc.Center.X ? 1 : -1;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

                FlyToPoint(goalPosition, Vector2.Zero);
            }

            if (npc.ai[1] >= 60 && npc.ai[1] < 440 && npc.ai[1] % 5 == 0 && Main.netMode != 1)
            {
                int numShots = 12;
                for (int i = 0; i < numShots; i++)
                {
                    float rotationValue = -npc.direction * MathHelper.Pi / 2f * ((float)Math.Cos((npc.ai[1] - 60) / 380f * MathHelper.TwoPi) - 1) / 2f;

                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots + rotationValue) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: 1);

                    if (npc.ai[1] % 10 == 0)
                    {
                        Vector2 targetPoint = npc.Center + new Vector2(800, 0).RotatedBy(i * MathHelper.TwoPi / numShots + rotationValue);
                        Vector2 launchPoint = targetPoint + new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2 + rotationValue);
                        Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f, ai1: 1);
                        launchPoint = targetPoint - new Vector2(1920, 0).RotatedBy(i * MathHelper.TwoPi / numShots + MathHelper.PiOver2 + rotationValue);
                        Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 30f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai0: 120f, ai1: 1);
                    }
                }
            }

            if ((npc.ai[1] >= 60 && npc.ai[1] < 440) && npc.ai[1] % 10 == 0)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 440)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SideSuperScythesAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            }
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 360;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] >= 60 && npc.ai[1] <= 90 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
            {
                int number = (int)(npc.ai[1] - 60) / 10;

                for (int i = -number; i <= number; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(npc.direction, 0).RotatedBy(i / 3f * MathHelper.TwoPi / 6) * 0.5f, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1f);
                }
            }
            if (npc.ai[1] - 30 >= 60 && npc.ai[1] - 30 <= 90 && (npc.ai[1] - 30) % 10 == 0)
            {
                Main.PlaySound(SoundID.Item71, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 150)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SideBlastsAttackHard()
        {
            Player player = Main.player[npc.target];

            if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 720;

            FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.4f, maxYAcc: 0.4f);

            int shotPeriod = 60;
            int numShots = 4;
            float shotSpeed = 0.5f;
            float shotDistanceFromPlayer = 300;

            if (npc.ai[1] >= 60 && npc.ai[1] % shotPeriod == 60 % shotPeriod && Main.netMode != 1)
            {
                float angleRatio = shotDistanceFromPlayer / (player.Center - npc.Center).Length();
                if (angleRatio > 1)
                {
                    angleRatio = 1;
                }

                int direction = npc.ai[1] % (shotPeriod * 2) == 60 % (shotPeriod * 2) ? 1 : -1;
                Vector2 shotVelocity = (player.Center - npc.Center).SafeNormalize(Vector2.Zero).RotatedBy(direction * Math.Asin(angleRatio)) * shotSpeed;

                Projectile.NewProjectile(npc.Center, shotVelocity, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, shotVelocity.RotatedBy(0.15f), ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, shotVelocity.RotatedBy(-0.15f), ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, -shotVelocity, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, -shotVelocity.RotatedBy(0.15f), ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, -shotVelocity.RotatedBy(-0.15f), ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
            }

            if (npc.ai[1] >= 90 && npc.ai[1] % shotPeriod == 90 % shotPeriod)
            {
                Main.PlaySound(SoundID.Item71, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 60 + shotPeriod * numShots)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void IceSpiralAttackHard()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else
            {
                npc.velocity = Vector2.Zero;

                if (npc.ai[1] == 60 || npc.ai[1] == 240 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShield>(), 80, 0f, Main.myPlayer);
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<RotatingIceShards>(), 80, 0f, Main.myPlayer, ai0: i, ai1: 0f);
                    }
                }

                if (npc.ai[1] > 120 && npc.ai[1] < 600 && npc.ai[1] % 10 == 0)
                {
                    Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.5f, ModContent.ProjectileType<IceShard>(), 80, 0f, Main.myPlayer, ai0: 1.04f);
                }

                if (npc.ai[1] == 60 || npc.ai[1] == 240)
                {
                    Main.PlaySound(29, npc.Center, 88);
                }
                if (npc.ai[1] == 120 || npc.ai[1] == 300)
                {
                    Main.PlaySound(SoundID.Item120, npc.Center);
                }

                if (npc.ai[1] % 10 == 0 && npc.ai[1] < 600 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShardArena>(), 80, 0f, Main.myPlayer, ai0: 1);
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShardArena>(), 80, 0f, Main.myPlayer, ai0: -1);
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 720)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void ShieldBonkHard()
        {
            int dashTime = 24;
            int downTime = 51;
            int numDashes = 4;

            Player player = Main.player[npc.target];

            holdingShield = true;

            if (npc.ai[1] < 60 || (npc.ai[1] - 60) % (dashTime + downTime) > dashTime)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 180;

                FlyToPoint(goalPosition, player.velocity, 4f, 2f);
            }
            else if ((npc.ai[1] - 60) % (dashTime + downTime) == 0)
            {
                canShieldBonk = true;

                npc.width = 40;
                npc.position.X -= 11;

                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                npc.velocity.X += npc.direction * 15;
                npc.velocity.Y = player.velocity.Y / 2;
            }
            else if ((npc.ai[1] - 60) % (dashTime + downTime) == dashTime)
            {
                if (canShieldBonk)
                {
                    npc.width = 18;
                    npc.position.X += 11;
                }

                canShieldBonk = false;

                npc.velocity.X -= npc.direction * 15;
            }
            else
            {
                //spawn moths
                if (npc.ai[1] % 4 == 0 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(player.Center + new Vector2(-npc.spriteDirection * 2048 + player.velocity.X * 64f, player.velocity.Y * 64f), new Vector2(npc.spriteDirection * 32, 0), ModContent.ProjectileType<MothProjectile>(), 80, 0f, Main.myPlayer);
                }
            }

            //custom stuff for player EoC shield bonks
            //adapted from how the player detects SoC collision
            if (canShieldBonk)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && Main.player[i].dash == 2 && Main.player[i].eocDash > 0 && Main.player[i].eocHit < 0)
                    {
                        Rectangle shieldHitbox = new Rectangle((int)((double)Main.player[i].position.X + (double)Main.player[i].velocity.X * 0.5 - 4.0), (int)((double)Main.player[i].position.Y + (double)Main.player[i].velocity.Y * 0.5 - 4.0), Main.player[i].width + 8, Main.player[i].height + 8);
                        if (shieldHitbox.Intersects(npc.getRect()))
                        {
                            //custom stuff for player EoC shield bonks
                            //adapted from how the player detects SoC collision
                            npc.width = 18;
                            npc.position.X += 11;

                            npc.direction *= -1;
                            npc.velocity.X += npc.direction * 30;
                            canShieldBonk = false;

                            Main.PlaySound(SoundID.NPCHit4, npc.Center);

                            //redo the player's SoC bounce motion
                            int num40 = Main.player[i].direction;
                            if (Main.player[i].velocity.X < 0f)
                            {
                                num40 = -1;
                            }
                            if (Main.player[i].velocity.X > 0f)
                            {
                                num40 = 1;
                            }
                            Main.player[i].eocDash = 10;
                            Main.player[i].dashDelay = 30;
                            Main.player[i].velocity.X = -num40 * 9;
                            Main.player[i].velocity.Y = -4f;
                            Main.player[i].immune = true;
                            Main.player[i].immuneNoBlink = true;
                            Main.player[i].immuneTime = 4;
                            Main.player[i].eocHit = i;

                            break;
                        }
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 60 + numDashes * (dashTime + downTime))
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SlimeBonkHard()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(0, -240) + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else
            {
                bool justBounced = false;

                if (npc.ai[1] == 60)
                {
                    justBounced = true;

                    Main.PlaySound(SoundID.Item81, npc.Center);

                    onSlimeMount = true;

                    if (npc.velocity.Y < 0) npc.velocity.Y /= 2;
                    npc.velocity.X = player.velocity.X;

                    npc.width = 40;
                    npc.position.X -= 11;
                    npc.height = 64;
                    drawOffsetY = -15;

                    //mount dusts from slime mount
                    for (int i = 0; i < 100; i++)
                    {
                        int num2 = Dust.NewDust(new Vector2(npc.position.X - 20f, npc.position.Y), npc.width + 40, npc.height, 56);
                        Main.dust[num2].scale += (float)Main.rand.Next(-10, 21) * 0.01f;
                        Main.dust[num2].noGravity = true;
                        Dust obj2 = Main.dust[num2];
                        obj2.velocity += npc.velocity * 0.8f;
                    }
                }
                else if (onSlimeMount)
                {
                    if (npc.ai[1] == 599)
                    {
                        npc.velocity = player.velocity;
                        onSlimeMount = false;

                        npc.width = 18;
                        npc.position.X += 11;
                        npc.height = 40;
                        drawOffsetY = -5;

                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<FishronPlatform>() && Main.projectile[i].ai[1] == 0)
                            {
                                Main.projectile[i].ai[1] = 1;
                            }
                            else if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<FloatingBubble>() && Main.projectile[i].ai[1] < 0)
                            {
                                Main.projectile[i].ai[0] = 1f;
                                Main.projectile[i].ai[1] = -0.2f;
                            }
                        }
                    }
                    else if (npc.velocity.Y < 0 || npc.Hitbox.Top - 240 < player.Hitbox.Bottom)
                    {
                        npc.velocity.Y += 0.9f;

                        npc.direction = npc.velocity.X > 0 ? -1 : 1;
                        npc.spriteDirection = -npc.direction;
                    }
                    else
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<FishronPlatform>() && Main.projectile[i].ai[1] == 0)
                            {
                                Main.projectile[i].Kill();
                                break;
                            }
                        }

                        npc.velocity.Y = Math.Min(-36f, npc.velocity.Y - 36f);
                        npc.velocity.X = player.velocity.X + (player.Center.X - npc.Center.X) / 60f;

                        npc.direction = npc.velocity.X > 0 ? -1 : 1;
                        npc.spriteDirection = -npc.direction;

                        justBounced = true;
                    }
                }
                else
                {
                    npc.velocity *= 0.98f;
                }

                //summon bigger fish
                if (justBounced)
                {
                    if (npc.ai[1] <= 540 && Main.netMode != 1)
                    {
                        float determinant = Math.Max(0, (npc.velocity.Y - player.velocity.Y) * (npc.velocity.Y - player.velocity.Y) - 4 * 0.45f * (npc.Center.Y - player.Center.Y + 240));
                        float eta = Math.Max(3, (-(npc.velocity.Y - player.velocity.Y) + (float)Math.Sqrt(determinant)) / 0.9f);
                        float speed = 28f;
                        Vector2 targetPoint = new Vector2(npc.Center.X + npc.velocity.X * eta, player.Center.Y + 240 + player.velocity.Y * eta);
                        Vector2 shotPosition = targetPoint + new Vector2(npc.direction * eta * speed, 0);
                        Vector2 shotVelocity = (targetPoint - shotPosition) / eta;

                        Projectile.NewProjectile(shotPosition, shotVelocity, ModContent.ProjectileType<FishronPlatform>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI);
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void MothronsAndLampAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 1080;

                FlyToPoint(goalPosition, player.velocity, 0.8f, 0.8f);
            }
            else
            {
                if (npc.ai[1] == 60)
                {
                    npc.ai[2] = npc.Center.X;
                    npc.ai[3] = npc.Center.Y;

                    Main.PlaySound(SoundID.Zombie, npc.Center + new Vector2(npc.direction, 0) * 1500, 104);
                    Main.LocalPlayer.GetModPlayer<DTPlayer>().screenShakeTime = 60;

                    if (Main.netMode != 1)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<SigilArena>(), 80, 0f, Main.myPlayer, ai0: 600);
                        Projectile.NewProjectile(npc.Center + new Vector2(npc.direction, 0) * 1500, new Vector2(-npc.direction * 5, 0), ModContent.ProjectileType<SunLamp>(), 80, 0f, Main.myPlayer, ai0: 1f);
                    }
                }

                //mothron singing sound
                if (npc.ai[1] == 120)
                {
                    Main.PlaySound(SoundID.Zombie, (int)npc.Center.X, (int)npc.Center.Y, 73, volumeScale: 2f, pitchOffset: -1);
                }

                int period = 60;
                float number = 10;

                if (Main.netMode != 1 && npc.ai[1] >= 90 && npc.ai[1] <= 480 && npc.ai[1] % period == 0)
                {
                    Vector2 arenaCenter = new Vector2(npc.ai[2], npc.ai[3]);

                    //ray X position
                    float relativeRayPosition = npc.direction * 1500 - npc.direction * 5 * (npc.ai[1] + 30 - 60);

                    //angle of the arena still available
                    float availableAngle = (float)Math.Acos(relativeRayPosition / 1200f);
                    if (npc.direction == 1)
                    {
                        availableAngle = MathHelper.Pi - availableAngle;
                    }
                    if (availableAngle > MathHelper.Pi / 2)
                    {
                        availableAngle = MathHelper.Pi / 2;
                    }

                    float availableHeight = (float)Math.Sqrt(1200f * 1200f - relativeRayPosition * relativeRayPosition);

                    for (int i = 0; i < number; i++)
                    {
                        float angleModifier = ((2 * i - (number - 1)) / number + ((npc.ai[1] / 1.618f / period % 1) * 2 - 1) / number);
                        angleModifier = (angleModifier * angleModifier * angleModifier + angleModifier) / 2;

                        float shotAngle = -npc.direction * availableAngle * angleModifier;
                        float goalHeight = arenaCenter.Y + -npc.direction * shotAngle / availableAngle * availableHeight;

                        Projectile.NewProjectile(arenaCenter + new Vector2(-1800f * npc.direction, 0f).RotatedBy(shotAngle), new Vector2(32f * npc.direction, 0).RotatedBy(shotAngle), ModContent.ProjectileType<MothronProjectile>(), 80, 0f, Main.myPlayer, ai0: goalHeight, ai1: npc.direction * 1500 - npc.direction * 5 * (npc.ai[1] - 60) + arenaCenter.X);
                    }
                }

                Vector2 goalPosition = new Vector2(npc.ai[2], npc.ai[3]) + new Vector2(-npc.direction, 0) * 1080;

                FlyToPoint(goalPosition, Vector2.Zero, 0.25f, 0.25f);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void HeavenPetAttackHard()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] == 60 && Main.netMode != 1)
            {
                int proj = Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<HeavenPetProjectile>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI, ai1: 0f);
                Main.projectile[proj].localAI[1] = npc.localAI[0];

                Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataLastPrism>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI, ai1: 0f);
            }
            Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

            npc.spriteDirection = npc.direction;

            FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void AncientDoomMinefield()
        {
            Player player = Main.player[npc.target];

            int numRings = 16;
            int period = 4;

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else if (((int)npc.ai[1] - 60) / period <= numRings)
            {
                npc.velocity = Vector2.Zero;

                //shoot inward-spiraling fireballs
                if (npc.ai[1] == 60)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBossFireballRing>(), 80, 0f, Main.myPlayer, ai0: 1200, ai1: 1);
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBossFireballRing>(), 80, 0f, Main.myPlayer, ai0: 1400, ai1: -1);
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBossFireballRing>(), 80, 0f, Main.myPlayer, ai0: 1600, ai1: 1);
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBossFireballRing>(), 80, 0f, Main.myPlayer, ai0: 1800, ai1: -1);
                }

                //make a whole boatload of mines in sequence
                if ((npc.ai[1] - 60) % period == 0 && Main.netMode != 1)
                {
                    int i = ((int)npc.ai[1] - 60) / period;

                    //ai0 is radius multiplier, ai1 is rotation
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBossMine>(), 80, 0f, Main.myPlayer, ai0: 0, ai1: i);
                }

                if (npc.ai[1] == 60)
                {
                    Main.PlaySound(29, npc.Center, 89);
                }
            }
            else
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 60;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.065f, maxYAcc: 0.065f);

                if (npc.ai[1] == (1 + numRings) * period + 60)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBusterSword>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI, ai1: 1f);
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 540)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void IceScythesAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.05f, maxYAcc: 0.05f);

                if (npc.ai[1] == 59)
                {
                    //RoD dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));

                    npc.Center = goalPosition;
                    npc.velocity = Vector2.Zero;

                    //RoD arrival dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));
                    teleportTime = 1f;
                }
            }
            else
            {
                npc.velocity = Vector2.Zero;

                if (npc.ai[1] == 60 || npc.ai[1] == 240 || npc.ai[1] == 420 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShield>(), 80, 0f, Main.myPlayer);
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<RotatingIceShards>(), 80, 0f, Main.myPlayer, ai0: i, ai1: 0f);
                    }
                }

                if (npc.ai[1] == 120 || npc.ai[1] == 300 || npc.ai[1] == 480 || npc.ai[1] == 660 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, npc.DirectionTo(player.Center) * 0.5f, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 2);
                    Projectile.NewProjectile(npc.Center, npc.DirectionTo(player.Center).RotatedBy(MathHelper.PiOver2) * 0.5f, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 2);
                    Projectile.NewProjectile(npc.Center, npc.DirectionTo(player.Center).RotatedBy(-MathHelper.PiOver2) * 0.5f, ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 2);
                }

                if (npc.ai[1] == 60 || npc.ai[1] == 240 || npc.ai[1] == 420)
                {
                    Main.PlaySound(29, npc.Center, 88);
                }
                if (npc.ai[1] == 120 || npc.ai[1] == 300 || npc.ai[1] == 480)
                {
                    Main.PlaySound(SoundID.Item120, npc.Center);
                }
                if (npc.ai[1] == 150 || npc.ai[1] == 330 || npc.ai[1] == 510 || npc.ai[1] == 690)
                {
                    Main.PlaySound(SoundID.Item71, npc.Center);
                }

                if (npc.ai[1] % 10 == 0 && npc.ai[1] < 780 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShardArena>(), 80, 0f, Main.myPlayer, ai0: 1);
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<IceShardArena>(), 80, 0f, Main.myPlayer, ai0: -1);
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 900)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void AncientDoomMinefieldHard()
        {
            Player player = Main.player[npc.target];

            int numRings = 9;
            int period = 8;

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.05f, maxYAcc: 0.05f);

                if (npc.ai[1] == 59)
                {
                    //RoD dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));

                    npc.Center = goalPosition;
                    npc.velocity = Vector2.Zero;

                    //RoD arrival dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));
                    teleportTime = 1f;
                }
            }
            else if (((int)npc.ai[1] - 60) / period <= numRings)
            {
                npc.velocity = Vector2.Zero;

                if (npc.ai[1] == 60 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<SigilArena>(), 80, 0f, Main.myPlayer, ai0: 600);
                }

                //make a whole boatload of mines in sequence
                if ((npc.ai[1] - 60) % period == 0 && Main.netMode != 1)
                {
                    int i = ((int)npc.ai[1] - 60) / period;

                    //ai0 is radius multiplier, ai1 is rotation
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBossMine2>(), 80, 0f, Main.myPlayer, ai0: 0, ai1: i);
                }

                if (npc.ai[1] == 60)
                {
                    Main.PlaySound(29, npc.Center, 89);
                }
            }
            else
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 60;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.065f, maxYAcc: 0.065f);

                if (npc.ai[1] == (1 + numRings) * period + 60)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CataBusterSword>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI, ai1: 2f);
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 720)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void FishronsMothsAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.05f, maxYAcc: 0.05f);
            }
            else
            {
                if (npc.ai[1] == 60)
                {
                    Main.PlaySound(SoundID.Item81, npc.Center);

                    onSlimeMount = true;

                    npc.width = 40;
                    npc.position.X -= 11;
                    npc.height = 64;
                    drawOffsetY = -15;

                    //mount dusts from slime mount
                    for (int i = 0; i < 100; i++)
                    {
                        int num2 = Dust.NewDust(new Vector2(npc.position.X - 20f, npc.position.Y), npc.width + 40, npc.height, 56);
                        Main.dust[num2].scale += (float)Main.rand.Next(-10, 21) * 0.01f;
                        Main.dust[num2].noGravity = true;
                        Dust obj2 = Main.dust[num2];
                        obj2.velocity += npc.velocity * 0.8f;
                    }
                }

                npc.velocity.Y += 0.6f;

                if (npc.ai[1] % 120 == 30)
                {
                    npc.velocity.Y = -32f;
                }
                else if (npc.ai[1] % 120 == 60)
                {
                    float direction = player.velocity.X > 0 ? 1 : -1;

                    //RoD dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));

                    npc.position = player.Center + new Vector2(-direction * 480, -2430);
                    npc.velocity = Vector2.Zero;

                    //RoD arrival dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));
                    teleportTime = 1f;

                    if (Main.netMode != 1)
                    {
                        Projectile.NewProjectile(player.Center + new Vector2(direction * 2400, 0), new Vector2(-direction * 32, 0), ModContent.ProjectileType<DoomedFishron>(), 80, 0f, Main.myPlayer);
                        for (int i = 1; i <= 50; i++)
                        {
                            float yOffset = (float)(Math.Sqrt(1.5f * i - 0.5f)) * 256;
                            Projectile.NewProjectile(player.Center + new Vector2(direction * 2400 - 0.5f * direction * Math.Abs(yOffset), yOffset), new Vector2(-direction * 32, 0), ModContent.ProjectileType<MothProjectile>(), 80, 0f, Main.myPlayer);
                            Projectile.NewProjectile(player.Center + new Vector2(direction * 2400 - 0.5f * direction * Math.Abs(yOffset), -yOffset), new Vector2(-direction * 32, 0), ModContent.ProjectileType<MothProjectile>(), 80, 0f, Main.myPlayer);
                        }
                    }
                }
            }

            if (npc.ai[1] == 779)
            {
                onSlimeMount = false;

                npc.width = 18;
                npc.position.X += 11;
                npc.height = 40;
                drawOffsetY = -5;
            }

            npc.ai[1]++;
            if (npc.ai[1] == 780)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void MothronsAndLampCircularAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] <= 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 720;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.05f, maxYAcc: 0.05f);

                if (npc.ai[1] == 60)
                {
                    //RoD dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));

                    npc.Center = goalPosition;
                    npc.velocity = Vector2.Zero;

                    //RoD arrival dusts
                    Main.TeleportEffect(npc.Hitbox, 1, 0, MathHelper.Clamp(1f - teleportTime * 0.99f, 0.01f, 1f));
                    teleportTime = 1f;

                    npc.ai[2] = npc.Center.X;
                    npc.ai[3] = npc.Center.Y;
                }
            }

            //mothron singing sound
            if (npc.ai[1] == 120)
            {
                Main.PlaySound(SoundID.Zombie, (int)npc.Center.X, (int)npc.Center.Y, 73, volumeScale: 2f, pitchOffset: -1);
            }

            if (npc.ai[1] == 60 && Main.netMode != 1)
            {
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<SigilArena>(), 80, 0f, Main.myPlayer, ai0: 900);
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<CelestialLamp>(), 80, 0f, Main.myPlayer);
            }
            if (npc.ai[1] % 20 == 0 && npc.ai[1] >= 120 && npc.ai[1] < 900 && Main.netMode != 1)
            {
                for (int i = 0; i < 7; i++)
                {
                    float rotation = (float)Math.Sin(npc.ai[1] / 60f + npc.ai[1] * npc.ai[1] / 108000f) / 2f + i * MathHelper.TwoPi / 7;

                    Projectile.NewProjectile(new Vector2(npc.ai[2], npc.ai[3]) + new Vector2(1200, 960).RotatedBy(rotation), new Vector2(0, -32).RotatedBy(rotation), ModContent.ProjectileType<MothronSpiralProjectile>(), 80, 0f, Main.myPlayer, ai0: 1);
                    Projectile.NewProjectile(new Vector2(npc.ai[2], npc.ai[3]) + new Vector2(1200, -960).RotatedBy(rotation), new Vector2(0, 32).RotatedBy(rotation), ModContent.ProjectileType<MothronSpiralProjectile>(), 80, 0f, Main.myPlayer, ai0: -1);
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 1740)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void MegaSprocketVsMegaBaddySuperCinematicDesperationAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.localAI[0] == 0)
            {
                if (Main.rand.Next(1920) < npc.ai[1])
                    Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<ShadowDust>());
            }

            if (npc.ai[1] == 60)
            {
                npc.ai[2] = player.Center.X;
                npc.ai[3] = player.Center.Y;

                Main.LocalPlayer.AddBuff(ModContent.BuffType<Buffs.MysteriousPresence>(), 1560);

                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(new Vector2(npc.ai[2], npc.ai[3]), Vector2.Zero, ModContent.ProjectileType<MegaBaddy>(), 80, 0f, Main.myPlayer, ai0: player.whoAmI);
                }
            }
            else if (npc.ai[1] == 120 && Main.netMode != 1)
            {
                int proj = Projectile.NewProjectile(npc.Center, (new Vector2(npc.ai[2], npc.ai[3]) - npc.Center) / 59f, ModContent.ProjectileType<MegaSprocket>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI, ai1: 2f);
                Main.projectile[proj].localAI[1] = npc.localAI[0];
            }

            if (npc.ai[1] < 120)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else if (npc.ai[1] < 1620)
            {
                /*npc.direction = -player.direction;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = new Vector2(npc.ai[2], npc.ai[3]) * 2 - player.Center;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);*/
                
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = new Vector2(npc.ai[2], npc.ai[3]) + (new Vector2(npc.ai[2], npc.ai[3]) - player.Center).RotatedBy(- MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 50;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else
            {
                npc.velocity = Vector2.Zero;
            }

            npc.ai[1]++;
            if (npc.ai[1] == 1920)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return canShieldBonk || (onSlimeMount && npc.velocity.Y > target.velocity.Y);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (canShieldBonk)
            {
                npc.width = 18;
                npc.position.X += 11;

                npc.direction *= -1;
                npc.velocity.X += npc.direction * 30;
                canShieldBonk = false;
            }
            else if (onSlimeMount)
            {
                npc.velocity.Y = -24f;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<FishronPlatform>() && Main.projectile[i].ai[1] == 0)
                    {
                        Main.projectile[i].ai[1] = 1;

                        if (Main.netMode != 1)
                        {
                            float determinant = Math.Max(0, (npc.velocity.Y - target.velocity.Y) * (npc.velocity.Y - target.velocity.Y) - 4 * 0.45f * (npc.Center.Y - target.Center.Y + 240));
                            float eta = Math.Max(3, (-(npc.velocity.Y - target.velocity.Y) + (float)Math.Sqrt(determinant)) / 0.9f);
                            float speed = 28f;
                            Vector2 targetPoint = new Vector2(npc.Center.X + npc.velocity.X * eta, target.Center.Y + 240 + target.velocity.Y * eta);
                            Vector2 shotPosition = targetPoint + new Vector2(npc.direction * eta * speed, 0);
                            Vector2 shotVelocity = (targetPoint - shotPosition) / eta;

                            Projectile.NewProjectile(shotPosition, shotVelocity, ModContent.ProjectileType<FishronPlatform>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI);
                        }

                        break;
                    }
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            if (hitDialogueCooldown == 0)
            {
                CombatText.NewText(npc.getRect(), auraColor, "Save your energy, you can't hurt me.", true);
                hitDialogueCooldown = 120;
            }
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (hitDialogueCooldown == 0)
            {
                if (projectile.ranged)
                {
                    CombatText.NewText(npc.getRect(), auraColor, "Save your ammunition, it can't break my shield.", true);
                }
                else if (projectile.magic)
                {
                    CombatText.NewText(npc.getRect(), auraColor, "Wasting mana is all you're doing here.", true);
                }
                else if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type] || projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type])
                {
                    CombatText.NewText(npc.getRect(), auraColor, "Call off your minions, they won't target me.", true);
                }
                else if (projectile.thrown)
                {
                    CombatText.NewText(npc.getRect(), auraColor, "Throwing? Post-Moon Lord? Really?", true);
                }
                else
                {
                    CombatText.NewText(npc.getRect(), auraColor, "Save your energy, you can't hurt me.", true);
                }
                hitDialogueCooldown = 120;
            }
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage = 0;
            iceShieldCooldown = 60;
            return true;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            if (onSlimeMount)
            {
                npc.frameCounter = 0;
                npc.frame.Y = frameHeight * 5;
            }
            else
            {
                npc.frameCounter++;
                if (npc.frameCounter == 3)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;
                }
                if (npc.frame.Y >= frameHeight * 5)
                {
                    npc.frame.Y = 0;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            int trailLength = 1;
            if (canShieldBonk || onSlimeMount)
            {
                trailLength = 5;
            }

            for (int i = trailLength - 1; i >= 0; i--)
            {
                if (i == 0 || i % 2 == (int)npc.ai[1] % 2)
                {
                    float alpha = (trailLength - i) / (float)trailLength;
                    Vector2 center = npc.oldPos[i] + new Vector2(npc.width, npc.height) / 2;

                    SpriteEffects effects;

                    if (drawSpawnTransitionRing)
                    {
                        Color useColor = useRainbowColorTransition ? Main.hslToRgb((auraCounter / 120f) % 1, 1f, 0.5f) : spawnTransitionColor;

                        Texture2D ringTexture = ModContent.GetTexture("Terraria/Projectile_490");
                        Texture2D ringTexture2 = ModContent.GetTexture("Terraria/Extra_34");
                        Rectangle frame = ringTexture.Frame();
                        Rectangle frame2 = ringTexture2.Frame();
                        effects = SpriteEffects.None;

                        float rotation = (npc.ai[1] - 180) / 20f;
                        float alphaModifier = ((npc.ai[1] - 180) / 120f) * ((npc.ai[1] - 180) / 120f);
                        float scaleModifier = 1 - (npc.ai[1] - 180) / 120f;

                        for (int j = -1; j < 3; j++)
                        {
                            spriteBatch.Draw(ringTexture, center - Main.screenPosition, frame, useColor * alpha * alphaModifier * (float)Math.Pow(0.5, j), rotation, frame.Size() / 2f, scaleModifier * (float)Math.Pow(2, j), effects, 0f);
                            spriteBatch.Draw(ringTexture2, center - Main.screenPosition, frame2, useColor * alpha * alphaModifier * (float)Math.Pow(0.5, j - 0.5), -rotation, frame2.Size() / 2f, scaleModifier * (float)Math.Pow(2, j), effects, 0f);
                        }

                        Texture2D silhouetteTexture = mod.GetTexture("NPCs/CataBoss/CataBossSilhouette");
                        effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        Vector2 silhouetteOffset = new Vector2(-npc.spriteDirection * 3, drawOffsetY);

                        if (npc.localAI[0] == 0)
                        {
                            silhouetteTexture = mod.GetTexture("NPCs/CataBoss/ShadowCataBossSilhouette");
                        }

                        for (int j = 1; j <= 6; j++)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                Vector2 individualOffset = new Vector2(j * scaleModifier * 128f, 0).RotatedBy(k * MathHelper.TwoPi / 4 + j * MathHelper.TwoPi / 8);

                                spriteBatch.Draw(silhouetteTexture, center - Main.screenPosition + silhouetteOffset + individualOffset, npc.frame, useColor * alpha * alphaModifier * ((7 - j) / 6f) * 0.5f, 0f, npc.frame.Size() / 2f, 1f, effects, 0f);
                            }
                        }
                    }

                    if (drawAura)
                    {
                        Color useColor = useRainbowColorAura ? Main.hslToRgb((auraCounter / 120f) % 1, 1f, 0.5f) : auraColor;

                        Texture2D silhouetteTexture = mod.GetTexture("NPCs/CataBoss/CataBossSilhouette");
                        effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        Vector2 silhouetteOffset = new Vector2(-npc.spriteDirection * 3, drawOffsetY);

                        if (npc.localAI[0] == 0)
                        {
                            silhouetteTexture = mod.GetTexture("NPCs/CataBoss/ShadowCataBossSilhouette");
                        }

                        for (int k = 0; k < 8; k++)
                        {
                            Vector2 individualOffset = new Vector2(4, 0).RotatedBy(k * MathHelper.TwoPi / 8 + auraCounter / 20f);

                            spriteBatch.Draw(silhouetteTexture, center - Main.screenPosition + silhouetteOffset + individualOffset, npc.frame, useColor * alpha * 0.5f, 0f, npc.frame.Size() / 2f, 1f, effects, 0f);
                        }
                    }

                    if (onSlimeMount)
                    {
                        Texture2D mountTexture = ModContent.GetTexture("Terraria/Mount_Slime");
                        Rectangle frame = mountTexture.Frame(1, 4, 0, 1);
                        effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                        Vector2 mountOffset = new Vector2(npc.spriteDirection * 0, 10);
                        spriteBatch.Draw(mountTexture, center - Main.screenPosition + mountOffset, frame, Color.White * alpha, 0f, frame.Size() / 2f, 1f, effects, 0f);
                    }

                    Texture2D npcTexture = Main.npcTexture[npc.type];

                    if (npc.localAI[0] == 0)
                    {
                        npcTexture = mod.GetTexture("NPCs/CataBoss/ShadowCataBoss");
                    }

                    effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    Vector2 npcOffset = new Vector2(-npc.spriteDirection * 3, drawOffsetY);
                    spriteBatch.Draw(npcTexture, center - Main.screenPosition + npcOffset, npc.frame, Color.White * alpha, 0f, npc.frame.Size() / 2f, 1f, effects, 0f);

                    if (holdingShield)
                    {
                        Texture2D shieldTexture = ModContent.GetTexture("Terraria/Acc_Shield_5");
                        Rectangle frame = shieldTexture.Frame(1, 20);
                        effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                        Vector2 shieldOffset = new Vector2(npc.spriteDirection * 3, -4);

                        spriteBatch.Draw(shieldTexture, center - Main.screenPosition + shieldOffset, frame, Color.White * alpha, 0f, frame.Size() / 2f, 1f, effects, 0f);
                    }

                    if (drawEyeTrail)
                    {
                        float eyeTrailLength = 20;
                        Texture2D eyeTexture = mod.GetTexture("NPCs/CataBoss/CataBossEyeGlow");
                        Rectangle frame = eyeTexture.Frame();
                        effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        Vector2 eyeOffset = new Vector2(npc.spriteDirection * 4, drawOffsetY - 12);
                        Color useColor = useRainbowColorAura ? Main.hslToRgb((auraCounter / 120f) % 1, 1f, 0.5f) : auraColor;

                        for (int j=0; j<eyeTrailLength; j++)
                        {
                            float scale = (eyeTrailLength - j) / eyeTrailLength;
                            spriteBatch.Draw(eyeTexture, npc.oldPos[i + j] + new Vector2(npc.width, npc.height) / 2 - Main.screenPosition + eyeOffset, frame, useColor * (alpha * scale * 0.75f), 0f, frame.Size() / 2f, scale, effects, 0f);
                            float scale2 = (eyeTrailLength - j - 0.5f) / eyeTrailLength;
                            spriteBatch.Draw(eyeTexture, (npc.oldPos[i + j] + npc.oldPos[i + j + 1]) / 2 + new Vector2(npc.width, npc.height) / 2 - Main.screenPosition + eyeOffset, frame, useColor * (alpha * scale2 * 0.75f), 0f, frame.Size() / 2f, scale2, effects, 0f);
                        }
                    }

                    //draw RoD
                    if (teleportTime > 0.9f)
                    {
                        Texture2D rodTexture = ModContent.GetTexture("Terraria/Item_"+ItemID.RodofDiscord);
                        Rectangle frame = rodTexture.Frame();
                        effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        Vector2 drawCenter = npc.spriteDirection == 1 ? new Vector2(frame.Width, frame.Height) : new Vector2(0, frame.Height);
                        Vector2 rodOffset = new Vector2(npc.spriteDirection * 0, 0);
                        float rodRotation = -npc.spriteDirection * (teleportTime * 10 - 9.5f) * MathHelper.Pi + npc.spriteDirection * MathHelper.PiOver2;

                        spriteBatch.Draw(rodTexture, center - Main.screenPosition + rodOffset, frame, Color.White * alpha, rodRotation, drawCenter, 1f, effects, 0f);
                    }

                    if (iceShieldCooldown > 0)
                    {
                        Texture2D shieldTexture = ModContent.GetTexture("Terraria/Projectile_464");
                        Rectangle frame = shieldTexture.Frame();
                        effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                        float shieldAlpha = iceShieldCooldown / 120f;
                        Vector2 shieldOffset = new Vector2(0, -2);

                        spriteBatch.Draw(shieldTexture, center - Main.screenPosition + shieldOffset, frame, Color.White * shieldAlpha * alpha, 0f, frame.Size() / 2f, 1f, effects, 0f);
                    }
                }
            }

            return false;
        }

        public override bool CheckDead()
        {
            if (killable)
            {
                //doesn't actually happen yet
                if (npc.localAI[0] == 1)
                {
                    DTWorld.DownedCataBoss = true;
                    NPC.NewNPC((int)npc.position.X, (int)npc.position.Y + npc.height / 2, ModContent.NPCType<CataclysmicArmageddon>());
                }
                return true;
            }
            npc.life = npc.lifeMax;
            return false;
        }

        public override bool CheckActive()
        {
            return false;
        }
    }

    public class CataBossScythe : ModProjectile
    {
        //demon scythe but no dust and it passes through tiles
        public override string Texture => "DeathsTerminus/NPCs/CataBoss/CataDemonScythe"; //"Terraria/Projectile_" + ProjectileID.DemonSickle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demon Scythe");
            
            /*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 64, 128, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = i / (float)(texture.Width - 1);
                    float y = j / (float)(texture.Height - 1);

                    int r = 255;
					int g = 255;
					int b = 255;
					int alpha = (int)(255 * (1 - x) * 4 * y * (1 - y));

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "CataBossScytheTelegraph.png", FileMode.Create), texture.Width, texture.Height);
            
            texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 64, 128, false, SurfaceFormat.Color);
			list = new System.Collections.Generic.List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = i / (float)(texture.Width - 1);
                    float y = j / (float)(texture.Height - 1);

                    float radiusSquared = (1 + x * x + 4 * y * (y - 1));

                    int r = 255;
					int g = 255;
					int b = 255;
					int alpha = radiusSquared > 1 ? 0 : (int)(255 * (1 - radiusSquared));

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "CataBossTelegraphCap.png", FileMode.Create), texture.Width, texture.Height);*/
        }

        public override void SetDefaults()
        {
            projectile.width = 42; //48;
            projectile.height = 42; //48;
            projectile.alpha = 32;
            projectile.light = 0.2f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;//0.9f;
            projectile.timeLeft = 130;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation += projectile.direction * 0.8f;
            projectile.ai[0] += 1f;
            if (!(projectile.ai[0] < 30f))
            {
                if (projectile.ai[0] < 120f)
                {
                    projectile.velocity *= 1.06f;
                }
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] == 0)
            {
                spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Purple * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(projectile.velocity.Length(), projectile.width / 128f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Purple * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);
            }
            else if (projectile.ai[1] == 1)
            {
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataEclipseScythe"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Orange * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(projectile.velocity.Length(), projectile.width / 128f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Orange * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);
            }
            else if (projectile.ai[1] == 2)
            {
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataCelestialScythe"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.LightBlue * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(projectile.velocity.Length(), projectile.width / 128f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.LightBlue * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class CataBossSuperScythe : ModProjectile
    {
        //demon scythe but no dust and it passes through tiles
        public override string Texture => "DeathsTerminus/NPCs/CataBoss/CataDemonScythe"; //"Terraria/Projectile_" + ProjectileID.DemonSickle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Super Scythe");
        }

        public override void SetDefaults()
        {
            projectile.width = 42;//48;
            projectile.height = 42;// 48;
            projectile.alpha = 32;
            projectile.light = 0.2f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.scale = 1f;// 0.9f;
            projectile.timeLeft = 160;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation += projectile.direction * 0.8f;
            projectile.ai[0] += 1f;
            if (projectile.ai[0] == 30f)
            {
                projectile.velocity *= 60f;
            }

            if (projectile.ai[0] >= 30 && (projectile.ai[0] - 30) % 7 == 0 && Main.netMode != 1)
            {
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: projectile.ai[1]);
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: projectile.ai[1]);
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2) * 0.5f, ModContent.ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: projectile.ai[1]);
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] == 0)
            {
                spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Purple * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(30, projectile.width / 128f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Purple * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);
            }
            else if (projectile.ai[1] == 1)
            {
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataEclipseScythe"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Orange * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(30, projectile.width / 128f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Orange * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);
            }
            else if (projectile.ai[1] == 2)
            {
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataCelestialScythe"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.LightBlue * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(30, projectile.width / 128f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.LightBlue * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class RotatingIceShards : ModProjectile
    {
        public override string Texture => "Terraria/Extra_35";

        private static int shardRadius = 12;
        private static int shardCount = 24;

        //ring of ice shards
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shard");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 420;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft <= 360)
            {
                float angle = MathHelper.TwoPi * projectile.timeLeft / 360f;

                //set radius and rotation
                projectile.localAI[1] = 600 * (float)Math.Sqrt(2 - 2 * Math.Cos(angle));
                projectile.rotation = projectile.ai[1] + projectile.ai[0] * (angle + MathHelper.Pi) / 2;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.localAI[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < shardRadius)
                {
                    return true;
                }
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            for (int i = 0; i < shardCount; i++)
            {
                Rectangle frame = texture.Frame(1, 3);

                for (int j = Math.Min(projectile.timeLeft, 360); j >= Math.Max(0, projectile.timeLeft - 60); j--)
                {
                    float angle = MathHelper.TwoPi * j / 360f;
                    float radius = 600 * (float)Math.Sqrt(2 - 2 * Math.Cos(angle));
                    float rotation = projectile.ai[1] + projectile.ai[0] * (angle + MathHelper.Pi) / 2;
                    float alphaMultiplier = Math.Max(0, (60 - projectile.timeLeft + j) / 60f);

                    spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(radius * projectile.scale, 0).RotatedBy(rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White * alphaMultiplier * 0.03f, rotation + i * MathHelper.TwoPi / shardCount - MathHelper.PiOver2 + projectile.ai[0] * MathHelper.Pi * (1 + j / 360f), new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
                }

                if (projectile.timeLeft <= 360)
                    spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.localAI[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White, projectile.rotation + i * MathHelper.TwoPi / shardCount - MathHelper.PiOver2 + projectile.ai[0] * MathHelper.Pi * (1 + projectile.timeLeft / 360f), new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class IceShardArena : ModProjectile
    {
        public override string Texture => "Terraria/Extra_35";

        private static int shardRadius = 12;
        private static int shardCount = 24;

        //ring of ice shards
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shard");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 200;

            projectile.hide = true;
        }

        public override void AI()
        {
            //initial position = new Vector2(1200, ±2400)
            //velocity is new vector2(0, ±24)

            //set radius and rotation
            Vector2 positionOffset = new Vector2(1200, projectile.ai[0] * 2400) + new Vector2(0, -projectile.ai[0] * 24) * (200 - projectile.timeLeft);

            projectile.localAI[1] = positionOffset.Length();
            projectile.rotation = positionOffset.ToRotation();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.localAI[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < shardRadius)
                {
                    return true;
                }
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            for (int i = 0; i < shardCount; i++)
            {
                Rectangle frame = texture.Frame(1, 3);

                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.localAI[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White, MathHelper.PiOver2 * (projectile.ai[0] + 1) + i * MathHelper.TwoPi / shardCount, new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class IceShard : ModProjectile
    {
        public override string Texture => "Terraria/Extra_35";

        private static int shardRadius = 12;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shard");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 200;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.velocity *= projectile.ai[0];

            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 circleCenter = projectile.Center;
            float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < shardRadius;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 3);

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class IceShield : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_464";

        private static int shardRadius = 12;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shield");
            Main.projFrames[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.width = 60;
            projectile.height = 60;
            projectile.alpha = 96;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 60;
        }

        public override void AI()
        {
            projectile.ai[0] += 0.01f;
            projectile.rotation += projectile.ai[0];
            projectile.alpha = projectile.timeLeft * 128 / 60;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 circleCenter = projectile.Center;
            float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < projectile.width / 2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame();

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(46, 51), projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class SigilArena : ModProjectile
    {
        private static int sigilRadius = 27;
        private static int sigilCount = 80;


        //the arena!
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbiting Sigil");
            Main.projFrames[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.scale = 2f;
            projectile.timeLeft = 600;
        }

        public override void AI()
        {
            if (projectile.localAI[0] == 0)
            {
                projectile.timeLeft = (int)projectile.ai[0];
            }

            projectile.scale = 1f;
            projectile.hostile = true;

            //rotation increment
            projectile.rotation += 0.01f;

            //set radius and center (replace with more dynamic AI later)
            projectile.ai[1] = 1200 + Math.Max(0, 20 * (60 - projectile.localAI[0])) + Math.Max(0, 20 * (60 - projectile.timeLeft));

            if (projectile.scale >= 1)
            {
                if ((Main.LocalPlayer.Center - projectile.Center).Length() > projectile.ai[1] * projectile.scale)
                {
                    Vector2 normal = (Main.LocalPlayer.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 relativeVelocity = Main.LocalPlayer.velocity - projectile.velocity;

                    Main.LocalPlayer.Center = projectile.Center + normal * projectile.ai[1] * projectile.scale;

                    if (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y > 0)
                    {
                        Main.LocalPlayer.velocity -= normal * (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y);
                    }
                }
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 4)
                {
                    projectile.frame = 0;
                }
            }

            projectile.localAI[0]++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < sigilCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < sigilRadius)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            for (int i = 0; i < sigilCount; i++)
            {
                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount), new Rectangle(0, 96 * projectile.frame, 66, 96), Color.White * projectile.scale, projectile.rotation + i * MathHelper.TwoPi / sigilCount, new Vector2(33, 65), projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class SunLamp : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sun Lamp");

            /*
            int textureFramesX = 15;
            int textureFramesY = 4;
            int textureFrameWidth = 96;
            int textureFrameHeight = 603;
            Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 2 * textureFramesX * textureFrameWidth, textureFramesY * textureFrameHeight, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
                    if (i < texture.Width / 2)
                    {
                        int frameX = i / textureFrameWidth;
                        int frameY = j / textureFrameHeight;
                        int frame = frameX + frameY * textureFramesX;
                        float x = Math.Abs(2 * (i % textureFrameWidth) / (float)(textureFrameWidth - 1) - 1);
                        float y = MathHelper.TwoPi * (j % textureFrameHeight) / (float)textureFrameHeight;

                        float waveFunction = (float)(
                                1 / 4f * Math.Cos(12 * (y + 12f) + frame * MathHelper.TwoPi / 12 + 12f) +
                                1 / 4f * Math.Cos(-15 * (y + 15f) + frame * MathHelper.TwoPi / 15 + 15f) +
                                1 / 4f * Math.Cos(-20 * (y + 20f) + frame * MathHelper.TwoPi / 20 + 20f) +
                                1 / 4f * Math.Cos(30 * (y + 30f) + frame * MathHelper.TwoPi / 30 + 30f)
                            );

                        float luminosityFactor = (float)Math.Pow((1 - Math.Pow(x, 4)), 2);
                        float waviness = (float)(0.5f * Math.Exp(-50 * Math.Pow(x - 0.8f, 2)));
                        float index = luminosityFactor * (1 - waviness * waveFunction);

                        int r = 255;
                        int g = 255 - (int)(64 * (1 - index));
                        int b = 255 - (int)(255 * (1 - index));
                        int alpha = (int)(255 * index);

                        list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
                    }
                    else
                    {
                        int frameX = (i - texture.Width) / textureFrameWidth;
                        int frameY = j / textureFrameHeight;
                        int frame = frameX + frameY * textureFramesX;
                        float x = Math.Abs(2 * (i % textureFrameWidth) / (float)(textureFrameWidth - 1) - 1);
                        float y = MathHelper.TwoPi * (j % textureFrameHeight) / (float)textureFrameHeight;

                        float waveFunction = (float)(
                                1 / 4f * Math.Cos(12 * (y + 12f) + frame * MathHelper.TwoPi / 12 + 12f) +
                                1 / 4f * Math.Cos(-15 * (y + 15f) + frame * MathHelper.TwoPi / 15 + 15f) +
                                1 / 4f * Math.Cos(-20 * (y + 20f) + frame * MathHelper.TwoPi / 20 + 20f) +
                                1 / 4f * Math.Cos(30 * (y + 30f) + frame * MathHelper.TwoPi / 30 + 30f)
                            );

                        float luminosityFactor = (float)Math.Pow((1 - Math.Pow(x, 4)), 2);
                        float waviness = (float)(0.5f * Math.Exp(-50 * Math.Pow(x - 0.8f, 2)));
                        float index = luminosityFactor * (1 - waviness * waveFunction);
                        float eclipseLuminosityFactor = (float)Math.Pow(1 - Math.Pow(1 - Math.Pow(x, 2), 64), Math.Pow(64, 4));

                        float hue = (index / 4f - 1 / 12f) % 1;
                        float saturation = (float)Math.Pow(eclipseLuminosityFactor, 2);
                        float luminosity = index * eclipseLuminosityFactor;
                        Color color = Main.hslToRgb(hue, saturation, luminosity);

                        int r = color.R;//448 - (int)(512 * index);
                        int g = color.G;//384 - (int)(512 * index);
                        int b = color.B;//256 - (int)(512 * index);
                        int alpha = x >= 1 ? 0 : (int)(255 * index);

                        list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
                    }
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "SunLamp.png", FileMode.Create), texture.Width, texture.Height);
            */
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 540;
            projectile.scale = 0f;

            projectile.hide = true;
        }

        public override void AI()
        {
            DelegateMethods.v3_1 = new Vector3(255 / 128f, 220 / 128f, 64 / 128f);
            Utils.PlotTileLine(projectile.Center + new Vector2(0, 2048), projectile.Center + new Vector2(0, -2048), 26, DelegateMethods.CastLight);

            if (projectile.scale < 1f && projectile.timeLeft > 60)
            {
                projectile.scale += 1 / 60f;
            }
            else if (projectile.timeLeft <= 60)
            {
                projectile.scale -= 1 / 60f;
                projectile.velocity.X *= 0.95f;
            }

            projectile.direction = projectile.velocity.X > 0 ? 1 : -1;

            if (projectile.timeLeft > 60)
            {
                if (projectile.Center.X < Main.LocalPlayer.Center.X ^ projectile.direction == 1)
                {
                    Vector2 normal = new Vector2(projectile.direction, 0);
                    Vector2 relativeVelocity = Main.LocalPlayer.velocity - projectile.velocity;

                    Main.LocalPlayer.Center = new Vector2(projectile.Center.X, Main.LocalPlayer.Center.Y);

                    if (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y > 0)
                    {
                        Main.LocalPlayer.velocity -= normal * (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y);
                    }
                }
            }

            projectile.frame++;
            if ((projectile.frame < 60 || projectile.frame >= 120) && projectile.ai[0] == 1)
            {
                projectile.frame = 60;
            }
            else if (projectile.frame >= 60 && projectile.ai[0] == 0)
            {
                projectile.frame = 0;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center + new Vector2(0, 2048), projectile.Center + new Vector2(0, -2048), 64 * projectile.scale, ref point);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        //need to make wobble
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle frame = texture.Frame(30, 4, projectile.frame / 4, projectile.frame % 4);

            for (int i = (int)((-Main.screenHeight + Main.screenPosition.Y - projectile.Center.Y) / frame.Height - 1); i <= (int)((Main.screenHeight + Main.screenPosition.Y - projectile.Center.Y) / frame.Height + 1); i++)
            {
                spriteBatch.Draw(texture, new Vector2(0, frame.Height * i) + projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, frame.Size() / 2, new Vector2(projectile.scale, 1), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class BabyMothronProjectile : ModProjectile
    {
        public override string Texture => "Terraria/NPC_479";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Baby Mothron");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 400;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft > 400 - 33)
            {
                projectile.velocity *= 0.95f;
            }
            else
            {
                projectile.velocity.Y += projectile.velocity.Y * projectile.velocity.Y / (projectile.Center.Y - projectile.ai[0]);

                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 6;
            }
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
            projectile.direction = projectile.velocity.X > 0 ? -1 : 1;
            projectile.spriteDirection = projectile.direction;

            //test for death via ray of sunshine
            projectile.ai[1] += 5 * projectile.direction;
            if (projectile.ai[1] < projectile.Center.X ^ projectile.direction == 1)
            {
                projectile.Kill();
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 4)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 3)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCHit23, projectile.Center);

            Gore.NewGore(projectile.position, projectile.velocity, 681, projectile.scale);
            Gore.NewGore(projectile.position, projectile.velocity, 682, projectile.scale);
            Gore.NewGore(projectile.position, projectile.velocity, 683, projectile.scale);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 3, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, frame.Size() / 2, projectile.scale, effects, 0f);

            return false;
        }
    }

    public class HeavenPetProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sprocket");
            Main.projFrames[projectile.type] = 7;
        }

        private float cogRotation = 0;

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.alpha = 0;
            projectile.light = 1f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 660;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.scale = projectile.ai[1] + 1f;

            if (projectile.timeLeft > 60)
            {
                Player player = Main.player[Main.npc[(int)projectile.ai[0]].target];

                if (projectile.timeLeft <= 600 - 60)
                {
                    projectile.hostile = true;

                    projectile.velocity -= new Vector2(0.25f, 0).RotatedBy(projectile.rotation) / projectile.scale;
                    projectile.velocity *= 0.95f;

                    if (projectile.timeLeft == 600 - 60)
                    {
                        Main.PlaySound(SoundID.Item122, projectile.Center);
                    }
                    else if (projectile.timeLeft % 60 == 0)
                    {
                        Main.PlaySound(SoundID.Item15, projectile.Center);
                    }
                }
                else
                {
                    projectile.hostile = false;

                    projectile.velocity -= new Vector2(0.1f, 0).RotatedBy(projectile.rotation);
                    projectile.velocity *= 0.95f;
                }

                float maxTurn = 0.02f / projectile.scale;

                float rotationOffset = (player.Center - projectile.Center).ToRotation() - projectile.rotation;
                while (rotationOffset > MathHelper.Pi)
                {
                    rotationOffset -= MathHelper.TwoPi;
                }
                while (rotationOffset < -MathHelper.Pi)
                {
                    rotationOffset += MathHelper.TwoPi;
                }
                if (rotationOffset > maxTurn)
                {
                    projectile.rotation += maxTurn;
                }
                else if (rotationOffset < -maxTurn)
                {
                    projectile.rotation -= maxTurn;
                }
                else
                {
                    projectile.rotation = (player.Center - projectile.Center).ToRotation();
                }
            }
            else
            {
                projectile.hostile = false;

                NPC boss = Main.npc[(int)projectile.ai[0]];

                projectile.velocity += (boss.Center + boss.velocity * projectile.timeLeft - projectile.Center - projectile.velocity * projectile.timeLeft) / (projectile.timeLeft * projectile.timeLeft);
            }

            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 7)
                {
                    projectile.frame = 0;
                }
            }

            cogRotation += projectile.velocity.X * 0.1f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + new Vector2(4096, 0).RotatedBy(projectile.rotation), 22 * projectile.scale, ref point);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame;
            SpriteEffects effects;

            //draw the pet
            Texture2D texture = Main.projectileTexture[projectile.type];
            if (projectile.localAI[1] == 0)
            {
                texture = mod.GetTexture("NPCs/CataBoss/DarkPetProjectile");
            }

            frame = texture.Frame(1, 7, 0, projectile.frame);
            effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            float wingRotation = projectile.velocity.X * 0.1f;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, wingRotation, frame.Size() / 2, projectile.scale, effects, 0f);

            Texture2D texture2 = mod.GetTexture("NPCs/CataBoss/HeavenPetProjectile_Cog");
            if (projectile.localAI[1] == 0)
            {
                texture2 = mod.GetTexture("NPCs/CataBoss/DarkPetProjectile_Cog");
            }

            frame = texture2.Frame();
            effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            spriteBatch.Draw(texture2, projectile.Center - Main.screenPosition, frame, Color.White, cogRotation, frame.Size() / 2, projectile.scale, effects, 0f);


            if (projectile.timeLeft > 60)
            {
                //draw the prism
                //adapted from last prism drawcode
                Texture2D prismTexture = ModContent.GetTexture("Terraria/Projectile_633");
                frame = prismTexture.Frame(1, 5, 0, (projectile.timeLeft / (projectile.timeLeft <= 600 - 60 ? 1 : 3)) % 5);
                effects = SpriteEffects.None;
                Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(20 * projectile.scale, 0).RotatedBy(projectile.rotation);

                spriteBatch.Draw(prismTexture, drawPosition, frame, Color.White, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);

                float scaleFactor2 = (float)Math.Cos(Math.PI * 2f * (projectile.timeLeft / 30f)) * 2f + 2f;
                if (projectile.timeLeft <= 600 - 60)
                {
                    scaleFactor2 = 4f;
                }
                for (float num350 = 0f; num350 < 4f; num350 += 1f)
                {
                    spriteBatch.Draw(prismTexture, drawPosition + new Vector2(0, 1).RotatedBy(num350 * (Math.PI * 2f) / 4f) * scaleFactor2, frame, Color.White.MultiplyRGBA(new Color(255, 255, 255, 0)) * 0.03f, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);
                }

                if (projectile.timeLeft > 600 - 60)
                {
                    //draw the telegraph line
                    float telegraphAlpha = (60 + projectile.timeLeft - 600) / 30f * (600 - projectile.timeLeft) / 30f;
                    spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/HeavenPetProjectileTelegraph"), projectile.Center + new Vector2(30 * projectile.scale, 0).RotatedBy(projectile.rotation) - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * telegraphAlpha, projectile.rotation, new Vector2(0, 0.5f), new Vector2(4096, 1), SpriteEffects.None, 0f);
                }
                else
                {
                    //draw the beam
                    //adapted from last prism drawcode

                    for (int i = 0; i < 6; i++)
                    {
                        //texture
                        Texture2D tex7 = ModContent.GetTexture("Terraria/Projectile_632");
                        //laser length
                        float num528 = 4096;

                        Color value42 = Main.hslToRgb(i / 6f, 1f, 0.5f);
                        value42.A = 0;

                        Vector2 drawOffset = new Vector2(4, 0).RotatedBy(projectile.timeLeft * 0.5f + i * MathHelper.TwoPi / 6);

                        //start position
                        Vector2 value45 = projectile.Center.Floor() + drawOffset + new Vector2(36 * projectile.scale, 0).RotatedBy(projectile.rotation);

                        value45 += Vector2.UnitX.RotatedBy(projectile.rotation) * projectile.scale * 10.5f;
                        num528 -= projectile.scale * 14.5f * projectile.scale;
                        Vector2 vector90 = new Vector2(projectile.scale);
                        DelegateMethods.f_1 = 1f;
                        DelegateMethods.c_1 = value42 * 0.75f * projectile.Opacity;
                        _ = projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
                        Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90, DelegateMethods.RainbowLaserDraw);
                        DelegateMethods.c_1 = new Color(255, 255, 255, 127) * 0.75f * projectile.Opacity;
                        Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90 / 2f, DelegateMethods.RainbowLaserDraw);
                    }
                }
            }

            return false;
        }
    }

    public class CataLastPrism : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_633";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Last Prism");
            Main.projFrames[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.alpha = 0;
            projectile.light = 1f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 600;
        }

        public override void AI()
        {
            projectile.scale = projectile.ai[1] + 1f;

            Player player = Main.player[Main.npc[(int)projectile.ai[0]].target];

            if (projectile.timeLeft <= 540 - 60)
            {
                projectile.hostile = true;

                projectile.Center = Main.npc[(int)projectile.ai[0]].Center;

                if (projectile.timeLeft == 540 - 60)
                {
                    Main.PlaySound(SoundID.Item122, projectile.Center);
                }
                else if (projectile.timeLeft % 60 == 0)
                {
                    Main.PlaySound(SoundID.Item15, projectile.Center);
                }
            }
            else
            {
                projectile.hostile = false;

                projectile.Center = Main.npc[(int)projectile.ai[0]].Center;
            }

            float maxTurn = 0.0175f / projectile.scale;

            float rotationOffset = (player.Center - projectile.Center).ToRotation() - projectile.rotation;
            while (rotationOffset > MathHelper.Pi)
            {
                rotationOffset -= MathHelper.TwoPi;
            }
            while (rotationOffset < -MathHelper.Pi)
            {
                rotationOffset += MathHelper.TwoPi;
            }
            if (rotationOffset > maxTurn)
            {
                projectile.rotation += maxTurn;
            }
            else if (rotationOffset < -maxTurn)
            {
                projectile.rotation -= maxTurn;
            }
            else
            {
                projectile.rotation = (player.Center - projectile.Center).ToRotation();
            }

            while (projectile.rotation >= MathHelper.TwoPi)
            {
                projectile.rotation -= MathHelper.TwoPi;
            }
            while (projectile.rotation < 0)
            {
                projectile.rotation += MathHelper.TwoPi;
            }

            Main.npc[(int)projectile.ai[0]].direction = (projectile.rotation > MathHelper.PiOver2 && projectile.rotation < 3 * MathHelper.PiOver2) ? -1 : 1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + new Vector2(4096, 0).RotatedBy(projectile.rotation), 44 * projectile.scale, ref point);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame;
            SpriteEffects effects;

            //draw the prism
            //adapted from last prism drawcode
            Texture2D prismTexture = Main.projectileTexture[projectile.type];
            frame = prismTexture.Frame(1, 5, 0, (projectile.timeLeft / (projectile.timeLeft <= 600 - 60 ? 1 : 3)) % 5);
            effects = SpriteEffects.None;
            Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(20 * projectile.scale, 0).RotatedBy(projectile.rotation);

            spriteBatch.Draw(prismTexture, drawPosition, frame, Color.White, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);

            float scaleFactor2 = (float)Math.Cos(Math.PI * 2f * (projectile.timeLeft / 30f)) * 2f + 2f;
            if (projectile.timeLeft <= 540 - 60)
            {
                scaleFactor2 = 4f;
            }
            for (float num350 = 0f; num350 < 4f; num350 += 1f)
            {
                spriteBatch.Draw(prismTexture, drawPosition + new Vector2(0, 1).RotatedBy(num350 * (Math.PI * 2f) / 4f) * scaleFactor2, frame, Color.White.MultiplyRGBA(new Color(255, 255, 255, 0)) * 0.03f, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);
            }

            if (projectile.timeLeft > 540 - 60)
            {
                //draw the telegraph line
                float telegraphAlpha = (60 + projectile.timeLeft - 540) / 30f * (540 - projectile.timeLeft) / 30f;
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/HeavenPetProjectileTelegraph"), projectile.Center + new Vector2(30 * projectile.scale, 0).RotatedBy(projectile.rotation) - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * telegraphAlpha, projectile.rotation, new Vector2(0, 0.5f), new Vector2(4096, 1), SpriteEffects.None, 0f);
            }
            else
            {
                //draw the beam
                //adapted from last prism drawcode

                for (int i = 0; i < 6; i++)
                {
                    //texture
                    Texture2D tex7 = ModContent.GetTexture("Terraria/Projectile_632");
                    //laser length
                    float num528 = 4096;

                    Color value42 = Main.hslToRgb(i / 6f, 1f, 0.5f);
                    value42.A = 0;

                    Vector2 drawOffset = new Vector2(4, 0).RotatedBy(projectile.timeLeft * 0.5f + i * MathHelper.TwoPi / 6);

                    //start position
                    Vector2 value45 = projectile.Center.Floor() + drawOffset + new Vector2(46 * projectile.scale, 0).RotatedBy(projectile.rotation);

                    value45 += Vector2.UnitX.RotatedBy(projectile.rotation) * projectile.scale * 10.5f;
                    num528 -= projectile.scale * 14.5f * projectile.scale;
                    Vector2 vector90 = new Vector2(projectile.scale * 2);
                    DelegateMethods.f_1 = 1f;
                    DelegateMethods.c_1 = value42 * 0.75f * projectile.Opacity;
                    _ = projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
                    Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90, DelegateMethods.RainbowLaserDraw);
                    DelegateMethods.c_1 = new Color(255, 255, 255, 127) * 0.75f * projectile.Opacity;
                    Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90 / 2f, DelegateMethods.RainbowLaserDraw);
                }
            }

            return false;
        }
    }

    public class FishronPlatform : ModProjectile
    {
        public override string Texture => "Terraria/NPC_370";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sitting Duke");
            Main.projFrames[projectile.type] = 8;

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        float bubbleShotProgress = 0f;
        int bubbleCount = 0;

        public override void SetDefaults()
        {
            projectile.width = 100;
            projectile.height = 100;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 400;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.ai[1] == 0)
            {
                NPC boss = Main.npc[(int)projectile.ai[0]];
                Player player = Main.player[boss.target];

                float determinant = Math.Max(0, (boss.velocity.Y - player.velocity.Y) * (boss.velocity.Y - player.velocity.Y) - 4 * 0.45f * (boss.Center.Y - player.Center.Y + 240));
                float eta = Math.Max(3, (-(boss.velocity.Y - player.velocity.Y) + (float)Math.Sqrt(determinant)) / 0.9f);
                Vector2 targetPoint = new Vector2(boss.Center.X + boss.velocity.X * eta, player.Center.Y + 240 + player.velocity.Y * eta + projectile.height);
                Vector2 targetVelocity = (targetPoint - projectile.Center) / eta;
                projectile.velocity += (targetVelocity - projectile.velocity) / 10f;

                bubbleShotProgress += Math.Abs(projectile.velocity.X);
                if (bubbleShotProgress >= 80)
                {
                    bubbleShotProgress = 0;
                    bubbleCount++;
                    if (bubbleCount == 4)
                    {
                        bubbleCount = 0;
                        if (Main.netMode != 1)
                            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 0.995f, ai1: 0.1f);
                    }
                    else
                    {
                        if (Main.netMode != 1)
                            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 0.95f, ai1: -0.08f);
                    }
                }
            }
            else
            {
                projectile.velocity.Y += 0.15f;
            }

            projectile.rotation = projectile.velocity.ToRotation();
            projectile.direction = projectile.velocity.X > 0 ? 1 : -1;
            projectile.spriteDirection = projectile.direction;

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 8)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            if (timeLeft > 0)
            {
                Main.PlaySound(SoundID.NPCHit14, projectile.Center);

                Gore.NewGore(projectile.Center - Vector2.UnitX * 20f * (float)projectile.direction, projectile.velocity, 576, projectile.scale);
                Gore.NewGore(projectile.Center - Vector2.UnitY * 30f, projectile.velocity, 574, projectile.scale);
                Gore.NewGore(projectile.Center, projectile.velocity, 575, projectile.scale);
                Gore.NewGore(projectile.Center + Vector2.UnitX * 20f * (float)projectile.direction, projectile.velocity, 573, projectile.scale);
                Gore.NewGore(projectile.Center - Vector2.UnitY * 30f, projectile.velocity, 574, projectile.scale);
                Gore.NewGore(projectile.Center, projectile.velocity, 575, projectile.scale);

                NPC boss = Main.npc[(int)projectile.ai[0]];

                for (int i = 0; i < 12; i++)
                {
                    Projectile.NewProjectile(projectile.Center, new Vector2(boss.velocity.X * 0.5f, 0) + new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / 8), ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 0.99f, ai1: 0.08f);
                    Projectile.NewProjectile(projectile.Center, new Vector2(boss.velocity.X * 0.5f, 0) + new Vector2(3, 0).RotatedBy((i + 0.5f) * MathHelper.TwoPi / 8), ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 0.99f, ai1: 0.08f);
                }
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 8, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale, effects, 0f);
            }

            return false;
        }
    }

    public class DoomedFishron : ModProjectile
    {
        public override string Texture => "Terraria/NPC_370";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Doomed Fishron");
            Main.projFrames[projectile.type] = 8;

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        float bubbleShotProgress = 0f;

        public override void SetDefaults()
        {
            projectile.width = 100;
            projectile.height = 100;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 90;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.direction = projectile.velocity.X > 0 ? 1 : -1;
            projectile.spriteDirection = projectile.direction;

            bubbleShotProgress += Math.Abs(projectile.velocity.X);

            float bubbleShotProgressRequired = 160 * 200 / (200f + projectile.timeLeft);

            if (bubbleShotProgress >= bubbleShotProgressRequired)
            {
                bubbleShotProgress -= bubbleShotProgressRequired;
                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(projectile.Center - new Vector2(projectile.direction * bubbleShotProgress, 0), Vector2.Zero, ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 1f, ai1: 0.05f);
                    Projectile.NewProjectile(projectile.Center - new Vector2(projectile.direction * bubbleShotProgress, 0), Vector2.Zero, ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 1f, ai1: -0.05f);
                }
            }

            if (projectile.timeLeft == 60)
            {
                if (Main.netMode != 1)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Projectile.NewProjectile(projectile.Center, new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / 8), ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 1f);//, ai0: 0.99f, ai1: 0.08f);
                        Projectile.NewProjectile(projectile.Center, new Vector2(3, 0).RotatedBy((i + 0.5f) * MathHelper.TwoPi / 8), ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 1f);//, ai0: 0.99f, ai1: 0.08f);
                    }
                }
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 8)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCHit14, projectile.Center);

            Gore.NewGore(projectile.Center - Vector2.UnitX * 20f * (float)projectile.direction, projectile.velocity, 576, projectile.scale);
            Gore.NewGore(projectile.Center - Vector2.UnitY * 30f, projectile.velocity, 574, projectile.scale);
            Gore.NewGore(projectile.Center, projectile.velocity, 575, projectile.scale);
            Gore.NewGore(projectile.Center + Vector2.UnitX * 20f * (float)projectile.direction, projectile.velocity, 573, projectile.scale);
            Gore.NewGore(projectile.Center - Vector2.UnitY * 30f, projectile.velocity, 574, projectile.scale);
            Gore.NewGore(projectile.Center, projectile.velocity, 575, projectile.scale);

            for (int i = 0; i < 12; i++)
            {
                Projectile.NewProjectile(projectile.Center, new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / 8), ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 1f);//, ai0: 0.99f, ai1: 0.08f);
                Projectile.NewProjectile(projectile.Center, new Vector2(3, 0).RotatedBy((i + 0.5f) * MathHelper.TwoPi / 8), ModContent.ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer, ai0: 1f);//, ai0: 0.99f, ai1: 0.08f);
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 8, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale, effects, 0f);
            }

            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Teal * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(projectile.velocity.Length(), projectile.width / 128f), SpriteEffects.None, 0f);
            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.Teal * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);

            return false;
        }
    }

    public class FloatingBubble : ModProjectile
    {
        public override string Texture => "Terraria/NPC_371";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Death Bubble");
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 480;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.velocity *= projectile.ai[0];
            projectile.velocity.Y -= projectile.ai[1];

            projectile.rotation += projectile.velocity.X * 0.1f;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath3, projectile.Center);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 2, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White * 0.5f, projectile.rotation, new Vector2(24, 24), projectile.scale, effects, 0f);

            return false;
        }
    }

    public class MothProjectile : ModProjectile
    {
        public override string Texture => "Terraria/NPC_205";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moth");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 40;
            projectile.height = 40;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 400;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
            projectile.direction = projectile.velocity.X > 0 ? -1 : 1;
            projectile.spriteDirection = projectile.direction;

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 5)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 3)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 3, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Vector2 drawOffset = new Vector2(0, -14);

            spriteBatch.Draw(texture, projectile.Center + drawOffset - Main.screenPosition, frame, Color.White, projectile.rotation, frame.Size() / 2, projectile.scale, effects, 0f);

            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.DarkOliveGreen * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 64), new Vector2(projectile.velocity.Length(), projectile.width / 128f), SpriteEffects.None, 0f);
            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), Color.DarkOliveGreen * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 64), new Vector2(projectile.width / 128f, projectile.width / 128f), SpriteEffects.None, 0f);

            return false;
        }
    }

    public class MothronProjectile : ModProjectile
    {
        public override string Texture => "Terraria/NPC_477";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mothron");
            Main.projFrames[projectile.type] = 6;

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 13;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 64;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 400;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft > 400 - 33)
            {
                projectile.velocity *= 0.95f;
            }
            else
            {
                projectile.velocity.Y += projectile.velocity.Y * projectile.velocity.Y / (projectile.Center.Y - projectile.ai[0]);

                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 6f;
            }
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
            projectile.direction = projectile.velocity.X > 0 ? -1 : 1;
            projectile.spriteDirection = projectile.direction;

            //test for death via ray of sunshine
            projectile.ai[1] += 5 * projectile.direction;
            if (projectile.ai[1] < projectile.Center.X ^ projectile.direction == 1)
            {
                projectile.Kill();
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 6)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCKilled, (int)projectile.Center.X, (int)projectile.Center.Y, 44, volumeScale: 0.5f);

            Gore.NewGore(projectile.position, projectile.velocity, 687, projectile.scale);
            Gore.NewGore(projectile.position, projectile.velocity, 688, projectile.scale);
            Gore.NewGore(projectile.position, projectile.velocity, 689, projectile.scale);
            Gore.NewGore(projectile.position, projectile.velocity, 690, projectile.scale);
            Gore.NewGore(projectile.position, projectile.velocity, 691, projectile.scale);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            for (int i = projectile.oldPos.Length - 1; i >= 0; i -= 3)
            {
                Rectangle frame = texture.Frame(1, 6, 0, ((projectile.frame * 3 + projectile.frameCounter + 6 - i) / 3) % 6);

                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                if (i != 0) alpha /= 2;
                spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale, effects, 0f);
            }

            return false;
        }
    }

    public class CataBossMine : ModProjectile
    {
        public override string Texture => "Terraria/NPC_" + NPCID.AncientDoom;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Celestial Doom");

            Main.projFrames[projectile.type] = 5;
        }

        private int sigilCount
        {
            get
            {
                return projectile.ai[1] == 0 ? 1 : (int)projectile.ai[1] * 4;
            }
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 0;
            projectile.light = 0.5f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 480;

            projectile.hide = true;
        }

        public override void AI()
        {
            float mineTime = 90f;

            if ((480 - projectile.timeLeft) < mineTime)
            {
                projectile.hostile = false;

                projectile.ai[0] += 3.125f * projectile.ai[1] * (mineTime - (480 - projectile.timeLeft)) / mineTime;

                projectile.alpha = (int)(256 - 128 * ((480 - projectile.timeLeft) / mineTime));
            }
            else
            {
                projectile.hostile = true;
                projectile.alpha = 0;
            }

            projectile.rotation = projectile.ai[1];

            if (projectile.timeLeft < 30)
            {
                Vector2 oldCenter = projectile.Center;
                projectile.width += 4;
                projectile.height += 4;
                projectile.Center = oldCenter;
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 4)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 5)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < sigilCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.ai[0] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < projectile.width / 2)
                {
                    return true;
                }
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            for (int i = 0; i < sigilCount; i++)
            {
                Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(projectile.ai[0] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount);

                if (projectile.timeLeft >= 30)
                {
                    Texture2D texture = Main.projectileTexture[projectile.type];

                    Rectangle frame = texture.Frame(1, 5, 0, projectile.frame);
                    SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    spriteBatch.Draw(texture, drawPosition, frame, Color.White * (1 - projectile.alpha / 255f), 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }
                else
                {
                    Texture2D texture = mod.GetTexture("NPCs/CataBoss/CelestialDoom");

                    Rectangle frame = texture.Frame(1, 5, 0, projectile.frame);
                    SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    spriteBatch.Draw(texture, drawPosition, frame, Color.White * (1 - projectile.alpha / 255f) * (1 - projectile.width / 168f), 0f, frame.Size() / 2, projectile.width / 48f, effects, 0f);

                    spriteBatch.Draw(texture, drawPosition, frame, Color.White * (1 - projectile.alpha / 255f) * (projectile.timeLeft / 30f), 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }
            }
            return false;
        }
    }

    public class CataBossMine2 : ModProjectile
    {
        public override string Texture => "Terraria/NPC_" + NPCID.AncientDoom;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Celestial Doom");

            Main.projFrames[projectile.type] = 5;
        }

        private int sigilCount
        {
            get
            {
                return projectile.ai[1] == 0 ? 1 : (int)projectile.ai[1] * 6;
            }
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 0;
            projectile.light = 0.5f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 600;

            projectile.hide = true;
        }

        public override void AI()
        {
            float mineTime = 90f;

            if ((600 - projectile.timeLeft) < mineTime)
            {
                projectile.hostile = false;

                projectile.ai[0] += 3.75f * projectile.ai[1] * (mineTime - (600 - projectile.timeLeft)) / mineTime;

                projectile.alpha = (int)(256 - 128 * ((600 - projectile.timeLeft) / mineTime));
            }
            else
            {
                projectile.hostile = true;
                projectile.alpha = 0;
            }

            projectile.rotation = projectile.ai[1] * 4f / 6f;

            if (projectile.timeLeft < 30)
            {
                Vector2 oldCenter = projectile.Center;
                projectile.width += 4;
                projectile.height += 4;
                projectile.Center = oldCenter;
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 4)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 5)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < sigilCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.ai[0] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < projectile.width / 2)
                {
                    return true;
                }
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            for (int i = 0; i < sigilCount; i++)
            {
                Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(projectile.ai[0] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount);

                if (projectile.timeLeft >= 30)
                {
                    Texture2D texture = Main.projectileTexture[projectile.type];

                    Rectangle frame = texture.Frame(1, 5, 0, projectile.frame);
                    SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    spriteBatch.Draw(texture, drawPosition, frame, Color.White * (1 - projectile.alpha / 255f), 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }
                else
                {
                    Texture2D texture = mod.GetTexture("NPCs/CataBoss/CelestialDoom");

                    Rectangle frame = texture.Frame(1, 5, 0, projectile.frame);
                    SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    spriteBatch.Draw(texture, drawPosition, frame, Color.White * (1 - projectile.alpha / 255f) * (1 - projectile.width / 168f), 0f, frame.Size() / 2, projectile.width / 48f, effects, 0f);

                    spriteBatch.Draw(texture, drawPosition, frame, Color.White * (1 - projectile.alpha / 255f) * (projectile.timeLeft / 30f), 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }
            }
            return false;
        }
    }

    public class CataBusterSword : ModProjectile
    {
        public override string Texture => "Terraria/Item_426";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Buster Sword");
            Main.projFrames[projectile.type] = 1;

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 140;
            projectile.height = 140;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 2f;
            projectile.timeLeft = 400;
        }

        public override void AI()
        {
            if (projectile.localAI[0] == 0)
            {
                projectile.localAI[0] = 1;

                projectile.direction = Main.npc[(int)projectile.ai[0]].spriteDirection;
                projectile.spriteDirection = projectile.direction;

                projectile.rotation = projectile.AngleTo(Main.player[Main.npc[(int)projectile.ai[0]].target].Center) - 3 * MathHelper.PiOver4;

                if (projectile.ai[1] == 2f)
                {
                    projectile.timeLeft = 600;
                }

                projectile.localAI[1] = projectile.timeLeft;
            }

            if (projectile.spriteDirection != Main.npc[(int)projectile.ai[0]].spriteDirection)
            {
                projectile.rotation = 3 * MathHelper.PiOver2 - projectile.rotation;
            }

            projectile.direction = Main.npc[(int)projectile.ai[0]].spriteDirection;
            projectile.spriteDirection = projectile.direction;

            projectile.rotation += (float)Math.Sin(projectile.timeLeft / projectile.localAI[1] * MathHelper.Pi) * 50f * MathHelper.Pi / projectile.localAI[1] / 2f * projectile.direction * projectile.ai[1];
            projectile.Center = Main.npc[(int)projectile.ai[0]].Center;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + new Vector2(140, -140).RotatedBy(projectile.rotation), 20 * projectile.scale, ref point);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame();

            for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
            {
                SpriteEffects effects = projectile.oldSpriteDirection[i] == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Vector2 center = projectile.oldSpriteDirection[i] == 1 ? new Vector2(0, 80) : new Vector2(70, 80);
                float rotationOffset = projectile.oldSpriteDirection[i] == 1 ? 0 : MathHelper.PiOver2;
                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;

                spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i] + rotationOffset, center, projectile.scale, effects, 0f);
            }

            return false;
        }
    }

    public class CataBossFireballRing : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.CultistBossFireBall;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Celestial Flame");

            Main.projFrames[projectile.type] = 4;
        }

        int numFireballs = 32;

        public override void SetDefaults()
        {
            projectile.width = 38;
            projectile.height = 38;
            projectile.alpha = 0;
            projectile.light = 0.5f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 90;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation += projectile.ai[1] * 0.02f;
            projectile.ai[0] -= 20f / 3f;

            if (projectile.timeLeft <= 30)
            {
                projectile.alpha += 8;
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 4)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 4)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < numFireballs; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.ai[0] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / numFireballs);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < projectile.width / 2)
                {
                    return true;
                }
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float trailLength = 10f;

            for (int i = 0; i < numFireballs; i++)
            {
                for (int j = 0; j < trailLength; j++)
                {
                    float oldAI0 = projectile.ai[0] + (20f / 3f) * j;
                    float oldRotation = projectile.rotation - projectile.ai[1] * 0.02f * j;
                    float alpha = 1 - j / trailLength;

                    Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(oldAI0 * projectile.scale, 0).RotatedBy(oldRotation + i * MathHelper.TwoPi / numFireballs);

                    Texture2D texture = Main.projectileTexture[projectile.type];

                    Rectangle frame = texture.Frame(1, 4, 0, projectile.frame);
                    SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    spriteBatch.Draw(texture, drawPosition, frame, Color.White * (1 - projectile.alpha / 255f) * alpha, oldRotation, frame.Size() / 2, projectile.scale * alpha, effects, 0f);
                }
            }
            return false;
        }
    }

    public class MegaSprocket : ModProjectile
    {
        public override string Texture => "DeathsTerminus/NPCs/CataBoss/HeavenPetProjectile";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mega Sprocket");
            Main.projFrames[projectile.type] = 7;

            /*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 512, 512, false, SurfaceFormat.Color);
			List<Color> list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
                {
                    float x = (2 * (i - 50) / (float)(texture.Width - 100 - 1) - 1);
                    float y = (2 * (j - 50) / (float)(texture.Height - 100 - 1) - 1);

                    float radius = (float)Math.Sqrt(x * x + y * y);
                    float theta = (float)Math.Atan2(x, y);

                    float edgeAmount = 0.3f;
                    float edgeThin = 16;
                    float edgePosition = 0.8f;

                    float index = (float)(Math.Pow(radius, 4) + (Math.Cos(2 * theta - MathHelper.PiOver2) + 1) * edgeAmount * (Math.Exp(-Math.Pow(edgeThin * (radius - edgePosition), 2)) + Math.Exp(-Math.Pow(edgeThin * (radius + edgePosition), 2))));
                    if (radius > 1) index = index * 4 - 3;
                    index = (float)Math.Max(0, Math.Min(2 - index, index));

                    int r = 255;
                    int g = 255;
                    int b = 255;
                    int alpha = (int)(255 * index);

                    list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "MegaSprocketShield.png", FileMode.Create), texture.Width, texture.Height);*/
        }

        private const int lifeTime = 1800;

        private float cogRotation = 0;
        private float shieldAlpha = 0;

        public override void SetDefaults()
        {
            projectile.width = 66;
            projectile.height = 66;
            projectile.alpha = 0;
            projectile.light = 1f;
            projectile.aiStyle = -1;
            projectile.hostile = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = lifeTime;
        }

        public override void AI()
        {
            if (projectile.localAI[0] == 0)
            {
                projectile.scale = projectile.ai[1] + 1f;
            }

            if (projectile.localAI[0] == 60)
            {
                projectile.hostile = true;
            }

            Vector2 oldCenter = projectile.Center;
            projectile.width = (int)(projectile.scale * 66);
            projectile.height = (int)(projectile.scale * 66);
            projectile.Center = oldCenter;

            if (projectile.timeLeft == lifeTime)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (Main.netMode != 1)
                        Projectile.NewProjectile(projectile.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / 6f), ModContent.ProjectileType<MegaSprocketPrism>(), 80, 0f, Main.myPlayer, ai0: i, ai1: projectile.whoAmI);
                }
            }

            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 7)
                {
                    projectile.frame = 0;
                }
            }

            if (projectile.timeLeft > 300)
            {
                projectile.velocity *= (1 - 1 / 60f);

                shieldAlpha = (shieldAlpha + 1 / 240f) / (1 + 1 / 240f);
            }
            else if (projectile.timeLeft > 150)
            {
                projectile.velocity *= (1 - 1/60f);

                projectile.hostile = false;

                if (shieldAlpha > 0)
                {
                    shieldAlpha = shieldAlpha - 1 / 150f;
                }
                else
                {
                    shieldAlpha = 0;
                }
            }
            else
            {
                NPC boss = Main.npc[(int)projectile.ai[0]];

                projectile.velocity += (boss.Center + boss.velocity * projectile.timeLeft - projectile.Center - projectile.velocity * projectile.timeLeft) / (projectile.timeLeft * projectile.timeLeft);
                projectile.scale -= 2 / 150f;
            }

            projectile.localAI[0]++;
            cogRotation += projectile.localAI[0] / 50000f + projectile.localAI[0] * projectile.localAI[0] / 100000000f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 circleCenter = projectile.Center;
            float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < projectile.width / 2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame;
            SpriteEffects effects;

            //draw the pet
            Texture2D texture = Main.projectileTexture[projectile.type];
            if (projectile.localAI[1] == 0)
            {
                texture = mod.GetTexture("NPCs/CataBoss/DarkPetProjectile");
            }

            frame = texture.Frame(1, 7, 0, projectile.frame);
            effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            float wingRotation = projectile.velocity.X * 0.1f;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, wingRotation, frame.Size() / 2, projectile.scale, effects, 0f);

            Texture2D texture2 = mod.GetTexture("NPCs/CataBoss/HeavenPetProjectile_Cog");
            if (projectile.localAI[1] == 0)
            {
                texture2 = mod.GetTexture("NPCs/CataBoss/DarkPetProjectile_Cog");
            }

            frame = texture2.Frame();
            effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            spriteBatch.Draw(texture2, projectile.Center - Main.screenPosition, frame, Color.White, cogRotation, frame.Size() / 2, projectile.scale, effects, 0f);

            Texture2D texture3 = mod.GetTexture("NPCs/CataBoss/MegaSprocketShield");
            frame = texture3.Frame();

            spriteBatch.Draw(texture3, projectile.Center - Main.screenPosition, frame, Color.White * shieldAlpha, cogRotation / 2, frame.Size() / 2, projectile.scale / 6f, effects, 0f);

            return false;
        }
    }

    public class MegaSprocketPrism : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_633";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Last Prism");
            Main.projFrames[projectile.type] = 5;
        }

        private const int lifeTime = 1200;
        private bool firingPrism
        {
            get { return projectile.timeLeft <= lifeTime - 120; }
        }

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.alpha = 0;
            projectile.light = 1f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = lifeTime;
        }

        public override void AI()
        {
            if (projectile.localAI[0] == 0)
            {
                projectile.rotation = projectile.velocity.ToRotation();
            }

            Projectile owner = Main.projectile[(int)projectile.ai[1]];

            float radius = 48 + Math.Max(0, (projectile.localAI[0] - 120));
            projectile.rotation += projectile.localAI[0] / 50000f + projectile.localAI[0] * projectile.localAI[0] / 100000000f;

            projectile.velocity = owner.Center + new Vector2(radius, 0).RotatedBy(projectile.rotation) - projectile.Center;

            projectile.hostile = firingPrism;

            projectile.localAI[0]++;

            if (projectile.timeLeft == lifeTime - 120)
            {
                Main.PlaySound(SoundID.Item122, projectile.Center);
            }
            else if (projectile.timeLeft % 60 == 0 && projectile.timeLeft <= lifeTime - 120)
            {
                Main.PlaySound(SoundID.Item15, projectile.Center);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center + new Vector2(20, 0).RotatedBy(projectile.rotation), projectile.Center + new Vector2(4096, 0).RotatedBy(projectile.rotation), 44 * projectile.scale, ref point);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame;
            SpriteEffects effects;

            //Prisms here are different, each one is a different individual color
            Color value42 = Main.hslToRgb((projectile.ai[0] / 6f + projectile.localAI[0] / 360f) % 1, 1f, 0.5f);
            value42.A = 0;

            //draw connections to prisms
            float radius = 48 + Math.Max(0, (projectile.localAI[0] - 120));
            float alpha = 1 - radius / 1200f;
            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), value42 * (alpha * 0.4f), projectile.rotation + MathHelper.Pi, new Vector2(0, 64), new Vector2(radius / 64f, projectile.width / 64f), SpriteEffects.None, 0f);
            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 128), value42 * (alpha * 0.4f), projectile.rotation, new Vector2(0, 64), new Vector2(projectile.width / 64f, projectile.width / 64f), SpriteEffects.None, 0f);

            //draw prism telegraph glow/background glow
            float scaleModifier = projectile.timeLeft <= lifeTime - 120 ? 5 : 1;
            float alphaModifier = projectile.timeLeft <= lifeTime - 120 ? 0.5f : 0.2f;
            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center + new Vector2(8 * scaleModifier * projectile.scale, 0).RotatedBy(projectile.rotation) - Main.screenPosition, new Rectangle(0, 0, 64, 128), value42 * alphaModifier, projectile.rotation, new Vector2(0, 64), new Vector2((1200 - radius) / 64f, projectile.width / 128f * scaleModifier), SpriteEffects.None, 0f);
            spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossTelegraphCap"), projectile.Center + new Vector2(8 * scaleModifier * projectile.scale, 0).RotatedBy(projectile.rotation) - Main.screenPosition, new Rectangle(0, 0, 64, 128), value42 * alphaModifier, projectile.rotation + MathHelper.Pi, new Vector2(0, 64), new Vector2(projectile.width / 128f * scaleModifier, projectile.width / 128f * scaleModifier), SpriteEffects.None, 0f);

            //draw the prism
            //adapted from last prism drawcode
            Texture2D prismTexture = Main.projectileTexture[projectile.type];
            frame = prismTexture.Frame(1, 5, 0, (projectile.timeLeft / (projectile.timeLeft <= 600 - 60 ? 1 : 3)) % 5);
            effects = SpriteEffects.None;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            spriteBatch.Draw(prismTexture, drawPosition, frame, Color.White, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);

            float scaleFactor2 = (float)Math.Cos(Math.PI * 2f * (projectile.timeLeft / 30f)) * 2f + 2f;
            if (projectile.timeLeft <= lifeTime - 120)
            {
                scaleFactor2 = 4f;
            }
            for (float num350 = 0f; num350 < 4f; num350 += 1f)
            {
                spriteBatch.Draw(prismTexture, drawPosition + new Vector2(0, 1).RotatedBy(num350 * (Math.PI * 2f) / 4f) * scaleFactor2, frame, Color.White.MultiplyRGBA(new Color(255, 255, 255, 0)) * 0.03f, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);
            }

            if (projectile.timeLeft > lifeTime - 120)
            {
                //draw the telegraph line
                float telegraphAlpha = (120 - lifeTime + projectile.timeLeft) / 30f * (lifeTime - 60 - projectile.timeLeft) / 30f;
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/HeavenPetProjectileTelegraph"), projectile.Center + new Vector2(10 * projectile.scale, 0).RotatedBy(projectile.rotation) - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * telegraphAlpha, projectile.rotation, new Vector2(0, 0.5f), new Vector2(4096, 1), SpriteEffects.None, 0f);
            }
            if (firingPrism)
            {
                //draw the beam
                //adapted from last prism drawcode

                for (int i = 0; i < 6; i++)
                {
                    //texture
                    Texture2D tex7 = ModContent.GetTexture("Terraria/Projectile_632");
                    //laser length
                    float num528 = 1200 - (48 + Math.Max(0, (projectile.localAI[0] - 120)));

                    Vector2 drawOffset = new Vector2(4, 0).RotatedBy(projectile.timeLeft * 0.5f + i * MathHelper.TwoPi / 6);

                    //start position
                    Vector2 value45 = projectile.Center.Floor() + drawOffset + new Vector2(26 * projectile.scale, 0).RotatedBy(projectile.rotation);

                    value45 += Vector2.UnitX.RotatedBy(projectile.rotation) * projectile.scale * 10.5f;
                    num528 -= projectile.scale * 14.5f * projectile.scale;
                    Vector2 vector90 = new Vector2(projectile.scale * 2);
                    DelegateMethods.f_1 = 1f;
                    DelegateMethods.c_1 = value42 * 0.75f * projectile.Opacity;
                    _ = projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
                    Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90, DelegateMethods.RainbowLaserDraw);
                    DelegateMethods.c_1 = new Color(255, 255, 255, 127) * 0.75f * projectile.Opacity;
                    Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90 / 2f, DelegateMethods.RainbowLaserDraw);
                }
            }

            return false;
        }
    }

    public class MegaBaddy : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mysterious Presence");
            Main.projFrames[projectile.type] = 7;
        }

        private const int initTime = 60;
        private const int lifeTime = 1200;
        private const int fadeTime = 180;
        private const int departTime = 300;
        private const float projectileRadius = 1200;

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.alpha = 255;
            projectile.light = 1f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 2f;
            projectile.timeLeft = initTime + lifeTime + fadeTime + departTime;
        }

        public override void AI()
        {
            if (projectile.timeLeft > lifeTime + fadeTime + departTime)
            {
                projectile.scale -= 1f / initTime;
                projectile.alpha = (int)(((projectile.timeLeft - lifeTime - fadeTime - departTime) / (float)initTime) * 255);
            }
            else if (projectile.timeLeft > fadeTime + departTime)
            {
                projectile.scale = 1f;

                float x = (lifeTime - (projectile.timeLeft - fadeTime - departTime)) / (float)lifeTime;
                float a = 1 / 8f; //starting increment
                float b = 1 / 4f; //ending increment
                float c = 1 / 1f; //amount by which it 'dips'
                float increment = a + (b - a) * x - c * x * x * (1 - x) * (1 - x);
                projectile.localAI[0] += increment;

                if (projectile.localAI[0] >= 1 && Main.netMode != 1)
                {
                    projectile.localAI[0] -= 1;

                    for (int i = 0; i < 6; i++)
                    {
                        float shotExtraTime = projectile.localAI[0] / increment;
                        float shotRotation = (lifeTime - (projectile.timeLeft - fadeTime - departTime) - shotExtraTime) * (lifeTime - (projectile.timeLeft - fadeTime - departTime) - shotExtraTime) / 52000f + i * MathHelper.TwoPi / 6f;
                        Projectile.NewProjectile(projectile.Center + new Vector2(projectileRadius, 0).RotatedBy(shotRotation) + new Vector2(-4, 0).RotatedBy(shotRotation) * shotExtraTime, new Vector2(-4, 0).RotatedBy(shotRotation), ModContent.ProjectileType<Shadow>(), 80, 0f, Main.myPlayer);
                    }
                }
            }
            else
            {
                projectile.hostile = false;
            }

            if (projectile.timeLeft - departTime < 450)
            {
                projectile.alpha = Math.Max(0, (int)(255 - ((projectile.timeLeft - departTime) / 450f) * 255));
            }

            if (projectile.timeLeft > departTime)
            {
                projectile.rotation = projectile.DirectionTo(Main.player[(int)projectile.ai[0]].Center).ToRotation();
                projectile.spriteDirection = projectile.DirectionTo(Main.player[(int)projectile.ai[0]].Center).X > 0 ? 1 : -1;
            }
            else if (projectile.timeLeft == departTime - 90 || projectile.timeLeft == departTime - 120)
            {
                projectile.spriteDirection *= -1;
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 circleCenter = projectile.Center;

            float furthestX = targetHitbox.X + targetHitbox.Size().X / 2 - circleCenter.X > 0 ? targetHitbox.X + targetHitbox.Size().X : targetHitbox.X;
            float furthestY = targetHitbox.Y + targetHitbox.Size().Y / 2 - circleCenter.Y > 0 ? targetHitbox.Y + targetHitbox.Size().Y : targetHitbox.Y;
            return new Vector2(circleCenter.X - furthestX, circleCenter.Y - furthestY).Length() > projectileRadius * projectile.scale;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture;
            Rectangle frame;
            float numDraws;
            SpriteEffects effects = SpriteEffects.None;

            float alphaModifier = 1 - projectile.alpha / 255f;

            if (alphaModifier > 0)
            {
                //draw the baddy
                //draw the baddy's particles
                texture = mod.GetTexture("NPCs/CataBoss/ShadowParticle");
                numDraws = 1600;
                Rectangle[] frameList = new Rectangle[]
                {
                new Rectangle(0,0,14,14),
                new Rectangle(16,4,10,10),
                new Rectangle(28,6,8,8)
                };
                for (int i = 0; i < numDraws; i++)
                {
                    frame = frameList[i % 3];

                    float distanceInwards = (i * i + (i % 3 + 2) * (lifeTime - projectile.timeLeft)) % 60;
                    float rotationOffset = (i * i) % numDraws + (i * i + (i % 3 + 2) * (lifeTime - projectile.timeLeft)) / 60;
                    float alpha = (60 - distanceInwards) / 60;
                    spriteBatch.Draw(texture, projectile.Center + new Vector2(projectileRadius * projectile.scale - distanceInwards, 0).RotatedBy(i * MathHelper.TwoPi / numDraws + rotationOffset) - Main.screenPosition, frame, Color.White * alpha * alphaModifier, 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }
                texture = mod.GetTexture("NPCs/CataBoss/ShadowParticleBig");
                frame = texture.Frame();
                numDraws = 800;
                for (int i = 0; i < numDraws; i++)
                {
                    float distanceInwards = (i * i + 2 * (lifeTime - projectile.timeLeft)) % 60 - texture.Width / 2;
                    float rotationOffset = (i * i) % numDraws + (i * i + 2 * (lifeTime - projectile.timeLeft)) / 60;
                    float alpha = (60 - distanceInwards) / 60;
                    spriteBatch.Draw(texture, projectile.Center + new Vector2(projectileRadius * projectile.scale - distanceInwards, 0).RotatedBy(i * MathHelper.TwoPi / numDraws + rotationOffset) - Main.screenPosition, frame, Color.White * alpha * alphaModifier, 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }

                //draw the baddy's body
                texture = Main.projectileTexture[projectile.type];
                frame = texture.Frame();
                numDraws = 48;

                for (int i = 0; i < numDraws; i++)
                {
                    spriteBatch.Draw(texture, projectile.Center + new Vector2(projectileRadius * projectile.scale, 0).RotatedBy(i * MathHelper.TwoPi / numDraws) - Main.screenPosition, frame, Color.White * alphaModifier, i * MathHelper.TwoPi / 48, new Vector2(0, 0.5f), projectile.scale * projectileRadius, effects, 0f);
                }

                texture = mod.GetTexture("NPCs/CataBoss/FogParticle");
                numDraws = 1600;
                frameList = new Rectangle[]
                {
                new Rectangle(0,0,54,10),
                new Rectangle(0,16,32,6),
                new Rectangle(54,14,26,6),
                new Rectangle(80,18,32,8),
                new Rectangle(94,0,60,10),
                new Rectangle(130,22,20,6)
                };
                for (int i = 0; i < numDraws; i++)
                {
                    frame = frameList[i % 6];

                    float timeAlive = (i * i + (lifeTime - projectile.timeLeft)) % 60;
                    float rotationOffset = (i * i) % numDraws + (i * i + (lifeTime - projectile.timeLeft)) / 60;
                    float distanceOutwards = 40 + (i * i) % numDraws;
                    float alpha = timeAlive * (60 - timeAlive) / 900f;

                    spriteBatch.Draw(texture, projectile.Center + new Vector2(projectileRadius * projectile.scale + distanceOutwards, 0).RotatedBy(i * MathHelper.TwoPi / numDraws + rotationOffset) - Main.screenPosition, frame, Color.White * alpha * alphaModifier, 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }

                texture = mod.GetTexture("NPCs/CataBoss/LightningParticle");
                numDraws = 400;
                frameList = new Rectangle[]
                {
                new Rectangle(0,0,8,16),
                new Rectangle(10,0,12,8),
                new Rectangle(24,0,18,16),
                new Rectangle(44,0,12,22),
                new Rectangle(58,0,24,18),
                new Rectangle(84,0,16,20)
                };
                for (int i = 0; i < numDraws; i++)
                {
                    frame = frameList[i % 6];

                    float timeAlive = (i * i + (lifeTime - projectile.timeLeft)) % 60;
                    float rotationOffset = MathHelper.Pi + (i * i) % numDraws + (i * i + (lifeTime - projectile.timeLeft)) / 60;
                    float distanceOutwards = 40 + (i * i) % numDraws * 3;
                    float alpha = (60 - timeAlive) / 60f;

                    spriteBatch.Draw(texture, projectile.Center + new Vector2(projectileRadius * projectile.scale + distanceOutwards, 0).RotatedBy(i * MathHelper.TwoPi / numDraws + rotationOffset) - Main.screenPosition, frame, Color.White * alpha * alphaModifier, 0f, frame.Size() / 2, projectile.scale, effects, 0f);
                }
            }

            //draw the baddy's eyes
            texture = mod.GetTexture("NPCs/CataBoss/BaddyPet_Eyes");
            frame = texture.Frame(1, 2, 0, projectile.timeLeft > departTime ? 0 : 1);
            effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 drawEyesOffset = projectile.timeLeft > departTime ? Vector2.Zero : new Vector2(0, - (float)Math.Exp((departTime - projectile.timeLeft) / 10f - 15));

            spriteBatch.Draw(texture, projectile.Center + new Vector2(projectileRadius * projectile.scale + 240, 0).RotatedBy(projectile.rotation) - Main.screenPosition + drawEyesOffset, frame, Color.White, 0f, frame.Size() / 2, 4f, effects, 0f);

            if (projectile.timeLeft > fadeTime + departTime)
            {
                for (int i = 0; i < 6; i++)
                {
                    float rotation = ((lifeTime - (projectile.timeLeft - fadeTime - departTime)) * (lifeTime - (projectile.timeLeft - fadeTime - departTime)) / 50000f + i * MathHelper.TwoPi / 6f) % MathHelper.TwoPi;
                    effects = rotation < MathHelper.Pi ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    spriteBatch.Draw(texture, projectile.Center + new Vector2(projectileRadius * projectile.scale + 120, 0).RotatedBy(rotation) - Main.screenPosition, frame, Color.White * alphaModifier, 0f, frame.Size() / 2, 2f, effects, 0f);
                }
            }

            return false;
        }
    }

    public class Shadow : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadow");
            Main.projFrames[projectile.type] = 1;

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 14;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 275;

            projectile.hide = true;
        }

        public override void AI()
        {
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = mod.GetTexture("NPCs/CataBoss/ShadowOutline");

            Rectangle frame = texture.Frame();
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale * alpha, effects, 0f);
            }

            Texture2D texture2 = Main.projectileTexture[projectile.type];

            Rectangle frame2 = texture2.Frame();

            for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                spriteBatch.Draw(texture2, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame2, Color.White * alpha, projectile.oldRot[i], frame2.Size() / 2, projectile.scale * alpha, effects, 0f);
            }

            return false;
        }
    }

    public class MothronSpiralProjectile : ModProjectile
    {
        public override string Texture => "Terraria/NPC_477";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mothron");
            Main.projFrames[projectile.type] = 6;

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 13;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 64;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 314 + 30;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 314)
            {
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 6;
            }
            else if (projectile.timeLeft < 314)
            {
                projectile.velocity = projectile.velocity.RotatedBy(-projectile.ai[0] * MathHelper.Pi / 314f);
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
            projectile.direction = projectile.velocity.X > 0 ? -1 : 1;
            projectile.spriteDirection = projectile.direction;

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 6)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCKilled, (int)projectile.Center.X, (int)projectile.Center.Y, 44, volumeScale: 0.5f);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            for (int i = projectile.oldPos.Length - 1; i >= 0; i -= 3)
            {
                Rectangle frame = texture.Frame(1, 6, 0, ((projectile.frame * 3 + projectile.frameCounter + 6 - i) / 3) % 6);

                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                if (i != 0) alpha /= 2;
                spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale, effects, 0f);
            }

            return false;
        }
    }

    public class CelestialLamp : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Celestial Lamp");

            /*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 512, 512, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
                {
                    float x = (2 * i / (float)(texture.Width - 1) - 1);
                    float y = (2 * j / (float)(texture.Width - 1) - 1);

                    float distanceSquared = x * x + y * y;
                    float index = 1 - distanceSquared;

                    int r = 255 - (int)(64 * (1 - index));
                    int g = 255 - (int)(128 * (1 - index));
                    int b = 255;
                    int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

                    list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "CelestialLamp.png", FileMode.Create), texture.Width, texture.Height);*/
        }

        public override void SetDefaults()
        {
            projectile.width = 512;
            projectile.height = 512;
            projectile.alpha = 0;
            projectile.light = 4f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 1620;
            projectile.scale = 1 / 600f;
        }

        public override void AI()
        {
            if (projectile.scale < 2f && projectile.timeLeft > 60)
            {
                projectile.scale = (projectile.scale + 1 / 600f) / (2 + 1 / 600f) * 2;
            }
            else if (projectile.timeLeft <= 60)
            {
                projectile.scale -= 1 / 60f;
            }

            Vector2 oldCenter = projectile.Center;
            projectile.width = (int)(512 * projectile.scale);
            projectile.height = (int)(512 * projectile.scale);
            projectile.Center = oldCenter;

            if (projectile.timeLeft == 60 || projectile.timeLeft == 300 && Main.netMode != 1)
            {
                for (int i = 0; i < 12; i++)
                {
                    Projectile.NewProjectile(projectile.Center, new Vector2(0.7f, 0).RotatedBy(i * MathHelper.TwoPi / 12), ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 2f);
                }
            }
            else if (projectile.timeLeft == 180 || projectile.timeLeft == 420 && Main.netMode != 1)
            {
                for (int i = 0; i < 12; i++)
                {
                    Projectile.NewProjectile(projectile.Center, new Vector2(0.7f, 0).RotatedBy((i + 0.5f) * MathHelper.TwoPi / 12), ModContent.ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 2f);
                }
            }
            if (projectile.timeLeft == 30 || projectile.timeLeft == 150 || projectile.timeLeft == 270 || projectile.timeLeft == 390)
            {
                Main.PlaySound(SoundID.Item71, projectile.Center);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 circleCenter = projectile.Center;
            float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < projectile.width / 2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame();

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White * (1 - projectile.alpha / 255f), projectile.rotation, frame.Size() / 2, projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class CataBossStar : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.FallingStar;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Falling Star");
        }
        public override void SetDefaults()
        {
            projectile.width = 22;
            projectile.height = 22;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
            projectile.timeLeft = 3600;
            projectile.tileCollide = false;
            projectile.light = 0.9f;
            projectile.scale = 1.2f;
        }

        public override void AI()
        {
            if (projectile.ai[1] == 0f && !Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
            {
                projectile.ai[1] = 1f;
                projectile.netUpdate = true;
            }
            if (projectile.timeLeft < 3600 - 180)
            {
                projectile.tileCollide = true;
            }
            if (projectile.soundDelay == 0)
            {
                projectile.soundDelay = 20 + Main.rand.Next(40);
                Main.PlaySound(SoundID.Item9, projectile.position);
            }
            projectile.rotation += (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y)) * 0.01f * (float)projectile.direction;
            if (projectile.ai[1] == 1f)
            {
                projectile.light = 0.9f;
                if (Main.rand.Next(10) == 0)
                {
                    Vector2 position30 = projectile.position;
                    int width27 = projectile.width;
                    int height27 = projectile.height;
                    float speedX13 = projectile.velocity.X * 0.5f;
                    float speedY13 = projectile.velocity.Y * 0.5f;
                    Color newColor = default(Color);
                    Dust.NewDust(position30, width27, height27, 58, speedX13, speedY13, 150, newColor, 1.2f);
                }
                if (Main.rand.Next(20) == 0)
                {
                    Gore.NewGore(projectile.position, new Vector2(projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f), Main.rand.Next(16, 18));
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item10, projectile.position);
            int num537 = 10;
            int num538 = 3;
            for (int num539 = 0; num539 < num537; num539++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 1.2f);
            }
            for (int num540 = 0; num540 < num538; num540++)
            {
                int num541 = Main.rand.Next(16, 18);
                Gore.NewGore(projectile.position, new Vector2(projectile.velocity.X * 0.05f, projectile.velocity.Y * 0.05f), num541);
            }
            for (int num542 = 0; num542 < 10; num542++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 57, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 1.2f);
            }
            for (int num543 = 0; num543 < 3; num543++)
            {
                Gore.NewGore(projectile.position, new Vector2(projectile.velocity.X * 0.05f, projectile.velocity.Y * 0.05f), Main.rand.Next(16, 18));
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.White;
            return true;
        }
    }

    public class CataBossRod : ModProjectile
    {
        //rod of discord of doom
        public override string Texture => "Terraria/Item_" + ItemID.RodofDiscord;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rod of Judgement");
        }

        public override void SetDefaults()
        {
            projectile.width = 34;
            projectile.height = 34;
            projectile.scale = 10f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = 1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 1200;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation += projectile.velocity.X / 0.1f;

            Player player = Main.player[(int)projectile.ai[0]];
            projectile.velocity += ((player.Center - projectile.Center).SafeNormalize(Vector2.Zero) * Math.Max(16, player.velocity.Length() + 4) - projectile.velocity) / 20f;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            projectile.Kill();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame();

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White * (1 - projectile.alpha / 255f), projectile.rotation, frame.Size() / 2, projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class ShadowDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.velocity *= 1.5f;

            dust.frame = Main.rand.Next(new Rectangle[]
            {
                new Rectangle(0,0,14,14),
                new Rectangle(0,16,10,10),
                new Rectangle(0,28,8,8),
            });
            dust.position -= dust.frame.Size() / 2;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.scale -= 0.02f;
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class CataBossSky : CustomSky
    {
        public static int celestialObject;

        private bool isActive;
        public const int ECLIPSE_FRAME_SIZE = 512;
        private static int eclipseFrame;

        public override void OnLoad()
        {
        }

        public override void Update(GameTime gameTime)
        {
            eclipseFrame++;
            if (eclipseFrame == 60) eclipseFrame = 0;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            //draw the sky and the eclipse of doom
            if (maxDepth >= 0 && minDepth < 0)
            {
                spriteBatch.Draw(ModContent.GetTexture("DeathsTerminus/NPCs/CataBoss/HeavenPetProjectileTelegraph"), new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

                switch (celestialObject)
                {
                    case 1:
                        if (TextureCache.EclipseTexture != null)
                            spriteBatch.Draw(TextureCache.EclipseTexture.Value, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300) - new Vector2(ECLIPSE_FRAME_SIZE / 2), new Rectangle((eclipseFrame % 10) * ECLIPSE_FRAME_SIZE, (eclipseFrame / 10) * ECLIPSE_FRAME_SIZE, ECLIPSE_FRAME_SIZE, ECLIPSE_FRAME_SIZE), Color.White);
                        break;
                    case 2:
                    if (TextureCache.BlueSunTexture != null)
                            spriteBatch.Draw(TextureCache.BlueSunTexture.Value, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300) - new Vector2(ECLIPSE_FRAME_SIZE / 2), new Rectangle((eclipseFrame % 10) * ECLIPSE_FRAME_SIZE, (eclipseFrame / 10) * ECLIPSE_FRAME_SIZE, ECLIPSE_FRAME_SIZE, ECLIPSE_FRAME_SIZE), Color.White);
                        break;
                    case 3:
                    if (TextureCache.RainbowSunTexture != null)
                            spriteBatch.Draw(TextureCache.RainbowSunTexture.Value, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300) - new Vector2(ECLIPSE_FRAME_SIZE / 2), new Rectangle((eclipseFrame % 10) * ECLIPSE_FRAME_SIZE, (eclipseFrame / 10) * ECLIPSE_FRAME_SIZE, ECLIPSE_FRAME_SIZE, ECLIPSE_FRAME_SIZE), Color.White);
                        break;
                }
            }
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            return isActive;
        }
    }
}