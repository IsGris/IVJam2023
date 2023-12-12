using UnityEngine;
using UnityEngine.Tilemaps;

// tile that represents benefits
[CreateAssetMenu(menuName = "2D/Tiles/BenefitTile")]
public class BenefitTile : Tile
{
	public enum BenefitType 
	{ 
		Attack,
		Armor,
		Health,
		Skin
	}


}