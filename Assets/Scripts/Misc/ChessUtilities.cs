using System.Collections.Generic;
using Unity.VisualScripting;
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
		// init all variables
		ChessUtilities.BoardTileMap = BoardTileMap;
		ChessUtilities.StartBoardPos = StartBoardPos;
		ChessUtilities.EndBoardPos = EndBoardPos;
		ChessUtilities.EquipHighlightTile = EquipHighlightTile;
		ChessUtilities.EmptyHighlightTile = EmptyHighlightTile;
		ChessUtilities.AttackHighlightTile = AttackHighlightTile;
	}

	public static List<Vector3Int> GetAllEnemiesThatCanAttackTile(in Vector2Int tile)
	{
		List<EnemyTile> AllEnemies = new List<EnemyTile>(); // all enemies
		List<Vector3Int> result = new List<Vector3Int>(); // enemies that can attack tile
		for (int x = BoardTileMap.cellBounds.x; x < BoardTileMap.cellBounds.x + BoardTileMap.cellBounds.size.x; x++) // get all enemies
		{
			for (int y = BoardTileMap.cellBounds.y; y < BoardTileMap.cellBounds.y + BoardTileMap.cellBounds.size.y; y++)
			{
				Vector3Int CellPosition = new Vector3Int(x, y, 0);
				TileBase CurrentTile = BoardTileMap.GetTile(CellPosition);
				if (CurrentTile is EnemyTile)
					AllEnemies.Add(CurrentTile as EnemyTile);
			}
		}

		foreach (var Enemy in Spawner.instance.EnemiesInBoard) // get enemies that can attack tile
		{
			if (GetAvialiblEnemyMoves(new Vector2Int(Enemy.Key.x, Enemy.Key.y), Enemy.Value.Item1).Contains(tile))
				result.Add(Enemy.Key);
		}

		for (int i = 0; i < result.Count; i++)
		{
			if (Spawner.instance.EnemiesInBoard[result[i]].Item1 == ChessPiece.King) // if has king then king will be last
			{
				Vector3Int king = result[i];
				result.RemoveAt(i);
				result.Add(king);
			}
		}

		return result; // return enemies that can attack tile
	}

	public static List<Vector2Int> GetAvialiblEnemyMoves(in Vector2Int StartPosition, in ChessPiece MovingChessPiece)
	{
		List<Vector2Int> AvialiblePosition = new List<Vector2Int>(); // where we can move

		switch (MovingChessPiece) // depends on what ChessPiece we check avialible moves
		{
			case ChessPiece.Pawn:
				{
					TileBase ForwardTile = BoardTileMap.GetTile(new Vector3Int(StartPosition.x, StartPosition.y - 1, 0));
					TileBase ForwardLeftTile = BoardTileMap.GetTile(new Vector3Int(StartPosition.x - 1, StartPosition.y - 1, 0));
					TileBase ForwardRightTile = BoardTileMap.GetTile(new Vector3Int(StartPosition.x + 1, StartPosition.y - 1, 0));

					AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y - 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y - 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y - 1));
				}
				break;
			case ChessPiece.Knight:
				{ // all knight moves
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y - 2));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y - 2));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 2, StartPosition.y - 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 2, StartPosition.y - 1));
				}
				break;
			case ChessPiece.Bishop:
				{ // all bishop moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left diagonal else right diagonal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create diagonal
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y - i));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y - i, 0)))) // check if enemy in path then stop diagonal in this position
								break;
						}
					}
				}
				break;
			case ChessPiece.Rook:
				{ // all rook moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left horizontal else right horizontal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create horizontal
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y, 0)))) // check if enemy in path then stop horizontal in this position
								break;
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++) // create vertical
					{
						AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y - i));
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x, StartPosition.y - i, 0)))) // check if enemy in path then stop vertical in this position
							break;
					}
				}
				break;
			case ChessPiece.Queen:
				{ // bishop moves + rook moves = queen moves

					// all bishop moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left diagonal else right diagonal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create diagonal
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y - i));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y - i, 0))))  // check if enemy in path then stop diagonal in this position
								break;
						}
					}

					// all rook moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left horizontal else right horizontal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create horizontal
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y, 0)))) // check if enemy in path then stop horizontal in this position
								break;
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++) // create vertical
					{
						AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y - i));
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x, StartPosition.y - i, 0)))) // check if enemy in path then stop horizontal in this position
							break;
					}
				}
				break;
			case ChessPiece.King:
				{
					// all king moves
					AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y - 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y - 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y - 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y));
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left horizontal else right horizontal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create horizontal
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y, 0)))) // check if enemy in path then stop horizontal in this position
								break;
						}
					}
				}
				break;
		}
		return AvialiblePosition;
	}

	public static List<Vector2Int> GetAvialiblPlayereMoves(in Vector2Int StartPosition, in ChessPiece MovingChessPiece)
    {
		List<Vector2Int> AvialiblePosition = new List<Vector2Int>(); // where we can move

		switch (MovingChessPiece) // depends on what ChessPiece we check avialible moves
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
					else // if not blocked use standart pawn rules
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
				{ // all knight moves
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 1, StartPosition.y + 2));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 1, StartPosition.y + 2));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x - 2, StartPosition.y + 1));
					AvialiblePosition.Add(new Vector2Int(StartPosition.x + 2, StartPosition.y + 1));
				}
				break;
			case ChessPiece.Bishop:
				{ // all bishop moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left diagonal else right diagonal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create diagonal
						{
							if (Spawner.instance.EnemiesInBoard.ContainsKey(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i, 0))) // check if enemy in path then stop diagonal in this position
							{
								AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i));
								break;
							}
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i));
						}
					}
				}
				break;
			case ChessPiece.Rook:
				{ // all rook moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left horizontal else right horizontal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create horizontal
						{
							if (Spawner.instance.EnemiesInBoard.ContainsKey(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y, 0))) // check if enemy in path then stop horizontal in this position
							{
								AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
								break;
							}
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++) // create vertical
					{
						if (Spawner.instance.EnemiesInBoard.ContainsKey(new Vector3Int(StartPosition.x, StartPosition.y + i, 0))) // check if enemy in path then stop vertical in this position
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + i));
							break;
						}
						AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + i));
					}
				}
				break;
			case ChessPiece.Queen:
				{ // bishop moves + rook moves = queen moves

					// all bishop moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left diagonal else right diagonal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create diagonal
						{
							if (Spawner.instance.EnemiesInBoard.ContainsKey(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i, 0))) // check if enemy in path then stop diagonal in this position
							{
								AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i));
								break;
							}
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y + i));
						}
					}

					// all rook moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left horizontal else right horizontal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create horizontal
						{
							if (Spawner.instance.EnemiesInBoard.ContainsKey(new Vector3Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y, 0))) // check if enemy in path then stop horizontal in this position
							{ 
								AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
								break;
							}
							
							AvialiblePosition.Add(new Vector2Int(StartPosition.x + (IsLeft == 0 ? -i : i), StartPosition.y));
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++) // create vertical
					{
						if (Spawner.instance.EnemiesInBoard.ContainsKey(new Vector3Int(StartPosition.x, StartPosition.y + i, 0))) // check if enemy in path then stop vertical in this position
						{
							AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + i));
							break;
						}
						AvialiblePosition.Add(new Vector2Int(StartPosition.x, StartPosition.y + i));
					}
				}
				break;
			case ChessPiece.King:
				{
					// all king moves
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

	// check if tile has benefit in it
	public static bool IsBenefitTile(in TileBase tile)
	{
		if (tile is BenefitTile)
			return true;
		else
			return false;
	}

	// check if tile is empty
	public static bool IsEmptyTile(in TileBase tile)
	{
		if (tile == null || tile is BenefitTile)
			return true;
		else
			return false;
	}

	// check if tile is enemy
	public static bool IsEnemyTile(in TileBase tile)
	{
		if (tile is EnemyTile)
			return true;
		else
			return false;
	}

	// return TileBase depends on what tile tipe is in argument
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
