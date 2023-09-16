using TreeEditor;
using Unity.VisualScripting;
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

	// sprites which will be used for cage design depending on the type of benefit
	[SerializeField] public Sprite AttackSprite;
	[SerializeField] public Sprite ArmorSprite;
	[SerializeField] public Sprite HealthSprite;

	[SerializeField] private TileBase PawnSprite;
	[SerializeField] private TileBase KnightSprite;
	[SerializeField] private TileBase BishopSprite;
	[SerializeField] private TileBase RookSprite;
	[SerializeField] private TileBase QueenSprite;
	[SerializeField] private TileBase KingSprite;

	public void Init(BenefitType Type, ChessPiece? piece = null, Vector3Int? pos = null)
	{
		
		// inits
		switch (Type) 
		{
			case BenefitType.Attack:
				this.sprite = HealthSprite;
				break;
			case BenefitType.Armor:
				this.sprite = AttackSprite; 
				break;
			case BenefitType.Health:
				this.sprite = ArmorSprite;
				break;
			case BenefitType.Skin:
				switch (piece)
				{
					case ChessPiece.Pawn:
						GameObject.FindGameObjectWithTag("SpriteTileMap").GetComponent<Tilemap>().SetTile((Vector3Int)pos, PawnSprite);
						break;
					case ChessPiece.Knight:
						GameObject.FindGameObjectWithTag("SpriteTileMap").GetComponent<Tilemap>().SetTile((Vector3Int)pos, KnightSprite);
						break;
					case ChessPiece.Bishop:
						GameObject.FindGameObjectWithTag("SpriteTileMap").GetComponent<Tilemap>().SetTile((Vector3Int)pos, BishopSprite);
						break;
					case ChessPiece.Rook:
						GameObject.FindGameObjectWithTag("SpriteTileMap").GetComponent<Tilemap>().SetTile((Vector3Int)pos, RookSprite);
						break;
					case ChessPiece.Queen:
						GameObject.FindGameObjectWithTag("SpriteTileMap").GetComponent<Tilemap>().SetTile((Vector3Int)pos, QueenSprite);
						break;
					case ChessPiece.King:
						GameObject.FindGameObjectWithTag("SpriteTileMap").GetComponent<Tilemap>().SetTile((Vector3Int)pos, KingSprite);
						break;
				}
				break;
		}
	}

}