using UnityEngine;
using UnityEngine.Tilemaps;

// tile that represents benefits
[CreateAssetMenu(menuName = "2D/Tiles/BenefitTile")]
public class BenefitTile : Tile
{
	public enum BenefitType 
	{ 
		Attack,
		Armor,
		Health
	}
	public BenefitType Type; // type of the benefit
	public uint Amount; // how many units of the benefit type will be given to player

	// sprites which will be used for cage design depending on the type of benefit
	[SerializeField] private Sprite AttackSprite;
	[SerializeField] private Sprite ArmorSprite;
	[SerializeField] private Sprite HealthSprite;

	public void Init(BenefitType Type, uint Amount)
	{
		// inits
		this.Type = Type;
		this.Amount = Amount;
		switch (Type) 
		{
			case BenefitType.Attack:
				this.sprite = AttackSprite; 
				break;
			case BenefitType.Armor:
				this.sprite = ArmorSprite;
				break;
			case BenefitType.Health:
				this.sprite = HealthSprite;
				break;
		}
	}

}