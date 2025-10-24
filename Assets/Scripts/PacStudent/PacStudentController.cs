using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour, ITeleportable
{
    public GameObject ShadowReference { get; private set; }
    private Animator _shadowReferenceAnimator;
    public bool IsInTeleport { get; set; } = false;

    [Header("Movement")]
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private float movementSpeed;
    [SerializeField] private TileBase[] wallTiles;
    [SerializeField] private ParticleSystem dirtParticleSystem;
    [SerializeField] private ParticleSystem angerParticleSystem;
    [SerializeField] private ParticleSystem bloodParticleSystem;

    [Header("Audio")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip eatClip;
    [SerializeField] private AudioClip wallHitClip;

    [Header("Tiles")]
    [SerializeField] private TileBase pelletTile;
    [SerializeField] private TileBase powerPelletTile;
    [SerializeField] private TileBase dirtEmptyTile;
    [SerializeField] private PowerPelletSpawn powerPelletSpawn;

    [Header("Cherry")]
    [SerializeField] private CherryController cherryController;

    [Header("Animation")]
    [SerializeField] private float deathAnimationLength;

    private enum InputKeys
    {
        LEFT = 0,
        RIGHT,
        UP,
        DOWN
    }

    private Animator _animator;
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _worldCellSize;
    private GameInput _gameInput;
    private Vector2Int _currentDirection = new Vector2Int(1, 0);
    private InputAction[] _movementKeys = new InputAction[4];
    private InputAction _lastInput;
    private InputAction _currentInput;
    private Vector2Int _targetCellDestination;
    private bool _inLerp = false;
    private string _currentDirectionAnimation = "None";
    private Coroutine _activeMoveCoroutine;
    private Vector2 _initialSpawnPosition;

    private bool _isDead = false;
    private float _deadTimer = 0.0f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _worldCellSize = Vector2.Scale(levelMap.cellSize, new Vector3(
            Mathf.Abs(levelMap.transform.localScale.x),
            Mathf.Abs(levelMap.transform.localScale.y),
            Mathf.Abs(levelMap.transform.localScale.z)
        ));

        PlayerSetup();

        ShadowReference = Instantiate(gameObject);
        PacStudentController shadowReferenceController = ShadowReference.GetComponent<PacStudentController>();
        Destroy(shadowReferenceController);
        _shadowReferenceAnimator = ShadowReference.GetComponent<Animator>();
        ShadowReference.SetActive(false);

        if (ShadowReference.activeSelf)
        {
            _shadowReferenceAnimator.SetTrigger(_currentDirectionAnimation);
            _shadowReferenceAnimator.speed = 0.0f;
        }
    }

    private void PlayerSetup()
    {
        _gameInput = GameManager.Instance.InputManager.GameInput;
        _movementKeys[(int)InputKeys.LEFT] = _gameInput.Player.LeftKey;
        _movementKeys[(int)InputKeys.RIGHT] = _gameInput.Player.RightKey;
        _movementKeys[(int)InputKeys.UP] = _gameInput.Player.UpKey;
        _movementKeys[(int)InputKeys.DOWN] = _gameInput.Player.DownKey;

        transform.position = GetWorldPosNormalized(transform.position);
        _initialSpawnPosition = transform.position;
        _targetCellDestination = GetCellPosFromWorld(transform.position);

        _currentDirectionAnimation = "Right";
        _animator.SetTrigger(_currentDirectionAnimation);
        _animator.speed = 0.0f;

        _inLerp = false;
        _lastInput = null;
    }

    private void Update()
    {
        if (GameManager.Instance.GetPlayerLives() <= 0)
        {
            Destroy(_spriteRenderer);
        }

        if (!GameManager.Instance.GameStarted) return;

        foreach (var key in _movementKeys)
        {
            if (!key.IsPressed() || _lastInput == key) continue;

            _lastInput = key;
        }

        if (!_inLerp && !_isDead)
        {
            _inLerp = true;
            _activeMoveCoroutine = StartCoroutine(MoveToPos(transform.position, GetWorldPosFromCell(_targetCellDestination), 1.0f / movementSpeed, 0.0f));
        }

        if (_isDead && _activeMoveCoroutine != null) StopCoroutine(_activeMoveCoroutine);

        if (_isDead)
        {
            _deadTimer += Time.deltaTime;

            if (_deadTimer >= deathAnimationLength)
            {
                _deadTimer = 0.0f;
                transform.position = _initialSpawnPosition;
                _isDead = false;
                PlayerSetup();
            }
        }
    }

    private IEnumerator MoveToPos(Vector2 startPos, Vector2 endPos, float duration, float setTime)
    {
        float time = setTime;
        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Position reached
        transform.position = endPos;
        _inLerp = false;

        Vector2Int desiredDirection = GetNewDirection(_lastInput);
        if (!CheckIsWallTile(_targetCellDestination + desiredDirection)) // Empty tile in desired direction
        {
            _currentInput = _lastInput;
            _currentDirection = GetNewDirection(_currentInput);
            _targetCellDestination += new Vector2Int(
                _currentDirection.x,
                _currentDirection.y
            );

            if (!dirtParticleSystem.isPlaying && _animator.speed != 0.0f) dirtParticleSystem.Play();
            var particleVel = dirtParticleSystem.velocityOverLifetime;

            if (_currentDirection == new Vector2Int(-1, 0) && (_currentDirectionAnimation != "Left" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Left";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

                if (ShadowReference.activeSelf)
                {
                    _shadowReferenceAnimator.SetTrigger(_currentDirectionAnimation);
                    _animator.speed = 1.0f;
                }

                particleVel.x = new ParticleSystem.MinMaxCurve(1.0f);
                particleVel.y = new ParticleSystem.MinMaxCurve(0.0f);
            }
            else if (_currentDirection == new Vector2Int(1, 0) && (_currentDirectionAnimation != "Right" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Right";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

                if (ShadowReference.activeSelf)
                {
                    _shadowReferenceAnimator.SetTrigger(_currentDirectionAnimation);
                    _animator.speed = 1.0f;
                }

                particleVel.x = new ParticleSystem.MinMaxCurve(-1.0f);
                particleVel.y = new ParticleSystem.MinMaxCurve(0.0f);
            }
            else if (_currentDirection == new Vector2Int(0, 1) && (_currentDirectionAnimation != "Up" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Up";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

                if (ShadowReference.activeSelf)
                {
                    _shadowReferenceAnimator.SetTrigger(_currentDirectionAnimation);
                    _animator.speed = 1.0f;
                }

                particleVel.x = new ParticleSystem.MinMaxCurve(0.0f);
                particleVel.y = new ParticleSystem.MinMaxCurve(-1.0f);
            }
            else if (_currentDirection == new Vector2Int(0, -1) && (_currentDirectionAnimation != "Down" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Down";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

                if (ShadowReference.activeSelf)
                {
                    _shadowReferenceAnimator.SetTrigger(_currentDirectionAnimation);
                    _animator.speed = 1.0f;
                }

                particleVel.x = new ParticleSystem.MinMaxCurve(0.0f);
                particleVel.y = new ParticleSystem.MinMaxCurve(1.0f);
            }
        }
        else if (_currentDirection != desiredDirection && !CheckIsWallTile(_targetCellDestination + _currentDirection)) // Wall tile in desired direction, no wall tile in current direction
        {
            _targetCellDestination += new Vector2Int(
                _currentDirection.x,
                _currentDirection.y
            );
        }
        else
        {
            if (dirtParticleSystem.isPlaying) dirtParticleSystem.Stop();
            _animator.speed = 0.0f;

            if (ShadowReference.activeSelf) _animator.speed = 1.0f;

            if (_audioSource.clip != wallHitClip)
            {
                _audioSource.clip = wallHitClip;
                _audioSource.Play();
                angerParticleSystem.Play();
            }
        }

        Vector2Int currentCellPos = GetCellPosFromWorld(transform.position);
        TileBase destTile = levelMap.GetTile(new Vector3Int(currentCellPos.x, currentCellPos.y, 0));
        if (_animator.speed != 0.0f)
        {
            _audioSource.clip = (destTile == pelletTile || destTile == powerPelletTile) ? eatClip : moveClip;
            _audioSource.Play();
        }

        if (destTile == pelletTile || destTile == powerPelletTile) levelMap.SetTile(new Vector3Int(currentCellPos.x, currentCellPos.y, 0), dirtEmptyTile);

        if (destTile == pelletTile)
        {
            GameManager.Instance.AddScore(10);
            GameManager.Instance.CollectedPellet();
        }

        if (destTile == powerPelletTile)
        {
            powerPelletSpawn.DeletePowerPelletAtPos(currentCellPos);
            GameManager.Instance.AddScore(50);
            GameManager.Instance.GhostManager.TriggerScared(10.0f, 3.0f);
            GameManager.Instance.CollectedPellet();
        }

        if (IsInTeleport == true) IsInTeleport = false;
    }

    public Vector2 GetWorldPosFromCell(Vector2Int cellPos)
    {
        return new Vector2(
            cellPos.x * _worldCellSize.x + (_worldCellSize.x / 2.0f),
            cellPos.y * _worldCellSize.y + (_worldCellSize.y / 2.0f)
        );
    }

    public Vector2Int GetCellPosFromWorld(Vector2 worldPos)
    {
        return new Vector2Int(
            (int)Mathf.Floor(worldPos.x / _worldCellSize.x),
            (int)Mathf.Floor(worldPos.y / _worldCellSize.y)
        );
    }

    public Vector2 GetWorldPosNormalized(Vector2 worldPos)
    {
        return GetWorldPosFromCell(GetCellPosFromWorld(worldPos));
    }

    public void TeleportMoveStudent(Vector2 startPos, Vector2 endPos)
    {
        StopCoroutine(_activeMoveCoroutine);

        _targetCellDestination = GetCellPosFromWorld(endPos);
        _inLerp = true;

        _activeMoveCoroutine = StartCoroutine(MoveToPos(startPos, endPos, 1.0f / movementSpeed, (1.0f / movementSpeed) * 0.50f));
    }

    private Vector2Int GetNewDirection(InputAction action)
    {
        if (action == _movementKeys[(int)InputKeys.LEFT])
        {
            return new Vector2Int(-1, 0);
        }
        else if (action == _movementKeys[(int)InputKeys.RIGHT])
        {
            return new Vector2Int(1, 0);
        }
        else if (action == _movementKeys[(int)InputKeys.UP])
        {
            return new Vector2Int(0, 1);
        }
        else if (action == _movementKeys[(int)InputKeys.DOWN])
        {
            return new Vector2Int(0, -1);
        }
        else
        {
            return new Vector2Int(0, 0);
        }
    }

    private bool CheckIsWallTile(Vector2Int tilePos)
    {
        TileBase tileToCheck = levelMap.GetTile((Vector3Int)tilePos);

        foreach (var tile in wallTiles)
        {
            if (tileToCheck == tile) return true;
        }

        return false;
    }

    public string GetCurrentDirectionAnimation()
    {
        return _currentDirectionAnimation;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Cherry"))
        {
            cherryController.PlayerDestroyCherry();
            GameManager.Instance.AddScore(100);
        } else if (collision.CompareTag("Enemy") && !_isDead)
        {
            GhostController colliderGhostController = collision.GetComponent<GhostController>();
            if (colliderGhostController.GhostIsDead) return;

            if (!GameManager.Instance.GhostManager.GhostsAreScared)
            {
                GameManager.Instance.AddPlayerLife(-1);
                _isDead = true;
                _animator.SetTrigger("Death");
                dirtParticleSystem.Stop();
                bloodParticleSystem.Play();

                GameManager.Instance.GhostManager.PlayerDeathEvent(deathAnimationLength);
            } else
            {
                colliderGhostController.TriggerGhostDeath();
            }
        }
    }

    public float GetMovementSpeed()
    {
        return movementSpeed;
    }
}
