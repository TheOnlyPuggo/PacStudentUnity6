using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameAudioManager GameAudioManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public GhostManager GhostManager { get; private set; }
    public int GameScore { get; private set; }
    public bool GameStarted { get; private set; } = false;

    [SerializeField] private int playerLives;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Animator startCountdownAnimator;
    [SerializeField] private AnimationClip startCountdownAnimClip;
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private AnimationClip gameOverAnimClip;
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private TileBase normalPellet;
    [SerializeField] private TileBase powerPellet;

    private float _startCountdownLength;
    private float _startTimer = 0.0f;
    private bool _startAnimActive = false;

    private float _gameOverLength;
    private float _gameOverTimer;
    private bool _gameOverAnimActive = false;

    private int _totalPellets = 0;
    private int _collectedPellets = 0;

    private bool _playerDead = false;

    // Timer
    private int _tenMilliseconds;
    private int _seconds;
    private int _minutes;
    private float _secondTimer = 0.0f;

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

    private void Start()
    {
        _startCountdownLength = startCountdownAnimClip.length;
        startCountdownAnimator.SetTrigger("CountdownStart");
        _startAnimActive = true;

        _gameOverLength = gameOverAnimClip.length;

        // Count pellet tiles
        foreach (Vector3Int cellPos in levelMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = levelMap.GetTile(cellPos);
            if (tile == normalPellet || tile == powerPellet)
            {
                _totalPellets += 1;
            }
        }
    }

    private void Update()
    {
        scoreText.text = GameScore.ToString("D6");
        livesText.text = playerLives.ToString();

        if (_startAnimActive)
        {
            _startTimer += Time.deltaTime;

            if (_startTimer >=  _startCountdownLength)
            {
                startCountdownAnimator.SetTrigger("Default");
                _startTimer = 0.0f;
                _startAnimActive = false;
                GameStarted = true;
            }
        }

        if (GameStarted)
        {
            _secondTimer += Time.deltaTime;
            if (_secondTimer >= 1.0f)
            {
                _secondTimer = 0.0f;

                _seconds += 1;

                if (_seconds >= 60)
                {
                    _seconds = 0;
                    _minutes += 1;
                }
            }

            _tenMilliseconds = (int)(_secondTimer * 100.0f);

            timerText.text = _minutes.ToString("D2") + ":" + _seconds.ToString("D2") + ":" + _tenMilliseconds.ToString("D2");
        }

        if (_collectedPellets >= _totalPellets && !_gameOverAnimActive) TriggerGameOver();
        if (playerLives <= 0 && !_playerDead)
        {
            _playerDead = true;
            TriggerGameOver();
        }

        if (_gameOverAnimActive)
        {
            _gameOverTimer += Time.deltaTime;

            if (_gameOverTimer >= _gameOverLength)
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void AddScore(int n)
    {
        GameScore += n;
    }

    public void AddPlayerLife(int n)
    {
        playerLives += n;
    }

    public void CollectedPellet()
    {
        _collectedPellets += 1;
    }

    public int GetPlayerLives()
    {
        return playerLives;
    }

    public void TriggerGameOver()
    {
        gameOverAnimator.SetTrigger("GameOver");
        _gameOverAnimActive = true;
        GameStarted = false;

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (PlayerPrefs.GetInt("Level1HighScore") < GameScore)
            {
                PlayerPrefs.SetInt("Level1HighScore", GameScore);
            }

            bool achievedBestTime = false;

            if (PlayerPrefs.GetInt("HasSetTimeBefore") == 0) achievedBestTime = true;

            if (_minutes < PlayerPrefs.GetInt("Level1BestMinute")) achievedBestTime = true;
            if (
                _minutes == PlayerPrefs.GetInt("Level1BestMinute") && 
                _seconds < PlayerPrefs.GetInt("Level1BestSecond")
            ) achievedBestTime = true;
            if (
                _minutes == PlayerPrefs.GetInt("Level1BestMinute") && 
                _seconds == PlayerPrefs.GetInt("Level1BestSecond") && 
                _tenMilliseconds < PlayerPrefs.GetInt("Level1BestTenMilli")
            ) achievedBestTime = true; 

            if (achievedBestTime && !_playerDead)
            {
                PlayerPrefs.SetInt("Level1BestMinute", _minutes);
                PlayerPrefs.SetInt("Level1BestSecond", _seconds);
                PlayerPrefs.SetInt("Level1BestTenMilli", _tenMilliseconds);
                PlayerPrefs.SetInt("HasSetTimeBefore", 1);
            }
        }
    }
}
