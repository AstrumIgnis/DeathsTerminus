using System.Reflection;
using Terraria;

namespace DeathsTerminus.Common.Utils
{
    public static class DTUtils
    {
        public static void SetLiquidSpeed(this NPC npc, float water = 0.5f, float lava = 0.5f, float honey = 0.25f)
        {
            var type = typeof(NPC);
            type.GetField("waterMovementSpeed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(npc, water);
            type.GetField("lavaMovementSpeed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(npc, lava);
            type.GetField("honeyMovementSpeed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(npc, honey);
        }
    }
}