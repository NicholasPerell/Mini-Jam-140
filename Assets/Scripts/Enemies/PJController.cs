using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class PJController : EnemyController
{
    LevelData.EnemyData enemyData;

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
        enemyData = levelData.enemies[GetIndex()];

        Vector2Int difference = enemyData.position - levelData.playerPosition;
        for (int i = 0; i < 4; i++)
        {
            if (difference == TurnFacingToVector((DirectionFacing)i))
            {
                DeclareTurnOver(enemyData.position, (DirectionFacing)i);
                return;
            }
        }
        DeclareTurnOver(enemyData.position, enemyData.directionFacing);
    }

    public override void RequestDie()
    {
        StartCoroutine(Coroutine());
        IEnumerator Coroutine()
        {
            PlayerController player = GameObject.FindAnyObjectByType<PlayerController>();

            Vector3 worldPosition = tilemap.transform.position + tilemap.tileAnchor + new Vector3(enemyData.position.x * tilemap.cellSize.x, enemyData.position.y * tilemap.cellSize.y, 0);
            AudioSystem.Instance?.RequestSound(player.AlternatingStep ? "PlayerJump01" : "PlayerJump02");
            Sequence sequence = DOTween.Sequence();
            sequence.Append(player.transform.DOJump(worldPosition, 0.1f, 1, .25f));
            yield return new WaitForSeconds(.25f);
            player.SetPJs();
            visuals.enabled = false;
            yield return new WaitForSeconds(.5f);
            player.SetFacing(DirectionFacing.DOWN);
            yield return new WaitForSeconds(.5f);
            DeclareDeathComplete();
        }

    }
}
