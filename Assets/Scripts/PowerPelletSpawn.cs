using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PowerPelletSpawn : MonoBehaviour
{
    [SerializeField] private Tilemap levelMap;
    [SerializeField] private TileBase powerPelletTile;
    [SerializeField] private GameObject powerPellet;

    private List<GameObject> powerPelletList = new List<GameObject>();
    private List<Vector2Int> powerPelletPositionsList = new List<Vector2Int>();

    void Start()
    {
        foreach (Vector3Int pos in levelMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = levelMap.GetTile(pos);
            if (tile == powerPelletTile)
            {
                GameObject newPowerPellet = Instantiate(powerPellet);
                newPowerPellet.transform.position = new Vector3(levelMap.GetCellCenterWorld(pos).x, levelMap.GetCellCenterWorld(pos).y, newPowerPellet.transform.position.z);
                powerPelletList.Add(newPowerPellet);
                powerPelletPositionsList.Add((Vector2Int)pos);
            }
        }
    }

    public void DeletePowerPelletAtPos(Vector2Int pos)
    {
        for (int i = 0; i < powerPelletList.Count; i++)
        {
            if (powerPelletPositionsList[i] == pos)
            {
                Destroy(powerPelletList[i]);
                powerPelletList.RemoveAt(i);
                powerPelletPositionsList.RemoveAt(i);
                return;
            }
        }
    }
}
