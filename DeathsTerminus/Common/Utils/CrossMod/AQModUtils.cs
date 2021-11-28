using Terraria.ModLoader;

namespace DeathsTerminus.Common.Utils.CrossMod
{
    public class AQModUtils
    {
        private Mod _aQMod;

        internal AQModUtils(Mod mod)
        {
            _aQMod = mod;
        }

        public bool GlimmerEvent_IsActive()
        {
            return (bool)_aQMod.Call("glimmerevent_isactive");
        }
    }
}