using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using System;

namespace DeathsTerminus.Buffs
{
    public class MysteriousPresence : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Mysterious Presence");
            Description.SetDefault("A mysterious presence has cursed you");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<DTPlayer>().mysteriousPresence = true;
        }
    }
}