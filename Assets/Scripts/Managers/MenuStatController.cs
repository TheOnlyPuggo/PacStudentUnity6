using TMPro;
using UnityEngine;

public class MenuStatController : MonoBehaviour
{
    [SerializeField] private TMP_Text levelOneHighScoreText;
    [SerializeField] private TMP_Text levelOneBestTimeText;

    private void Start()
    {
        levelOneHighScoreText.text = "High Score: " + PlayerPrefs.GetInt("Level1HighScore");

        int bestMinute = PlayerPrefs.GetInt("Level1BestMinute");
        int bestSecond = PlayerPrefs.GetInt("Level1BestSecond");
        int bestTenMilli = PlayerPrefs.GetInt("Level1BestTenMilli");

        levelOneBestTimeText.text = "Time: " + bestMinute.ToString("D2") + ":" + bestSecond.ToString("D2") + ":" + bestTenMilli.ToString("D2");
    }
}
