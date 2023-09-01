using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookController : EnemyController
{
    protected override Vector2Int[] GetAttackVisionTiles(Vector2Int enemy, Vector2Int[] walls)
    {
        return new Vector2Int[] { enemy };
    }

    protected override Vector2Int[] GetPeripheralVisionTiles(Vector2Int enemy, Vector2Int[] walls)
    {
        return new Vector2Int[] { enemy };
        //throw new System.NotImplementedException();
    }
}
