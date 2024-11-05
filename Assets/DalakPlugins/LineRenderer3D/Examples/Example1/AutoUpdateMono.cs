using Dalak.LineRenderer3D;
using UnityEngine;

namespace Dalak.LineRenderer3D
{
    public class AutoUpdateMono : MonoBehaviour
    {
        LineRenderer3D lineRenderer;
        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer3D>();
        }

        void Update()
        {
            lineRenderer.UpdateMesh();
        }
    }
}