using System;
using UnityEngine;
using UnityEngine.Tilemaps;

// tile that represents enemy
[CreateAssetMenu(menuName = "2D/Tiles/EnemyTile")]
public class EnemyTile : Tile
{
	[NonSerialized] public ChessPiece ChessPiece; // which chess piece will be used for the enemy
	[NonSerialized] public EnemyStatistics Statistics;
	
	// sprites depends on what ChessPiece is using this enemy
	[SerializeField] private Sprite PawnSprite;
	[SerializeField] private Sprite KnightSprite;
	[SerializeField] private Sprite BishopSprite;
	[SerializeField] private Sprite RookSprite;
	[SerializeField] private Sprite QueenSprite;
	[SerializeField] private Sprite KingSprite;

	// location of the enemy in tilemap
	public Vector2Int Location;

	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
	{
		// inits
		Location = new Vector2Int(location.x, location.y);
		ChessPiece = (ChessPiece)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);

		return true;
	}

	// call to inits enemy and give stats to enemy
	public void Init(EnemyStatistics Statistics)
	{
		this.Statistics = Statistics;
		switch (ChessPiece)
		{
			case ChessPiece.Pawn:
				this.sprite = PawnSprite;
				break;
			case ChessPiece.Knight:
				this.sprite = KnightSprite;
				break;
			case ChessPiece.Bishop:
				this.sprite = BishopSprite;
				break;
			case ChessPiece.Rook:
				this.sprite = RookSprite;
				break;
			case ChessPiece.Queen:
				this.sprite = QueenSprite;
				break;
			case ChessPiece.King:
				this.sprite = KingSprite;
				break;
		}
	}

	// called when enemy is died
	public void Death()
	{
	}
}