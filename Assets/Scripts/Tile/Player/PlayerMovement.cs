using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Assertions;
using static ChessUtilities;
using System.Collections.Generic;

// using for player to move
public class PlayerMovement : MonoBehaviour
{
	public static PlayerMovement instance; // can be only on component in scene

	// tilemaps that used for implementation of class
	[SerializeField] private Tilemap BoardTileMap = null;
	[SerializeField] private Tilemap SpriteTileMap = null;
	[SerializeField] public Tilemap HighlightMovesTileMap = null;

	// tiles that used for implementation of class
	[SerializeField] private TileBase EquipHighlightTile = null;
	[SerializeField] private TileBase EmptyHighlightTile = null;
	[SerializeField] public TileBase AttackHighlightTile = null;

	[SerializeField] private AudioSource SFX;
	[SerializeField] private AudioSource Music;
	[SerializeField] private AudioClip MoveSound;
	[SerializeField] private AudioClip DeathSound;
	[SerializeField] private AudioClip AttackSound;

	// board bounds
	[SerializeField] private Vector2Int StartBoardPos;
	[SerializeField] private Vector2Int EndBoardPos;

	private Vector2Int? ReservedPosition = null;

	// true if highlighted available moves on board
	private bool IsHighlighting = false;

	private void Awake()
	{
		// inits
		Assert.IsNotNull(BoardTileMap);
		Assert.IsNotNull(HighlightMovesTileMap);
		Assert.IsNotNull(EquipHighlightTile);
		Assert.IsNotNull(EmptyHighlightTile);
		Assert.IsNotNull(AttackHighlightTile);
		instance = this;
		ChessUtilities.Init(BoardTileMap, StartBoardPos, EndBoardPos, EquipHighlightTile, EmptyHighlightTile, AttackHighlightTile);
		PlayerTile.DeathCanvas = GameObject.FindGameObjectWithTag("DeathCanvas");
		PlayerTile.DeathCanvas.SetActive(false);
		Music.Play();
	}

	void Update()
	{
		if (ReservedPosition != null && !PlayerTile.instance.IsFighting)
		{
			MovePlayer((Vector2Int)ReservedPosition);
			ReservedPosition = null;
		}

		if (Input.GetMouseButtonDown(0) && !PlayerTile.instance.IsFighting) // if press mouse button and can move then handle
		{
			Vector3Int ClickPosition = BoardTileMap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			TileBase ClickedTile = BoardTileMap.GetTile(ClickPosition);

			if (ClickedTile == null && !IsHighlighting) // if click to nowhere then return
				return;

			if (IsHighlighting) // check if currently need to move player
			{
				if (IsAvialibleToMove(PlayerTile.instance.Location, new Vector2Int(ClickPosition.x, ClickPosition.y), PlayerTile.instance.ChessPiece)) // if can move where player clicked than move
				{
					// try get benefit from cell then move
					TryGetBenefit(new Vector2Int(ClickPosition.x, ClickPosition.y));
					
					// clear highlights
					HighlightMovesTileMap.ClearAllTiles();
					IsHighlighting = false;
					
					List<Vector3Int> AllFightingTiles = new List<Vector3Int>(); // an array that contains all the enemies that we have to fight after our move

					bool IsPlayerAttackFirst = false;
					if (IsEnemyTile(ClickedTile)) // if our player attack some chess piece we will attack first
					{
						IsPlayerAttackFirst = true;
						AllFightingTiles.Add(ClickPosition);
					}
					AllFightingTiles.AddRange(GetAllEnemiesThatCanAttackTile(new Vector2Int(ClickPosition.x, ClickPosition.y))); // add all enemies that can attack us after move

					if (AllFightingTiles.Count > 0) // if have enemies to fight with then fight
					{
						// starting fights
						if (!IsPlayerAttackFirst)
						{
							MovePlayer(new Vector2Int(ClickPosition.x, ClickPosition.y));
						} else {
							ReservedPosition = new Vector2Int(ClickPosition.x, ClickPosition.y);
						}
						Vector3Int FirstEnemy = AllFightingTiles[0];
						AllFightingTiles.RemoveAt(0);
						StartCoroutine(PlayerTile.instance.StartAttack(IsPlayerAttackFirst, FirstEnemy, AllFightingTiles));
					} else
					{
						MovePlayer(new Vector2Int(ClickPosition.x, ClickPosition.y));
					}
				} else // if cannot move where player clicked than clear highlights
				{
					HighlightMovesTileMap.ClearAllTiles();
					IsHighlighting = false;
				}
			}
			else if (ClickedTile is PlayerTile) // check if need to highlight avialible moves
			{
				HighlightCellsToMove(PlayerTile.instance.ChessPiece);
				IsHighlighting = true;
			}
		}

		if (PlayerTile.instance.Location.y == StartBoardPos.y && !PlayerTile.instance.IsFighting)
		{
			ReachEnd();
		}
	}

	private void ReachEnd()
	{
		Spawner.instance.Spawn(ref PlayerTile.instance.Statistics);
	}

	public void PlayAttackSound()
	{
		SFX.PlayOneShot(AttackSound);
	}

	public void PlayDeathSound()
	{
		Music.Stop();
		SFX.PlayOneShot(DeathSound);
	}

	// function that moves player to another cell
	public void MovePlayer(in Vector2Int EndPosition)
	{
		PlayerTile.IsMoving = true;
		SFX.PlayOneShot(MoveSound);
		Vector2Int StartLocation = PlayerTile.instance.Location;
		BoardTileMap.SetTile(new Vector3Int(EndPosition.x, EndPosition.y, 0), PlayerTile.instance);
		BoardTileMap.SetTile(new Vector3Int(StartLocation.x, StartLocation.y, 0), null);
		PlayerTile.IsMoving = false;
	}

	public void StartNewLevel()
	{
		MovePlayer(new Vector2Int(StartBoardPos.x + 2, EndBoardPos.y));
	}

	// try to get benefit if cell have benefit in it
	private void TryGetBenefit(in Vector2Int BenefitPosition)
	{
        if ( IsBenefitTile(BoardTileMap.GetTile(new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0))) ) // if has benefit handle
        {
			BenefitTile tile = BoardTileMap.GetTile(new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0)) as BenefitTile; // get benefit in variable
			switch (Spawner.instance.BenefitsInBoard[new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0)].Item1) // depends on benefit add to stats
			{
				case BenefitTile.BenefitType.Attack:
					PlayerTile.instance.Statistics.Attack += Spawner.instance.BenefitsInBoard[new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0)].Item2;
					break;
				case BenefitTile.BenefitType.Health:
					PlayerTile.instance.Statistics.Health += Spawner.instance.BenefitsInBoard[new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0)].Item2;
					break;
				case BenefitTile.BenefitType.Armor:
					PlayerTile.instance.Statistics.Armor += Spawner.instance.BenefitsInBoard[new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0)].Item2;
					break;
				case BenefitTile.BenefitType.Skin:
					PlayerTile.instance.ChessPiece = (ChessPiece)Spawner.instance.BenefitsInBoard[new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0)].Item2;
					break;
			}
			BoardTileMap.SetTile(new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0), null); // remove benefit
			SpriteTileMap.SetTile(new Vector3Int(BenefitPosition.x, BenefitPosition.y, 0), null); // remove benefit
		}
    }

	// check if ChessPiece can move in some cell
	private bool IsAvialibleToMove(in Vector2Int StartPosition, in Vector2Int EndPosition, in ChessPiece MovingChessPiece)
	{
		if (EndPosition.y > StartBoardPos.y || EndPosition.x < StartBoardPos.x || EndPosition.x > EndBoardPos.x) // if not in board then false
			return false;

		if (GetAvialiblPlayereMoves(StartPosition, MovingChessPiece).Contains(EndPosition)) // if can move by rules of chess than true else no
			return true;
		else
			return false;
	}

	// function that highlight all cells where player can move
	private void HighlightCellsToMove(in ChessPiece MovingChessPiece)
	{
		Vector2Int PlayerPosition = PlayerTile.instance.Location; // current player position
		
		switch (MovingChessPiece) // depends on what ChessPiece we check avialible moves
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
					else // if not blocked use standart pawn rules
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
				{ // all knight moves
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
				{ // all bishop moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left diagonal else right diagonal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create diagonal
						{
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0)))) // check if enemy in path then stop diagonal in this position
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
				{ // all rook moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left horizontal else right horizontal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create horizontal
						{
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0)))) // check if enemy in path then stop horizontal in this position
							{
								HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), AttackHighlightTile);
								break;
							}
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0))));
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++) // create vertical
					{
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0)))) // check if enemy in path then stop vertical in this position
						{
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), AttackHighlightTile);
							break;
						}
						HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0))));
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
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0)))) // check if enemy in path then stop diagonal in this position
							{
								HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0), AttackHighlightTile);
								break;
							}
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y + i, 0))));
						}
					}

					// all rook moves(can go out of bounds)
					for (int IsLeft = 0; IsLeft < 2; IsLeft++) // if left then create left horizontal else right horizontal
					{
						for (int i = 1; i <= Mathf.Max(StartBoardPos.x, EndBoardPos.x) - Mathf.Min(StartBoardPos.x, EndBoardPos.x); i++) // create horizontal
						{
							if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0)))) // check if enemy in path then stop horizontal in this position
							{
								HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), AttackHighlightTile);
								break;
							}
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x + (IsLeft == 0 ? -i : i), PlayerPosition.y, 0))));
						}
					}
					for (int i = 1; i <= Mathf.Max(StartBoardPos.y, EndBoardPos.y) - Mathf.Min(StartBoardPos.y, EndBoardPos.y); i++) // create vertical
					{
						if (IsEnemyTile(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0)))) // check if enemy in path then stop vertical in this position
						{
							HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), AttackHighlightTile);
							break;
						}
						HighlightMovesTileMap.SetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0), GetTileHighlight(BoardTileMap.GetTile(new Vector3Int(PlayerPosition.x, PlayerPosition.y + i, 0))));
					}
				}
				break;
			case ChessPiece.King:
				{ // all king moves
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
