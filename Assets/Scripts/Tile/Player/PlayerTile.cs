using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using System.Collections;

// enum that represents ChessPiece
public enum ChessPiece
{
	Pawn, // пешка
	Knight, // конь
	Bishop, // слон
	Rook, // ладья
	Queen, // ферзь
	King // король
}

// tile that represents player
[CreateAssetMenu(menuName = "2D/Tiles/PlayerTile")]
public class PlayerTile : Tile
{
	[SerializeField] private Color HitColor; // color when somebody was hit (player or enemy)

	// sprites depends on what ChessPiece is using player
	[SerializeField] private Sprite PawnSprite;
	[SerializeField] private Sprite KnightSprite;
	[SerializeField] private Sprite BishopSprite;
	[SerializeField] private Sprite RookSprite;
	[SerializeField] private Sprite QueenSprite;
	[SerializeField] private Sprite KingSprite;

	[NonSerialized] public static GameObject DeathCanvas; // player tile can be only one
	[NonSerialized] public static PlayerTile instance; // player tile can be only one
	
	[NonSerialized] public bool IsFighting = false; // true when fights with some enemy
	[NonSerialized] public ChessPiece ChessPiece; // what chess piece does the player represent?
	public PlayerStatistics Statistics; 
	public Vector2Int Location;
	public static bool IsMoving = false; // if the player is currently moving, this variable will be true

	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
	{
		// inits
		if (!IsMoving) // check if player is started playing and not moving than give random chess piece
		{
			ChessPiece = (ChessPiece)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);
		}
		if (Statistics == null)
			Statistics = CreateInstance<PlayerStatistics>();
		instance = this;
		Location = new Vector2Int(location.x, location.y);
		return true;
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		base.GetTileData(position, tilemap, ref tileData);
		Location = new Vector2Int(position.x, position.y);
		switch (ChessPiece)
		{
			case ChessPiece.Pawn:
				tileData.sprite = PawnSprite;
				break;
			case ChessPiece.Knight:
				tileData.sprite = KnightSprite;
				break;
			case ChessPiece.Bishop:
				tileData.sprite = BishopSprite;
				break;
			case ChessPiece.Rook:
				tileData.sprite = RookSprite;
				break;
			case ChessPiece.Queen:
				tileData.sprite = QueenSprite;
				break;
			case ChessPiece.King:
				tileData.sprite = KingSprite;
				break;
		}
	}

	// when the player begins a battle with one or more opponents, this function is called
	public IEnumerator StartAttack(bool IsPlayerAttackFirst, Vector3Int enemyTile, List<Vector3Int> EnemiesLeft)
	{
		IsFighting = true;
		yield return new WaitForSeconds(0.2f); // give let the player realize that there will be a fight now

		if (IsPlayerAttackFirst) // check if player attacks first or enemy
		{
			while (true) // as long as there are opponents with whom the player fights, the loop works
			{
				#region HIGHLIGHT
				PlayerMovement.instance.HighlightMovesTileMap.SetTile(enemyTile, PlayerMovement.instance.AttackHighlightTile);
				PlayerMovement.instance.HighlightMovesTileMap.SetTile(new Vector3Int(PlayerTile.instance.Location.x, PlayerTile.instance.Location.y), PlayerMovement.instance.AttackHighlightTile);
				#endregion HIGHLIGHT
				
				yield return new WaitForSeconds(0.4f); // give let the player realize that there will be a fight now

				#region PLAYER_ATTACK
				int damage = Statistics.Attack; // damage that player give enemy
				PlayerMovement.instance.PlayAttackSound();
				if (damage > Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor) // take into account the enemy's armor
				{
					damage -= Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor;
					Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor = 0;
				}
				else
				{
					Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor -= damage;
					damage = 0;
				}

				if (Spawner.instance.EnemiesInBoard[enemyTile].Item2.Health <= damage) // check if this is a fatal blow to the enemy
				{
					Statistics.Gold += Spawner.instance.EnemiesInBoard[enemyTile].Item2.GoldCost; // add enemy gold to player
					Statistics.Score += 1;
					PlayerMovement.instance.HighlightMovesTileMap.ClearAllTiles();
					Spawner.instance.Board.SetTile(enemyTile, null); // death
					Spawner.instance.SpriteTileMap.SetTile(enemyTile, null);
					Spawner.instance.EnemiesInBoard.Remove(enemyTile);
					if (EnemiesLeft.Count > 0) // switch to another enemy if has else stop fight
					{
						enemyTile = EnemiesLeft[0];
						EnemiesLeft.RemoveAt(0);
					}
					else
					{
						PlayerTile.instance.IsFighting = false;
						break;
					}
				}
				else // if not fatal then reduce enemy hp
				{
					Spawner.instance.EnemiesInBoard[enemyTile].Item2.Health -= damage;
				}
				#endregion PLAYER_ATTACK

				yield return new WaitForSeconds(0.4f); // give let the player realize that there will be a fight now

				#region ENEMY_ATTACK
				damage = Spawner.instance.EnemiesInBoard[enemyTile].Item2.Attack; // damage that enemy give player
				if (damage > this.Statistics.Armor) // take into account the enemy's armor
				{
					damage -= this.Statistics.Armor;
					this.Statistics.Armor = 0;
				}
				else
				{
					this.Statistics.Armor -= damage;
					damage = 0;
				}
				if (this.Statistics.Health <= damage) // check if this is a fatal blow to the player
				{
					Death();
					break;
				}
				else
				{
					PlayerMovement.instance.PlayAttackSound();
					this.Statistics.Health -= damage;
				}
				#endregion ENEMY_ATTACK
			}
		} else
		{
			while (true)
			{
				#region HIGHLIGHT
				PlayerMovement.instance.HighlightMovesTileMap.SetTile(enemyTile, PlayerMovement.instance.AttackHighlightTile);
				PlayerMovement.instance.HighlightMovesTileMap.SetTile(new Vector3Int(PlayerTile.instance.Location.x, PlayerTile.instance.Location.y), PlayerMovement.instance.AttackHighlightTile);
				#endregion HIGHLIGHT

				yield return new WaitForSeconds(0.4f); // give let the player realize that there will be a fight now

				#region ENEMY_ATTACK
				int damage = Spawner.instance.EnemiesInBoard[enemyTile].Item2.Attack; // damage that enemy give player
				if (damage > this.Statistics.Armor) // take into account the enemy's armor
				{
					damage -= this.Statistics.Armor;
					this.Statistics.Armor = 0;
				}
				else
				{
					this.Statistics.Armor -= damage;
					damage = 0;
				}
				if (this.Statistics.Health <= damage) // check if this is a fatal blow to the player
				{
					Death();
					break;
				}
				else
				{
					PlayerMovement.instance.PlayAttackSound();
					this.Statistics.Health -= damage;
				}
				#endregion ENEMY_ATTACK

				yield return new WaitForSeconds(0.4f); // give let the player realize that there will be a fight now

				#region PLAYER_ATTACK
				damage = Statistics.Attack; // damage that player give enemy

				PlayerMovement.instance.PlayAttackSound();
				if (damage > Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor) // take into account the enemy's armor
				{
					damage -= Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor;
					Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor = 0;
				}
				else
				{
					Spawner.instance.EnemiesInBoard[enemyTile].Item2.Armor -= damage;
					damage = 0;
				}

				if (Spawner.instance.EnemiesInBoard[enemyTile].Item2.Health <= damage) // check if this is a fatal blow to the enemy
				{
					Statistics.Gold += Spawner.instance.EnemiesInBoard[enemyTile].Item2.GoldCost; // add enemy gold to player
					Statistics.Score += 1;
					PlayerMovement.instance.HighlightMovesTileMap.ClearAllTiles();
					Spawner.instance.Board.SetTile(enemyTile, null); // death
					Spawner.instance.SpriteTileMap.SetTile(enemyTile, null);
					Spawner.instance.EnemiesInBoard.Remove(enemyTile);
					if (EnemiesLeft.Count > 0) // switch to another enemy if has else stop fight
					{
						enemyTile = EnemiesLeft[0];
						EnemiesLeft.RemoveAt(0);
					}
					else
					{
						PlayerTile.instance.IsFighting = false;
						break;
					}
				}
				else // if not fatal then reduce enemy hp
				{
					Spawner.instance.EnemiesInBoard[enemyTile].Item2.Health -= damage;
				}
				#endregion PLAYER_ATTACK

			}
		}
		PlayerTile.instance.IsFighting = false;

	}

	// called when player is died
	public void Death()
	{
		DeathCanvas.SetActive(true);
		PlayerMovement.instance.PlayDeathSound();
	}
}