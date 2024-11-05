using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Dalak.LineRenderer3D;

[RequireComponent(typeof(LineRenderer3D))]
public class CameraLineController : MonoBehaviour
{
    [Header("Components")]
    public Camera mainCamera;
    public Camera filmCamera;
    [SerializeField]private LineController lineRenderer3DPrefab; // Reference to the prefab
    private LineRenderer3D currentLine;
    private GameObject cameraRig;

    [Header("Line Settings")]
    [SerializeField] private float lineRadius = 0.25f;
    [SerializeField] private int vertexPerLoop = 4;
    [SerializeField] private int cornerLoops = 3;
    public Color lineColor = Color.white;

    [Header("Preview Settings")]
    private RenderTexture previewRT;
    private bool previewInitialized = false;
    private Rect previewWindowRect;

    [Header("Camera Movement")]
    public float defaultSpeed = 0.25f; // meters per second
    private bool isDrawing = false;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float movementProgress = 0f;
    private bool isMoving = false;
    private bool reverseDirection = false;

    [Header("UI Settings")]
    private CameraMode currentMode = CameraMode.None;

    public static GameObject Instance = new GameObject();

    public enum CameraMode
    {
        None,
        Pan,
        Track,
        Steadicam
    }

    void Awake()
    {
        Instance = this.gameObject;
    }

    void Start()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
            if (!mainCamera)
            {
                Debug.LogError("No camera found!");
                enabled = false;
                return;
            }
        }

        if (lineRenderer3DPrefab == null)
        {
            Debug.LogError("LineRenderer3D prefab not assigned in inspector!");
            enabled = false;
            return;
        }

        // Setup preview window rect (16:9 aspect ratio)
        float previewWidth = Screen.width * 0.25f;
        float previewHeight = previewWidth * 9f / 16f;
        previewWindowRect = new Rect(
            Screen.width - previewWidth - 20,
            Screen.height - previewHeight - 20,
            previewWidth,
            previewHeight
        );

        // Create camera rig
        cameraRig = new GameObject("CameraRig");
        if (filmCamera)
        {
            filmCamera.transform.SetParent(cameraRig.transform);
            InitializePreviewRT();
        }
    }

    void InitializePreviewRT()
    {
        if (previewRT != null)
        {
            previewRT.Release();
        }

        previewRT = new RenderTexture(
            (int)previewWindowRect.width,
            (int)previewWindowRect.height,
            24,
            RenderTextureFormat.ARGB32
        );
        previewRT.Create();

        filmCamera.targetTexture = previewRT;
        previewInitialized = true;
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("FILE", GUILayout.Width(100)))
        {
            currentMode = CameraMode.None;
        }
        if (GUILayout.Button("PAN", GUILayout.Width(100)))
        {
            currentMode = CameraMode.Pan;
        }
        if (GUILayout.Button("TRACK", GUILayout.Width(100)))
        {
            currentMode = CameraMode.Track;
        }
        if (GUILayout.Button("STEADICAM", GUILayout.Width(100)))
        {
            currentMode = CameraMode.Steadicam;
        }
        GUILayout.EndHorizontal();

        // Draw preview window
        if (previewInitialized && previewRT != null)
        {
            GUI.Box(previewWindowRect, "");
            GUI.DrawTexture(previewWindowRect, previewRT);
        }
    }

    void Update()
    {
        HandleInput();
        UpdateCameraMovement();
    }

    void OnDisable()
    {
        if (previewRT != null)
        {
            previewRT.Release();
            previewRT = null;
        }
    }

    void HandleInput()
    {
        if (currentMode == CameraMode.Pan || currentMode == CameraMode.Track)
        {
            // Start drawing line
            if (Input.GetMouseButtonDown(0) && !isDrawing)
            {
                startPoint = GetMouseWorldPosition();
                CreateNewLine();
                isDrawing = true;
            }

            // Update line while drawing
            if (isDrawing && currentLine != null)
            {
                Vector3 currentPoint = GetMouseWorldPosition();
                UpdateLine(startPoint, currentPoint);
            }

            // Finish drawing line
            if (Input.GetMouseButtonUp(0) && isDrawing && currentLine != null)
            {
                endPoint = GetMouseWorldPosition();
                UpdateLine(startPoint, endPoint);
                isDrawing = false;
                BeginCameraMovement();
            }
        }

        // Camera rotation controls
        if (Input.GetKey(KeyCode.R))
        {
            float rotX = Input.GetAxis("Mouse X") * 2f;
            float rotY = Input.GetAxis("Mouse Y") * 2f;
            cameraRig.transform.Rotate(Vector3.up, rotX, Space.World);
            cameraRig.transform.Rotate(Vector3.right, -rotY, Space.World);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    void CreateNewLine()
    {
        if (lineRenderer3DPrefab != null)
        {
            // Instantiate from prefab
            GameObject lineObj = new GameObject("CameraLine");
            currentLine = Instantiate(lineRenderer3DPrefab.currentLine, lineObj.transform);

            // Configure line settings
            if (currentLine != null)
            {
                currentLine.pathData.positions.Clear();
                currentLine.pipeMeshSettings.radius = lineRadius;
                currentLine.pipeMeshSettings.nVertexPerLoop = vertexPerLoop;
                currentLine.pipeMeshSettings.nCornerLoops = cornerLoops;
            }
            else
            {
                Debug.LogError("Failed to instantiate LineRenderer3D component");
            }
        }
        else
        {
            Debug.LogError("LineRenderer3D prefab not assigned");
        }
    }

    void UpdateLine(Vector3 start, Vector3 end)
    {
        if (currentLine != null)
        {
            currentLine.pathData.positions.Clear();
            currentLine.pathData.positions.Add(start);
            currentLine.pathData.positions.Add(end);
            currentLine.UpdateMesh();
        }
    }

    void BeginCameraMovement()
    {
        movementProgress = 0f;
        isMoving = true;
        // Position camera at start of line
        cameraRig.transform.position = startPoint;
    }

    void UpdateCameraMovement()
    {
        if (!isMoving) return;

        float direction = reverseDirection ? -1f : 1f;
        movementProgress += (defaultSpeed * direction * Time.deltaTime) / Vector3.Distance(startPoint, endPoint);
        movementProgress = Mathf.Clamp01(movementProgress);

        // Move camera along line
        cameraRig.transform.position = Vector3.Lerp(startPoint, endPoint, movementProgress);

        // Check if we reached the end
        if (movementProgress >= 1f || movementProgress <= 0f)
        {
            isMoving = false;
        }
    }

    public void ToggleDirection()
    {
        reverseDirection = !reverseDirection;
        isMoving = true;
    }

    public void SetMode(CameraMode mode)
    {
        currentMode = mode;

    }

   
}