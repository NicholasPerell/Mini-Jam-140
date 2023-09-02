using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using System.Linq;
using System;
using System.Text;

public abstract class EnemyController : TurnEntityController
{
    private int index;
    private Vector2Int runningTowards = -Vector2Int.one;
    private bool moving => runningTowards != -Vector2Int.one;
    bool seenPlayer = false;
    [SerializeField]
    AlertActions chargeIcon;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("How many units moved per second")]
    protected float movementSpeed = 1;

    public event UnityAction OnAttackPlayer;

    public void Initialize(Tilemap _tilemap, int _index)
    {
        tilemap = _tilemap;
        index = _index;
    }

    public int GetIndex()
    {
        return index;
    }

    public override void BeginTurn(LevelData levelData)
    {
        base.BeginTurn(levelData);

        Debug.Log("Enemy Begin Turn");

        LevelData.EnemyData enemyData = levelData.enemies[index];
        if (CheckTargetInAttack(enemyData.position, enemyData.directionFacing, levelData.playerPosition, levelData.walls)) //Attacking Player
        {
            Debug.Log("CheckTargetInAttackTrue");
            chargeIcon.EnableAlertIcon(!seenPlayer);
            seenPlayer = true;
            AttackPlayer(enemyData.position, enemyData.directionFacing, levelData.playerPosition, levelData.walls);
        }
        else if (seenPlayer) //Chasing Player
        {
            runningTowards = levelData.playerPosition;
            MoveTowardPosition(enemyData.position, enemyData.directionFacing, runningTowards, levelData.walls);
        }
        else if (CheckTargetInPeripheral(enemyData.position, enemyData.directionFacing, levelData.playerPosition, levelData.walls)) //Player Spotted
        {
            runningTowards = levelData.playerPosition;
            seenPlayer = true;
            chargeIcon.EnableAlertIcon(seenPlayer);
            ReactTowardPosition(enemyData.position, enemyData.directionFacing, runningTowards, levelData.walls);
        }
        else if(moving) //Moving towards thrown coin
        {
            MoveTowardPosition(enemyData.position, enemyData.directionFacing, runningTowards, levelData.walls);
        }
        else if (CheckTargetsInAttack(enemyData.position, enemyData.directionFacing, levelData.coinPath, levelData.walls)
                || CheckTargetsInPeripheral(enemyData.position, enemyData.directionFacing, levelData.coinPath, levelData.walls)) //Coin Path Intersects With Vision Line
        {
            runningTowards = levelData.coinPath[0];
            chargeIcon.EnableQuestionIcon(true);
            ReactTowardPosition(enemyData.position, enemyData.directionFacing, runningTowards, levelData.walls);
        }
        else if (IsFacingWall(enemyData.position, enemyData.directionFacing, levelData.walls)) //Facing Wall
        {
            TurnAround(enemyData.position, enemyData.directionFacing);
        }
        else //Nuthin'
        {
            DeclareTurnOver(enemyData.position, enemyData.directionFacing);
        }
    }

    private bool IsFacingWall(Vector2Int position, DirectionFacing directionFacing, Vector2Int[] walls)
    {
        Vector2Int checkPosition = position + TurnFacingToVector(directionFacing);
        foreach(Vector2Int wall in walls)
        {
            if(wall == checkPosition)
            {
                return true;
            }
        }
        return false;
    }

    public Vector2Int[] GetVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls)
    {
        Vector2Int[] attack = GetAttackVisionTiles(enemy, facing, walls);
        Vector2Int[] peripheral = GetPeripheralVisionTiles(enemy, facing, walls);
        return attack.Union(peripheral).ToArray();
    }

    public delegate Vector2Int[] GatherTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls);
    public bool CheckTargetInAttack(Vector2Int enemy, DirectionFacing facing, Vector2Int target, Vector2Int[] walls) => CheckTargetInTiles(enemy, facing, target, walls, GetAttackVisionTiles);
    public bool CheckTargetInPeripheral(Vector2Int enemy, DirectionFacing facing, Vector2Int target, Vector2Int[] walls) => CheckTargetInTiles(enemy, facing, target, walls, GetPeripheralVisionTiles);
    private bool CheckTargetInTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int target, Vector2Int[] walls, GatherTiles gather)
    {
        Vector2Int[] tiles = gather(enemy, facing, walls);
        Array.Sort(tiles, new PositionComparer());
        return Array.BinarySearch(tiles, target, new PositionComparer()) > -1;
    }

    public bool CheckTargetsInAttack(Vector2Int enemy, DirectionFacing facing, Vector2Int[] targets, Vector2Int[] walls) => CheckTargetsInTiles(enemy, facing, targets, walls, GetAttackVisionTiles);
    public bool CheckTargetsInPeripheral(Vector2Int enemy, DirectionFacing facing, Vector2Int[] targets, Vector2Int[] walls) => CheckTargetsInTiles(enemy, facing, targets, walls, GetPeripheralVisionTiles);
    public bool CheckTargetsInTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] targets, Vector2Int[] walls, GatherTiles gather)
    {
        Vector2Int[] tiles = GetAttackVisionTiles(enemy, facing, walls);
        Array.Sort(tiles, new PositionComparer());
        foreach (Vector2Int target in targets)
        {
            if (Array.BinarySearch(tiles, target, new PositionComparer()) > -1)
            {
                return true;
            }
        }
        return false;
    }

    private void TurnAround(Vector2Int enemy, DirectionFacing facing)
    {
        facing = (DirectionFacing)(((int)facing + 2) % 4);
        SetFacing(facing);
        DeclareTurnOver(enemy, facing);
    }

    protected void DeclarePlayerSlain()
    {
        OnAttackPlayer?.Invoke();
    }

    public override void Die()
    {
        Debug.Log(gameObject.name + " Dying");
        Destroy(gameObject);
    }

    protected abstract Vector2Int[] GetAttackVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls);

    protected abstract Vector2Int[] GetPeripheralVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls);

    protected abstract void AttackPlayer(Vector2Int enemy, DirectionFacing facing, Vector2Int player, Vector2Int[] walls);
    
    protected abstract void ReactTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls);
   
    protected abstract void MoveTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls);

}
