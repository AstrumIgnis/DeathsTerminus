using DeathsTerminus.Enums;
using DeathsTerminus.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.NPCs
{
    public class DTGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool flawless = true;

        public override bool PreAI(NPC npc)
        {
            if (((npc.type == NPCID.EaterofWorldsHead) || (npc.type == NPCID.EaterofWorldsTail)) && flawless)
            {

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == NPCID.EaterofWorldsBody)
                    {
                        flawless = Main.npc[i].GetGlobalNPC<DTGlobalNPC>().flawless;
                        break;
                    }
                }
            }

            return true;
        }

        public override void NPCLoot(NPC npc)
        {
            if (flawless && npc.boss && npc.type != NPCID.MartianSaucer && npc.type != NPCID.MartianSaucerCore && !DTWorld.AnyFlawless)
            {
                DTWorld.AnyFlawless = true;

                if (Main.netMode != NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.WorldData);
                }
            }
        }
    }
}