using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/EnemyTile")]
public class EnemyTile : Tile
{
	[NonSerialized] public ChessPiece ChessPiece;
	[NonSerialized] public EnemyStatistics Statistics;
	
	[SerializeField] private Sprite PawnSprite;
	[SerializeField] private Sprite KnightSprite;
	[SerializeField] private Sprite BishopSprite;
	[SerializeField] private Sprite RookSprite;
	[SerializeField] private Sprite QueenSprite;
	[SerializeField] private Sprite KingSprite;

	public Vector2Int Location;

	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
	{
		Location = new Vector2Int(location.x, location.y);
		ChessPiece = (ChessPiece)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);

		return true;
	}

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

	public void Death()
	{
	}
}