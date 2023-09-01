using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class EnemyController : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer visuals;
    [SerializeField]
    Sprite[] normalSprites;

    public void SetFacing(DirectionFacing facing)
    {
        visuals.sprite = normalSprites[(int)facing];
    }

    public Vector2Int[] GetVisionTiles(Vector2Int enemy, Vector2Int[] walls)
    {
        Vector2Int[] attack = GetAttackVisionTiles(enemy, walls);
        Vector2Int[] peripheral = GetPeripheralVisionTiles(enemy, walls);
        return attack.Union(peripheral).ToArray();
    }

    public bool CheckTargetInAttack(Vector2Int enemy, Vector2Int target, Vector2Int[] walls)
    {
        Vector2Int[] tiles = GetAttackVisionTiles(enemy, walls);
        foreach (Vector2Int position in tiles)
        {
            if(position == target)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckTargetInPeripheral(Vector2Int enemy, Vector2Int target, Vector2Int[] walls)
    {
        Vector2Int[] tiles = GetPeripheralVisionTiles(enemy, walls);
        foreach (Vector2Int position in tiles)
        {
            if (position == target)
            {
                return true;
            }
        }
        return false;
    }

    protected abstract Vector2Int[] GetAttackVisionTiles(Vector2Int enemy, Vector2Int[] walls);

    protected abstract Vector2Int[] GetPeripheralVisionTiles(Vector2Int enemy, Vector2Int[] walls);

}
