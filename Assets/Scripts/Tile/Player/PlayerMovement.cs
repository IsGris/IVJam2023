using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Assertions;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] private Tilemap BoardTileMap = null;
	[SerializeField] private Tilemap HighlightMovesTileMap = null;

	[SerializeField] private TileBase EquipHighlightTile = null;
	[SerializeField] private TileBase EmptyHighlightTile = null;
	[SerializeField] private TileBase AttackHighlightTile = null;

	[SerializeField] private Vector2Int StartBoardPos;
	[SerializeField] private Vector2Int EndBoardPos;
	
	private bool IsMoving = false;

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
			TileBase clickedTile = BoardTileMap.GetTile(ClickPosition);

			if (clickedTile == null && !IsMoving)
				return;
			else if (IsMoving)
			{

			}
			else if (clickedTile is PlayerTile)
			{
				HighlightCellsToMove(PlayerTile.instance.ChessPiece);
				IsMoving = true;
			}
		}
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
