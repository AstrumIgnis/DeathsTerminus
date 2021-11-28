using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;

namespace DeathsTerminus
{
    public class DTWorld : ModWorld
    {
        public static bool AnyFlawless { get; set; }
        public static bool DownedCataBoss { get; set; }

        public override void Initialize()
        {
            AnyFlawless = false;
            DownedCataBoss = false;
        }

        public override TagCompound Save()
        {
            var keys = new List<string>();
            if (AnyFlawless)
            {
                keys.Add("true");
            }
            if (DownedCataBoss)
            {
                keys.Add("DownedCataBoss");
            }
            return new TagCompound { ["flawless"] = keys };
        }

        public override void Load(TagCompound tag)
        {
            var flawless = tag.GetList<string>("flawless");
            AnyFlawless = flawless.Contains("true");
            DownedCataBoss = flawless.Contains("DownedCataBoss");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = AnyFlawless;
            flags[1] = DownedCataBoss;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            AnyFlawless = flags[0];
            AnyFlawless = flags[1];
        }
    }
}