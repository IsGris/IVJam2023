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
		// inits
		if (Tilemap == null)
			Tilemap = GetComponent<Tilemap>();

		Assert.IsNotNull(Tilemap);
		Assert.IsNotNull(Tile);

		this.Fill();
	}

	// fill all from StartPos to EndPos using Tile in Tilemap from class
	public void Fill()
	{
		Fill(StartPos, EndPos);
	}

	// fill all from StartPos to EndPos using Tile in Tilemap from class
	public void Fill(in Vector2Int StartPos, in Vector2Int EndPos)
	{
		int minX = Mathf.Min(StartPos.x, EndPos.x);
		int maxX = Mathf.Max(StartPos.x, EndPos.x);
		int minY = Mathf.Min(StartPos.y, EndPos.y);
		int maxY = Mathf.Max(StartPos.y, EndPos.y);

		for (int x = minX; x <= maxX; x++) // iterate through each cell and set it to tile
		{
			for (int y = minY; y <= maxY; y++)
			{
				this.Tilemap.SetTile(new Vector3Int(x, y, 0), this.Tile);
			}
		}
	}
}
