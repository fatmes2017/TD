using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    [SerializeField] private GameObject mainPrefabe;
    [SerializeField] private int gridLength = 10;
    [SerializeField] private int gridWidth = 10;
   [SerializeField] private List<GameObject> createdTiles ;


    //private void Start()
    //{
    //    StartCoroutine(BuildGrid());
    //}


    //private IEnumerator BuildGrid()
    //{
    //    createdTiles = new List<GameObject>();
    //    for (int x = 0; x < gridLength; x++)
    //    {
    //        for (int z = 0; z < gridWidth; z++)
    //        {
    //            yield return new WaitForSeconds(.5f);
    //            CreateTile(x, z);
    //        }
    //    }
    //}
    [ContextMenu("Build grid")]
    private void BuildGrid()
    {
        createdTiles = new List<GameObject>();
        for (int x = 0; x < gridLength; x++)
        {
            for (int z = 0; z < gridWidth; z++)
            {
               
                CreateTile(x, z);
            }
        }
    }

    [ContextMenu("Clear grid")]
    private void ClearGrid()
    {
        for (int j = 0; j < createdTiles.Count; j++)
        {
            DestroyImmediate(createdTiles[j]);
            
        }
        createdTiles.Clear();
    }
    private void CreateTile(float xPosition, float zPosition)
    {
        Vector3 newPosition = new Vector3(xPosition, 0, zPosition);
      GameObject newTile=  Instantiate(mainPrefabe, newPosition, Quaternion.identity, transform);
      createdTiles.Add(newTile);
    }
}
