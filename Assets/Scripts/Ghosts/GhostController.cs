using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostController : MonoBehaviour
{
    public string CurrentAnimation { get; private set; } = "StillForward";
    public bool GhostIsDead { get; private set; } = false;

    [SerializeField] private Tilemap levelMap;
    [SerializeField] private float deathLength;

    private Animator _animator;

    private Vector2 _worldCellSize;
    private Vector2 _initialSpawnPosition;
    private float _ghostDeadTimer = 0.0f;

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

    private void Update()
    {
        if (!GameManager.Instance.GameStarted) return;

        if (GhostIsDead)
        {
            _ghostDeadTimer += Time.deltaTime;

            if (_ghostDeadTimer > deathLength)
            {
                GhostIsDead = false;
                _ghostDeadTimer = 0.0f;

                if (GameManager.Instance.GhostManager.GhostsAreScared)
                {
                    GameManager.Instance.GhostManager.SetGhostAnimState(this, GhostManager.GhostAnimState.Scared);
                } else
                {
                    GameManager.Instance.GhostManager.SetGhostAnimState(this, GhostManager.GhostAnimState.Normal);
                }
            }
        }
    }

    public void TriggerGhostAnimation(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }

    public void TriggerGhostReset()
    {
        transform.position = _initialSpawnPosition;
        _animator.SetTrigger("NormalForward");
    }

    public void TriggerGhostDeath()
    {
        _animator.SetTrigger("Death");
        GameManager.Instance.GameAudioManager.PlayOneGhostDeadMusic();
        GameManager.Instance.AddScore(300);
        GhostIsDead = true;
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
