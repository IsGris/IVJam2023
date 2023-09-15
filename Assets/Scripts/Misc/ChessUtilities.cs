using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ChessUtilities
{
	public static Tilemap BoardTileMap;
	public static Vector2Int StartBoardPos;
	public static Vector2Int EndBoardPos;
	public static TileBase EquipHighlightTile;
	public static TileBase EmptyHighlightTile;
	public static TileBase AttackHighlightTile;

	public static void Init(in Tilemap BoardTileMap, in Vector2Int StartBoardPos, in Vector2Int EndBoardPos, in TileBase EquipHighlightTile, in TileBase EmptyHighlightTile, in TileBase AttackHighlightTile)
	{
		ChessUtilities.BoardTileMap = BoardTileMap;
		ChessUtilities.StartBoardPos = StartBoardPos;
		ChessUtilities.EndBoardPos = EndBoardPos;
		ChessUtilities.EquipHighlightTile = EquipHighlightTile;
		ChessUtilities.EmptyHighlightTile = EmptyHighlightTile;
		ChessUtilities.AttackHighlightTile = AttackHighlightTile;
	}

	public static List<EnemyTile> GetAllEnemiesThatCanAttackTile(in Vector2Int tile)
	{
		List<EnemyTile> AllEnemies = new List<EnemyTile>();
		List<EnemyTile> result = new List<EnemyTile>();
		for (int x = BoardTileMap.cellBounds.x; x < BoardTileMap.cellBounds.x + BoardTileMap.cellBounds.size.x; x++)
		{
			for (int y = BoardTileMap.cellBounds.y; y < BoardTileMap.cellBounds.y + BoardTileMap.cellBounds.size.y; y++)
			{
				Vector3Int CellPosition = new Vector3Int(x, y, 0);
				TileBase CurrentTile = BoardTileMap.GetTile(CellPosition);
				if (CurrentTile is EnemyTile)
					AllEnemies.Add(CurrentTile as EnemyTile);
			}
		}

		foreach (EnemyTile Enemy in AllEnemies) 
		{
			if (GetAvialibleMoves(Enemy.Location, Enemy.ChessPiece).Contains(tile))
				result.Add(Enemy);
		}
		return result;
	}

	public static List<Vector2Int> GetAvialibleMoves(in Vector2Int StartPosition, in ChessPiece MovingChessPiece)
    {
		List<Vector2Int> AvialiblePosition = new List<Vector2Int>();

		switch (MovingChessPiece)
		{
			case ChessPiece.Pawn:
				{
					TileBase ForwardTile = BoardTileMap.GetTile(new Vector3Int(StartPosition.x, StartPosition.y + 1, 0));
					TileBase ForwardLeftTile = BoardTileMap.GetTile(new Vector3Int(StartPosition.x - 1, StartPosition.y + 1, 0));
					TileBase ForwardRightTile = BoardTileMap.GetTile(new Vector3Int(StartPosition.x + 1, StartPosition.y + 1, 0));

					if (!IsEmptyTile(ForwardTile) && (IsEmptyTile(ForwardLeftTile) && IsEmptyTile(ForwardRightTile))) // can attack forward if moving is blocked
					{
						AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + 1));
					}
					else
					{
						if (IsEmptyTile(ForwardTile))
							AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + 1));
						if (IsEnemyTile(ForwardLeftTile))
							AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y + 1));
						if (IsEnemyTile(ForwardRightTile))
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y + 1));
					}
				}
				break;
			case ChessPiece.Knight:
				{
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y + 2));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y + 2));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 2, StartPosition.y + 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 2, StartPosition.y + 1));
				}
				break;
			case ChessPiece.Bishop:
				{
					for (int IsLeft = 0; IsLeft < 2; IsLeft++)
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++)
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i, 0))))
								break;
						}
					}
				}
				break;
			case ChessPiece.Rook:
				{
					for (int IsLeft = 0; IsLeft < 2; IsLeft++)
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++)
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y, 0))))
								break;
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++)
					{
						AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + i));
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x, StartPosition.y + i, 0))))
							break;
					}
				}
				break;
			case ChessPiece.Queen:
				{
					for (int IsLeft = 0; IsLeft < 2; IsLeft++)
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++)
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i, 0))))
								break;
						}
					}
					for (int IsLeft = 0; IsLeft < 2; IsLeft++)
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++)
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y, 0))))
								break;
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++)
					{
						AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + i));
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x, StartPosition.y + i, 0))))
							break;
					}
				}
				break;
			case ChessPiece.King:
				{

					AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y + 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y + 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y));
				}
				break;
		}
		return AvialiblePosition;
	}
	public static bool IsBenefitTile(in TileBase tile)
	{
		if (tile is BenefitTile)
			return true;
		else
			return false;
	}

	public static bool IsEmptyTile(in TileBase tile)
	{
		if (tile == null || tile is BenefitTile)
			return true;
		else
			return false;
	}

	public static bool IsEnemyTile(in TileBase tile)
	{
		if (tile is EnemyTile)
			return true;
		else
			return false;
	}

	public static TileBase GetTileHighlight(in TileBase tile)
	{
		if (IsEnemyTile(tile))
			return AttackHighlightTile;
		else if (IsBenefitTile(tile))
			return EquipHighlightTile;
		else
			return EmptyHighlightTile;
	}
}
