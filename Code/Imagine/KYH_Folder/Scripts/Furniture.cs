using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    public Sprite[] furniture;
    public int furnitureNum;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        switch (furnitureNum)
        {
            case 0:
                spriteRenderer.sprite = furniture[furnitureNum];
                break;
            case 1:
                spriteRenderer.sprite = furniture[furnitureNum];
                break;
            case 2:
                spriteRenderer.sprite = furniture[furnitureNum];
                break;
            case 3:
                spriteRenderer.sprite = furniture[furnitureNum];
                break;
            case 4:
                spriteRenderer.sprite = furniture[furnitureNum];
                break;
            case 5:
                spriteRenderer.sprite = furniture[furnitureNum];

                break;
            case 6:
                spriteRenderer.sprite = furniture[furnitureNum];
                break;
            case 7:
                spriteRenderer.sprite = furniture[furnitureNum];
                break;
        }
    }
}
