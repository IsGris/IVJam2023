using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public enum ChessPiece
{
	Pawn, // пешка
	Knight, // конь
	Bishop, // слон
	Rook, // ладья
	Queen, // ферзь
	King // король
}

[CreateAssetMenu(menuName = "2D/Tiles/PlayerTile")]
public class PlayerTile : Tile
{
	[NonSerialized] public static PlayerTile instance;
	
	[NonSerialized] public ChessPiece ChessPiece;
	public PlayerStatistics Statistics;
	public Vector2Int Location;
	public static bool IsMoving = false;

	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
	{
		if (!IsMoving)
		{
			ChessPiece = (ChessPiece)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);
		}
		if (Statistics == null)
			Statistics = CreateInstance<PlayerStatistics>();
		instance = this;
		Location = new Vector2Int(location.x, location.y);
		return true;
	}
}