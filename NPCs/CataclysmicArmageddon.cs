using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using DeathsTerminus.Enums;
using DeathsTerminus.NPCs.CataBoss;

namespace DeathsTerminus.NPCs
{
    [AutoloadHead]
    public class CataclysmicArmageddon : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cataclysmic Armageddon");
            Main.npcFrameCount[npc.type] = 25;
            NPCID.Sets.ExtraFramesCount[npc.type] = 9;
            NPCID.Sets.AttackFrameCount[npc.type] = 4;
            NPCID.Sets.DangerDetectRange[npc.type] = 700;
            NPCID.Sets.AttackType[npc.type] = 0;
            NPCID.Sets.AttackTime[npc.type] = 90;
            NPCID.Sets.AttackAverageChance[npc.type] = 30;
            NPCID.Sets.HatOffsetY[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.townNPC = true;
            npc.friendly = true;
            npc.width = 18;
            npc.height = 40;
            drawOffsetY = 2;
            npc.aiStyle = (int)AIStyles.Passive;
            npc.damage = 10;
            npc.defense = 0;
            npc.lifeMax = 250;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.knockBackResist = 0.5f;
            animationType = NPCID.Guide;

            npc.buffImmune[BuffID.Suffocation] = true;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs, int money)
        {

            return DTWorld.AnyFlawless;

        }

        public override string TownNPCName()
        {
            return "";
        }

        public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;

        public override void AI()
        {
            npc.breath = 200;
            npc.width = 18;
            npc.height = 40;
        }

        public override string GetChat()
        {
            List<string> dialogue = new List<string>
            {

                "Struggling with a boss? Here's a couple tips. One, hit the boss. Two, don't get hit yourself. Easy, right?",
                "You don't like my outfit? Well I don't care about your feelings, so buzz off already!",
            };

            if (!NPC.downedMoonlord)
            {
                dialogue.Add("So, you'd like to challenge me? Prove yourself first, then we'll talk.");
            }

            int stylist = NPC.FindFirstNPC(NPCID.Stylist);
            if (stylist != -1)
            {
                dialogue.Add($"Can you tell {Main.npc[stylist].GivenName} to stop calling me pet names? I have a wife.");
            }

            if (Main.LocalPlayer.HasItem(ItemID.RodofDiscord))
            {
                dialogue.Add("EW! Get that repulsive rod away from me, you disgusting creature!");
            }

            if (!npc.homeless)
            {
                dialogue.Add("Well, I see you've got quite the cozy little home here. Would be a shame if it got blown up by something.");
            }

            if ((Main.LocalPlayer.armor[10].type == ItemID.BossMaskCultist) && (Main.LocalPlayer.armor[11].type == ItemID.BlueLunaticRobe))
            {
                dialogue.Add("Nice cosplay!");
                dialogue.Add("Huh, so you escaped too?");
            }

            if ((Main.LocalPlayer.name == "CataclysmicArma") || (Main.LocalPlayer.name == "Cata"))
            {
                dialogue.Add("Wait- since when was there a clone of me?");
            }

            if ((Main.LocalPlayer.name == "Astrum") || (Main.LocalPlayer.name == "Astrum Genesis"))
            {
                dialogue.Add("Did you have fun with destroyer?");
            }

            if ((Main.LocalPlayer.name == "Terry") || (Main.LocalPlayer.name == "terrynmuse"))
            {
                dialogue.Add("Hey look, a chocolate orange! I made sure to not run from the mines this time.");
                dialogue.Add("Terry, you are an ASSHOLE.");
            }

            if (Main.LocalPlayer.name == "NULL")
            {
                dialogue.Add("NULL, DON'T.");
            }

            if ((Main.LocalPlayer.name == "turingcomplete30") || (Main.LocalPlayer.name == "turing"))
            {
                dialogue.Add("HOW THE #&*$ DO THE POLARITIES EVEN WORK?!");
                if (NPC.downedMoonlord)
                {
                    dialogue.Add("Ah, the man himself has come to challenge me!");
                }

            }

            int solar = NPC.FindFirstNPC(NPCID.LunarTowerSolar);
            int nebula = NPC.FindFirstNPC(NPCID.LunarTowerNebula);
            int vortex = NPC.FindFirstNPC(NPCID.LunarTowerVortex);
            int stardust = NPC.FindFirstNPC(NPCID.LunarTowerStardust);
            if ((solar != -1) || (nebula != -1) || (vortex != -1) || (stardust != -1))
            {
                dialogue.Add("I swear, I had nothing to do with this!");
            }

            return Main.rand.Next(dialogue);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (!Main.LocalPlayer.HasItem(ItemID.RodofDiscord))
            {
                button = Language.GetTextValue("LegacyInterface.28");
                if (NPC.downedMoonlord)
                {
                    button2 = "Challenge";
                }
            }

        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {

            if (firstButton)
            {
                shop = true;
            }
            else
            {
                //Spawn Boss Here
                npc.Transform(mod.NPCType("CataBoss"));
                Main.npc[npc.whoAmI].localAI[0] = 1;
            }

        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            shop.item[nextSlot].SetDefaults(ItemID.DemonScythe);
            nextSlot++;
            if (NPC.killCount[Item.NPCtoBanner(NPCID.Mothron)] > 0)
            {
                shop.item[nextSlot].SetDefaults(ItemID.MothronWings);
                nextSlot++;
            }

        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            npc.life = 0;
            npc.checkDead();
            return true;
        }
    }
}
