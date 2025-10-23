using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostController : MonoBehaviour
{
    public string CurrentAnimation { get; private set; } = "StillForward";

    [SerializeField] private Tilemap levelMap;

    private Animator _animator;

    private Vector2 _worldCellSize;
    private Vector2 _initialSpawnPosition;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _worldCellSize = Vector2.Scale(levelMap.cellSize, new Vector3(
            Mathf.Abs(levelMap.transform.localScale.x),
            Mathf.Abs(levelMap.transform.localScale.y),
            Mathf.Abs(levelMap.transform.localScale.z)
        ));

        transform.position = GetWorldPosNormalized(transform.position);
        _initialSpawnPosition = transform.position;
    }

    public void TriggerGhostAnimation(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }

    public void TriggerGhostReset()
    {
        transform.position = _initialSpawnPosition;
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
}
