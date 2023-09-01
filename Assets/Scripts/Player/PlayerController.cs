using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer visuals;
    [SerializeField]
    Sprite[] normalSprites;

    [SerializeField] 
    CoinGrid CoinMarker;

    LevelData currentLevelData;

    bool isPlayerTurn = false;

    public event UnityAction OnTurnComplete;
    public bool whatStopsMovement = false;
    public float moveSpeed = 2f;
    public Transform movePoint;
    public GameObject objectToSpawnvert;
    public GameObject objectToSpawnhorz;
    public GameObject objectToMark;
    public int coinCount = 0;
    public int x = 0;
    public int y = 0;

    public void SetFacing(DirectionFacing facing)
    {
        visuals.sprite = normalSprites[(int)facing];
    }

    public void BeginTurn(LevelData levelData)
    {
        currentLevelData = levelData;
        coinCount = currentLevelData.coinsAtStart;
        isPlayerTurn = true;
    }

    void Start()
    {
        movePoint.parent = null;
        x = 0;
        y = 0;
        Array.Sort(currentLevelData.walls);
        int searchIndex = Array.BinarySearch(currentLevelData.walls, movePoint.position);
        if (searchIndex > -1)
        {
            whatStopsMovement = true;
        }
    }

    void Update()
    {
        if (isPlayerTurn == true)
        {

            if (whatStopsMovement == false)
            {
                transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, movePoint.position) <= .05)
                {

                    if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
                    {
                        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), .35f))
                        {
                            if (Input.GetKey(KeyCode.D))
                            {
                                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                                x = 1;
                                y = 0;
                            }
                            if (Input.GetKey(KeyCode.A))
                            {
                                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                                x = -1;
                                y = 0;
                            }
                        }
                    }
                    if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
                    {
                        if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), .35f))
                        {

                            if (Input.GetKey(KeyCode.W))
                            {
                                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                                y = 1;
                                x = 0;
                            }
                            if (Input.GetKey(KeyCode.S))
                            {
                                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                                y = -1;
                                x = 0;
                            }
                        }
                    }
                }
            }

            if (coinCount > 0)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (x == 1)
                    {
                        CoinMarker.CreateGrid(currentLevelData); 
                    }
                    else if (x == -1)
                    {
                        CoinMarker.CreateGrid(currentLevelData);
                    }
                    else if (y == 1)
                    {
                        CoinMarker.CreateGrid(currentLevelData);
                    }
                    else if (y == -1)
                    {
                        CoinMarker.CreateGrid(currentLevelData);
                    }
                }
            }
        }
    }

    private void DeclareTurnOver()
    {
        OnTurnComplete?.Invoke();
        isPlayerTurn = false;
    }
}
