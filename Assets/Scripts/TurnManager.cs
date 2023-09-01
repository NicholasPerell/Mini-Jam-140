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

    LevelData currentLevelData;
    List<EnemyController> enemies;

    public event UnityAction<LevelData> OnInitializeLevel;

    private void Start()
    {
        enemies = new List<EnemyController>();
        StartLevel(currentLevelDataObject);
    }

    private void StartLevel(LevelDataObject levelDataObject)
    {
        StartLevel(levelDataObject.Data);
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
            enemyController.SetFacing(enemyData.directionFacing);
            enemies.Add(enemyController);
        }

        foreach (Vector2Int pillar in currentLevelData.pillars)
        {
            GameObject.Instantiate(pillarPrefab, ConvertToWorldPos(pillar) + (Vector3.one - tilemap.tileAnchor), Quaternion.identity);
        }
    }

    private Vector3 ConvertToWorldPos(Vector2Int cellPosition)
    {
        Vector3 worldPosition = tilemap.transform.position + tilemap.tileAnchor + new Vector3(cellPosition.x * tilemap.cellSize.x, cellPosition.y * tilemap.cellSize.y, 0);
        return worldPosition;
    }
}
