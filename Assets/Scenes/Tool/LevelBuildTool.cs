using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelBuildTool : MonoBehaviour
{
    [SerializeField]
    Tilemap tilemap;

    [SerializeField]
    [Min(0)]
    int coins;

    [Space]
    [SerializeField]
    LevelDataObject levelToSaveTo;

#if UNITY_EDITOR
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
            level.coins = coins;

            levelToSaveTo.Load(level);
        }
    }
#endif
}
