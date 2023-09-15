using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/ChessBoardTile")]
public class ChessBoardTile : Tile
{

	[SerializeField] private Sprite WhiteSprite;
	[SerializeField] private Sprite BlackSprite;

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		base.GetTileData(position, tilemap, ref tileData);
		if ((position.x + position.y) % 2 == 0)
			tileData.sprite = WhiteSprite;
		else
			tileData.sprite = BlackSprite;
	}
}