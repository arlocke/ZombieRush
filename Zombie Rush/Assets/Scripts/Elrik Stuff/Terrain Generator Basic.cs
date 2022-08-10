using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorBasic : MonoBehaviour
{
    public List<GameObject> terrainObjects = new List<GameObject>();
    public int quantity = 1;
    public Vector2 sPoint = new Vector2(0, 0);

    private int height = 1;
    private int width = 1;

    private void Awake()
    {
        generateTiles();
    }
    private void Start()
    {
        
    }
    private void generateTiles()
    {
        int count = 0;
        while(count < quantity)
        {
            //Make Tiles Go Burrrr
        }

    }
}
