
public static class Stats
{
	public static int GetLevelAttack(in int level)
	{
		return level * 5;
	}

	public static int GetLevelHealth(in int level)
	{
		if (level == 1) return 15;
		else return GetLevelHealth(level - 1) + GetLevelArmor(level) + GetLevelItemHealth(level);
	}

	public static int GetLevelArmor(in int level)
	{
		if (level == 1) return 10;
		else return GetLevelArmor(level - 1) - GetEnemyGeneralizeAttack(level) + GetLevelItemArmor(level);
	}

	public static int GetLevelItemHealth(in int level)
	{
		return 5 + (level * 5);
	}

	public static int GetPlayerGeneralizeArmor(in int level)
	{
		return GetLevelItemArmor(level) + GetLevelArmor(level);
	}

	public static int GetPlayerGeneralizeHealth(in int level)
	{
		return GetLevelHealth(level) + GetLevelItemHealth(level);
	}

	public static int GetPlayerGeneralizeAttack(in int level)
	{
		return GetLevelAttack(level) + GetLevelItemAttack(level);
	}

	public static int GetEnemyGeneralizeAttack(in int level)
	{
		return level * 5;
	}

	public static int GetLevelItemArmor(in int level)
	{
		return 5 + (level - 1) * 3;
	}

	public static int GetLevelItemAttack(in int level)
	{
		return 5;
	}

	public static int GetLevelEnemyCount(in int level)
	{
		return 5;
	}

	public static int GetLevelEnemyArmor(in int level)
	{
		if (level == 1)
			return 2;
		else return GetLevelEnemyArmor(level - 1) + 1;
	}

	public static int GetLevelEnemyHealth(in int level)
	{
		if (level == 1)
			return 5;
		else
			return GetLevelEnemyHealth(level - 1) + 1;
	}

	public static int GetLevelEnemyAttack(in int level)
	{
		return level;
	}

}
