using UnityEngine;
using Dalak.LineRenderer3D;

public class SimpleCameraLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera filmCamera;
    [SerializeField] private LineRenderer3D lineRendererPrefab;

    [Header("Line Settings")]
    [SerializeField] private float lineRadius = 0.25f;
    [SerializeField] private float moveSpeed = 0.25f;

    [Header("Calibration")]
    [SerializeField] private float planeHeight = 0f;

    [Header("Preview Window")]
    [SerializeField] private Vector2 previewSize = new Vector2(320, 180); 
    [SerializeField] private Vector2 previewPadding = new Vector2(20, 20); 



    private LineRenderer3D currentLine;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private bool isDrawing;
    private bool isMoving;
    private float moveProgress;
    private GameObject cameraRig;

    // Preview window variables
    private RenderTexture previewRT;
    private Rect previewRect;

    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!lineRendererPrefab)
        {
            Debug.LogError("Please assign LineRenderer3D prefab!");
            enabled = false;
            return;
        }

        cameraRig = new GameObject("CameraRig");
        if (filmCamera)
        {
            filmCamera.transform.SetParent(cameraRig.transform);
            SetupPreviewWindow();
        }
    }

    void SetupPreviewWindow()
    {
        // Create preview render texture
        previewRT = new RenderTexture(
            (int)previewSize.x,
            (int)previewSize.y,
            24,
            RenderTextureFormat.ARGB32
        );

        // Set the preview window position (bottom-right corner)
        previewRect = new Rect(
            Screen.width - previewSize.x - previewPadding.x,
            Screen.height - previewSize.y - previewPadding.y,
            previewSize.x,
            previewSize.y
        );

        // Assign render texture to film camera
        filmCamera.targetTexture = previewRT;
    }

    void OnGUI()
    {
        if (previewRT != null)
        {
            // Draw background box
            GUI.Box(previewRect, "");

            // Draw camera preview
            GUI.DrawTexture(previewRect, previewRT);

            // Optional: Add label
            GUI.Label(new Rect(previewRect.x, previewRect.y - 20, 200, 20), "Camera Preview");
        }
    }

    void Update()
    {
        HandleInput();
        UpdateCameraMovement();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !isDrawing)
        {
            //startPoint = GetScreenToWorldPoint(Input.mousePosition);
            //CreateNewLine();
            //isDrawing = true;

            // Get the mouse position in screen-space
            Vector3 screenMousePos = Input.mousePosition;

            // Create a ray from the camera using the screen-space mouse position
            Ray ray = mainCamera.ScreenPointToRay(screenMousePos);

            // Perform a Raycast to find the intersection point with the scene
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Use the hit point as the start point for the line
                startPoint = hit.point;
                CreateNewLine();
                isDrawing = true;
            }
        }

        if (isDrawing && currentLine != null)
        {
            Vector3 currentPoint = new Vector3(0,0,0);
            //UpdateLine(startPoint, currentPoint);

            // Get the mouse position in screen-space
            Vector3 currentPos = Input.mousePosition;
            // Create a ray from the camera using the screen-space mouse position
            Ray ray = mainCamera.ScreenPointToRay(currentPos);
            // Perform a Raycast to find the intersection point with the scene
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                currentPoint = hit.point;
                UpdateLine(startPoint, currentPoint);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            //endPoint = GetScreenToWorldPoint(Input.mousePosition);
            //UpdateLine(startPoint, endPoint);
            //isDrawing = false;
            //BeginCameraMovement();

            Vector3 endPos = Input.mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(endPos);
            RaycastHit hit;
            if( Physics.Raycast(ray,out hit))
            {
                endPoint = hit.point;
                UpdateLine(startPoint, endPoint);
                isDrawing = false;
                BeginCameraMovement();
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            float rotX = Input.GetAxis("Mouse X") * 2f;
            float rotY = Input.GetAxis("Mouse Y") * 2f;
            cameraRig.transform.Rotate(Vector3.up, rotX, Space.World);
            cameraRig.transform.Rotate(Vector3.right, -rotY, Space.World);
        }
    }

    Vector3 GetScreenToWorldPoint(Vector3 screenPoint)
    {
        Plane drawPlane = new Plane(Vector3.up, new Vector3(0, planeHeight, 0));
        Ray ray = mainCamera.ScreenPointToRay(screenPoint);
        float enter;

        if (drawPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Debug.Log($"Screen Point: {screenPoint}, World Point: {hitPoint}");
            return hitPoint;
        }

        Debug.LogWarning("Raycast failed to hit plane!");
        return Vector3.zero;
    }

    void CreateNewLine()
    {
        GameObject lineObj = new GameObject("CameraLine");
        currentLine = Instantiate(lineRendererPrefab, lineObj.transform);
        currentLine.pathData.positions.Clear();
        currentLine.pipeMeshSettings.radius = lineRadius;
    }

    void UpdateLine(Vector3 start, Vector3 end)
    {
        if (currentLine != null && start != Vector3.zero && end != Vector3.zero)
        {
            currentLine.pathData.positions.Clear();
            currentLine.pathData.positions.Add(start);
            currentLine.pathData.positions.Add(end);
            Debug.Log($"Line Points - Start: {start}, End: {end}, Distance: {Vector3.Distance(start, end)}");
            currentLine.UpdateMesh();
        }
    }

    void BeginCameraMovement()
    {
        if (startPoint != Vector3.zero && endPoint != Vector3.zero)
        {
            moveProgress = 0f;
            isMoving = true;
            cameraRig.transform.position = startPoint;

            Vector3 direction = (endPoint - startPoint).normalized;
            if (direction != Vector3.zero)
            {
                cameraRig.transform.forward = direction;
            }
        }
    }

    void UpdateCameraMovement()
    {
        if (!isMoving) return;

        moveProgress += moveSpeed * Time.deltaTime / Vector3.Distance(startPoint, endPoint);
        moveProgress = Mathf.Clamp01(moveProgress);

        cameraRig.transform.position = Vector3.Lerp(startPoint, endPoint, moveProgress);

        if (moveProgress >= 1f)
        {
            isMoving = false;
        }
    }

    void OnDrawGizmos()
    {
        if (isDrawing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(startPoint, 0.1f);
            Vector3 currentPoint = GetScreenToWorldPoint(Input.mousePosition);
            Gizmos.DrawSphere(currentPoint, 0.1f);
        }
    }

    void OnDisable()
    {
        // Clean up render texture when script is disabled
        if (previewRT != null)
        {
            previewRT.Release();
            previewRT = null;
        }
    }
}