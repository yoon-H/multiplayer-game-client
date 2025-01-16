using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    public List<GameObject> tiles = new List<GameObject>();

    private List<Vector3> positions = new List<Vector3>();

    private void Awake()
    {
        foreach(var tile in tiles)
        {
            positions.Add(tile.transform.position);
        }
    }

    public void ResetTiles()
    {
        for(int i=0; i<tiles.Count; i++)
        {
            tiles[i].transform.position = positions[i];
        }
    }
}
