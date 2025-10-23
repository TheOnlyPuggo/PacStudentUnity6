using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CherryController : MonoBehaviour
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private float cherrySpawnDelay;
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private float spawnMargin;
    [SerializeField] private float cherryMoveSpeed;

    private GameObject _activeCherry;
    private float _cherrySpawnTime = 0.0f;
    private Vector2 _mapCenter;

    // Cherry Spawn
    private Vector2 _tileMapWorldSize;

    // Cherry Movement
    private bool _cherryIsMoving = false;
    private bool _cherryIsCloseToCenter = false;
    private Vector2 _cherryMoveDirection;

    private void Start()
    {
        BoundsInt mapBounds = levelMap.cellBounds;
        Grid mapGrid = levelMap.layoutGrid;
        _tileMapWorldSize = new Vector2(mapGrid.cellSize.x * mapBounds.size.x, mapGrid.cellSize.y * mapBounds.size.y);

        _mapCenter = new Vector2(levelMap.transform.position.x, levelMap.transform.position.y + 0.5f);
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted) return;

        CherrySpawnManage();
        CherryMoveManage();
        CherryDestroyHandle();
    }

    private void CherrySpawnManage()
    {
        if (_activeCherry != null) return;

        if (_cherrySpawnTime <= cherrySpawnDelay)
        {
            _cherrySpawnTime += Time.deltaTime;
            return;
        }

        _activeCherry = Instantiate(cherryPrefab, transform);
        _cherrySpawnTime = 0.0f;

        // Random position on map
        Vector3 _cherrySpawnPos = new Vector3(
            Random.Range(-_tileMapWorldSize.x / 2.0f + 0.5f, _tileMapWorldSize.x / 2.0f - 0.5f),
            Random.Range(-_tileMapWorldSize.y / 2.0f + 0.5f, _tileMapWorldSize.y / 2.0f + 0.5f),
            _activeCherry.transform.position.z
        );

        // Move towards closest edge
        if (Mathf.Abs(_cherrySpawnPos.x) < Mathf.Abs(_cherrySpawnPos.y))
        {
            _cherrySpawnPos.x = _tileMapWorldSize.x / 2.0f * (Mathf.Abs(_cherrySpawnPos.x) / _cherrySpawnPos.x);
            _cherrySpawnPos.x += 0.5f * -(Mathf.Abs(_cherrySpawnPos.x) / _cherrySpawnPos.x);
            _cherrySpawnPos.x += spawnMargin * (Mathf.Abs(_cherrySpawnPos.x) / _cherrySpawnPos.x);
        }
        else
        {
            _cherrySpawnPos.y = _tileMapWorldSize.y / 2.0f * (Mathf.Abs(_cherrySpawnPos.y) / _cherrySpawnPos.y);
            _cherrySpawnPos.y += 0.5f;
            _cherrySpawnPos.y += spawnMargin * (Mathf.Abs(_cherrySpawnPos.y) / _cherrySpawnPos.y);
        }

        _activeCherry.transform.position = _cherrySpawnPos;
    }

    private void CherryMoveManage()
    {
        if (_activeCherry == null) return;

        if (_cherryIsMoving == false)
        {
            _cherryMoveDirection = (_mapCenter - (Vector2)_activeCherry.transform.position).normalized;
            _cherryIsMoving = true;
        }

        _activeCherry.transform.position += new Vector3(
            _cherryMoveDirection.x * cherryMoveSpeed * Time.deltaTime,
            _cherryMoveDirection.y * cherryMoveSpeed * Time.deltaTime,
            0.0f
        );

        if (Vector2.Distance(_activeCherry.transform.position, _mapCenter) <= 0.1f) _cherryIsCloseToCenter = true;
    }

    private void CherryDestroyHandle()
    {
        if (_activeCherry == null) return;

        if (_cherryIsCloseToCenter == false) return;
        if (
            _activeCherry.transform.position.x < _tileMapWorldSize.x / 2.0f + 0.5f &&
            _activeCherry.transform.position.x > -_tileMapWorldSize.x / 2.0f - 0.5f &&
            _activeCherry.transform.position.y < _tileMapWorldSize.y / 2.0f + 0.5f &&
            _activeCherry.transform.position.y > -_tileMapWorldSize.y / 2.0f + 0.5f
        ) return;

        _cherryIsMoving = false;
        _cherryIsCloseToCenter = false;
        Destroy(_activeCherry);
    }

    public void PlayerDestroyCherry()
    {
        _cherryIsMoving = false;
        _cherryIsCloseToCenter = false;
        Destroy(_activeCherry);
    }
}
