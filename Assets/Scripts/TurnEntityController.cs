using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class PositionComparer : IComparer<Vector2Int>
{
    public int Compare(Vector2Int x, Vector2Int y)
    {
        int result = x.x.CompareTo(y.x);
        if(result == 0)
        {
            result = x.y.CompareTo(y.y);
        }
        return result;
    }
}

public abstract class TurnEntityController : MonoBehaviour
{
    [SerializeField]
    protected SpriteRenderer visuals;
    [SerializeField]
    Sprite[] normalSprites;
    [SerializeField]
    protected Tilemap tilemap;

    protected LevelData currentLevelData;
    [SerializeField]
    private bool isEntityTurn = false;
    protected bool IsEntityTurn
    {
        get { return isEntityTurn; }
    }
    public event UnityAction<Vector2Int,DirectionFacing> OnTurnComplete;

    public virtual void BeginTurn(LevelData levelData)
    {
        currentLevelData = levelData;
        isEntityTurn = true;
    }

    protected void DeclareTurnOver(Vector2Int position, DirectionFacing facing)
    {
        Debug.Log(gameObject + ": DeclareTurnOver");
        isEntityTurn = false;
        OnTurnComplete?.Invoke(position, facing);
    }

    public virtual void SetFacing(DirectionFacing facing)
    {
        visuals.sprite = normalSprites[(int)facing];
    }

    protected Vector2Int TurnFacingToVector(DirectionFacing facing)
    {
        Vector2Int vector = Vector2Int.zero;
        switch (facing)
        {
            case DirectionFacing.RIGHT:
                vector = Vector2Int.right;
                break;
            case DirectionFacing.UP:
                vector = Vector2Int.up;
                break;
            case DirectionFacing.LEFT:
                vector = Vector2Int.left;
                break;
            case DirectionFacing.DOWN:
                vector = Vector2Int.down;
                break;
        }
        return vector;
    }

    protected Vector2Int TurnLeftToVector(DirectionFacing facing) => TurnOffsetToVector(facing, 1);
    protected Vector2Int TurnBehindToVector(DirectionFacing facing) => TurnOffsetToVector(facing, 2);
    protected Vector2Int TurnRightToVector(DirectionFacing facing) => TurnOffsetToVector(facing, 3);

    private Vector2Int TurnOffsetToVector(DirectionFacing facing, int offset)
    {
        facing = (DirectionFacing)(((int)facing + offset) % 4);
        return TurnFacingToVector(facing);
    }

    protected Vector3 ConvertToWorldPos(Vector2Int cellPosition)
    {
        Vector3 worldPosition = tilemap.transform.position + tilemap.tileAnchor + new Vector3(cellPosition.x * tilemap.cellSize.x, cellPosition.y * tilemap.cellSize.y, 0);
        return worldPosition;
    }
}
