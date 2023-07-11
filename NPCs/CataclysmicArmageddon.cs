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
            // DisplayName.SetDefault("Cataclysmic Armageddon");
            Main.npcFrameCount[NPC.type] = 25;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[NPC.type] = 700;
            NPCID.Sets.AttackType[NPC.type] = 0;
            NPCID.Sets.AttackTime[NPC.type] = 90;
            NPCID.Sets.AttackAverageChance[NPC.type] = 30;
            NPCID.Sets.HatOffsetY[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            DrawOffsetY = 2;
            NPC.aiStyle = (int)AIStyles.Passive;
            NPC.damage = 10;
            NPC.defense = 0;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Guide;

            NPC.buffImmune[BuffID.Suffocation] = true;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            return DTWorld.AnyFlawless;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string> { "" };
        }

        public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;

        public override void AI()
        {
            NPC.breath = 200;
            NPC.width = 18;
            NPC.height = 40;
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

            if (!NPC.homeless)
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

        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {

            if (firstButton)
            {
                shopName = "Shop";
            }
            else
            {
                //Spawn Boss Here
                NPC.Transform(Mod.Find<ModNPC>("CataBoss").Type);
                Main.npc[NPC.whoAmI].localAI[0] = 1;
            }

        }
        public override void AddShops()
        {
            var npcShop = new NPCShop(Type)
                .Add(ItemID.DemonScythe)
                .Add(ItemID.MothronWings, CataMothCondition);
            npcShop.Register();
        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            NPC.life = 0;
            NPC.checkDead();
        }
        public static Condition CataMothCondition = new("Mods.DeathsTerminus.Conditions.CataMothCondition", () => NPC.killCount[Item.NPCtoBanner(NPCID.Mothron)] > 0);
    }
}
