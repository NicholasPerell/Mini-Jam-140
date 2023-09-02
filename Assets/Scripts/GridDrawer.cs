using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class GridDrawer : MonoBehaviour
{
    [SerializeField]
    TurnManager turnManager;

    [Space]
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    Tile[] floorTiles;
    [SerializeField]
    Tile[] wallTiles;
    [SerializeField]
    [Range(.5f, 1f)]
    float checkerFloorTint = .5f;

    void OnEnable()
    {
        turnManager.OnInitializeLevel += RespondToInitializeLevel;
    }

    private void OnDisable()
    {
        turnManager.OnInitializeLevel -= RespondToInitializeLevel;
    }

    private void RespondToInitializeLevel(LevelData levelData)
    {
        tilemap.ClearAllTiles();

        foreach(Vector2Int cellPosition in levelData.walls)
        {
            int index = cellPosition.x % 2 + cellPosition.y % 2 * 2;
            tilemap.SetTile(new Vector3Int(cellPosition.x,cellPosition.y), wallTiles[index]);
        }

        tilemap.FloodFill(new Vector3Int(levelData.playerPosition.x, levelData.playerPosition.y), floorTiles[0]);
        CheckerFloorTiles();
    }


    void CheckerFloorTiles()
    {
        BoundsInt bounds = tilemap.cellBounds;
        int sizeSum = bounds.size.x * bounds.size.y * bounds.size.z;
        Vector3Int position;
        TileBase tileBase;
        for (int i = 0; i < sizeSum; i++)
        {
            position = bounds.min + new Vector3Int(i % bounds.size.x, 
                                                    i / bounds.size.x % bounds.size.y, 
                                                    i / bounds.size.x / bounds.size.y % bounds.size.z);
            tileBase = tilemap.GetTile(position);
            foreach(Tile floorTile in floorTiles)
            {
                if(floorTile == tileBase)
                {
                    bool isEven = (position.x + position.y) % 2 == 0;
                    ((Tile)tileBase).color = isEven ? Color.white : new Color(checkerFloorTint, checkerFloorTint, checkerFloorTint);
                    tilemap.RefreshTile(position);
                }
            }
        }
    }
}
