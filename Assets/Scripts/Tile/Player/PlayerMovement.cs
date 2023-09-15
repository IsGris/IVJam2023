using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Assertions;
using static ChessUtilities;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
	public static PlayerMovement instance;
	[SerializeField] private Tilemap BoardTileMap = null;
	[SerializeField] private Tilemap HighlightMovesTileMap = null;

	[SerializeField] private TileBase EquipHighlightTile = null;
	[SerializeField] private TileBase EmptyHighlightTile = null;
	[SerializeField] private TileBase AttackHighlightTile = null;

	[SerializeField] private Vector2Int StartBoardPos;
	[SerializeField] private Vector2Int EndBoardPos;

	private bool IsHighlighting = false;

	private void Awake()
	{
		Assert.IsNotNull(BoardTileMap);
		Assert.IsNotNull(HighlightMovesTileMap);
		Assert.IsNotNull(EquipHighlightTile);
		Assert.IsNotNull(EmptyHighlightTile);
		Assert.IsNotNull(AttackHighlightTile);
		instance = this;
		ChessUtilities.Init(BoardTileMap, StartBoardPos, EndBoardPos, EquipHighlightTile, EmptyHighlightTile, AttackHighlightTile);
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && !PlayerTile.instance.IsFighting)
		{
			Vector3Int ClickPosition = BoardTileMap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			TileBase ClickedTile = BoardTileMap.GetTile(ClickPosition);

			if (ClickedTile == null && !IsHighlighting)
				return;

			if (IsHighlighting)
			{
				if (IsAvialibleToMove(PlayerTile.instance.Location, new Vector2Int(ClickPosition.x, ClickPosition.y), PlayerTile.instance.ChessPiece))
				{
					TryGetBenefit(new Vector2Int(ClickPosition.x, ClickPosition.y));
					MovePlayer(new Vector2Int(ClickPosition.x, ClickPosition.y));
					HighlightMovesTileMap.ClearAllTiles();
					IsHighlighting = false;
					List<EnemyTile> AllFightingTiles = new List<EnemyTile>();

					bool IsPlayerAttackFirst = false;
					if (IsEnemyTile(ClickedTile))
					{
						IsPlayerAttackFirst = true;
						AllFightingTiles.Add(ClickedTile as EnemyTile);
					}
					AllFightingTiles.AddRange(GetAllEnemiesThatCanAttackTile(PlayerTile.instance.Location));

					if (AllFightingTiles.Count > 0)
					{
						PlayerTile.instance.IsFighting = true;
						EnemyTile FirstEnemy = AllFightingTiles[0];
						AllFightingTiles.RemoveAt(0);
						StartCoroutine(PlayerTile.instance.StartAttack(IsPlayerAttackFirst, FirstEnemy, AllFightingTiles));
					}
				} else
				{
					HighlightMovesTileMap.ClearAllTiles();
					IsHighlighting = false;
				}
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
		PlayerTile.IsMoving = false;
	}

	private void TryGetBenefit(in Vector2Int BenefitPosition)
	{
        if ( IsBenefitTile(BoardTileMap.GetTile(new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0))) )
        {
			BenefitTile tile = BoardTileMap.GetTile(new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0)) as BenefitTile;
			switch (tile.Type) 
			{
				case BenefitTile.BenefitType.Attack:
					PlayerTile.instance.Statistics.Attack += tile.Amount;
					break;
				case BenefitTile.BenefitType.Health:
					PlayerTile.instance.Statistics.Health += tile.Amount;
					break;
				case BenefitTile.BenefitType.Armor:
					PlayerTile.instance.Statistics.Armor += tile.Amount;
					break;
			}
			BoardTileMap.SetTile(new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0), null);
        }
    }

	private bool IsAvialibleToMove(in Vector2Int StartPosition, in Vector2Int EndPosition, in ChessPiece MovingChessPiece)
	{
		if (EndPosition.y > StartBoardPos.y || EndPosition.x < StartBoardPos.x || EndPosition.x > EndBoardPos.x)
			return false;

		if (GetAvialibleMoves(StartPosition, MovingChessPiece).Contains(EndPosition))
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
	
	
}
