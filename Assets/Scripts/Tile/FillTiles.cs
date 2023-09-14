using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class FillTiles : MonoBehaviour
{
    [SerializeField] private Tilemap Tilemap;
	[SerializeField] private TileBase Tile;
	[SerializeField] private Vector2Int StartPos;
	[SerializeField] private Vector2Int EndPos;

	void Start()
    {
		if (Tilemap == null)
			Tilemap = GetComponent<Tilemap>();

		Assert.IsNotNull(Tilemap);
		Assert.IsNotNull(Tile);
	}

	public void Fill()
	{
		Fill(StartPos, EndPos);
	}

	public void Fill(in Vector2 StartPos, in Vector2 EndPos)
	{
		for (int i = (int)StartPos.x; i != EndPos.x + (StartPos.x < EndPos.x ? 1 : -1); i += (StartPos.x < EndPos.x ? 1 : -1))
		{
			for (int k = (int)StartPos.y; k != EndPos.x + (StartPos.y < EndPos.y ? 1 : -1); k += (StartPos.y < EndPos.y ? 1 : -1))
			{
				this.Tilemap.SetTile(new Vector3Int(i, k, 0), this.Tile);
			}
		}
	}
}
