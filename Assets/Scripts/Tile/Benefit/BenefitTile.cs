using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/BenefitTile")]
public class BenefitTile : Tile
{
	public enum BenefitType 
	{ 
		Attack,
		Armor,
		Health
	}
	public BenefitType Type;
	public uint Amount;

	[SerializeField] private Sprite AttackSprite;
	[SerializeField] private Sprite ArmorSprite;
	[SerializeField] private Sprite HealthSprite;

	public void Init(BenefitType Type, uint Amount)
	{
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