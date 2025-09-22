using UnityEngine;
using UnityEngine.Tilemaps;

public class PowerPelletSpawn : MonoBehaviour
{
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private TileBase powerPelletTile;
    [SerializeField] private GameObject powerPellet;

    void Start()
    {
        foreach (Vector3Int pos in levelMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = levelMap.GetTile(pos);
            if (tile == powerPelletTile)
            {
                GameObject newPowerPellet = Instantiate(powerPellet);
                newPowerPellet.transform.position = (Vector2)levelMap.GetCellCenterWorld(pos);
            }
        }
    }
}
