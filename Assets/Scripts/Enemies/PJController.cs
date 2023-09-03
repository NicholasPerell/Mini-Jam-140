using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class PJController : EnemyController
{
    protected override void AttackPlayer(Vector2Int enemy, DirectionFacing facing, Vector2Int player, Vector2Int[] walls, Vector2Int[] otherEnemies, Vector2Int[] pillars, int[] pillarIndex)
    {
        throw new NotImplementedException();
    }

    protected override Vector2Int[] GetAttackVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls, Vector2Int[] otherEnemies)
    {
        return new Vector2Int[0];
    }

    protected override Vector2Int[] GetPeripheralVisionTiles(Vector2Int enemy, DirectionFacing facing, Vector2Int[] walls, Vector2Int[] otherEnemies)
    {
        return new Vector2Int[0];
    }

    protected override void MoveTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls, Vector2Int[] otherEnemies, Vector2Int[] pillars, int[] pillarIndex, Vector2Int player)
    {
        throw new NotImplementedException();
    }

    protected override void ReactTowardPosition(Vector2Int enemy, DirectionFacing facing, Vector2Int position, Vector2Int[] walls, Vector2Int[] otherEnemies, Vector2Int[] pillars, int[] pillarIndex, Vector2Int player)
    {
        throw new NotImplementedException();
    }

    public override void BeginTurn(LevelData levelData)
    {
        Vector2Int difference = levelData.enemies[GetIndex()].position - levelData.playerPosition;
        for (int i = 0; i < 4; i++)
        {
            if (difference == TurnFacingToVector((DirectionFacing)i))
            {
                DeclareTurnOver(levelData.enemies[GetIndex()].position, (DirectionFacing)i);
                return;
            }
        }
        DeclareTurnOver(levelData.enemies[GetIndex()].position, levelData.enemies[GetIndex()].directionFacing);
    }

    public override void RequestDie()
    {
        Sequence sequence = DOTween.Sequence();
        //sequence.Append()
        DeclareDeathComplete();
    }
}
