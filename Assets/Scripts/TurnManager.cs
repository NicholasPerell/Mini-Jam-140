using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class TurnManager : MonoBehaviour
{
    [SerializeField]
    LevelDataObject[] levels;
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
    [Space]
    [Header("UI Elements")]
    [SerializeField]
    RectTransform blackScreen;
    [SerializeField]
    [Min(0)]
    float swipeScreenTime, coveringScreenTime;
    [SerializeField]
    GameObject gameOverPanel;
    
    [SerializeField]
    LevelData currentLevelData;
    int levelIndex;
    List<EnemyController> enemies;
    int enemyIndex;
    List<PillarController> pillars;

    public event UnityAction<LevelData> OnInitializeLevel;

#if UNITY_EDITOR
    public void SetLevel(LevelDataObject toTest)
    {
        levels = new LevelDataObject[] { toTest };
    }
#endif

    private void Start()
    {
        enemies = new List<EnemyController>();
        pillars = new List<PillarController>();
        blackScreen.gameObject.SetActive(true);
        levelIndex = 0;
        StartLevel(levels[levelIndex]);
    }

    public void RestartLevel()
    {
        StartLevel(levels[levelIndex]);
    }

    private void CheckForNextLevel()
    {
        levelIndex++;
        if (levelIndex < levels.Length)
        {
            StartLevel(levels[levelIndex]);
        }
        else
        {
            HandleWin();
        }
    }

    private void StartLevel(LevelDataObject levelDataObject)
    {
        StartLevel(levelDataObject.Data.Copy());
    }

    private void StartLevel(LevelData levelData)
    {
        for(int i = 0; i < enemies.Count;i++)
        {
            Destroy(enemies[i].gameObject);
        }
        enemies.Clear();
        for (int i = 0; i < pillars.Count; i++)
        {
            Destroy(pillars[i].gameObject);
        }
        pillars.Clear();

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
            pillars.Add(GameObject.Instantiate(pillarPrefab, ConvertToWorldPos(pillar) + (Vector3.one - tilemap.tileAnchor), Quaternion.identity).GetComponent<PillarController>());
        }

        UncoverScreen(PerformPlayerTurn);
    }

    private void PerformPlayerTurn()
    {
        currentLevelData.coinPath = null;
        player.BeginTurn(currentLevelData);
        player.OnCoinsUpdated += Player_OnCoinsUpdated;
        player.OnScarfUpdated += Player_OnScarfUpdated;
        player.OnStealthKillEnemy += RespondToPlayerStealthKillEnemy;
        player.OnTurnComplete += RespondToPlayerTurnComplete;
    }

    private void Player_OnScarfUpdated(List<int> arg0)
    {
        currentLevelData.pillarsWrapped = arg0;
        for(int i = 0; i < currentLevelData.pillars.Length; i++)
        {
            pillars[i].SetWrapped(currentLevelData.pillarsWrapped.Contains(i));
        }
    }

    private void Player_OnCoinsUpdated(Vector2Int[] arg0)
    {
        currentLevelData.coinPath = arg0;
        currentLevelData.coins--;
    }

    private void RespondToPlayerStealthKillEnemy(int arrayIndex, UnityAction callback)
    {
        int i = 0;
        for(i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].GetIndex() == arrayIndex)
            {
                currentLevelData.enemies[arrayIndex].position = -Vector2Int.one;
                enemies[i].OnDeathComplete += () =>
                {
                    callback();
                };
                enemies[i].RequestDie();
                enemies.RemoveAt(i);
                break;
            }
        }
    }

    private void RespondToPlayerTurnComplete(Vector2Int position, DirectionFacing facing)
    {
        currentLevelData.playerPosition = position;
        currentLevelData.playerDirectionFacing = facing;
        player.OnCoinsUpdated -= Player_OnCoinsUpdated;
        player.OnScarfUpdated -= Player_OnScarfUpdated;
        player.OnStealthKillEnemy -= RespondToPlayerStealthKillEnemy;
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
            enemies[enemyIndex].OnAttackPlayer += RespondToEnemyAttackPlayer;
            enemies[enemyIndex].OnTurnComplete += RespondToEnemyTurnComplete;
            enemies[enemyIndex].BeginTurn(currentLevelData);
        }
        else if(enemyIndex == 0)
        {
            //Enemies all slain! Next Level!
            CoverScreen(CheckForNextLevel);
        }
        else
        {
            PerformPlayerTurn();
        }
    }

    private void RespondToEnemyTurnResultDeath()
    {
        throw new NotImplementedException();
    }

    private void RespondToEnemyAttackPlayer()
    {
        player.OnDeathComplete += RespondToPlayerDeathComplete;
        player.RequestDie();
    }

    private void RespondToPlayerDeathComplete()
    {
        Debug.Log("RespondToPlayerDeathComplete");
        player.OnDeathComplete -= RespondToPlayerDeathComplete;
        CoverScreen(() => { gameOverPanel.SetActive(true); });
    }

    private void RespondToEnemyTurnComplete(Vector2Int position, DirectionFacing facing)
    {
        Debug.Log("RespondToEnemyTurnComplete");
        enemies[enemyIndex].OnAttackPlayer -= RespondToEnemyAttackPlayer;
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

    private void HandleWin()
    {
        Debug.Log("WIN");
    }

    private Vector3 ConvertToWorldPos(Vector2Int cellPosition)
    {
        Vector3 worldPosition = tilemap.transform.position + tilemap.tileAnchor + new Vector3(cellPosition.x * tilemap.cellSize.x, cellPosition.y * tilemap.cellSize.y, 0);
        return worldPosition;
    }

    private void UncoverScreen(TweenCallback callback = null)
    {
        blackScreen.pivot = new Vector2(1, 0.5f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(blackScreen.DOScaleX(0, swipeScreenTime));
        sequence.AppendCallback(callback);
    }

    private void CoverScreen(TweenCallback callback = null)
    {
        blackScreen.pivot = new Vector2(0, 0.5f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(blackScreen.DOScaleX(1, swipeScreenTime));
        sequence.AppendCallback(callback);
    }
}
