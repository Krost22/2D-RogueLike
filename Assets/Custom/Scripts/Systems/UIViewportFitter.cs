using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIViewportFitter : MonoBehaviour
{
    [Tooltip("The camera that defines the gameplay viewport.")]
    public Camera targetCamera;

    private RectTransform rectTransform;

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null || rectTransform == null) return;

        // Get the viewport rect from the camera (0-1 coordinates)
        Rect camRect = targetCamera.rect;

        // Apply to the UI Panel's anchors
        // This makes the panel occupy exactly the same relative screen space as the camera viewport
        rectTransform.anchorMin = new Vector2(camRect.x, camRect.y);
        rectTransform.anchorMax = new Vector2(camRect.x + camRect.width, camRect.y + camRect.height);

        // Zero out offsets so it stretches perfectly between anchors
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
