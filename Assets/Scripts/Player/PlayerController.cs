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
    [SerializeField]
    GameObject deathIndicatorChild;

    int coinCount = 0;
    bool coinTossSelection = false;

    Vector2Int finishingPosition;
    DirectionFacing finishingFacing;
    bool isTakingInput = false;

    public event UnityAction<int, UnityAction> OnStealthKillEnemy;

    public override void BeginTurn(LevelData levelData)
    {
        base.BeginTurn(levelData);
        currentLevelData = levelData;
        coinCount = currentLevelData.coins;
        isTakingInput = true;

        Debug.Log("Player Begin Turn");

    }

    void Update()
    {
        if (IsEntityTurn && isTakingInput)
        {
            if (coinCount > 0)
            {
                if (Input.GetKeyDown(KeyCode.E))
                { 
                    CoinMarker.CreateGrid(currentLevelData);
                }
            }

            if(!coinTossSelection)
            {
                CheckForMovementInput();
            }
        }
    }

    void CheckForMovementInput()
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
        AudioSystem.Instance?.RequestSound("ScarfWhipAttack01");
        OnStealthKillEnemy.Invoke(enemyArrayIndex, RespondToPieceMoved);
        //MovePlayerTo(player);
    }

    void MovePlayerTo(Vector2Int cellPosition)
    {
        finishingPosition = cellPosition;
        Vector3 worldPosition = tilemap.transform.position + tilemap.tileAnchor + new Vector3(cellPosition.x * tilemap.cellSize.x, cellPosition.y * tilemap.cellSize.y, 0);
        Debug.Log("MovePlayerTo: " + cellPosition + " -> " + worldPosition);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(worldPosition, playerMoveDuration));
        mySequence.Insert(0,visuals.transform.DOLocalJump(Vector3.zero, playerJumpPower, 1, playerMoveDuration));
        mySequence.AppendCallback(RespondToPieceMoved);
    }

    private void RespondToPieceMoved()
    {
        DeclareTurnOver(finishingPosition, finishingFacing);
    }

    public override void RequestDie()
    {
        StartCoroutine(Coroutine());
        IEnumerator Coroutine()
        {
            deathIndicatorChild.SetActive(true);
            Animator anim = deathIndicatorChild.GetComponent<Animator>();
            AudioSystem.Instance?.RequestSound("EnemyAttack01");
            yield return new WaitForSeconds(.05f);
            anim.Play("slashPlaying");
            yield return new WaitForSeconds(.25f);
            AudioSystem.Instance?.RequestSound("Death01");
            //TODO: Trigger the hurt/slain animation once it exists
            yield return new WaitForSeconds(.5f);
            deathIndicatorChild.SetActive(false);
            DeclareDeathComplete();
        }
    }
}
