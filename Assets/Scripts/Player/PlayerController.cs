using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer visuals;
    [SerializeField]
    Sprite[] normalSprites;

    LevelData currentLevelData;

    bool isPlayerTurn = false;

    public event UnityAction OnTurnComplete;

    public void SetFacing(DirectionFacing facing)
    {
        visuals.sprite = normalSprites[(int)facing];
    }

    public void BeginTurn(LevelData levelData)
    {
        currentLevelData = levelData;
        isPlayerTurn = true;
    }

    private void DeclareTurnOver()
    {
        OnTurnComplete?.Invoke();
        isPlayerTurn = false;
    }
}
