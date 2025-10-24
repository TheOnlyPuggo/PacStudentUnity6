using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostController : MonoBehaviour
{
    public string CurrentAnimation { get; private set; } = "StillForward";
    public bool GhostIsDead { get; private set; } = false;

    private enum GhostBehaviour
    {
        GHOST1,
        GHOST2,
        GHOST3,
        GHOST4
    }

    [SerializeField] private Tilemap levelMap;
    [SerializeField] private float deathLength;
    [SerializeField] private TileBase[] pathTiles;
    [SerializeField] private TileBase ghostExitTile;
    [SerializeField] private GhostBehaviour ghostBehaviour;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform ghost4NodesParent;

    private Animator _animator;

    private Vector2 _worldCellSize;
    private Vector2 _initialSpawnPosition;
    private float _ghostDeadTimer = 0.0f;
    private bool _hasUsedExitTile = false;
    private int _ghost4NodeIndex = 0;
    private List<Transform> _ghost4WallNodes = new List<Transform>();
    private float _playerMoveSpeed;

    // Movement
    private Vector2Int[] relativePositionsToQuery =
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1)
    };

    private bool _inLerp = false;
    private Vector2Int _lastTilePosition;
    private Coroutine _activeMoveCoroutine;

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

        BoundsInt mapBounds = levelMap.cellBounds;
        Grid mapGrid = levelMap.layoutGrid;

        if (ghost4NodesParent != null)
        {
            Transform[] ghost4Nodes = ghost4NodesParent.GetComponentsInChildren<Transform>();
            foreach (Transform ghost4Node in ghost4Nodes)
            {
                if (ghost4Node != ghost4NodesParent) _ghost4WallNodes.Add(ghost4Node);
            }
        }

        _playerMoveSpeed = playerTransform.GetComponent<PacStudentController>().GetMovementSpeed();

        GhostMovementSetup();
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted) return;

        //GhostDeathHandleOld();
        GhostDeathHandle();
        GhostMovementHandle();
    }

    private void GhostMovementSetup()
    {
        transform.position = GetWorldPosNormalized(transform.position);
        _initialSpawnPosition = transform.position;
        _lastTilePosition = GetCellPosFromWorld(transform.position);
    }

    private void GhostMovementHandle()
    {
        if (!_inLerp && !GhostIsDead && !GameManager.Instance.GhostManager.PlayerIsDead)
        {
            _inLerp = true;
            List<Vector2Int> availableMovePositions = QueryAvailableMovePositions(GetCellPosFromWorld(transform.position), _lastTilePosition);

            int randomPos = Random.Range(0, availableMovePositions.Count);

            float moveSpeed;

            if (GameManager.Instance.GhostManager.GhostsAreScared) moveSpeed = 1.0f / (_playerMoveSpeed * 0.9f * 0.5f);
            else moveSpeed = 1.0f / (_playerMoveSpeed * 0.9f);

            _activeMoveCoroutine = StartCoroutine(GhostMoveToPos(transform.position, GetWorldPosFromCell(availableMovePositions[randomPos]), moveSpeed, 0.0f));
            _lastTilePosition = GetCellPosFromWorld(transform.position);
        }
    }

    private List<Vector2Int> QueryAvailableMovePositions(Vector2Int posToQueryFrom, Vector2Int excludePosition)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        foreach (Vector2Int queryDirection in relativePositionsToQuery)
        {
            Vector2Int queryPos = posToQueryFrom + queryDirection;
            TileBase queryTile = levelMap.GetTile((Vector3Int)queryPos);

            if (queryTile == ghostExitTile && !_hasUsedExitTile)
            {
                _hasUsedExitTile = true;
                availablePositions.Clear();
                availablePositions.Add(queryPos);
                return availablePositions;
            }

            foreach (TileBase pathTile in pathTiles)
            {
                if (pathTile == queryTile && queryPos != excludePosition) availablePositions.Add(queryPos);
            }
        }

        if (ghostBehaviour == GhostBehaviour.GHOST4 && Vector2.Distance(transform.position, _ghost4WallNodes[_ghost4NodeIndex].transform.position) < 0.5f)
        {
            _ghost4NodeIndex += 1;
            if (_ghost4NodeIndex >= _ghost4WallNodes.Count) _ghost4NodeIndex = 0;
        }

        if (availablePositions.Count == 0) availablePositions.Add(excludePosition);
        else if (availablePositions.Count > 1)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (ghostBehaviour == GhostBehaviour.GHOST1 || GameManager.Instance.GhostManager.GhostsAreScared)
            {
                List<Vector2Int> closePositions = new List<Vector2Int>();

                for (int i = 0; i < availablePositions.Count; i++)
                {
                    if (Vector2.Distance(GetWorldPosFromCell(availablePositions[i]), playerTransform.position) >= distanceToPlayer)
                    {
                        closePositions.Add(availablePositions[i]);
                    }
                }

                if (closePositions.Count != 0) return closePositions;
                else return availablePositions;
            } else if (ghostBehaviour == GhostBehaviour.GHOST2)
            {
                List<Vector2Int> farPositions = new List<Vector2Int>();

                for (int i = 0; i < availablePositions.Count; i++)
                {
                    if (Vector2.Distance(GetWorldPosFromCell(availablePositions[i]), playerTransform.position) <= distanceToPlayer)
                    {
                        farPositions.Add(availablePositions[i]);
                    }
                }

                if (farPositions.Count != 0) return farPositions;
                else return availablePositions;
            } else if (ghostBehaviour == GhostBehaviour.GHOST3)
            {
                return availablePositions;
            } else if (ghostBehaviour == GhostBehaviour.GHOST4)
            {
                float closestDistanceToNode = -1.0f;
                Vector2Int closestPos = availablePositions[0];

                for (int i = 0; i < availablePositions.Count; i++)
                {
                    if (Vector2.Distance(GetWorldPosFromCell(availablePositions[i]), _ghost4WallNodes[_ghost4NodeIndex].transform.position) <= closestDistanceToNode || closestDistanceToNode == -1.0f)
                    {
                        closestDistanceToNode = Vector2.Distance(GetWorldPosFromCell(availablePositions[i]), _ghost4WallNodes[_ghost4NodeIndex].transform.position);
                        closestPos = availablePositions[i];
                    }
                }


                List<Vector2Int> closestPosList = new List<Vector2Int>();
                closestPosList.Add(closestPos);
                return closestPosList;
            }
        }

        return availablePositions;
    }

    private IEnumerator GhostMoveToPos(Vector2 startPos, Vector2 endPos, float duration, float setTime)
    {
        if (endPos.x < startPos.x)
        {
            if (GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "ScaredLeft")
            {
                CurrentAnimation = "ScaredLeft";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
            else if (!GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "NormalLeft")
            {
                CurrentAnimation = "NormalLeft";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
        }
        else if (endPos.x > startPos.x)
        {
            if (GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "ScaredRight")
            {
                CurrentAnimation = "ScaredRight";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
            else if (!GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "NormalRight")
            {
                CurrentAnimation = "NormalRight";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
        }
        else if (endPos.y > startPos.y)
        {
            if (GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "ScaredBackward")
            {
                CurrentAnimation = "ScaredBackward";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
            else if (!GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "NormalBackward")
            {
                CurrentAnimation = "NormalBackward";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
        }
        else if (endPos.y < startPos.y)
        {
            if (GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "ScaredForward")
            {
                CurrentAnimation = "ScaredForward";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
            else if (!GameManager.Instance.GhostManager.GhostsAreScared && CurrentAnimation != "NormalForward")
            {
                CurrentAnimation = "NormalForward";
                _animator.SetTrigger(CurrentAnimation);
                _animator.speed = 1.0f;
            }
        }

        float time = setTime;
        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        _inLerp = false;
    }

    public void ResetGhostToStart()
    {
        _inLerp = false;
        _activeMoveCoroutine = null;
        transform.position = _initialSpawnPosition;
        _hasUsedExitTile = false;
    }

    private void GhostDeathHandle()
    {
        if (!GhostIsDead) return;

        if (_activeMoveCoroutine != null) _activeMoveCoroutine = null;

        if (Vector2.Distance(transform.position, _initialSpawnPosition) < 0.05f)
        {
            GhostIsDead = false;
            ResetGhostToStart();

            GameAudioManager gameAudioManager = GameManager.Instance.GameAudioManager;
            AudioSource gameAudioSource = gameAudioManager.GetComponent<AudioSource>();
            if (gameAudioSource.clip == gameAudioManager.GetOneGhostDeadClip() && GameManager.Instance.GhostManager.SeeAmountOfGhostsDead() == 0)
            {
                gameAudioSource.clip = gameAudioManager.GetGhostScaredClip();
                gameAudioSource.Play();
            }
        }

        Vector2 spawnDirection = (_initialSpawnPosition - (Vector2)transform.position).normalized;
        float moveSpeed = 1.0f / (_playerMoveSpeed * 0.9f * 0.5f);
        transform.position += new Vector3(
            spawnDirection.x * moveSpeed * Time.deltaTime,
            spawnDirection.y * moveSpeed * Time.deltaTime,
            0.0f
        );
    }

    private void GhostDeathHandleOld()
    {
        if (!GhostIsDead) return;

        _ghostDeadTimer += Time.deltaTime;

        if (_ghostDeadTimer > deathLength)
        {
            GhostIsDead = false;
            _ghostDeadTimer = 0.0f;

            if (GameManager.Instance.GhostManager.GhostsAreScared)
            {
                GameManager.Instance.GhostManager.SetGhostAnimState(this, GhostManager.GhostAnimState.Scared);
                GameManager.Instance.GameAudioManager.PlayScaredGhostMusic(-1.0f);
            }
            else
            {
                GameManager.Instance.GhostManager.SetGhostAnimState(this, GhostManager.GhostAnimState.Normal);
            }
        }
    }

    public void TriggerGhostAnimation(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }

    public void TriggerGhostDeath()
    {
        _animator.SetTrigger("Death");
        GameAudioManager gameAudioManager = GameManager.Instance.GameAudioManager;
        if (gameAudioManager.GetComponent<AudioSource>().clip != gameAudioManager.GetOneGhostDeadClip())
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
