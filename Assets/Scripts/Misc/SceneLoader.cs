using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (sceneName == "MainLevel")
        {
            PlayerTile.instance.Statistics.Score = 0;
            PlayerTile.instance.Statistics.Health = 0;
            PlayerTile.instance.Statistics.Attack = 0;
			PlayerTile.instance.Statistics.Armor = 0;
            PlayerTile.instance.Statistics.Level = 0;
        }
        SceneManager.LoadScene(sceneName);
    }
}
