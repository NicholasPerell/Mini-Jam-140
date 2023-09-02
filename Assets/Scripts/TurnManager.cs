using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class TurnManager : MonoBehaviour
{
    [SerializeField]
    LevelDataObject currentLevelDataObject;
    [SerializeField]
    PlayerController player;
    [SerializeField]
    Tilemap tilemap;
    [Space]
    [Header("Enemy Prefabs")]
    [SerializeField]
    GameObject rookPrefab;
    [Space]
    [Header("Environment Objects")]
    [SerializeField]
    GameObject pillarPrefab;

    [SerializeField]
    LevelData currentLevelData;
    List<EnemyController> enemies;
    int enemyIndex;

    public event UnityAction<LevelData> OnInitializeLevel;

    private void Start()
    {
        enemies = new List<EnemyController>();
        StartLevel(currentLevelDataObject);
    }

    private void StartLevel(LevelDataObject levelDataObject)
    {
        StartLevel(levelDataObject.Data.Copy());
    }

    private void StartLevel(LevelData levelData)
    {
        currentLevelData = levelData;

        OnInitializeLevel?.Invoke(currentLevelData);

        player.transform.position = ConvertToWorldPos(currentLevelData.playerPosition);
        player.SetFacing(currentLevelData.playerDirectionFacing);

        GameObject instantiated;
        GameObject prefab;
        EnemyController enemyController;
        foreach(LevelData.EnemyData enemyData in currentLevelData.enemies)
        {
            switch (enemyData.type)
            {
                case EnemyType.ROOK:
                    prefab = rookPrefab;
                    break;
                default:
                    prefab = rookPrefab;
                    break;
            }

            instantiated = GameObject.Instantiate(prefab, ConvertToWorldPos(enemyData.position), Quaternion.identity);
            enemyController = instantiated.GetComponent<EnemyController>();
            enemyController.Initialize(tilemap, enemies.Count);
            enemyController.SetFacing(enemyData.directionFacing);
            enemies.Add(enemyController);
        }

        foreach (Vector2Int pillar in currentLevelData.pillars)
        {
            GameObject.Instantiate(pillarPrefab, ConvertToWorldPos(pillar) + (Vector3.one - tilemap.tileAnchor), Quaternion.identity);
        }

        PerformPlayerTurn();
    }

    private void PerformPlayerTurn()
    {
        player.BeginTurn(currentLevelData);
        player.OnStealthKillEnemy += RespondToPlayerStealthKillEnemy;
        player.OnTurnComplete += RespondToPlayerTurnComplete;
    }

    private void RespondToPlayerStealthKillEnemy(int arrayIndex)
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].GetIndex() == arrayIndex)
            {
                currentLevelData.enemies[arrayIndex].position = -Vector2Int.one;
                enemies[i].Die();
                enemies.RemoveAt(i);
                break;
            }
        }
    }

    private void RespondToPlayerTurnComplete(Vector2Int position, DirectionFacing facing)
    {
        currentLevelData.playerPosition = position;
        currentLevelData.playerDirectionFacing = facing;
        player.OnTurnComplete -= RespondToPlayerTurnComplete;
        PerformEnemyTurns();
    }

    private void PerformEnemyTurns()
    {
        enemyIndex = 0;
        CheckForEnemyTurn();
    }

    private void CheckForEnemyTurn()
    {
        if(enemyIndex < enemies.Count)
        {
            enemies[enemyIndex].OnTurnComplete += RespondToEnemyTurnComplete;
            enemies[enemyIndex].BeginTurn(currentLevelData);
        }
        else if(enemyIndex == 0)
        {
            //Enemies all slain! Next Level!
        }
        else
        {
            PerformPlayerTurn();
        }
    }

    private void RespondToEnemyTurnComplete(Vector2Int position, DirectionFacing facing)
    {
        Debug.Log("RespondToEnemyTurnComplete");
        enemies[enemyIndex].OnTurnComplete -= RespondToEnemyTurnComplete;
        int arrayIndex = enemies[enemyIndex].GetIndex();
        currentLevelData.enemies[arrayIndex].position = position;
        currentLevelData.enemies[arrayIndex].directionFacing = facing;

        if(position.x < 0 && position.y < 0) //Enemy slain
        {
            enemies.RemoveAt(enemyIndex);
        }
        else
        {
            enemyIndex++;
        }

        CheckForEnemyTurn();
    }

    private Vector3 ConvertToWorldPos(Vector2Int cellPosition)
    {
        Vector3 worldPosition = tilemap.transform.position + tilemap.tileAnchor + new Vector3(cellPosition.x * tilemap.cellSize.x, cellPosition.y * tilemap.cellSize.y, 0);
        return worldPosition;
    }
}
