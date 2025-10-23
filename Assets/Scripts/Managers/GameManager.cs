using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameAudioManager GameAudioManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public GhostManager GhostManager { get; private set; }
    public int GameScore { get; private set; }

    [SerializeField] private int playerLives;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("GameManager Instance already exists");
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        GameAudioManager = GetComponentInChildren<GameAudioManager>();
        InputManager = GetComponentInChildren<InputManager>();
        GhostManager = GetComponentInChildren<GhostManager>();
    }

    private void Update()
    {
        scoreText.text = GameScore.ToString("D6");
        livesText.text = playerLives.ToString();
    }

    public void AddScore(int n)
    {
        GameScore += n;
    }

    public void AddPlayerLife(int n)
    {
        playerLives += n;
    }
}
