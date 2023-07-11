using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using DeathsTerminus.NPCs.CataBoss;
using DeathsTerminus.NPCs;

namespace DeathsTerminus.Items
{
    public class CatastrophicSigil : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Catastrophic Sigil");
            // Tooltip.SetDefault("Challenges the master mothman");
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 40;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Expert;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }
        public override bool CanUseItem(Player player)
        {
            if (NPC.downedMoonlord)
            {
                return !NPC.AnyNPCs(Mod.Find<ModNPC>("CataBoss").Type);
            } else
                return false;
        }
        public override bool? UseItem(Player player)
        {
            int cata = NPC.FindFirstNPC(ModContent.NPCType<CataclysmicArmageddon>());

            if (cata > -1 && Main.npc[cata].active)
            {
                string message = "Cataclysmic Armageddon has awoken!";

                Main.npc[cata].Transform(ModContent.NPCType<CataBoss>());
                Main.npc[cata].localAI[0] = 1;

                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(message, 175, 75, 255);
                else if (Main.netMode == NetmodeID.Server)
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(175, 75, 255));
            }
            else
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CataBoss>());
            }
            return true;
        }
    }

    public class CatastrophicSigilDebugP2 : ModItem
    {
        public override string Texture => "DeathsTerminus/Items/CatastrophicSigil";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Catastrophic Sigil Phase 2");
            // Tooltip.SetDefault("Challenges the master mothman");
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 40;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Expert;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }
        public override bool CanUseItem(Player player)
        {
            if (NPC.downedMoonlord)
            {
                return !NPC.AnyNPCs(Mod.Find<ModNPC>("CataBoss").Type);
            }
            else
                return false;
        }
        public override bool? UseItem(Player player)
        {
            int cata = NPC.FindFirstNPC(ModContent.NPCType<CataclysmicArmageddon>());

            if (cata > -1 && Main.npc[cata].active)
            {
                string message = "Cataclysmic Armageddon has awoken!";

                Main.npc[cata].Transform(ModContent.NPCType<CataBoss>());

                Main.npc[cata].localAI[0] = 1;
                Main.npc[cata].ai[0] = 11;

                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(message, 175, 75, 255);
                else if (Main.netMode == NetmodeID.Server)
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(175, 75, 255));
            }
            else
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CataBoss>());

                cata = NPC.FindFirstNPC(ModContent.NPCType<CataBoss>());

                Main.npc[cata].localAI[0] = 0;
                Main.npc[cata].ai[0] = 11;
            }

            return true;
        }
    }

    public class CatastrophicSigilDebugP3 : ModItem
    {
        public override string Texture => "DeathsTerminus/Items/CatastrophicSigil";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Catastrophic Sigil Phase 3");
            // Tooltip.SetDefault("Challenges the master mothman");
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 40;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Expert;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }
        public override bool CanUseItem(Player player)
        {
            if (NPC.downedMoonlord)
            {
                return !NPC.AnyNPCs(Mod.Find<ModNPC>("CataBoss").Type);
            }
            else
                return false;
        }
        public override bool? UseItem(Player player)
        {
            int cata = NPC.FindFirstNPC(ModContent.NPCType<CataclysmicArmageddon>());

            if (cata > -1 && Main.npc[cata].active)
            {
                string message = "Cataclysmic Armageddon has awoken!";

                Main.npc[cata].Transform(ModContent.NPCType<CataBoss>());

                Main.npc[cata].localAI[0] = 1;
                Main.npc[cata].ai[0] = 23;

                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(message, 175, 75, 255);
                else if (Main.netMode == NetmodeID.Server)
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(175, 75, 255));
            }
            else
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CataBoss>());

                cata = NPC.FindFirstNPC(ModContent.NPCType<CataBoss>());

                Main.npc[cata].localAI[0] = 0;
                Main.npc[cata].ai[0] = 23;
            }

            return true;
        }
    }

    public class CatastrophicSigilDebugP4 : ModItem
    {
        public override string Texture => "DeathsTerminus/Items/CatastrophicSigil";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Catastrophic Sigil Phase 4");
            // Tooltip.SetDefault("Challenges the master mothman");
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 40;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Expert;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }
        public override bool CanUseItem(Player player)
        {
            if (NPC.downedMoonlord)
            {
                return !NPC.AnyNPCs(Mod.Find<ModNPC>("CataBoss").Type);
            }
            else
                return false;
        }
        public override bool? UseItem(Player player)
        {
            int cata = NPC.FindFirstNPC(ModContent.NPCType<CataclysmicArmageddon>());

            if (cata > -1 && Main.npc[cata].active)
            {
                string message = "Cataclysmic Armageddon has awoken!";

                Main.npc[cata].Transform(ModContent.NPCType<CataBoss>());

                Main.npc[cata].localAI[0] = 1;
                Main.npc[cata].ai[0] = 28;

                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(message, 175, 75, 255);
                else if (Main.netMode == NetmodeID.Server)
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(175, 75, 255));
            }
            else
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CataBoss>());

                cata = NPC.FindFirstNPC(ModContent.NPCType<CataBoss>());

                Main.npc[cata].localAI[0] = 0;
                Main.npc[cata].ai[0] = 28;
            }

            return true;
        }
    }
}