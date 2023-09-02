using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class RookController : EnemyController
{
    Vector2Int finishingPosition;
    DirectionFacing finishingFacing;
    bool playerFound;
    protected override Vector2Int[] GetAttackVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        Array.Sort(walls, new PositionComparer());

        Vector2Int path = enemy;
        Vector2Int forward = TurnFacingToVector(facing);
        for(int i = 0; i < 30; i++)
        {
            path += forward;
            if(Array.BinarySearch(walls,path, new PositionComparer()) > -1)
            {
                break;
            }
            tiles.Add(path);
        }

        return tiles.ToArray();
    }

    protected override Vector2Int[] GetPeripheralVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls)
    {
        Vector2Int forward = TurnFacingToVector(facing);
        Vector2Int right = TurnRightToVector(facing);
        Vector2Int[] possible = new Vector2Int[] { enemy + right, enemy - right,
            enemy + forward + right, enemy + forward - right };
        List<Vector2Int> tiles = new List<Vector2Int>(possible);
        tiles.Sort(new PositionComparer());

        int searchIndex;
        foreach (Vector2Int wall in walls)
        {
            searchIndex = tiles.BinarySearch(wall, new PositionComparer());
            if (searchIndex > -1)
            {
                tiles.RemoveAt(searchIndex);
            }
        }

        return tiles.ToArray();
    }

    protected override void ReactTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls)
    {
        Vector2Int diff = position - enemy;
        bool moveDistance = Vector2Int.FloorToInt(((Vector2)diff).normalized) == TurnFacingToVector(facing);
        if(moveDistance)
        {
            MoveTowardPosition(enemy, facing, position, walls);
        }
        else
        {
            int offset = Vector2.Dot(diff, TurnLeftToVector(facing)) > 0 ? 1 : 3;
            facing = (DirectionFacing)(((int)facing + offset) % 4);
            SetFacing(facing);
            DeclareTurnOver(enemy, facing);
        }
    }
    protected override void MoveTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls)
    {
        Vector2Int path = enemy;
        Vector2Int diff = position - enemy;

        bool facingCorrectly = Vector2.Dot(diff,TurnFacingToVector(facing)) > 0;
        bool facingAdjacent = Vector2.Dot(diff,TurnFacingToVector(facing)) == 0;
        bool facingLeftward = Vector2.Dot(diff,TurnLeftToVector(facing)) > 0;
        bool alignedHorizontally = enemy.x == position.x;
        bool alignedVertically = enemy.y == position.y;

        if(!facingCorrectly)
        {
            if(facingAdjacent)
            {
                if(facingLeftward)
                {
                    facing = (DirectionFacing)(((int)facing + 1) % 4);
                }
                else
                {
                    facing = (DirectionFacing)(((int)facing + 3) % 4);
                }
            }
            else
            {
                    facing = (DirectionFacing)(((int)facing + 2) % 4);
            }
        }
        Vector2Int towards = TurnFacingToVector(facing);

        playerFound = false;

        for (int i = 0; i < 30; i++)
        {
            playerFound = path + towards == currentLevelData.playerPosition;
            if (playerFound
                || Array.BinarySearch(walls, path + towards, new PositionComparer()) > -1
                || (path.x == position.x && !alignedHorizontally) 
                || (path.y == position.y && !alignedVertically))
            {
                break;
            }
            path += towards;
        }

        SetFacing(facing);

        //TODO: Check for scarf in the way

        finishingPosition = path;
        finishingFacing = facing;
        walkingParticles.Play();
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(ConvertToWorldPos(path),  (path - enemy).magnitude / movementSpeed));
        mySequence.AppendCallback(RespondToPieceMoved);
    }

    protected override void AttackPlayer(Vector2Int enemy, DirectionFacing facing, Vector2Int player, Vector2Int[] walls)
    {
        MoveTowardPosition(enemy, facing, player, walls);
    }

    private void RespondToPieceMoved()
    {
        walkingParticles.Stop();

        if (playerFound)
        {
            DeclarePlayerSlain();
        }
        else
        {
            DeclareTurnOver(finishingPosition, finishingFacing);
        }
    }
}
