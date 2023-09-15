using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/BenefitTile")]
public class BenefitTile : Tile
{
	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
	{
		return true;
	}

}