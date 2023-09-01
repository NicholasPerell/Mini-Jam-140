using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGrid : MonoBehaviour
{

    [SerializeField] private int _width, _height;
    [SerializeField] private CoinMark _tilePrefab;

    LevelData currentLevelData;

    public void CreateGrid(LevelData levelData)
    {
        currentLevelData = levelData;
        GenerateGrid();
    }


    void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(currentLevelData.playerPosition.x, currentLevelData.playerPosition.y), Quaternion.identity);
                spawnedTile.name = $"CoinMark {x} {y}"; 
            }
        }
    }
}
