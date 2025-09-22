using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private bool useLevelGenerator;
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private TileBase[] levelTiles;
    [SerializeField] private TileBase[] wallTiles;
    [SerializeField] private TileBase[] trackTiles;
    [SerializeField] private TileBase[] cornerTiles;

    int[,] _levelMapArray =
        {
            {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
            {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
            {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
            {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
            {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
            {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
            {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
            {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
            {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
            {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
            {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
            {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
            {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
            {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
            {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
        };

    (int, int)[] _arrayQueries = new (int, int)[]
    {
        (0, -1),
        (1, -1),
        (1, 0),
        (1, 1),
        (0, 1),
        (-1, 1),
        (-1, 0),
        (-1, -1),
    };

    private Vector2 _worldCellSize;

    void Start()
    {
        if (!useLevelGenerator) return;
        levelMap.ClearAllTiles();

        _worldCellSize = Vector2.Scale(levelMap.cellSize, levelMap.transform.localScale);
        levelMap.transform.position = new Vector3(
            transform.position.x - (_worldCellSize.x * _levelMapArray.GetLength(1)),
            transform.position.y + (_worldCellSize.y * _levelMapArray.GetLength(0)),
            transform.position.z
        );
        levelMap.transform.localScale = new Vector3(
            levelMap.transform.localScale.x,
            -levelMap.transform.localScale.y,
            levelMap.transform.localScale.z
        );

        for (int y = 0; y < _levelMapArray.GetLength(0); y++)
        {
            for (int x = 0; x < _levelMapArray.GetLength(1); x++)
            {
                int wallScore = GetWallScore(new Vector3Int(x, y, 0));

                switch (_levelMapArray[y, x])
                {
                    case 3:
                    case 1:
                        CornerTilePlaceHandle(new Vector3Int(x, y, 0), wallScore, GetTrackTilesAroundAmount(new Vector3Int(x, y, 0)));
                        break;
                    case 2:
                    case 4:
                        WallTilePlaceHandle(new Vector3Int(x, y, 0), wallScore);
                        break;
                    default:
                        levelMap.SetTile(new Vector3Int(x, y, 0), levelTiles[_levelMapArray[y, x]]);
                        break;
                }
            }
        }
    }

    private int GetWallScore(Vector3Int cellPos)
    {
        int currentWallScore = 0;
        int wallIncrementTrack = 1;

        foreach ((int x, int y) query in _arrayQueries)
        {
            if (
                CheckWithinBounds(cellPos.x + query.x, cellPos.y + query.y) &&
                IsWallTile(levelTiles[_levelMapArray[cellPos.y + query.y, cellPos.x + query.x]])
            )
            {
                currentWallScore += wallIncrementTrack;
            }
            wallIncrementTrack *= 2;
        }

        return currentWallScore;
    }

    private int GetTrackTilesAroundAmount(Vector3Int cellPos)
    {
        int result = 0;

        foreach ((int x, int y) query in _arrayQueries)
        {
            if (
                CheckWithinBounds(cellPos.x + query.x, cellPos.y + query.y) &&
                IsTrackTile(new Vector2Int(cellPos.x + query.x, cellPos.y + query.y))
            )
            {
                result += 1;
            }
        }

        Debug.Log($"CellPos: ({cellPos.x}, {cellPos.y}) Empty Amount: {result}");
        return result;
    }

    private bool CheckWithinBounds(int xPos, int yPos)
    {
        if (xPos < _levelMapArray.GetLength(1) && xPos >= 0 && yPos < _levelMapArray.GetLength(0) && yPos >= 0) return true;
        return false;
    }

    private bool IsWallTile(TileBase tile)
    {
        foreach (TileBase wallTile in wallTiles)
        {
            if (tile == wallTile) return true;
        }

        return false;
    }

    private bool IsTrackTile(Vector2Int cellPos)
    {

        TileBase tile = levelTiles[_levelMapArray[cellPos.y, cellPos.x]];

        foreach (TileBase trackTile in trackTiles)
        {
            if (tile == trackTile) return true;
        }

        return false;
    }

    private float CornerPieceRotationAmount(int wallScore)
    {
        switch (wallScore)
        {
            case 5: return 0.0f;
            case 20: return 90.0f;
            case 80: return 180.0f;
            case 65: return 270.0f;
        }

        return 0.0f;
    }

    private float WallPieceRotationAmount(int wallScore)
    {
        switch (wallScore)
        {
            case 5: return 0.0f;
            case 10: return 90.0f;
        }

        return 0.0f;
    }

    private void CornerTilePlaceHandle(Vector3Int pos, int wallScore, int trackTilesAmount)
    {
        TileBase tileToSet = trackTilesAmount < 2 ? cornerTiles[0] : cornerTiles[1];

        levelMap.SetTile(new Vector3Int(pos.x, pos.y, 0), tileToSet);
        levelMap.SetTransformMatrix(
            new Vector3Int(pos.x, pos.y, 0),
            Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, CornerPieceRotationAmount(wallScore)), Vector3.one)
        );
    }

    private void WallTilePlaceHandle(Vector3Int pos, int wallScore)
    {
        levelMap.SetTile(new Vector3Int(pos.x, pos.y, 0), levelTiles[2]);
    }
}
