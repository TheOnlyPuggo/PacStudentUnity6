using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private float movementDuration;
    [SerializeField] private TileBase[] wallTiles;
    [SerializeField] private ParticleSystem dirtParticleSystem;

    [Header("Audio")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip eatClip;
    [SerializeField] private AudioClip wallHitClip;
    [SerializeField] private TileBase pelletTile;
    [SerializeField] private TileBase powerPelletTile;

    private enum InputKeys
    {
        LEFT = 0,
        RIGHT,
        UP,
        DOWN
    }

    private Animator _animator;
    private AudioSource _audioSource;

    private Vector2 _worldCellSize;
    private GameInput _gameInput;
    private Vector2Int _currentDirection = new Vector2Int(1, 0);
    private InputAction[] _movementKeys = new InputAction[4];
    private InputAction _lastInput;
    private InputAction _currentInput;
    private Vector2Int _targetCellDestination;
    private bool _inLerp = false;
    private string _currentDirectionAnimation = "None";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _worldCellSize = Vector2.Scale(levelMap.cellSize, new Vector3(
            Mathf.Abs(levelMap.transform.localScale.x),
            Mathf.Abs(levelMap.transform.localScale.y),
            Mathf.Abs(levelMap.transform.localScale.z)
        ));

        _gameInput = InputManager.Instance.GameInput;
        _movementKeys[(int)InputKeys.LEFT] = _gameInput.Player.LeftKey;
        _movementKeys[(int)InputKeys.RIGHT] = _gameInput.Player.RightKey;
        _movementKeys[(int)InputKeys.UP] = _gameInput.Player.UpKey;
        _movementKeys[(int)InputKeys.DOWN] = _gameInput.Player.DownKey;

        transform.position = GetWorldPosNormalized(transform.position);
        _targetCellDestination = GetCellPosFromWorld(transform.position);

        _currentDirectionAnimation = "Right";
        _animator.SetTrigger(_currentDirectionAnimation);
        _animator.speed = 0.0f;
    }

    private void Update()
    {
        foreach (var key in _movementKeys)
        {
            if (!key.IsPressed() || _lastInput == key) continue;

            _lastInput = key;
        }

        if (!_inLerp)
        {
            _inLerp = true;
            StartCoroutine(MoveToPos(transform.position, GetWorldPosFromCell(_targetCellDestination), movementDuration));
        }
    }

    private IEnumerator MoveToPos(Vector2 startPos, Vector2 endPos, float duration)
    {
        float time = 0.0f;
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

            if (!dirtParticleSystem.isPlaying) dirtParticleSystem.Play(); 
            var particleVel = dirtParticleSystem.velocityOverLifetime;

            if (_currentDirection == new Vector2Int(-1, 0) && (_currentDirectionAnimation != "Left" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Left";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

                particleVel.x = new ParticleSystem.MinMaxCurve(1.0f);
                particleVel.y = new ParticleSystem.MinMaxCurve(0.0f);
            }
            else if (_currentDirection == new Vector2Int(1, 0) && (_currentDirectionAnimation != "Right" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Right";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

                particleVel.x = new ParticleSystem.MinMaxCurve(-1.0f);
                particleVel.y = new ParticleSystem.MinMaxCurve(0.0f);
            }
            else if (_currentDirection == new Vector2Int(0, 1) && (_currentDirectionAnimation != "Up" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Up";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

                particleVel.x = new ParticleSystem.MinMaxCurve(0.0f);
                particleVel.y = new ParticleSystem.MinMaxCurve(-1.0f);
            }
            else if (_currentDirection == new Vector2Int(0, -1) && (_currentDirectionAnimation != "Down" || _animator.speed == 0.0f))
            {
                _currentDirectionAnimation = "Down";
                _animator.SetTrigger(_currentDirectionAnimation);
                _animator.speed = 1.0f;

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

            if (_audioSource.clip != wallHitClip)
            {
                _audioSource.clip = wallHitClip;
                _audioSource.Play();
            }
        }

        if (_animator.speed != 0.0f)
        {
            TileBase destTile = levelMap.GetTile(new Vector3Int(_targetCellDestination.x, _targetCellDestination.y, 0));
            _audioSource.clip = (destTile == pelletTile || destTile == powerPelletTile) ? eatClip : moveClip;

            _audioSource.Play();
        }
    }

    private Vector2 GetWorldPosFromCell(Vector2Int cellPos)
    {
        return new Vector2(
            cellPos.x * _worldCellSize.x + (_worldCellSize.x / 2.0f),
            cellPos.y * _worldCellSize.y + (_worldCellSize.y / 2.0f)
        );
    }

    private Vector2Int GetCellPosFromWorld(Vector2 worldPos)
    {
        return new Vector2Int(
            (int)Mathf.Floor(worldPos.x / _worldCellSize.x),
            (int)Mathf.Floor(worldPos.y / _worldCellSize.y)
        );
    }

    private Vector2 GetWorldPosNormalized(Vector2 worldPos)
    {
        return GetWorldPosFromCell(GetCellPosFromWorld(worldPos));
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
}
