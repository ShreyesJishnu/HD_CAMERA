using UnityEngine;
using System.Collections.Generic;
using Dalak.LineRenderer3D;

public class LineController : MonoBehaviour
{
    public Camera mainCamera;
    public LineRenderer3D currentLine;
    public GameObject lineRendererPrefab;

    private bool isDrawing = false;
    private List<Vector3> currentPoints = new List<Vector3>();

    void Start()
    {
        // Ensure we have reference to main camera
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        HandleLineDrawing();
    }

    void HandleLineDrawing()
    {
        // Start drawing on mouse down
        if (Input.GetMouseButtonDown(0))
        {
            StartNewLine();
        }
        // Continue drawing while mouse is held
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            UpdateLine();
        }
        // Finish drawing on mouse up
        else if (Input.GetMouseButtonUp(0))
        {
            FinishLine();
        }
    }

    void StartNewLine()
    {
        // Create new line instance
        GameObject newLineObj = Instantiate(lineRendererPrefab);
        currentLine = newLineObj.GetComponent<LineRenderer3D>();

        // Configure line settings
        currentLine.pipeMeshSettings.radius = 0.25f;
        currentLine.pipeMeshSettings.nVertexPerLoop = 8;  // Smooth line
        currentLine.pipeMeshSettings.nCornerLoops = 3;

        // Clear points list
        currentPoints.Clear();

        // Get initial point
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            currentPoints.Add(hit.point);
            currentLine.pathData.positions.Add(hit.point);
            isDrawing = true;
        }
    }

    void UpdateLine()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if point is far enough from last point to add
            if (currentPoints.Count > 0)
            {
                float distance = Vector3.Distance(hit.point, currentPoints[currentPoints.Count - 1]);
                if (distance > 0.1f)  // Minimum distance between points
                {
                    currentPoints.Add(hit.point);
                    currentLine.pathData.positions.Add(hit.point);
                    currentLine.UpdateMesh();
                }
            }
        }
    }

    void FinishLine()
    {
        if (currentLine != null && currentPoints.Count > 1)
        {
            currentLine.UpdateMesh();
        }

        isDrawing = false;
        currentLine = null;
    }
}

[System.Serializable]
public class LineSettings
{
    public float radius = 0.25f;
    public int vertexPerLoop = 8;
    public int cornerLoops = 3;
}