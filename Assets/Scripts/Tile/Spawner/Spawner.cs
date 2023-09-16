using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Stats;

public class Spawner : MonoBehaviour
{
	public static Spawner instance;
	[SerializeField] public Tilemap Board;
	[SerializeField] public Tilemap SpriteTileMap;

	[SerializeField] private PlayerTile Player;
	[SerializeField] private EnemyTile Enemy;
	[SerializeField] private BenefitTile Benefit;

	[SerializeField] private Vector2Int StartBoardPos;
	[SerializeField] private Vector2Int EndBoardPos;

	[SerializeField] private TileBase EnemyPawnSprite;
	[SerializeField] private TileBase EnemyKnightSprite;
	[SerializeField] private TileBase EnemyBishopSprite;
	[SerializeField] private TileBase EnemyRookSprite;
	[SerializeField] private TileBase EnemyQueenSprite;
	[SerializeField] private TileBase EnemyKingSprite;

	[NonSerialized] public Dictionary<Vector3Int, Tuple<BenefitTile.BenefitType, int>> BenefitsInBoard = new Dictionary<Vector3Int, Tuple<BenefitTile.BenefitType, int>>();
	[NonSerialized] public Dictionary<Vector3Int, Tuple<ChessPiece, EnemyStatistics>> EnemiesInBoard = new Dictionary<Vector3Int, Tuple<ChessPiece, EnemyStatistics>>();

	private void Start()
	{
		instance = this;
		Board.SetTile(new Vector3Int(UnityEngine.Random.Range(StartBoardPos.x, EndBoardPos.x + 1), EndBoardPos.y), Player);
		PlayerTile.instance.Statistics.Level = 0;
		Spawn(ref PlayerTile.instance.Statistics);
	}

	public void Spawn(ref PlayerStatistics statistics)
	{
		BenefitsInBoard.Clear();
		EnemiesInBoard.Clear();
		Board.ClearAllTiles();
		SpriteTileMap.ClearAllTiles();

		PlayerMovement.instance.StartNewLevel();

		while (Board.GetTile(new Vector3Int(StartBoardPos.x + 2, EndBoardPos.y)) == null)
		{
			PlayerMovement.instance.StartNewLevel();
		}

		statistics.Level += 1;
		if (statistics.Level == 1) 
		{
			statistics.Health = GetLevelHealth(statistics.Level);
			statistics.Attack += GetLevelAttack(statistics.Level);
			statistics.Armor += GetLevelArmor(statistics.Level);
		}
		
		for (int i = 0; i < 3; i++)
		{
			Vector3Int CurrentBenefitPos;
			while (true)
			{
				CurrentBenefitPos = new Vector3Int(UnityEngine.Random.Range(StartBoardPos.x, EndBoardPos.x + 1), UnityEngine.Random.Range(StartBoardPos.y - 1, EndBoardPos.y));
				if (Board.GetTile(CurrentBenefitPos) == null)
					break;
			}

			Board.SetTile(CurrentBenefitPos, Benefit);

			switch (i)
			{
				case 0:
					BenefitsInBoard.Add(CurrentBenefitPos, new Tuple<BenefitTile.BenefitType, int>(BenefitTile.BenefitType.Attack, GetLevelItemAttack(statistics.Level)));
					(Board.GetTile(CurrentBenefitPos) as BenefitTile).Init(BenefitTile.BenefitType.Attack);
					break;
				case 1:
					BenefitsInBoard.Add(CurrentBenefitPos, new Tuple<BenefitTile.BenefitType, int>(BenefitTile.BenefitType.Health, GetLevelItemHealth(statistics.Level)));
					(Board.GetTile(CurrentBenefitPos) as BenefitTile).Init(BenefitTile.BenefitType.Health);
					break;
				case 2:
					BenefitsInBoard.Add(CurrentBenefitPos, new Tuple<BenefitTile.BenefitType, int>(BenefitTile.BenefitType.Armor, GetLevelItemArmor(statistics.Level)));
					(Board.GetTile(CurrentBenefitPos) as BenefitTile).Init(BenefitTile.BenefitType.Armor);
					break;
			}
		}

		/*for (int i = 0; i < 2; i++)
		{
			Vector3Int CurrentBenefitPos;
			while (true)
			{
				CurrentBenefitPos = new Vector3Int(UnityEngine.Random.Range(StartBoardPos.x, EndBoardPos.x + 1), UnityEngine.Random.Range(StartBoardPos.y - 1, EndBoardPos.y));
				if (Board.GetTile(CurrentBenefitPos) == null)
					break;
			}
			Board.SetTile(CurrentBenefitPos, Benefit);

			int NewSkin = UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);

			while (NewSkin == (int)PlayerTile.instance.ChessPiece)
			{
				NewSkin = UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);
			}

			(Board.GetTile(CurrentBenefitPos) as BenefitTile).Init(BenefitTile.BenefitType.Skin, (ChessPiece)NewSkin, CurrentBenefitPos);

			BenefitsInBoard.Add(CurrentBenefitPos, new Tuple<BenefitTile.BenefitType, int>(BenefitTile.BenefitType.Skin, NewSkin));
		}*/

		for (int i = 0; i < 5; i++)
		{
			Vector3Int CurrentEnemyPos;
			while (true)
			{
				CurrentEnemyPos = new Vector3Int(UnityEngine.Random.Range(StartBoardPos.x, EndBoardPos.x + 1), UnityEngine.Random.Range(StartBoardPos.y - 1, EndBoardPos.y));
				if (Board.GetTile(CurrentEnemyPos) == null)
					break;
			}

			Board.SetTile(CurrentEnemyPos, Enemy);

			ChessPiece piece = (ChessPiece)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length - 1);
			EnemyStatistics enemyStatistics = new EnemyStatistics();
			enemyStatistics.Attack = GetLevelEnemyAttack(statistics.Level);
			enemyStatistics.Health = GetLevelEnemyHealth(statistics.Level);
			enemyStatistics.Armor = GetLevelEnemyArmor(statistics.Level);
			EnemiesInBoard.Add(CurrentEnemyPos, new Tuple<ChessPiece, EnemyStatistics>(piece, enemyStatistics));
			switch (piece)
			{
				case ChessPiece.Pawn:
					SpriteTileMap.SetTile(CurrentEnemyPos, EnemyPawnSprite);
					break;
				case ChessPiece.Knight:
					SpriteTileMap.SetTile(CurrentEnemyPos, EnemyKnightSprite);
					break;
				case ChessPiece.Bishop:
					SpriteTileMap.SetTile(CurrentEnemyPos, EnemyBishopSprite);
					break;
				case ChessPiece.Rook:
					SpriteTileMap.SetTile(CurrentEnemyPos, EnemyRookSprite);
					break;
				case ChessPiece.Queen:
					SpriteTileMap.SetTile(CurrentEnemyPos, EnemyQueenSprite);
					break;
				case ChessPiece.King:
					SpriteTileMap.SetTile(CurrentEnemyPos, EnemyKingSprite);
					break;
			}
			
		}

		if (statistics.Level > 0 && statistics.Level % 5 == 0) 
		{
			Vector3Int CurrentBossPos = new Vector3Int(EndBoardPos.x - 2, StartBoardPos.y);
			Board.SetTile(CurrentBossPos, Enemy);
			ChessPiece piece = ChessPiece.King;
			EnemyStatistics enemyStatistics = new EnemyStatistics();
			enemyStatistics.Attack = GetLevelEnemyAttack(statistics.Level);
			enemyStatistics.Health = GetLevelEnemyHealth(statistics.Level);
			enemyStatistics.Armor = GetLevelEnemyArmor(statistics.Level);
			EnemiesInBoard.Add(CurrentBossPos, new Tuple<ChessPiece, EnemyStatistics>(piece, enemyStatistics));
			switch (piece)
			{
				case ChessPiece.Pawn:
					SpriteTileMap.SetTile(CurrentBossPos, EnemyPawnSprite);
					break;
				case ChessPiece.Knight:
					SpriteTileMap.SetTile(CurrentBossPos, EnemyKnightSprite);
					break;
				case ChessPiece.Bishop:
					SpriteTileMap.SetTile(CurrentBossPos, EnemyBishopSprite);
					break;
				case ChessPiece.Rook:
					SpriteTileMap.SetTile(CurrentBossPos, EnemyRookSprite);
					break;
				case ChessPiece.Queen:
					SpriteTileMap.SetTile(CurrentBossPos, EnemyQueenSprite);
					break;
				case ChessPiece.King:
					SpriteTileMap.SetTile(CurrentBossPos, EnemyKingSprite);
					break;
			}
			
		}
	}
}
