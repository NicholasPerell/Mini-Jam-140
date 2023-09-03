using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using DG.Tweening;
using System;

public class GridDrawer : MonoBehaviour
{
    const int DROP_SHADOW_LAYER = 1;
    const int HIGHLIGHT_LAYER = 2;
    const int SELECTION_LAYER = 3;

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
    Tile floorTileA, floorTileB;
    [SerializeField]
    Tile[] wallTiles;
    [SerializeField]
    Tile dropShadowTile;
    [SerializeField]
    Tile coinTile, pillarTile, enemyTile;
    [SerializeField]
    AnimatedTile normalSelectionTile, cornerSelectionTile;

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

        Array.Sort(levelData.walls, new PositionComparer());

        Vector2Int cellPosition;
        for (int i = levelData.walls.Length - 1; i >= 0; i--)
        {
            cellPosition = levelData.walls[i];
            int index = UnityEngine.Random.Range(0,wallTiles.Length);
            tilemap.SetTile(new Vector3Int(cellPosition.x,cellPosition.y), wallTiles[index]);

            if(i < levelData.walls.Length - 1
                && levelData.walls[i+1].x == cellPosition.x
                && levelData.walls[i+1].y != cellPosition.y + 1)
            {
                tilemap.SetTile(new Vector3Int(cellPosition.x, cellPosition.y, DROP_SHADOW_LAYER), dropShadowTile);
            }
        }

        Vector3Int cellCenter = new Vector3Int(tilemap.cellBounds.size.x / 2, tilemap.cellBounds.size.y / 2);
        Vector3 worldCenter = tilemap.CellToWorld(cellCenter) + Vector3.one * .5f;
        worldCenter.z = Camera.main.transform.position.z;
        Camera.main.transform.position = worldCenter;

        tilemap.FloodFill(new Vector3Int(levelData.playerPosition.x, levelData.playerPosition.y), floorTileA);
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
            if (floorTileA == tileBase)
            {
                tilemap.SetTile(position, position.x % 2 == position.y % 2 ? floorTileA : floorTileB);
                tilemap.RefreshTile(position);
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
                tilemap.SetTile(new Vector3Int(oldPositionHighlighted.x, oldPositionHighlighted.y, SELECTION_LAYER), null);
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
                tilemap.SetTile(new Vector3Int(positionHighlighted.x, positionHighlighted.y, SELECTION_LAYER), selectionTile);
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
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward * HIGHLIGHT_LAYER);
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward * SELECTION_LAYER);
    }

    private void Player_OnScarfConsidered(Vector2Int[] pillarsInRange, UnityAction<Vector2Int> InputCallback)
    {
        ReturnInput = InputCallback;
        selectionTile = cornerSelectionTile;
        selectionTileOffset = Vector3.one * .5f;
        foreach (Vector2Int pillar in pillarsInRange)
        {
            tilemap.SetTile(new Vector3Int(pillar.x, pillar.y, HIGHLIGHT_LAYER), pillarTile);
        }
        possiblePositions = pillarsInRange;
    }

    private void Player_OnCoinsDismissed()
    {
        ReturnInput = null;
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward * HIGHLIGHT_LAYER);
        tilemap.DeleteCells(Vector3Int.forward, Vector3Int.forward * SELECTION_LAYER);
    }

    private void Player_OnCoinsConsidered(Vector2Int[] tossableTiles, UnityAction<Vector2Int> InputCallback)
    {
        ReturnInput = InputCallback;
        selectionTile = normalSelectionTile;
        selectionTileOffset = Vector3.zero;
        foreach (Vector2Int tossTo in tossableTiles)
        {
            tilemap.SetTile(new Vector3Int(tossTo.x, tossTo.y, HIGHLIGHT_LAYER), coinTile);
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
