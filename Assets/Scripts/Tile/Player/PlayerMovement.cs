using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] private Tilemap BoardTileMap = null;
	[SerializeField] private Tilemap HighlightMovesTileMap = null;

	[SerializeField] private TileBase EquipHighlightTile = null;
	[SerializeField] private TileBase EmptyHighlightTile = null;
	[SerializeField] private TileBase AttackHighlightTile = null;

	[SerializeField] private Vector2Int StartBoardPos;
	[SerializeField] private Vector2Int EndBoardPos;

	private bool IsHighlighting = false;

	private void Start()
	{
		Assert.IsNotNull(BoardTileMap);
		Assert.IsNotNull(HighlightMovesTileMap);
		Assert.IsNotNull(EquipHighlightTile);
		Assert.IsNotNull(EmptyHighlightTile);
		Assert.IsNotNull(AttackHighlightTile);
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector3Int ClickPosition = BoardTileMap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			TileBase ClickedTile = BoardTileMap.GetTile(ClickPosition);

			if (ClickedTile == null && !IsHighlighting)
				return;


			if (IsHighlighting)
			{
				if (IsAvialibleToMove(PlayerTile.instance.Location, new Vector2Int(ClickPosition.x, ClickPosition.y), PlayerTile.instance.ChessPiece))
					MovePlayer(new Vector2Int(ClickPosition.x, ClickPosition.y));
			}
			else if (ClickedTile is PlayerTile)
			{
				HighlightCellsToMove(PlayerTile.instance.ChessPiece);
				IsHighlighting = true;
			}
		}
	}

	private void MovePlayer(in Vector2Int EndPosition)
	{
		PlayerTile.IsMoving = true;
		Vector2Int StartLocation = PlayerTile.instance.Location;
		BoardTileMap.SetTile(new Vector3Int(EndPosition.x, EndPosition.y, 0), PlayerTile.instance);
		BoardTileMap.SetTile(new Vector3Int(StartLocation.x, StartLocation.y, 0), null);
		HighlightMovesTileMap.ClearAllTiles();
		IsHighlighting = false;
		PlayerTile.IsMoving = false;
	}

	private bool IsAvialibleToMove(in Vector2Int StartPosition, in Vector2Int EndPosition, in ChessPiece MovingChessPiece)
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

		if (AvialiblePosition.Contains(EndPosition))
			return true;
		else
			return false;
	}

	private void HighlightCellsToMove(in ChessPiece MovingChessPiece)
	{
		Vector2Int PlayerPosition = PlayerTile.instance.Location;
		switch (MovingChessPiece)
		{
			case ChessPiece.Pawn:
				{
					TileBase ForwardTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + 1, 0));
					TileBase ForwardLeftTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y + 1, 0));
					TileBase ForwardRightTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y + 1, 0));

					if ( !IsEmptyTile(ForwardTile) && (IsEmptyTile(ForwardLeftTile) && IsEmptyTile(ForwardRightTile)) ) // can attack forward if moving is blocked
					{
						HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + 1, 0), AttackHighlightTile);
					} 
					else
					{
						if (IsEmptyTile(ForwardTile))
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + 1, 0), GetTileHighlight(ForwardTile));
						if (IsEnemyTile(ForwardLeftTile))
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y + 1, 0), AttackHighlightTile);
						if (IsEnemyTile(ForwardRightTile))
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y + 1, 0), AttackHighlightTile);
					}
				}
				break;
			case ChessPiece.Knight:
				{
					TileBase ForwardLeftTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y + 2, 0));
					TileBase ForwardRightTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y + 2, 0));
					TileBase LeftForwardTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x - 2, PlayerPosition.y + 1, 0));
					TileBase RightForwardTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + 2, PlayerPosition.y + 1, 0));

					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y + 2, 0), GetTileHighlight(ForwardLeftTile));
					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y + 2, 0), GetTileHighlight(ForwardRightTile));
					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x - 2, PlayerPosition.y + 1, 0), GetTileHighlight(LeftForwardTile));
					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + 2, PlayerPosition.y + 1, 0), GetTileHighlight(RightForwardTile));
				}
				break;
			case ChessPiece.Bishop:
				{
					for (int IsLeft = 0; IsLeft < 2; IsLeft++)
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++)
						{
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0))))
							{
								HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0), AttackHighlightTile);
								break;
							}
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0))));
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
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0))))
							{
								HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), AttackHighlightTile);
								break;
							}
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0))));
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++)
					{
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0))))
						{
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), AttackHighlightTile);
							break;
						}
						HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0))));
					}
				}
				break;
			case ChessPiece.Queen:
				{
					for (int IsLeft = 0; IsLeft < 2; IsLeft++)
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++)
						{
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0))))
							{
								HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0), AttackHighlightTile);
								break;
							}
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0))));
						}
					}
					for (int IsLeft = 0; IsLeft < 2; IsLeft++)
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++)
						{
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0))))
							{
								HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), AttackHighlightTile);
								break;
							}
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0))));
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++)
					{
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0))))
						{
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), AttackHighlightTile);
							break;
						}
						HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0))));
					}
				}
				break;
			case ChessPiece.King:
				{
					TileBase ForwardTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + 1, 0));
					TileBase ForwardRightTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y + 1, 0));
					TileBase ForwardLeftTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y + 1, 0));
					TileBase LeftTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y, 0));
					TileBase RightTile = BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y, 0));

					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + 1, 0), GetTileHighlight(ForwardTile));
					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y + 1, 0), GetTileHighlight(ForwardRightTile));
					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y + 1, 0), GetTileHighlight(ForwardLeftTile));
					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x - 1, PlayerPosition.y, 0), GetTileHighlight(LeftTile));
					HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + 1, PlayerPosition.y, 0), GetTileHighlight(RightTile));
				}
				break;
		}
	}
	
	private bool IsBenefitTile(in TileBase tile)
	{
		if (tile is BenefitTile)
			return true;
		else
			return false;
	}

	private bool IsEmptyTile(in TileBase tile)
	{
		if (tile == null || tile is BenefitTile)
			return true;
		else
			return false;
	}

	private bool IsEnemyTile(in TileBase tile)
	{
		if (tile is EnemyTile)
			return true;
		else
			return false;
	}

	private TileBase GetTileHighlight(in TileBase tile)
	{
		if (IsEnemyTile(tile))
			return AttackHighlightTile;
		else if (IsBenefitTile(tile))
			return EquipHighlightTile;
		else 
			return EmptyHighlightTile;
	}
}
