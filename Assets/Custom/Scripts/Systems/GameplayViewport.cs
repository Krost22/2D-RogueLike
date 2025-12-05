using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class GameplayViewport : MonoBehaviour
{
    [Header("Viewport Settings")]
    [Tooltip("The desired aspect ratio for the gameplay area (Width / Height). E.g., 1.0 for square.")]
    public float targetAspectRatio = 1.0f;

    [Tooltip("Scale of the viewport relative to the screen size (0.0 to 1.0).")]
    [Range(0.1f, 1.0f)]
    public float viewportScale = 0.9f;

    [Header("Visual Control")]
    [Tooltip("Automatically position and scale the background elements.")]
    public bool autoUpdateBackgrounds = true;
    [Tooltip("Automatically position and scale the border elements.")]
    public bool autoUpdateBorders = true;

    [Header("Visuals Resources")]
    public Sprite outerBackground;
    public Color outerBackgroundColor = Color.black;
    [Space]
    public Sprite innerBackground;
    public Color innerBackgroundColor = Color.gray;
    [Space]
    public Sprite borderSprite;
    public Color borderColor = Color.white;
    public float borderWidth = 1.0f; // World units

    private Camera cam;
    private Camera backgroundCam;
    
    // Visual Objects
    private GameObject outerBgObj;
    private GameObject innerBgObj;
    [SerializeField] private GameObject[] borders = new GameObject[4]; // Top, Bottom, Left, Right

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        SetupBackgroundCamera();
        SetupVisuals();
        UpdateViewport();
    }

    void Update()
    {
        UpdateViewport();
    }

    void SetupBackgroundCamera()
    {
        // Check if child exists first
        Transform bgChild = transform.Find("BackgroundCamera");
        if (bgChild != null)
        {
            backgroundCam = bgChild.GetComponent<Camera>();
            outerBgObj = bgChild.Find("OuterBackground")?.gameObject;
        }

        if (backgroundCam == null)
        {
            GameObject bgObj = new GameObject("BackgroundCamera");
            bgObj.transform.SetParent(transform);
            
            backgroundCam = bgObj.AddComponent<Camera>();
            backgroundCam.depth = cam.depth - 2; // Behind everything
            backgroundCam.cullingMask = 0; // Don't render scene
            backgroundCam.clearFlags = CameraClearFlags.SolidColor;
            backgroundCam.backgroundColor = outerBackgroundColor;
            backgroundCam.orthographic = true;
            backgroundCam.orthographicSize = 5f; // Arbitrary, will scale sprite
        }
        
        // Ensure main camera clears depth only
        cam.clearFlags = CameraClearFlags.Depth;
    }

    void SetupVisuals()
    {
        // 1. Outer Background (Attached to Background Camera)
        if (outerBgObj == null && backgroundCam != null)
        {
            outerBgObj = new GameObject("OuterBackground");
            outerBgObj.transform.SetParent(backgroundCam.transform);
            outerBgObj.transform.localPosition = new Vector3(0, 0, 10);
            var sr = outerBgObj.AddComponent<SpriteRenderer>();
            sr.sprite = outerBackground;
            sr.color = Color.white;
            // Layer?
        }

        // 2. Inner Background (Attached to Main Camera, behind game)
        Transform innerChild = transform.Find("InnerBackground");
        if (innerChild != null) innerBgObj = innerChild.gameObject;
        
        if (innerBgObj == null)
        {
            innerBgObj = new GameObject("InnerBackground");
            innerBgObj.transform.SetParent(transform);
            innerBgObj.transform.localPosition = new Vector3(0, 0, 20); // Far back
            var sr = innerBgObj.AddComponent<SpriteRenderer>();
            sr.sprite = innerBackground;
            sr.color = innerBackgroundColor;
        }

        // 3. Borders (Attached to Main Camera)
        string[] borderNames = { "BorderTop", "BorderBottom", "BorderLeft", "BorderRight" };
        if (borders == null || borders.Length != 4) borders = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            if (borders[i] != null) continue;

            Transform b = transform.Find(borderNames[i]);
            if (b != null) borders[i] = b.gameObject;
            
            if (borders[i] == null)
            {
                borders[i] = new GameObject(borderNames[i]);
                borders[i].transform.SetParent(transform);
                var sr = borders[i].AddComponent<SpriteRenderer>();
                sr.sprite = borderSprite;
                sr.color = borderColor;
            }
        }
    }

    void UpdateViewport()
    {
        if (cam == null) return;

        // --- 1. Viewport Calculation ---
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        float availableHeight = screenHeight * viewportScale;
        float availableWidth = screenWidth * viewportScale;

        float targetWidth, targetHeight;

        if (availableWidth / availableHeight > targetAspectRatio)
        {
            targetHeight = availableHeight;
            targetWidth = targetHeight * targetAspectRatio;
        }
        else
        {
            targetWidth = availableWidth;
            targetHeight = targetWidth / targetAspectRatio;
        }

        float rectW = targetWidth / screenWidth;
        float rectH = targetHeight / screenHeight;
        float rectX = (1.0f - rectW) / 2.0f;
        float rectY = (1.0f - rectH) / 2.0f;

        cam.rect = new Rect(rectX, rectY, rectW, rectH);

        // --- 2. Visuals Update ---
        
        // Update Outer Background Scale to fill screen
        if (autoUpdateBackgrounds && backgroundCam != null && outerBgObj != null)
        {
            backgroundCam.backgroundColor = outerBackgroundColor;
            var sr = outerBgObj.GetComponent<SpriteRenderer>();
            sr.sprite = outerBackground;
            
            if (outerBackground != null)
            {
                float camHeight = 2f * backgroundCam.orthographicSize;
                float camWidth = camHeight * backgroundCam.aspect;
                
                // Scale sprite to fit/fill
                float spriteH = outerBackground.bounds.size.y;
                float spriteW = outerBackground.bounds.size.x;
                
                float scaleY = camHeight / spriteH;
                float scaleX = camWidth / spriteW;
                float maxScale = Mathf.Max(scaleX, scaleY); // Fill (Crop)
                
                outerBgObj.transform.localScale = new Vector3(maxScale, maxScale, 1);
            }
        }

        // Update Inner Background & Borders (World Space relative to Main Camera)
        float camOrthoSize = cam.orthographicSize;
        float camAspect = cam.aspect; // This is the VIEWPORT aspect ratio now!
        float worldHeight = 2f * camOrthoSize;
        float worldWidth = worldHeight * camAspect;

        // Inner Background
        if (autoUpdateBackgrounds && innerBgObj != null)
        {
            var sr = innerBgObj.GetComponent<SpriteRenderer>();
            sr.sprite = innerBackground;
            sr.color = innerBackgroundColor;
            
            if (innerBackground != null)
            {
                float spriteH = innerBackground.bounds.size.y;
                float spriteW = innerBackground.bounds.size.x;
                innerBgObj.transform.localScale = new Vector3(worldWidth / spriteW, worldHeight / spriteH, 1);
            }
        }

        // Borders
        if (autoUpdateBorders)
        {
            // Top
            UpdateBorder(borders[0], new Vector3(0, camOrthoSize + borderWidth/2f, 10), new Vector3(worldWidth + borderWidth*2, borderWidth, 1));
            // Bottom
            UpdateBorder(borders[1], new Vector3(0, -camOrthoSize - borderWidth/2f, 10), new Vector3(worldWidth + borderWidth*2, borderWidth, 1));
            // Left
            UpdateBorder(borders[2], new Vector3(-worldWidth/2f - borderWidth/2f, 0, 10), new Vector3(borderWidth, worldHeight, 1));
            // Right
            UpdateBorder(borders[3], new Vector3(worldWidth/2f + borderWidth/2f, 0, 10), new Vector3(borderWidth, worldHeight, 1));
        }
    }

    void UpdateBorder(GameObject obj, Vector3 localPos, Vector3 scale)
    {
        if (obj == null) return;
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
        
        var sr = obj.GetComponent<SpriteRenderer>();
        sr.sprite = borderSprite;
        sr.color = borderColor;
    }
}
