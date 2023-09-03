using System;
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

public enum EnemyAction
{
    QUESTION,
    CHARGE,
    ATTACK
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
        public EnemyAction enemyAction;
    }

    public Vector2Int[] walls;
    public Vector2Int playerPosition;
    public DirectionFacing playerDirectionFacing;
    public int coins;
    public Vector2Int[] pillars;
    public EnemyData[] enemies;

    //In-Game Data (not for start of level)
    //[HideInInspector]
    public Vector2Int[] coinPath;
    //[HideInInspector]
    public List<int> pillarsWrapped; //Consider making this an array too

    public LevelData Copy()
    {
        LevelData data = new LevelData();
        data.walls = new Vector2Int[walls.Length];
        for (int i = 0; i < walls.Length; i++)
        {
            data.walls[i] = walls[i];
        }
        data.playerPosition = playerPosition;
        data.playerDirectionFacing = playerDirectionFacing;
        data.coins = coins;
        data.pillars = new Vector2Int[pillars.Length];
        for (int i = 0; i < pillars.Length; i++)
        {
            data.pillars[i] = pillars[i];
        }
        data.enemies = new EnemyData[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            data.enemies[i] = enemies[i];
        }
        data.coinPath = new Vector2Int[coinPath.Length];
        for (int i = 0; i < coinPath.Length; i++)
        {
            data.coinPath[i] = coinPath[i];
        }
        data.pillarsWrapped = new List<int>();
        data.pillarsWrapped.AddRange(pillarsWrapped); //Consider making this an array too

        return data;
    }
}

[CreateAssetMenu(fileName = "NewLevelData.asset", menuName = "Level Data")]
public class LevelDataObject : ScriptableObject
{
    [SerializeField]
    LevelData data;
    [Space]
    [Space]
    [SerializeField]
    Texture2D loadingImage;

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

#if UNITY_EDITOR
    public void LoadFromImage()
    {
        if(loadingImage)
        {
            List<Vector2Int> walls = new List<Vector2Int>();

            Color32 wallColor = new Color32(0, 0, 0, 255);
            Color32 playerSpawnColor = new Color32(0, 255, 0, 255);

            Color32 colorFound;
            Color32[] all = loadingImage.GetPixels32();
            Vector2Int currentPosition;
            for (int i = 0; i < loadingImage.width; i++)
            {
                for (int j = 0; j < loadingImage.height; j++)
                {
                    colorFound = all[i + j * loadingImage.width];
                    currentPosition = new Vector2Int(i, j);
                    if (Match(colorFound, wallColor))
                    {
                        walls.Add(currentPosition);
                    }
                    else if(Match(colorFound, playerSpawnColor))
                    {
                        data.playerPosition = currentPosition;
                    }
                }
            }
            data.walls = walls.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    private bool Match(Color32 a, Color32 b)
    {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }
#endif
}