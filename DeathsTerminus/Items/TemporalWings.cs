using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace DeathsTerminus.Items
{
	[AutoloadEquip(EquipType.Wings)]
	public class TemporalWings : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Temporal Wings");
			Tooltip.SetDefault("Allows for flight and slow fall");
		}
		public override bool Autoload(ref string name)
		{
			return base.Autoload(ref name);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.wingTimeMax = 10000;
			player.wingTime = player.wingTimeMax;
		}


		public override void SetDefaults()
		{
			item.width = 26;
			item.height = 26;
			item.value = 10000;
			item.rare = ItemRarityID.Expert;
			item.accessory = true;
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
		   ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			player.wingsLogic = 22;
			ascentWhenFalling = 0.85f;
			ascentWhenRising = 0.15f;
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.15f;
		}

		public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
		{
			if (player.controlDown && player.controlJump && player.wingTime > 0f)
			{
				speed = 12f;
				acceleration *= 12f;
			}
			else
			{
				speed = 9f;
				acceleration *= 2.5f;
			}
		}
	}
}