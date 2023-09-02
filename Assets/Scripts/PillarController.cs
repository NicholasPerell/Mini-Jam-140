using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarController : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer visuals;
    [SerializeField]
    Sprite normal, wrapped;

    private void OnEnable()
    {
        SetWrapped(false);
    }

    public void SetWrapped(bool isWrapped)
    {
        visuals.sprite = isWrapped ? wrapped : normal;
    }
}
