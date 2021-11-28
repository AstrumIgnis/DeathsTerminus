using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Gores
{
    public class CataBossIceShard : ModGore
	{
		public override void OnSpawn(Gore gore)
		{
			gore.numFrames = 1;
			gore.behindTiles = false;
			gore.timeLeft = Gore.goreTime;
		}

        public override bool Update(Gore gore)
        {
			gore.velocity.Y += 0.15f;
			gore.position += gore.velocity;
			gore.rotation = gore.velocity.ToRotation() - MathHelper.PiOver2;
			gore.scale -= 1 / 120f;
			if (gore.scale <= 0) gore.active = false;

			return false;
        }

        public override Color? GetAlpha(Gore gore, Color lightColor)
        {
			return Color.White * 0.5f;
        }
    }
}
