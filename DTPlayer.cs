using DeathsTerminus.Buffs;
using DeathsTerminus.NPCs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DeathsTerminus
{
    public class DTPlayer : ModPlayer
    {
        public bool mysteriousPresence;

        public int screenShakeTime = 0;
        public Vector2 screenShakeModifier = Vector2.Zero;
        public Vector2 screenShakeVelocity = Vector2.One;

        public override void ModifyScreenPosition()
        {
            Main.screenPosition += screenShakeModifier;
        }

        public override void UpdateBadLifeRegen()
        {
            if (mysteriousPresence)
            {
                //Player.statLifeMax2 = (int)Math.Max(1, Player.statLifeMax2 * (Player.buffTime[Player.FindBuffIndex(BuffType<MysteriousPresence>())] - 1500) / 60f);
            }
        }

        public override void PostUpdate()
        {
            float maxScreenShakeDistance = 6;
            float screenShakeSpeed = 4;

            if (screenShakeTime > 0)
            {
                screenShakeVelocity.Normalize();
                screenShakeVelocity *= screenShakeSpeed;
                screenShakeModifier += screenShakeVelocity;
                if (screenShakeModifier.Length() >= maxScreenShakeDistance)
                {
                    screenShakeModifier.Normalize();
                    screenShakeModifier *= maxScreenShakeDistance;
                    screenShakeVelocity = -screenShakeSpeed * screenShakeModifier.SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.PiOver2);
                }
            }
            else
            {
                screenShakeModifier = screenShakeModifier.SafeNormalize(Vector2.Zero) * Math.Max(screenShakeModifier.Length() - screenShakeSpeed, 0);
            }
        }

        public override void ResetEffects()
        {
            mysteriousPresence = false;

            if (screenShakeTime > 0)
            {
                screenShakeTime--;
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    Main.npc[i].GetGlobalNPC<DTGlobalNPC>().flawless = false;
                }
            }
        }
    }
}