using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using DG.Tweening;
using System;

public class GridDrawer : MonoBehaviour
{
    [SerializeField]
    TurnManager turnManager;
    [SerializeField]
    PlayerController player;

    [Space]
    [SerializeField] LineRenderer scarfLine;

    [Space]
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    Tile[] floorTiles;
    [SerializeField]
    Tile[] wallTiles;
    [SerializeField]
    Tile coinTile, pillarTile, enemyTile;
    [SerializeField]
    AnimatedTile normalSelectionTile, cornerSelectionTile;
    [SerializeField]
    [Range(.5f, 1f)]
    float checkerFloorTint = .5f;

    //Input
    private AnimatedTile selectionTile;
    private UnityAction<Vector2Int> ReturnInput;
    private Vector2Int[] possiblePositions;
    private Vector2Int positionHighlighted = -Vector2Int.one;
    private Vector3 selectionTileOffset = Vector3.zero;

    //Scarf
    private Vector2Int[] pillars;
    private int[] pillarWrapOrder;


    void OnEnable()
    {
        turnManager.OnInitializeLevel += RespondToInitializeLevel;
        player.OnCoinsConsidered += Player_OnCoinsConsidered;
        player.OnCoinsDismissed += Player_OnCoinsDismissed;
        player.OnScarfConsidered += Player_OnScarfConsidered;
        player.OnScarfDismissed += Player_OnScarfDismissed;
        player.OnScarfUpdated += DrawScarf;
    }

    private void OnDisable()
    {
        turnManager.OnInitializeLevel -= RespondToInitializeLevel;
        player.OnCoinsConsidered -= Player_OnCoinsConsidered;
        player.OnCoinsDismissed -= Player_OnCoinsDismissed;
        player.OnScarfConsidered -= Player_OnScarfConsidered;
        player.OnScarfDismissed -= Player_OnScarfDismissed;
        player.OnScarfUpdated -= DrawScarf;
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
        pillars = levelData.pillars;
        pillarWrapOrder = new int[0];
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

    private void Update()
    {
        CheckForInput();
        UpdateLine();
    }

    private void CheckForInput()
    {
        if(ReturnInput != null)
        {
            Vector2Int oldPositionHighlighted = positionHighlighted;
            Vector3 mouseWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
            positionHighlighted = (Vector2Int)tilemap.WorldToCell(mouseWorldSpace - selectionTileOffset);

            if (positionHighlighted != oldPositionHighlighted)
            {
                tilemap.SetTile(new Vector3Int(oldPositionHighlighted.x, oldPositionHighlighted.y, 2), null);
            }

            bool overSelection = false;
            for(int i = 0; i < possiblePositions.Length; i++)
            {
                if(possiblePositions[i] == positionHighlighted)
                {
                    overSelection = true;
                    break;
                }
            }

            if (overSelection)
            {
                tilemap.SetTile(new Vector3Int(positionHighlighted.x, positionHighlighted.y, 2), selectionTile);
                if (Input.GetMouseButtonDown(0))
                {
                    ReturnInput(positionHighlighted);
                }
            }
        }
    }

    private void Player_OnScarfDismissed()
    {
        ReturnInput = null;
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward);
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward * 2);
    }

    private void Player_OnScarfConsidered(Vector2Int[] pillarsInRange, UnityAction<Vector2Int> InputCallback)
    {
        ReturnInput = InputCallback;
        selectionTile = cornerSelectionTile;
        selectionTileOffset = Vector3.one * .5f;
        foreach (Vector2Int pillar in pillarsInRange)
        {
            tilemap.SetTile(new Vector3Int(pillar.x, pillar.y, 1), pillarTile);
        }
        possiblePositions = pillarsInRange;
    }

    private void Player_OnCoinsDismissed()
    {
        ReturnInput = null;
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward);
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward * 2);
    }

    private void Player_OnCoinsConsidered(Vector2Int[] tossableTiles, UnityAction<Vector2Int> InputCallback)
    {
        ReturnInput = InputCallback;
        selectionTile = normalSelectionTile;
        selectionTileOffset = Vector3.zero;
        foreach (Vector2Int tossTo in tossableTiles)
        {
            tilemap.SetTile(new Vector3Int(tossTo.x, tossTo.y, 1), coinTile);
        }
        possiblePositions = tossableTiles;
    }

    private void DrawScarf(List<int> wrappingIndex)
    {
        pillarWrapOrder = wrappingIndex.ToArray();
        UpdateLine();
    }

    private void UpdateLine()
    {
        Vector3[] points = new Vector3[pillarWrapOrder.Length + 1];

        Vector2Int cellPos;
        for(int i = 0; i < points.Length - 1; i++)
        {
            cellPos = pillars[pillarWrapOrder[i]];
            points[i] = tilemap.CellToWorld(new Vector3Int(cellPos.x, cellPos.y, 0)) + new Vector3(1,1);
        }
        points[points.Length - 1] = player.transform.position;

        scarfLine.positionCount = points.Length;
        scarfLine.SetPositions(points);
    }
}
