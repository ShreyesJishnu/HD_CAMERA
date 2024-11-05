using System.Collections.Generic;
using UnityEngine;

namespace Dalak.LineRenderer3D
{
    /// <summary>
    /// An example of setting path data positions of lines.
    /// By setting positions of path data and pipe mesh settings on update,
    /// calling UpdateMesh after, an active pipe can be achieved
    /// </summary>
    public class Examples : MonoBehaviour
    {
        //PipeMeshSettings
        public float radius = 0.25f;
        public float circleRadius = 2;
        [Min(3)]public int nVertexPerLoop = 10;
        public int jigglePointCount = 10;
        [Min(0)]public int nCornerPerLoop = 1;

        public LineRenderer3D[] zigzagLines;
        public LineRenderer3D[] circleLines;
        public LineRenderer3D[] jiggleLines;
        public LineRenderer3D[] straightLines;
        public LineRenderer3D[] sharpLines;
        
        List<LineRenderer3D> lines = new List<LineRenderer3D>();
        
        readonly Vector3[] zigZagPositions = new[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(1,0,1),
            new Vector3(1,0,2),
            new Vector3(0,0,2),
            new Vector3(0,1,2),
            new Vector3(0,1,3),
            new Vector3(0,0,3),
            new Vector3(0,0,4),
            new Vector3(0,0,5),
        };
        
        readonly Vector3[] sharpPositions = new[]
        {
            new Vector3(-0.75f,0,0),
            new Vector3(-1f,0,10),
            new Vector3(1.75f,0,0),
            new Vector3(1.25f,0,10),
            new Vector3(2,0,10),
            new Vector3(3,0,0),
            new Vector3(3f,10,0),
        };

        readonly Vector3[] straightPositions = new[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(0,0,2),
            new Vector3(0,0,3),
            new Vector3(0,0,4),
            new Vector3(0,0,5),
        };


        void Awake()
        {
            foreach (var line in zigzagLines)
            {
                line.pathData.positions = new List<Vector3>(zigZagPositions);
                lines.Add(line);
            }

            foreach (var line in sharpLines)
            {
                line.pathData.positions = new List<Vector3>(sharpPositions);
                lines.Add(line);
            }
            
            foreach (var line in circleLines)
            {
                line.pathData.positions = new List<Vector3>(Circle(Vector3.zero, 20,circleRadius));
                lines.Add(line);
            }
            
            foreach (var line in straightLines)
            {
                line.pathData.positions = new List<Vector3>(straightPositions);
                lines.Add(line);
            }

            foreach (var line in jiggleLines)
            {
                lines.Add(line);
            }
        }


        void Update()
        {
            foreach (var line in jiggleLines)
            {
                line.pathData.positions.Clear();
                Vector3 jiggleStart = Vector3.zero;
                Vector3 jiggleEnd = Vector3.forward * 15;
                
                const float distance = 5;

                float d = Mathf.Sin(Time.time) * distance;
                Vector3 jiggleStartHandle = Vector3.Lerp(jiggleStart, jiggleEnd, 0.25f) + Vector3.up * d;
                Vector3 jiggleEndHandle = Vector3.Lerp(jiggleStart, jiggleEnd, 0.75f) + Vector3.down * d;
            
                for (int i = 0; i < jigglePointCount; i++)
                {
                    float t = (float)i / (jigglePointCount - 1);
                    line.pathData.positions.Add(MathUtils.CalculateBezier(jiggleStart, jiggleStartHandle, jiggleEnd, jiggleEndHandle,t));
                }

            }
            
            foreach (var line in lines)
            {
                //override pipeMeshSettings of each 3d line
                line.pipeMeshSettings.radius = radius;
                line.pipeMeshSettings.nVertexPerLoop = nVertexPerLoop;
                line.pipeMeshSettings.nCornerLoops = nCornerPerLoop;
                
                line.UpdateMesh();
            }

        }

        static Vector3[] Circle(Vector3 center, int nPoints, float radius)
        {
            Quaternion rot = Quaternion.Euler(0, 0, 360.0f / (nPoints - 1));
            Vector3 rad = Vector2.up * radius; 

            Vector3[] points = new Vector3[nPoints];
            for (int i = 0; i < nPoints; i++)
            {
                Vector3 pos = center + rad;
                rad = rot * rad;
                points[i] = pos;
            }
            return points;
        }
    }

}