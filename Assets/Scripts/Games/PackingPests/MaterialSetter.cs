using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
    public Material materialToSet; 

    void Start()
    {
        ApplyMaterialRecursively(transform);
    }

    void ApplyMaterialRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.material = materialToSet;
            }

            ApplyMaterialRecursively(child);
        }
    }
}

