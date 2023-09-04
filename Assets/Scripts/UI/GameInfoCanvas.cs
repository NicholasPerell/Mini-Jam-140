using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameInfoCanvas : MonoBehaviour
{
    [SerializeField]
    PlayerController player;
    [SerializeField]
    TurnManager turnManager;
    [SerializeField]
    TextMeshProUGUI data;
    int coins = 0; 

    private void OnEnable()
    {
        turnManager.OnInitializeLevel += RespondToInitializeLevel;
        player.OnCoinsUpdated += RespondToCoinsUpdated;
    }

    private void OnDisable()
    {
        turnManager.OnInitializeLevel -= RespondToInitializeLevel;
        player.OnCoinsUpdated -= RespondToCoinsUpdated;
    }

    private void RespondToInitializeLevel(LevelData arg0)
    {
        coins = arg0.coins;
        data.text = "" + coins;
    }


    private void RespondToCoinsUpdated(Vector2Int[] arg0)
    {
        coins--;
        data.text = "" + coins;
    }
}
