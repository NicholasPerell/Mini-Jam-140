using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSpawnIndicator : MonoBehaviour
{
    public enum Type
    {
        PLAYER,
        PILLAR,
        ROOK,
        KNIGHT,
        PAWN,
        QUEEN,
        BISHOP,
        PJ
    }

    public Type type;
    public DirectionFacing facing;

    [Space]
    [Space]

    [Header("DO NOT EDIT")]
    [SerializeField]
    SpriteRenderer typeVisual;
    [SerializeField]
    SpriteRenderer facingVisual;
    [SerializeField]
    Sprite[] typeSprites;
    [SerializeField]
    Sprite[] directionSprites;

    public void UpdateVisuals()
    {
        typeVisual.sprite = typeSprites[(int)type];
        facingVisual.sprite = (type != Type.PILLAR && type != Type.PJ) ? directionSprites[(int)facing] : null;
        facingVisual.transform.right = (type == Type.BISHOP) ? new Vector3(1, 1) : Vector3.right;

        Tilemap tilemap = GameObject.FindAnyObjectByType<Tilemap>();
        if (tilemap)
        {
            transform.position = tilemap.CellToWorld(tilemap.WorldToCell(transform.position)) + new Vector3(.5f, .5f);
        }
    }

    private void OnValidate()
    {
        UpdateVisuals();
    }

    private void OnDrawGizmosSelected()
    {
        Tilemap tilemap = GameObject.FindAnyObjectByType<Tilemap>();
        if (tilemap)
        {
            Gizmos.color = new Color(.5f,.5f,.5f,(type == Type.PILLAR ? 1.0f : 0.5f));
            Gizmos.DrawSphere(tilemap.CellToWorld(tilemap.WorldToCell(transform.position)) + new Vector3(.5f, .5f),0.1f);
        }
    }

}
