using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class TitleScaler : MonoBehaviour
{
    private CanvasScaler canvasScaler;
    public float referenceAspectRatio = 16f / 9f;

    void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        AdjustCanvasScaler();
    }

    void Update()
    {
        AdjustCanvasScaler();
    }

    void AdjustCanvasScaler()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;

        if (currentAspectRatio > referenceAspectRatio)
        {
            canvasScaler.matchWidthOrHeight = 1f;
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 0f;
        }
    }
}