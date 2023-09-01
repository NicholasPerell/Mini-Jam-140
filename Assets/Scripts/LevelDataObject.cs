using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    ROOK,
    KNIGHT,
    PAWN,
    QUEEN,
    BISHOP
}

public enum DirectionFacing
{
    RIGHT,
    UP,
    LEFT,
    DOWN
}

[System.Serializable]
public struct LevelData
{
    [System.Serializable]
    public struct EnemyData
    {
        public EnemyType type;
        public Vector2Int position;
        public DirectionFacing directionFacing;
    }

    public Vector2Int[] walls;
    public Vector2Int playerPosition;
    public DirectionFacing playerDirectionFacing;
    public int coinsAtStart;
    public Vector2Int[] pillars;
    public EnemyData[] enemies;
}

[CreateAssetMenu(fileName = "NewLevelData.asset", menuName = "Level Data")]
public class LevelDataObject : ScriptableObject
{
    [SerializeField]
    LevelData data;
    public LevelData Data
    {
        get
        {
            return data;
        }
    }

    public static LevelDataObject CreateInstance(LevelData _data)
    {
        LevelDataObject created = ScriptableObject.CreateInstance<LevelDataObject>();
        created.data = _data;
        return created;
    }
}