using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using System.Collections;

public enum ChessPiece
{
	Pawn, // пешка
	Knight, // конь
	Bishop, // слон
	Rook, // ладья
	Queen, // ферзь
	King // король
}

[CreateAssetMenu(menuName = "2D/Tiles/PlayerTile")]
public class PlayerTile : Tile
{
	[SerializeField] private Color HitColor;

	[NonSerialized] public static PlayerTile instance;
	
	[NonSerialized] public bool IsFighting = false;
	[NonSerialized] public ChessPiece ChessPiece;
	public PlayerStatistics Statistics;
	public Vector2Int Location;
	public static bool IsMoving = false;

	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
	{
		if (!IsMoving)
		{
			ChessPiece = (ChessPiece)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChessPiece)).Length);
		}
		if (Statistics == null)
			Statistics = CreateInstance<PlayerStatistics>();
		instance = this;
		Location = new Vector2Int(location.x, location.y);
		return true;
	}

	public IEnumerator StartAttack(bool IsPlayerAttackFirst, EnemyTile enemyTile, List<EnemyTile> EnemiesLeft)
	{
		yield return new WaitForSeconds(0.2f);

		if (IsPlayerAttackFirst)
		{
			while (true)
			{
				bool IsKilled = false;
				#region PLAYER_ATTACK
				uint damage = Statistics.Attack;
				if (damage > enemyTile.Statistics.Armor)
				{
					damage -= enemyTile.Statistics.Armor;
					enemyTile.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}

				if (enemyTile.Statistics.Health <= damage)
				{
					Statistics.Gold += enemyTile.Statistics.GoldCost;
					enemyTile.Death();
					IsKilled = true;
					if (EnemiesLeft.Count > 0)
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
				else
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion PLAYER_ATTACK

				if (!IsKilled)
				{
					enemyTile.color = HitColor;
					yield return new WaitForSeconds(0.2f);
					enemyTile.color = new Color(0, 0, 0);
					yield return new WaitForSeconds(0.3f);
				}

				#region ENEMY_ATTACK
				damage = enemyTile.Statistics.Attack;
				if (damage > this.Statistics.Armor)
				{
					damage -= this.Statistics.Armor;
					this.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}
				if (this.Statistics.Health <= damage)
				{
					Death();
				}
				else
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion ENEMY_ATTACK

				this.color = HitColor;
				yield return new WaitForSeconds(0.2f);
				this.color = new Color(0, 0, 0);
				yield return new WaitForSeconds(0.3f);
			}
		} else
		{
			while (true)
			{
				bool IsKilled = false;

				#region ENEMY_ATTACK
				uint damage = enemyTile.Statistics.Attack;
				if (damage > this.Statistics.Armor)
				{
					damage -= this.Statistics.Armor;
					this.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}
				if (this.Statistics.Health <= damage)
				{
					Death();
				}
				else
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion ENEMY_ATTACK

				this.color = HitColor;
				yield return new WaitForSeconds(0.2f);
				this.color = new Color(0, 0, 0);
				yield return new WaitForSeconds(0.5f);

				#region PLAYER_ATTACK
				damage = Statistics.Attack;
				if (damage > enemyTile.Statistics.Armor)
				{
					damage -= enemyTile.Statistics.Armor;
					enemyTile.Statistics.Armor = 0;
				}
				else
				{
					enemyTile.Statistics.Armor -= damage;
					damage = 0;
				}

				if (enemyTile.Statistics.Health <= damage)
				{
					Statistics.Gold += enemyTile.Statistics.GoldCost;
					enemyTile.Death();
					IsKilled = true;
					if (EnemiesLeft.Count > 0)
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
				else
				{
					enemyTile.Statistics.Health -= damage;
				}
				#endregion PLAYER_ATTACK

				if (!IsKilled)
				{
					enemyTile.color = HitColor;
					yield return new WaitForSeconds(0.2f);
					enemyTile.color = new Color(0, 0, 0);
					yield return new WaitForSeconds(0.3f);
				}
			}
		}
		
	}

	public void Death()
	{

	}
}