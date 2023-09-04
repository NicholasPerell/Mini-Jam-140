using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using System.Linq;
using System;
using System.Text;
using System.Collections;
using DG.Tweening;

public abstract class EnemyController : TurnEntityController
{
    //For you, Thomas!
    private float angleToTripBy = Mathf.Cos(15f);

    private int index;
    [SerializeField]
    private Vector2Int runningTowards = -Vector2Int.one;
    private bool moving => runningTowards != -Vector2Int.one;
    [SerializeField]
    bool seenPlayer = false;
    [SerializeField]
    AlertActions chargeIcon;
    [SerializeField]
    protected ParticleSystem walkingParticles;
    [SerializeField]
    GameObject shatterPrefab;

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

        Array.Sort(levelData.walls, new PositionComparer());

        Vector2Int[] otherEnemies = new Vector2Int[levelData.enemies.Length - 1];
        int j = 0;
        for (int i = 0; i < otherEnemies.Length; i++)
        {
            if(i == index)
            {
                j++;
            }
            otherEnemies[i] = levelData.enemies[i + j].position;
        }
        Array.Sort(otherEnemies, new PositionComparer());
        
        if(enemyData.position == runningTowards)
        {
            runningTowards = -Vector2Int.one;
        }

        if (CheckTargetInAttack(enemyData.position, enemyData.directionFacing, levelData.playerPosition, levelData.walls, otherEnemies)) //Attacking Player
        {
            Debug.Log("CheckTargetInAttackTrue");
            chargeIcon.EnableAlertIcon(!seenPlayer);
            seenPlayer = true;
            AttackPlayer(enemyData.position, enemyData.directionFacing, levelData.playerPosition, levelData.walls, otherEnemies, levelData.pillars, levelData.pillarsWrapped.ToArray());
        }
        else if (seenPlayer) //Chasing Player
        {
            runningTowards = levelData.playerPosition;
            MoveTowardPosition(enemyData.position, enemyData.directionFacing, runningTowards, levelData.walls, otherEnemies, levelData.pillars, levelData.pillarsWrapped.ToArray(), levelData.playerPosition);
        }
        else if (CheckTargetInPeripheral(enemyData.position, enemyData.directionFacing, levelData.playerPosition, levelData.walls, otherEnemies)) //Player Spotted
        {
            runningTowards = levelData.playerPosition;
            seenPlayer = true;
            chargeIcon.EnableAlertIcon(seenPlayer);
            ReactTowardPosition(enemyData.position, enemyData.directionFacing, runningTowards, levelData.walls, otherEnemies, levelData.pillars, levelData.pillarsWrapped.ToArray(), levelData.playerPosition);
        }
        else if(moving) //Moving towards thrown coin
        {
            MoveTowardPosition(enemyData.position, enemyData.directionFacing, runningTowards, levelData.walls, otherEnemies, levelData.pillars, levelData.pillarsWrapped.ToArray(), levelData.playerPosition);
        }
        else if (CheckTargetsInAttack(enemyData.position, enemyData.directionFacing, levelData.coinPath, levelData.walls, otherEnemies)
                || CheckTargetsInPeripheral(enemyData.position, enemyData.directionFacing, levelData.coinPath, levelData.walls, otherEnemies)) //Coin Path Intersects With Vision Line
        {
            //runningTowards = levelData.coinPath[0];
            chargeIcon.EnableQuestionIcon(true);
            ReactTowardPosition(enemyData.position, enemyData.directionFacing, levelData.coinPath[0], levelData.walls, otherEnemies, levelData.pillars, levelData.pillarsWrapped.ToArray(), levelData.playerPosition);
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

    public Vector2Int[] GetVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls, Vector2Int[] otherEnemies)
    {
        Vector2Int[] attack = GetAttackVisionTiles(enemy, facing, walls, otherEnemies);
        Vector2Int[] peripheral = GetPeripheralVisionTiles(enemy, facing, walls, otherEnemies);
        return attack.Union(peripheral).ToArray();
    }

    public delegate Vector2Int[] GatherTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls, Vector2Int[] otherEnemies);
    public bool CheckTargetInAttack(Vector2Int enemy, DirectionFacing facing, Vector2Int target, Vector2Int[] walls, Vector2Int[] otherEnemies) => CheckTargetInTiles(enemy, facing, target, walls, otherEnemies, GetAttackVisionTiles);
    public bool CheckTargetInPeripheral(Vector2Int enemy, DirectionFacing facing, Vector2Int target, Vector2Int[] walls, Vector2Int[] otherEnemies) => CheckTargetInTiles(enemy, facing, target, walls, otherEnemies, GetPeripheralVisionTiles);
    private bool CheckTargetInTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int target, Vector2Int[] walls, Vector2Int[] otherEnemies, GatherTiles gather)
    {
        Vector2Int[] tiles = gather(enemy, facing, walls, otherEnemies);
        Array.Sort(tiles, new PositionComparer());
        return Array.BinarySearch(tiles, target, new PositionComparer()) > -1;
    }

    public bool CheckTargetsInAttack(Vector2Int enemy, DirectionFacing facing, Vector2Int[] targets, Vector2Int[] walls, Vector2Int[] otherEnemies) => CheckTargetsInTiles(enemy, facing, targets, walls, otherEnemies, GetAttackVisionTiles);
    public bool CheckTargetsInPeripheral(Vector2Int enemy, DirectionFacing facing, Vector2Int[] targets, Vector2Int[] walls, Vector2Int[] otherEnemies) => CheckTargetsInTiles(enemy, facing, targets, walls, otherEnemies, GetPeripheralVisionTiles);
    public bool CheckTargetsInTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] targets, Vector2Int[] walls, Vector2Int[] otherEnemies, GatherTiles gather)
    {
        if(targets == null)
        {
            return false;
        }

        Vector2Int[] tiles = gather(enemy, facing, walls, otherEnemies);
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

    public override void RequestDie()
    {
        StartCoroutine(Coroutine());
        IEnumerator Coroutine()
        {
            float startRotationAmount = 30;
            float fullRotationAmount = 450;
            if(currentLevelData.enemies[index].directionFacing < DirectionFacing.LEFT)
            {
                startRotationAmount *= -1;
                fullRotationAmount *= -1;
            }
            visuals.transform.DOLocalRotate(Vector3.forward * startRotationAmount, .5f, RotateMode.FastBeyond360);
            yield return new WaitForSeconds(.5f);
            visuals.transform.DOLocalJump(Vector3.down * 0.14f, .25f, 1, .6f);
            visuals.transform.DOLocalRotate(Vector3.forward * fullRotationAmount, .6f, RotateMode.FastBeyond360);
            AudioSystem.Instance?.RequestSound("EnemyFall01");
            yield return new WaitForSeconds(.5f);
            GameObject.Instantiate(shatterPrefab, visuals.transform.position, Quaternion.identity);
            AudioSystem.Instance?.RequestSound("EnemyShatter01");
            yield return new WaitForSeconds(.3f);
            DeclareDeathComplete();
            Destroy(gameObject);
        }
    }

    StringBuilder debugMessanger = new StringBuilder();
    protected bool CrossesScarf(Vector2Int positionFrom, Vector2Int positionTo, Vector2Int[] pillars, int[] pillarIndex, Vector2Int player)
    {
        debugMessanger.Clear();
        debugMessanger.Append(gameObject.name).Append(" CrossesScarf\n")
            .Append(positionFrom).Append("->").Append(positionTo).Append("\n");

        bool crosses = false;
        if (pillarIndex.Length > 0)
        {
            for (int i = 0; !crosses && i < pillarIndex.Length - 1; i++)
            {
                if(LineSegmentsTrippable(positionFrom, positionTo, pillars[pillarIndex[i]], pillars[pillarIndex[i + 1]]))
                {
                    crosses = true;
                    break;
                }
            }
            debugMessanger.Append("crosses: ").Append(crosses ? "TRUE" : "FALSE");
            if (!crosses && positionTo != player)
            {
                crosses = LineSegmentsTrippable(positionFrom, positionTo, pillars[pillarIndex[pillarIndex.Length - 1]], player);
            }
        }


        //Debug.Log(debugMessanger.ToString());
        return crosses;
    }

    private bool LineSegmentsTrippable(Vector2Int a1, Vector2Int a2, Vector2Int b1, Vector2Int b2)
    {
        bool trippable = false;

        trippable = LineSegementsIntersect(a1, a2, b1, b2) && Vector2.Dot(((Vector2)a1-a2).normalized, ((Vector2)b1 -b2).normalized) > angleToTripBy;

        return trippable;
    }

    private bool LineSegementsIntersect(Vector2Int a1, Vector2Int a2, Vector2Int b1, Vector2Int b2)
    {
        bool intersects = false;

        bool a1IsFirst = a1.x < a2.x || (a1.x == a2.x && a1.y < a2.y);
        Vector2Int aLeft = a1IsFirst ? a1 : a2;
        Vector2Int aRight = a1IsFirst ? a2 : a1;
        Vector2Int aDelta = aLeft - aRight;
        float aSlope;
        if (aDelta.x != 0)
        {
            aSlope = aDelta.y * 1f / aDelta.x;
        }
        else
        {
            aSlope = float.PositiveInfinity;
        }

        bool b1IsFirst = b1.x < b2.x || (b1.x == b2.x && b1.y < b2.y);
        Vector2Int bLeft = b1IsFirst ? b1 : b2;
        Vector2Int bRight = b1IsFirst ? b2 : b1;
        Vector2Int bDelta = bLeft - bRight;        
        float bSlope;
        if (bDelta.x != 0)
        {
            bSlope = bDelta.y * 1f / bDelta.x;
        }
        else
        {
            bSlope = float.PositiveInfinity;
        }

        if(aSlope == bSlope)
        {
            Vector2Int aLeftClamped = aLeft;
            Vector2Int aRightClamped = aRight;
            aLeftClamped.Clamp(bLeft, bRight);
            aRightClamped.Clamp(bLeft, bRight);
            intersects = aLeftClamped == aLeft || aRightClamped == aRight;
        }
        else
        {
            intersects = IsCounterClockWise(aLeft, bLeft, bRight) != IsCounterClockWise(aRight, bLeft, bRight) && IsCounterClockWise(aLeft, aRight, bLeft) != IsCounterClockWise(aLeft, bLeft, bRight);
        }

        return intersects;
    }

    private bool IsCounterClockWise(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
    }

    protected abstract Vector2Int[] GetAttackVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls, Vector2Int[] otherEnemies);

    protected abstract Vector2Int[] GetPeripheralVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls, Vector2Int[] otherEnemies);

    protected abstract void AttackPlayer(Vector2Int enemy, DirectionFacing facing, Vector2Int player, Vector2Int[] walls, Vector2Int[] otherEnemies, Vector2Int[] pillars, int[] pillarIndex);
    
    protected abstract void ReactTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls, Vector2Int[] otherEnemies, Vector2Int[] pillars, int[] pillarIndex, Vector2Int player);
   
    protected abstract void MoveTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls, Vector2Int[] otherEnemies, Vector2Int[] pillars, int[] pillarIndex, Vector2Int player);

}
