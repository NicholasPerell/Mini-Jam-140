using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using DG.Tweening;

public class PlayerController : TurnEntityController
{
    public enum PlayerInputState
    {
        MOVEMENT,
        COIN,
        SCARF
    }

    [SerializeField]
    [Min(float.Epsilon)]
    float playerMoveDuration = .5f;
    [SerializeField]
    float playerJumpPower = .5f;
    [SerializeField]
    GameObject deathIndicatorChild;
    [SerializeField]
    GameObject coinChild;
    [SerializeField]
    [Min(1)]
    int distanceCanThrowCoin = 3;
    int coinCount = 0;

    bool isTakingInput;
    [SerializeField]
    PlayerInputState inputState = PlayerInputState.MOVEMENT;

    Vector2Int finishingPosition;
    DirectionFacing finishingFacing;
    
    public event UnityAction<int, UnityAction> OnStealthKillEnemy;
    public event UnityAction<Vector2Int[], UnityAction<Vector2Int>> OnCoinsConsidered;
    public event UnityAction<Vector2Int[]> OnCoinsUpdated;
    public event UnityAction OnCoinsDismissed;
    public event UnityAction<Vector2Int[], UnityAction<Vector2Int>> OnScarfConsidered;
    public event UnityAction<List<int>> OnScarfUpdated;
    public event UnityAction OnScarfDismissed;

    public override void BeginTurn(LevelData levelData)
    {
        base.BeginTurn(levelData);
        currentLevelData = levelData;
        coinCount = currentLevelData.coins;
        isTakingInput = true;
        inputState = PlayerInputState.MOVEMENT;
        Array.Sort(currentLevelData.walls, new PositionComparer());
        coinChild.SetActive(false);
        coinChild.transform.localPosition = Vector3.zero;

        Debug.Log("Player Begin Turn");
    }

    void Update()
    {
        CheckForInput();
    }

    void CheckForInput()
    {
        if (IsEntityTurn && isTakingInput)
        {
            if (coinCount > 0)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    CheckForNonMovementInput(PlayerInputState.COIN);
                }
            }

            if(Input.GetKeyDown(KeyCode.Q))
            {
                CheckForNonMovementInput(PlayerInputState.SCARF);
            }

            if (inputState == PlayerInputState.MOVEMENT)
            {
                CheckForMovementInput();
            }
        }
    }

    void CheckForMovementInput()
    {
        DirectionFacing inputFacing = DirectionFacing.RIGHT;
        bool GetKeyDown = false;
        if (Input.GetKeyDown(KeyCode.W))
        {
            inputFacing = DirectionFacing.UP;
            GetKeyDown = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            inputFacing = DirectionFacing.DOWN;
            GetKeyDown = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            inputFacing = DirectionFacing.RIGHT;
            GetKeyDown = true;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            inputFacing = DirectionFacing.LEFT;
            GetKeyDown = true;
        }

        if (GetKeyDown)
        {
            if (inputFacing == currentLevelData.playerDirectionFacing)
            {
                AttemptMoveOne(currentLevelData.playerPosition, inputFacing, currentLevelData.enemies, currentLevelData.walls);
            }
            else
            {
                SetFacing(inputFacing);
                currentLevelData.playerDirectionFacing = inputFacing;
            }
        }
    }

    void CheckForNonMovementInput(PlayerInputState desiredInputState)
    {
        PlayerInputState previousInputState = inputState;
        bool toggleOn = desiredInputState != inputState;
        if (toggleOn)
        {
            inputState = desiredInputState;
        }
        else
        {
            inputState = PlayerInputState.MOVEMENT;
        }

        switch (previousInputState)
        {
            case PlayerInputState.COIN:
                OnCoinsDismissed?.Invoke();
                break;
            case PlayerInputState.SCARF:
                OnScarfDismissed?.Invoke();
                break;
        }

        switch (inputState)
        {
            case PlayerInputState.COIN:
                OnCoinsConsidered?.Invoke(FindCoinTossableSpaces(), ThrowCoin);
                break;
            case PlayerInputState.SCARF:
                OnScarfConsidered?.Invoke(FindInReachPillars(), TieScarf);
                break;
        }
    }

    void ThrowCoin(Vector2Int position)
    {
        List<Vector2Int> coinPath = new List<Vector2Int>();

        Vector2Int backwards = TurnBehindToVector(currentLevelData.playerDirectionFacing);

        for(Vector2Int tile = position; tile != currentLevelData.playerPosition; tile += backwards)
        {
            coinPath.Add(tile);
        }

        OnCoinsUpdated?.Invoke(coinPath.ToArray());
        OnCoinsDismissed?.Invoke();
        DeclareTurnOver(currentLevelData.playerPosition, currentLevelData.playerDirectionFacing);
    }

    void TieScarf(Vector2Int position)
    {
        List<int> pillarsWrapped = currentLevelData.pillarsWrapped;
        int choosenPillarIndex;
        for(choosenPillarIndex = 0; choosenPillarIndex < currentLevelData.pillars.Length; choosenPillarIndex++)
        {
            if(currentLevelData.pillars[choosenPillarIndex] == position)
            {
                break;
            }
        }

        int indexInWrapped = pillarsWrapped.LastIndexOf(choosenPillarIndex);
        bool alreadyWrapped = indexInWrapped > -1;

        if(!alreadyWrapped)
        {
            pillarsWrapped.Add(choosenPillarIndex);
        }
        else if(indexInWrapped == pillarsWrapped.Count -1)
        {
            pillarsWrapped.RemoveAt(indexInWrapped);
        }

        OnScarfUpdated?.Invoke(pillarsWrapped);
        OnScarfDismissed?.Invoke();
        DeclareTurnOver(currentLevelData.playerPosition, currentLevelData.playerDirectionFacing);
    }

    Vector2Int[] FindCoinTossableSpaces()
    {
        PositionComparer positionComparer = new PositionComparer();
        List<Vector2Int> tossablePositions = new List<Vector2Int>();
        Vector2Int playerPosition = currentLevelData.playerPosition;
        Vector2Int towards = TurnFacingToVector(currentLevelData.playerDirectionFacing);

        Vector2Int[] enemyPositions = new Vector2Int[currentLevelData.enemies.Length];
        for(int i = 0; i < enemyPositions.Length; i++)
        {
            enemyPositions[i] = currentLevelData.enemies[i].position;
        }
        Array.Sort(enemyPositions, positionComparer);

        Vector2Int pathTo = playerPosition;
        for(int i = 0; i < distanceCanThrowCoin; i++)
        {
            pathTo += towards;
            if(Array.BinarySearch(currentLevelData.walls, pathTo, positionComparer) > -1)
            {
                break;
            }
            else if (Array.BinarySearch(enemyPositions, pathTo, positionComparer) < 0)
            {
                tossablePositions.Add(pathTo);
            }
        }

        tossablePositions.Sort(positionComparer);
        return tossablePositions.ToArray();
    }

    Vector2Int[] FindInReachPillars()
    {
        List<Vector2Int> localPillars = new List<Vector2Int>();
        Vector2Int playerPosition = currentLevelData.playerPosition;
        Vector2Int[] toCheck = new Vector2Int[] { playerPosition, playerPosition + Vector2Int.left,
                                                    playerPosition + Vector2Int.down, playerPosition - Vector2Int.one};
        for (int i = 0; i < currentLevelData.pillars.Length; i++)
        {
            for (int j = 0; j < toCheck.Length; j++)
            {
                if(currentLevelData.pillars[i] == toCheck[j])
                {
                    localPillars.Add(toCheck[j]);
                }
            }
        }

        return localPillars.ToArray();
    }

    bool AttemptMoveOne(Vector2Int player, DirectionFacing facing, LevelData.EnemyData[] enemies, Vector2Int[] walls)
    {
        bool success = true;
        int behindEnemy = -1;

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
