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

	// when the player begins a battle with one or more opponents, this function is called
	public IEnumerator StartAttack(bool IsPlayerAttackFirst, EnemyTile enemyTile, List<EnemyTile> EnemiesLeft)
	{
		yield return new WaitForSeconds(0.2f); // give let the player realize that there will be a fight now

		if (IsPlayerAttackFirst) // check if player attacks first or enemy
		{
			while (EnemiesLeft.Count > 0) // as long as there are opponents with whom the player fights, the loop works
			{
				bool IsKilled = false; // if player killed enemy will be true and it resets every enemy
				#region PLAYER_ATTACK
				uint damage = Statistics.Attack; // damage that player give enemy

				if (damage > enemyTile.Statistics.Armor) // take into account the enemy's armor
				{
					damage -= enemyTile.Statistics.Armor;
					enemyTile.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}

				if (enemyTile.Statistics.Health <= damage) // check if this is a fatal blow to the enemy
				{
					Statistics.Gold += enemyTile.Statistics.GoldCost; // add enemy gold to player
					enemyTile.Death(); // kill enemy
					IsKilled = true;
					if (EnemiesLeft.Count > 0) // switch to another enemy if has else stop fight
					{
						enemyTile = EnemiesLeft[0];
						EnemiesLeft.RemoveAt(0);
					}
					else
					{
						PlayerTile.instance.IsFighting = false;
					}
				}
				else // if not fatal then reduce enemy hp
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion PLAYER_ATTACK

				if (!IsKilled) // if player killed enemy then no enemy to highlight with HitColor
				{
					enemyTile.color = HitColor;
					yield return new WaitForSeconds(0.2f);
					enemyTile.color = new Color(0, 0, 0);
					yield return new WaitForSeconds(0.3f);
				}

				#region ENEMY_ATTACK
				damage = enemyTile.Statistics.Attack; // damage that enemy give player
				if (damage > this.Statistics.Armor) // take into account the enemy's armor
				{
					damage -= this.Statistics.Armor;
					this.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}
				if (this.Statistics.Health <= damage) // check if this is a fatal blow to the player
				{
					Death();
				}
				else
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion ENEMY_ATTACK

				this.color = HitColor; // highlight player with hit color
				yield return new WaitForSeconds(0.2f);
				this.color = new Color(0, 0, 0);
				yield return new WaitForSeconds(0.3f);
			}
		} else
		{
			while (true)
			{
				bool IsKilled = false; // if player killed enemy will be true and it resets every enemy

				#region ENEMY_ATTACK
				uint damage = enemyTile.Statistics.Attack; // damage that enemy give player
				if (damage > this.Statistics.Armor) // take into account the enemy's armor
				{
					damage -= this.Statistics.Armor;
					this.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}
				if (this.Statistics.Health <= damage) // check if this is a fatal blow to the player
				{
					Death();
				}
				else
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion ENEMY_ATTACK

				this.color = HitColor; // highlight player with hit color
				yield return new WaitForSeconds(0.2f);
				this.color = new Color(0, 0, 0);
				yield return new WaitForSeconds(0.5f);

				#region PLAYER_ATTACK
				damage = Statistics.Attack; // damage that player give enemy

				if (damage > enemyTile.Statistics.Armor) // take into account the enemy's armor
				{
					damage -= enemyTile.Statistics.Armor;
					enemyTile.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}

				if (enemyTile.Statistics.Health <= damage) // check if this is a fatal blow to the enemy
				{
					Statistics.Gold += enemyTile.Statistics.GoldCost; // add enemy gold to player
					enemyTile.Death(); // kill enemy
					IsKilled = true;
					if (EnemiesLeft.Count > 0) // switch to another enemy if has else stop fight
					{
						enemyTile = EnemiesLeft[0];
						EnemiesLeft.RemoveAt(0);
					}
					else
					{
						PlayerTile.instance.IsFighting = false;
					}
				}
				else // if not fatal then reduce enemy hp
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion PLAYER_ATTACK

				if (!IsKilled) // if player killed enemy then no enemy to highlight with HitColor
				{
					enemyTile.color = HitColor;
					yield return new WaitForSeconds(0.2f);
					enemyTile.color = new Color(0, 0, 0);
					yield return new WaitForSeconds(0.3f);
				}
			}
		}
		
	}

	// called when player is died
	public void Death()
	{

	}
}