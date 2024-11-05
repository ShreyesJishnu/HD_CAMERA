using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static CameraLineController;

// Menu Manager Script
public class MenuManager : MonoBehaviour
{
    private Rect menuRect = new Rect(10, 10, 420, 40);

    private void OnGUI()
    {
        menuRect = GUI.Window(0, menuRect, DrawMenuWindow, "Menu");
    }

    private void DrawMenuWindow(int windowID)
    {
        GUILayout.BeginHorizontal();

        if (CameraLineController.Instance == null)
        {
            GUILayout.Label("Camera Controller not found!");
            return;
        }

        if (GUILayout.Button("FILE", GUILayout.Width(100), GUILayout.Height(25)))
        {
            // File menu functionality
        }

        if (GUILayout.Button("PAN", GUIStyles.ButtonStyle, GUILayout.Width(100), GUILayout.Height(25)))
        {
            CameraLineController.Instance.GetComponent<CameraLineController>().SetMode(CameraMode.Pan);
        }

        if (GUILayout.Button("TRACK", GUIStyles.ButtonStyle, GUILayout.Width(100), GUILayout.Height(25)))
        {
            CameraLineController.Instance.GetComponent<CameraLineController>().SetMode(CameraMode.Track);
        }

        if (GUILayout.Button("STEADICAM", GUIStyles.ButtonStyle, GUILayout.Width(100), GUILayout.Height(25)))
        {
            CameraLineController.Instance.GetComponent<CameraLineController>().SetMode(CameraMode.Steadicam);
        }

        GUILayout.EndHorizontal();

        // Make the window draggable
        GUI.DragWindow();
    }
}
public static class GUIStyles
{
    private static GUIStyle _buttonStyle;
    public static GUIStyle ButtonStyle
    {
        get
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.fontSize = 14;
                _buttonStyle.padding = new RectOffset(10, 10, 5, 5);
                _buttonStyle.normal.textColor = Color.white;
                _buttonStyle.hover.textColor = Color.yellow;
            }
            return _buttonStyle;
        }
    }
}

