using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelBuildTool : MonoBehaviour
{
    [Header("DO NOT EDIT")]
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    Tile wallTile;
    [SerializeField]
    GameObject spawnIndicatorPrefab;
    [SerializeField]
    PlayerController playerController;
    [SerializeField]
    TurnManager turnManager;
    [SerializeField]
    SpriteRenderer[] bottomLeftIndicators;

    [Space]
    [Header("Data To Edit")]
    [SerializeField]
    [Min(0)]
    int coinsToStartWith;

    [Space]
    [SerializeField]
    LevelDataObject levelToSaveTo;

#if UNITY_EDITOR
    private void Awake()
    {
        for (int i = 0; i < bottomLeftIndicators.Length; i++)
        {
            Destroy(bottomLeftIndicators[i].gameObject);
        }

        levelToSaveTo = ScriptableObject.CreateInstance<LevelDataObject>();
        Save();

        LevelSpawnIndicator[] spawnIndicators = GameObject.FindObjectsOfType<LevelSpawnIndicator>();
        for (int i = 0; i < spawnIndicators.Length; i++)
        {
            Destroy(spawnIndicators[i].gameObject);
        }

        playerController.gameObject.SetActive(true);
        turnManager.SetLevel(levelToSaveTo);
    }

    public void Save()
    {
        if (levelToSaveTo)
        {
            LevelData level = new LevelData();

            //Walls
            List<Vector2Int> walls = new List<Vector2Int>();
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
                if (tileBase != null)
                {
                    walls.Add(new Vector2Int(position.x,position.y));
                }
            }
            level.walls = walls.ToArray();

            //Players, Pillars, Enemies
            List<LevelData.EnemyData> enemies = new List<LevelData.EnemyData>();
            List<Vector2Int> pillars = new List<Vector2Int>();
            LevelSpawnIndicator[] spawnIndicators = GameObject.FindObjectsOfType<LevelSpawnIndicator>();
            Vector2Int positionSpawnedAt;
            foreach(LevelSpawnIndicator spawned in spawnIndicators)
            {
                positionSpawnedAt = (Vector2Int)tilemap.WorldToCell(spawned.transform.position);
                switch(spawned.type)
                {
                    case LevelSpawnIndicator.Type.PLAYER:
                        level.playerPosition = positionSpawnedAt;
                        level.playerDirectionFacing = spawned.facing;
                        break;
                    case LevelSpawnIndicator.Type.PILLAR:
                        pillars.Add(positionSpawnedAt);
                        break;
                    default:
                        LevelData.EnemyData enemy = new LevelData.EnemyData();
                        enemy.type = System.Enum.Parse<EnemyType>(spawned.type.ToString(),true);
                        enemy.position = positionSpawnedAt;
                        enemy.directionFacing = spawned.facing;
                        enemies.Add(enemy);
                        break;
                }
            }
            level.enemies = enemies.ToArray();
            level.pillars = pillars.ToArray();
            level.coinPath = new Vector2Int[0];
            level.pillarsWrapped = new List<int>();

            //Coins
            level.coins = coinsToStartWith;

            levelToSaveTo.Load(level);
        }
    }

    public void Load()
    {
        if (levelToSaveTo)
        {
            LevelData level = levelToSaveTo.Data.Copy();

            //Clear Old Stuff
            tilemap.ClearAllTiles();
            LevelSpawnIndicator[] spawnIndicators = GameObject.FindObjectsOfType<LevelSpawnIndicator>();
            for (int i = 0; i < spawnIndicators.Length; i++)
            {
                DestroyImmediate(spawnIndicators[i].gameObject);
            }

            //Walls
            foreach (Vector3Int wall in level.walls)
            {
                tilemap.SetTile(wall, wallTile);
            }

            //Player
            LevelSpawnIndicator player = Instantiate(spawnIndicatorPrefab, tilemap.CellToWorld((Vector3Int)level.playerPosition) + new Vector3(.5f, .5f), Quaternion.identity).GetComponent<LevelSpawnIndicator>();
            player.facing = level.playerDirectionFacing;
            player.type = LevelSpawnIndicator.Type.PLAYER;
            player.UpdateVisuals();

            //Pillars
            foreach (Vector3Int pillarPos in level.pillars)
            {
                LevelSpawnIndicator pillar = Instantiate(spawnIndicatorPrefab, tilemap.CellToWorld(pillarPos) + new Vector3(.5f, .5f), Quaternion.identity).GetComponent<LevelSpawnIndicator>();
                pillar.type = LevelSpawnIndicator.Type.PILLAR;
                pillar.UpdateVisuals();
            }

            //Enemies
            foreach (LevelData.EnemyData enemyData in level.enemies)
            {
                LevelSpawnIndicator enemy = Instantiate(spawnIndicatorPrefab, tilemap.CellToWorld((Vector3Int)enemyData.position) + new Vector3(.5f, .5f), Quaternion.identity).GetComponent<LevelSpawnIndicator>();
                enemy.type = System.Enum.Parse<LevelSpawnIndicator.Type>(enemyData.type.ToString());
                enemy.facing = enemyData.directionFacing;
                enemy.UpdateVisuals();
            }

            //Coins
            coinsToStartWith = level.coins;
        }
    }
#endif
}
