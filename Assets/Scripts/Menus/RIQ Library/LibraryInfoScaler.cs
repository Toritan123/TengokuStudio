using UnityEngine;

public class LibraryInfoScaler : MonoBehaviour
{
    public float targetAspect = 16f / 9f; 

    void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        float scale = currentAspect > targetAspect
            ? targetAspect / currentAspect
            : 1f;

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
