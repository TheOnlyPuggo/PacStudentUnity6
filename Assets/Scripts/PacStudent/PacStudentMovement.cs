using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentMovement : MonoBehaviour
{
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private TileBase pelletTile;
    [SerializeField] private float movementSpeed;
    [SerializeField] private Vector2Int[] gridMovementDirections;
    [SerializeField] private AudioClip movementClip;

    private Animator _animator;
    private AudioSource _audioSource;

    private Vector2 _worldCellSize;
    private int _currentMovementDirectionIndex;
    private Vector2 _currentTargetDestination;

    private bool _inMovementLerp = false;
    private Vector2 _initialPosition;
    private float _moveTimer = 0.0f;

    private Vector3Int _steppedOnTilePos;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        _worldCellSize = Vector2.Scale(levelMap.cellSize, new Vector3(
            Mathf.Abs(levelMap.transform.localScale.x),
            Mathf.Abs(levelMap.transform.localScale.y),
            Mathf.Abs(levelMap.transform.localScale.z)
        ));
        _currentMovementDirectionIndex = 0;
        _currentTargetDestination =
            transform.position + new Vector3(_worldCellSize.x * gridMovementDirections[0].x, _worldCellSize.y * gridMovementDirections[0].y, 0.0f);
        SetDirectionAnimTrigger();
        _audioSource.clip = movementClip;
    }

    void Update()
    {
        if (Vector2.Distance((Vector2)transform.position, _currentTargetDestination) > 0.01f)
        {
            if (!_inMovementLerp)
            {
                _inMovementLerp = true;
                _initialPosition = transform.position;
                _moveTimer = 0.0f;
            }
            _moveTimer += Time.deltaTime;

            transform.position = Vector3.Lerp(
                new Vector3(_initialPosition.x, _initialPosition.y, transform.position.z),
                new Vector3(_currentTargetDestination.x, _currentTargetDestination.y, transform.position.z),
                _moveTimer * movementSpeed / Mathf.Abs(gridMovementDirections[_currentMovementDirectionIndex].x + gridMovementDirections[_currentMovementDirectionIndex].y)
            );
        }

        if (Vector2.Distance((Vector2)transform.position, _currentTargetDestination) <= 0.01f && _inMovementLerp)
        {
            _inMovementLerp = false;
            transform.position = new Vector3(_currentTargetDestination.x, _currentTargetDestination.y, transform.position.z);
            CycleTargetDestination();
        }

        FootstepTrack();
    }

    private void CycleTargetDestination()
    {
        _currentMovementDirectionIndex += 1;
        if (_currentMovementDirectionIndex >= gridMovementDirections.Length) _currentMovementDirectionIndex = 0;
        Vector2Int currentGridMove = gridMovementDirections[_currentMovementDirectionIndex];
        _currentTargetDestination =
            transform.position + new Vector3(
                _worldCellSize.x * currentGridMove.x,
                _worldCellSize.y * currentGridMove.y,
                0.0f
            );

        ResetTriggers();
        SetDirectionAnimTrigger();
    }

    private void ResetTriggers()
    {
        _animator.ResetTrigger("Right");
        _animator.ResetTrigger("Left");
        _animator.ResetTrigger("Up");
        _animator.ResetTrigger("Down");
    }

    private void SetDirectionAnimTrigger()
    {
        Vector2Int currentGridMove = gridMovementDirections[_currentMovementDirectionIndex];
        if (currentGridMove.x > 0)
        {
            _animator.SetTrigger("Right");
        }
        else if (currentGridMove.x < 0)
        {
            _animator.SetTrigger("Left");
        }
        else if (currentGridMove.y > 0)
        {
            _animator.SetTrigger("Up");
        }
        else if (currentGridMove.y < 0)
        {
            _animator.SetTrigger("Down");
        }
    }

    private void FootstepTrack()
    {
        Vector3Int closestTileCellPos = levelMap.WorldToCell(transform.position);
        Vector3 closestTileWorldPos = levelMap.GetCellCenterWorld(closestTileCellPos);
        TileBase tile = levelMap.GetTile(closestTileCellPos);

        if (Vector2.Distance(closestTileWorldPos, transform.position) < 0.1f && closestTileCellPos != _steppedOnTilePos)
        {
            _steppedOnTilePos = closestTileCellPos;
            if (tile != pelletTile) _audioSource.Play();
        }
    }
}
