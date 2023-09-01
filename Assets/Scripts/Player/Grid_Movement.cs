using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_Movement : MonoBehaviour
{
    private float speed = 5;

    private bool isMoving;

    private Vector3 origPos, targetpos;

    private float timeToMove = 0.2f;

    public GameObject objectToSpawn;

    public GameObject spawnToObject;


    void Update()
    {
        if (Input.GetKey(KeyCode.W) && !isMoving)
            StartCoroutine(MovePlayer(Vector3.up));

        if (Input.GetKey(KeyCode.A) && !isMoving)
            StartCoroutine(MovePlayer(Vector3.left));

        if (Input.GetKey(KeyCode.S) && !isMoving)
            StartCoroutine(MovePlayer(Vector3.down));

        if (Input.GetKey(KeyCode.D) && !isMoving)
            StartCoroutine(MovePlayer(Vector3.right));

        if (Input.GetKey(KeyCode.R))
            Instantiate(objectToSpawn, transform.position, objectToSpawn.transform.rotation);


    }

    private IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;

        float elapsedTime = 0;

        origPos = transform.position;
        targetpos = origPos + direction;

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(origPos, targetpos, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetpos;

        isMoving = false;
    }
}
