using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarController : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer visualsTop,visualsBottom;
    [SerializeField]
    Sprite normalTop, wrappedTop;
    [SerializeField]
    Sprite normalBottom, wrappedBottom;

    private void OnEnable()
    {
        SetWrapped(false);
    }

    public void SetWrapped(bool isWrapped)
    {
        visualsTop.sprite = isWrapped ? wrappedTop : normalTop;
        visualsBottom.sprite = isWrapped ? wrappedBottom : normalBottom;
    }
}
