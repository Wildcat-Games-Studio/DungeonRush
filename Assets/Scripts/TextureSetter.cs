using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TextureSetter : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public void SetTexture(Texture texture)
    {
        spriteRenderer.sharedMaterial.SetTexture("_MaskTex", texture);
    }
}
