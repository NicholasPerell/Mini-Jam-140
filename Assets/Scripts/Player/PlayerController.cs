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
    public LayerMask whatStopsMovement;
    public float moveSpeed = 2f;
    public Transform movePoint;
    public GameObject objectToSpawnvert;
    public GameObject objectToSpawnhorz;

    public void SetFacing(DirectionFacing facing)
    {
        visuals.sprite = normalSprites[(int)facing];
    }

    public void BeginTurn(LevelData levelData)
    {
        currentLevelData = levelData;
        isPlayerTurn = true;
    }

    void Start()
    {
        movePoint.parent = null;
    }

    void Update()
    {

        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= .05)
        {

            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), .35f, whatStopsMovement))
                {
                        if (Input.GetKey(KeyCode.D))
                        {
                            movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                            Instantiate(objectToSpawnhorz, transform.position, objectToSpawnhorz.transform.rotation);
                        }
                        if (Input.GetKey(KeyCode.A))
                        {
                            movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                            Instantiate(objectToSpawnhorz, transform.position, objectToSpawnhorz.transform.rotation);
                        }
                }
            }
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), .35f, whatStopsMovement))
                {

                        if (Input.GetKey(KeyCode.W))
                        {
                            movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                            Instantiate(objectToSpawnvert, transform.position, objectToSpawnvert.transform.rotation);
                        }
                        if (Input.GetKey(KeyCode.S))
                        {
                            movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                            Instantiate(objectToSpawnvert, transform.position, objectToSpawnvert.transform.rotation);
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
