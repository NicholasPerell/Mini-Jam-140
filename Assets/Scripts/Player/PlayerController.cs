using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using DG.Tweening;

public class PlayerController : TurnEntityController
{
    [SerializeField]
    [Min(float.Epsilon)]
    float playerMoveDuration = .5f;
    [SerializeField]
    float playerJumpPower = .5f;
    [SerializeField] 
    CoinGrid CoinMarker;

    public bool whatStopsMovement = false;
    public float moveSpeed = 2f;
    public Transform movePoint;
    public GameObject objectToSpawnvert;
    public GameObject objectToSpawnhorz;
    public GameObject objectToMark;
    public int coinCount = 0;
    public int x = 0;
    public int y = 0;

    Vector2Int finishingPosition;
    DirectionFacing finishingFacing;
    [SerializeField]
    bool isTakingInput = false;

    public event UnityAction<int> OnStealthKillEnemy;

    public override void BeginTurn(LevelData levelData)
    {
        base.BeginTurn(levelData);
        currentLevelData = levelData;
        coinCount = currentLevelData.coinsAtStart;
        isTakingInput = true;

        Debug.Log("Player Begin Turn");

    }

    /*void Start()
    {
        movePoint.parent = null;
        x = 0;
        y = 0;
        Array.Sort(currentLevelData.walls);
        int searchIndex = Array.BinarySearch(currentLevelData.walls, movePoint.position);
        if (searchIndex > -1)
        {
            whatStopsMovement = true;
        }
    }*/

    void Update()
    {
        if (IsEntityTurn && isTakingInput)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                AttemptMoveOne(currentLevelData.playerPosition, DirectionFacing.UP, currentLevelData.enemies, currentLevelData.walls);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                AttemptMoveOne(currentLevelData.playerPosition, DirectionFacing.DOWN, currentLevelData.enemies, currentLevelData.walls);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                AttemptMoveOne(currentLevelData.playerPosition, DirectionFacing.RIGHT, currentLevelData.enemies, currentLevelData.walls);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                AttemptMoveOne(currentLevelData.playerPosition, DirectionFacing.LEFT, currentLevelData.enemies, currentLevelData.walls);
            }

            /*if (whatStopsMovement == false && false)
            {
                transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, movePoint.position) <= .05)
                {

                    if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
                    {
                        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), .35f))
                        {
                            if (Input.GetKey(KeyCode.D))
                            {
                                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                                x = 1;
                                y = 0;
                            }
                            if (Input.GetKey(KeyCode.A))
                            {
                                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                                x = -1;
                                y = 0;
                            }
                        }
                    }
                    if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
                    {
                        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), .35f))
                        {

                            if (Input.GetKey(KeyCode.W))
                            {
                                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                                y = 1;
                                x = 0;
                            }
                            if (Input.GetKey(KeyCode.S))
                            {
                                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                                y = -1;
                                x = 0;
                            }
                        }
                    }
                }
            }*/

            if (coinCount > 0)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (x == 1)
                    {
                        CoinMarker.CreateGrid(currentLevelData); 
                    }
                    else if (x == -1)
                    {
                        CoinMarker.CreateGrid(currentLevelData);
                    }
                    else if (y == 1)
                    {
                        CoinMarker.CreateGrid(currentLevelData);
                    }
                    else if (y == -1)
                    {
                        CoinMarker.CreateGrid(currentLevelData);
                    }
                }
            }
        }
    }

    bool AttemptMoveOne(Vector2Int player, DirectionFacing facing, LevelData.EnemyData[] enemies, Vector2Int[] walls)
    {
        bool success = true;
        int behindEnemy = -1;

        Array.Sort(walls, new PositionComparer());
        Debug.Log(player);
        Vector2Int requested = player + TurnFacingToVector(facing);
        Debug.Log(requested);
        SetFacing(facing);
        finishingFacing = facing;

        success = Array.BinarySearch(walls, requested, new PositionComparer()) < 0;

        Debug.Log(!success ? "Wall" : "Floor");

        for(int i = 0; success && i < enemies.Length; i++)
        {
            if(enemies[i].position == requested)
            {
                Debug.Log("Enemy in the way");
                success = false;
            }
            if(!success && facing == enemies[i].directionFacing)
            {
                Debug.Log("Enemy facing away");
                behindEnemy = i;
            }
        }

        if(success)
        {
            isTakingInput = false;
            MovePlayerTo(requested);
        }
        else if(behindEnemy > -1)
        {
            isTakingInput = false;
            StealthKill(player, facing, behindEnemy);
        }

        return success;
    }

    void StealthKill(Vector2Int player, DirectionFacing facing, int enemyArrayIndex)
    {
        //TODO set up kill/push animation!

        OnStealthKillEnemy.Invoke(enemyArrayIndex);
        MovePlayerTo(player);
    }

    void MovePlayerTo(Vector2Int cellPosition)
    {
        Debug.Log("MovePlayeTo: " + cellPosition);

        finishingPosition = cellPosition;
        Vector3 worldPosition = tilemap.transform.position + tilemap.tileAnchor + new Vector3(cellPosition.x * tilemap.cellSize.x, cellPosition.y * tilemap.cellSize.y, 0);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(worldPosition, playerMoveDuration));
        //mySequence.Insert(0,visuals.transform.DOLocalJump(Vector3.zero, playerJumpPower, 1, playerMoveDuration));
        mySequence.AppendCallback(RespondToPieceMoved);
    }

    private void RespondToPieceMoved()
    {
        DeclareTurnOver(finishingPosition, finishingFacing);
    }
}
