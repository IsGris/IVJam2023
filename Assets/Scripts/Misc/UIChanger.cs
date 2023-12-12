using TMPro;
using UnityEngine;

public class UIChanger : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI HPText;
    [SerializeField] TextMeshProUGUI AttackText;
    [SerializeField] TextMeshProUGUI ArmorText;
    [SerializeField] TextMeshProUGUI ScoreText;

    void Update()
    {
        HPText.text = PlayerTile.instance.Statistics.Health.ToString();
		AttackText.text = PlayerTile.instance.Statistics.Attack.ToString();
		ArmorText.text = PlayerTile.instance.Statistics.Armor.ToString();
		ScoreText.text = PlayerTile.instance.Statistics.Score.ToString();
    }
}
