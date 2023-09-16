using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
	public static PlayerSprite instance;

	private void Awake()
	{
		instance = this;
	}

	public void ChangeSprite(Sprite sprite)
	{
		GetComponent<SpriteRenderer>().sprite = sprite;
	}

	public void UpdatePosition()
	{
		this.transform.position = new Vector3Int(PlayerTile.instance.Location.x, PlayerTile.instance.Location.y, 0);
	}
}
