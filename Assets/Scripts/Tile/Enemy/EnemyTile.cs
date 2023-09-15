using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/EnemyTile")]
public class EnemyTile : Tile
{
	[NonSerialized] public ChessPiece ChessPiece;
	[NonSerialized] public Statistics Statistics;

	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
	{
		ChessPiece = (ChessPiece)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);

		return true;
	}

}