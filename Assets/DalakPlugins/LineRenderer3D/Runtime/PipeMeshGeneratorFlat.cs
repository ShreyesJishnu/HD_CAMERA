using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dalak.LineRenderer3D
{
    public class PipeMeshGeneratorFlat
    {
        struct DirectionInfo
        {
            public Vector3 dir;
            public Vector3 normal1;
            public Vector3 normal2;
            public float t;
        }

        DirectionInfo[] directionLookUp;
        
        /// <summary>
        /// Generates flat faces and caps 
        /// </summary>
        public void CreatePipeFlatEnded(PathData pathData, MeshData meshData, PipeMeshSettings pipeMeshSettings)
        {
            int nMidLoops = (pathData.positions.Count - 2) * pipeMeshSettings.nCornerLoops;
            
            // midLoops have 4 duplicates + 2 end loops with 2 duplicates 
            int nLoopVertices = nMidLoops * pipeMeshSettings.nVertexPerLoop * 4 + pipeMeshSettings.nVertexPerLoop * 2 * 2;

            // midLoops + 2 end loops
            int nLoops = nMidLoops + 2; 
            int nLoopTriangles = (nLoops - 1) * pipeMeshSettings.nVertexPerLoop * 2 * 3; // triangle number is same just change the indices


            // Add end caps
            int nVertices = nLoopVertices + (pipeMeshSettings.nVertexPerLoop + 1) * 2;
            int nTriangles = nLoopTriangles + (pipeMeshSettings.nVertexPerLoop * 3) * 2;


            // if (meshData.vertices == null || nVertices != meshData.vertices.Length)
            {
                meshData.vertices = new Vector3[nVertices];
                meshData.normals = new Vector3[nVertices];
                meshData.uvs = new Vector2[nVertices];
                meshData.triangles = new int[nTriangles];
                directionLookUp = new DirectionInfo[pipeMeshSettings.nVertexPerLoop];
                
                float halfStepLength = 0.5f / (pipeMeshSettings.nVertexPerLoop - 1);
                float angleOffset = pipeMeshSettings.angleOffset * Mathf.Deg2Rad;
                
                for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                {
                    float t = (float) s / (pipeMeshSettings.nVertexPerLoop - 1);
                    
                    float angle = angleOffset + t * Mathf.PI * 2;
                    float n1Angle = angleOffset + (t - halfStepLength) * Mathf.PI * 2;
                    float n2Angle = angleOffset + (t + halfStepLength) * Mathf.PI * 2; 
                    directionLookUp[s] = new DirectionInfo
                    {
                        dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0),
                        t = t,
                        normal1 = new Vector3(Mathf.Cos(n1Angle), Mathf.Sin(n1Angle), 0),
                        normal2 = new Vector3(Mathf.Cos(n2Angle), Mathf.Sin(n2Angle), 0)
                    };
                    
                }
            }

            int tIdx = 0;
            AddPipeLoopTriangles(meshData.triangles, ref tIdx,nLoops,pipeMeshSettings.nVertexPerLoop);

           
            AddFlatEndCapTriangles(meshData.triangles,ref tIdx, nLoopVertices, pipeMeshSettings.nVertexPerLoop);
            
            Debug.Assert(tIdx == meshData.triangles.Length, tIdx + ", " + meshData.triangles.Length);
            

            int vIdx = 0;
            if (pipeMeshSettings.nCornerLoops == 1)
            {
                 AddPipeLoopsSingleCorner(pathData,meshData,pipeMeshSettings,directionLookUp,0,ref vIdx);
            }
            else
            {
                AddPipeLoops(pathData,meshData,pipeMeshSettings,directionLookUp,0,ref vIdx);
            }
            
            
            
            Debug.Assert(vIdx == nLoopVertices);
            AddFlatEndCap(pathData,meshData,pipeMeshSettings,directionLookUp,nLoopVertices, ref vIdx);
            Debug.Assert(vIdx == nVertices);
        }
         
         
        static void AddPipeLoopTriangles(int[] triangles, ref int tIdx, int nLoops, int vertexPerLoop)
        {
            for (int segmentIdx = 0; segmentIdx < nLoops - 1; segmentIdx++)
            {
                int loop1Set1Start = segmentIdx * vertexPerLoop * 4; // blue - green 
                int loop1Set2Start = loop1Set1Start + vertexPerLoop; // red -  pink

                int loop2Set1Start = loop1Set2Start + vertexPerLoop; // blue 
                int loop2Set2Start = loop2Set1Start + vertexPerLoop; // red
                
                for (int s = 0; s < vertexPerLoop; s += 2)
                {
                    int ss = (s + 1) % vertexPerLoop;

                    triangles[tIdx++] = loop1Set1Start + s; 
                    triangles[tIdx++] = loop1Set1Start + ss;
                    triangles[tIdx++] = loop2Set1Start + s;

                    triangles[tIdx++] = loop1Set1Start + ss;
                    triangles[tIdx++] = loop2Set1Start + ss;
                    triangles[tIdx++] = loop2Set1Start + s;
                }
                
                for (int s = 1; s < vertexPerLoop; s += 2)
                {
                    int ss = (s + 1) % vertexPerLoop;

                    triangles[tIdx++] = loop1Set2Start + s; 
                    triangles[tIdx++] = loop1Set2Start + ss;
                    triangles[tIdx++] = loop2Set2Start + s;

                    triangles[tIdx++] = loop1Set2Start + ss;
                    triangles[tIdx++] = loop2Set2Start + ss;
                    triangles[tIdx++] = loop2Set2Start + s;
                }
            }
        }
        static void AddPipeLoops(PathData pathData, MeshData meshData, PipeMeshSettings pipeMeshSettings, DirectionInfo[] directionLookUp,float length, ref int vertexIdx)
        {
            int vIdx = vertexIdx;
            bool normalToggle = true;

            void AddLoop(Vector3 pos, Quaternion rot)
            {
                var useFirstNormal = normalToggle;
                normalToggle = !normalToggle;
                for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                {
                    ref var directionInfo = ref directionLookUp[s];
                    
                    float t = directionInfo.t;
                    Vector3 dir = rot * directionInfo.dir;
                    Vector3 normal1 = rot * directionInfo.normal1;
                    Vector3 normal2 = rot * directionInfo.normal2;
                    Vector3 p = pos + dir * pipeMeshSettings.radius;

                    useFirstNormal = !useFirstNormal;
                    
                    meshData.uvs[vIdx] = new Vector2(1 - t, Mathf.Repeat(length * pipeMeshSettings.lengthToUV, 1));
                    meshData.normals[vIdx] = useFirstNormal ? normal1 : normal2;
                    meshData.vertices[vIdx++] = p;
                }
            }


            AddLoop(pathData.positions[0], pathData.loopRotations[0]);
            AddLoop(pathData.positions[0], pathData.loopRotations[0]);
            
            float stepT = 1.0f / (pipeMeshSettings.nCornerLoops - 1);
            float stepLength = pipeMeshSettings.radius * 2 * stepT;

            length += pipeMeshSettings.radius;

            for (int pointIdx = 1; pointIdx < pathData.positions.Count - 1; pointIdx++)
            {
                Vector3 pos = pathData.positions[pointIdx];
                Vector3 toAfter = pathData.forwards[pointIdx];
                Vector3 toBefore = -pathData.forwards[pointIdx - 1];

                Vector3 p1 = pos + toBefore * pipeMeshSettings.radius;
                Vector3 p2 = pos + toAfter * pipeMeshSettings.radius;

                length += pathData.lengths[pointIdx - 1] - 2 * pipeMeshSettings.radius - stepLength;

                for (int i = 0; i < pipeMeshSettings.nCornerLoops; i++)
                {
                    float t = i * stepT;
                    Quaternion rot = Quaternion.Slerp(pathData.loopRotations[pointIdx - 1], pathData.loopRotations[pointIdx], t);
                    Vector3 p = Vector3.Lerp(p1, pos, t * 2);
                    if (t > 0.5f)
                    {
                        p = Vector3.Lerp(pos, p2, (t - 0.5f) * 2);
                    }
                 
                    length += stepLength;
                    AddLoop(p, rot);
                    AddLoop(p, rot);
                    AddLoop(p, rot);
                    AddLoop(p, rot);
                }
            }

            length += pathData.lengths[pathData.lengths.Length - 1] - pipeMeshSettings.radius;
            AddLoop(pathData.positions[pathData.positions.Count - 1], pathData.loopRotations[pathData.loopRotations.Length - 1]);
            AddLoop(pathData.positions[pathData.positions.Count - 1], pathData.loopRotations[pathData.loopRotations.Length - 1]);

            vertexIdx = vIdx;
        }
        
        static void AddPipeLoopsSingleCorner(PathData pathData, MeshData meshData, PipeMeshSettings pipeMeshSettings, DirectionInfo[] directionLookUp,float length, ref int vertexIdx)
        {
            int vIdx = vertexIdx;
            bool normalToggle = true;

            void AddLoop(Vector3 pos, Quaternion rot)
            {
                var useFirstNormal = normalToggle;
                normalToggle = !normalToggle;
                for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                {
                    ref var directionInfo = ref directionLookUp[s];
                    
                    float t = directionInfo.t;
                    
                    Vector3 dir = rot * directionInfo.dir;
                    Vector3 normal1 = rot * directionInfo.normal1;
                    Vector3 normal2 = rot * directionInfo.normal2;
                    Vector3 p = pos + dir * pipeMeshSettings.radius;

                    useFirstNormal = !useFirstNormal;
                    
                    meshData.uvs[vIdx] = new Vector2(1 - t, Mathf.Repeat(length * pipeMeshSettings.lengthToUV, 1));
                    meshData.normals[vIdx] = useFirstNormal ? normal1 : normal2;
                    meshData.vertices[vIdx++] = p;
                }
            }

            AddLoop(pathData.positions[0], pathData.loopRotations[0]);
            AddLoop(pathData.positions[0], pathData.loopRotations[0]);
            
            for (int pointIdx = 1; pointIdx < pathData.positions.Count - 1; pointIdx++)
            {
                length += pathData.lengths[pointIdx - 1];
                Vector3 pos = pathData.positions[pointIdx];
               
                AddLoop(pos, Quaternion.Slerp(pathData.loopRotations[pointIdx - 1], pathData.loopRotations[pointIdx], 0.5f));
                AddLoop(pos, Quaternion.Slerp(pathData.loopRotations[pointIdx - 1], pathData.loopRotations[pointIdx], 0.5f));
                AddLoop(pos, Quaternion.Slerp(pathData.loopRotations[pointIdx - 1], pathData.loopRotations[pointIdx], 0.5f));
                AddLoop(pos, Quaternion.Slerp(pathData.loopRotations[pointIdx - 1], pathData.loopRotations[pointIdx], 0.5f));
            }

            {
                length += pathData.lengths[pathData.lengths.Length - 1];
                Vector3 pos = pathData.positions[pathData.positions.Count - 1];
                AddLoop(pos, pathData.loopRotations[pathData.loopRotations.Length - 1]);
                AddLoop(pos, pathData.loopRotations[pathData.loopRotations.Length - 1]);
            }

            vertexIdx = vIdx;
        }
        
                
        static void AddFlatEndCapTriangles(int[] triangles, ref int tIdx, int nLoopVertices,int vertexPerLoop)
        {
            // 0,1,2,3,4 - nLoopVertices = 5
            // 5,6,7,8 - 9  nLoopVertices(5) + vertexPerLoop(4)
            // 10,11,12,13 - 14  nLoopVertices(5) + vertexPerLoop(4) * 2 + 1
            
            {
                int midPointIdx = nLoopVertices + vertexPerLoop;
                int capStartIdx = nLoopVertices;

                for (int i = 0; i < vertexPerLoop; i++)
                {
                    int ii = (i + 1) % vertexPerLoop;
                    triangles[tIdx++] = midPointIdx;
                    triangles[tIdx++] = capStartIdx + i;
                    triangles[tIdx++] = capStartIdx + ii;
                }
            }

            {
                int midPointIdx = nLoopVertices + vertexPerLoop * 2 + 1;
                int capStartIdx = nLoopVertices + vertexPerLoop + 1;

                for (int i = 0; i < vertexPerLoop; i++)
                {
                    int ii = (i + 1) % vertexPerLoop;
                    triangles[tIdx++] = midPointIdx;
                    triangles[tIdx++] = capStartIdx + ii;
                    triangles[tIdx++] = capStartIdx + i;
                }
            }
        }


        static void AddFlatEndCap(PathData pathData,MeshData meshData, PipeMeshSettings pipeMeshSettings, DirectionInfo[] directionLookUp,int nLoopVertices, ref int vIdx)
        {
            {
                int loopStartIdx = nLoopVertices - pipeMeshSettings.nVertexPerLoop;

                Vector3 normal = (pathData.positions[pathData.positions.Count - 1] - pathData.positions[pathData.positions.Count - 2]).normalized;

                for (int i = 0; i < pipeMeshSettings.nVertexPerLoop; i++)
                {
                    ref var directionInfo = ref directionLookUp[i];

                    Vector2 uv = directionInfo.dir;
                    uv = (uv + Vector2.one) * 0.5f;
                    uv.y *= pipeMeshSettings.lengthToUV;

                    meshData.vertices[vIdx] = meshData.vertices[loopStartIdx + i];
                    meshData.normals[vIdx] = normal;
                    meshData.uvs[vIdx] = uv;

                    vIdx++;
                }

                meshData.vertices[vIdx] = pathData.positions[pathData.positions.Count - 1];
                meshData.normals[vIdx] = normal;
                meshData.uvs[vIdx++] = new Vector2(0.5f, 0.5f * pipeMeshSettings.lengthToUV);
            }


            {
                int loopStartIdx = 0;
                Vector3 normal = (pathData.positions[1] - pathData.positions[0]).normalized;

                for (int i = 0; i < pipeMeshSettings.nVertexPerLoop; i++)
                {
                    ref var directionInfo = ref directionLookUp[i];

                    Vector2 uv = directionInfo.dir;
                    uv = (uv + Vector2.one) * 0.5f;
                    uv.y *= pipeMeshSettings.lengthToUV;

                    meshData.vertices[vIdx] = meshData.vertices[loopStartIdx + i];
                    meshData.normals[vIdx] = normal;
                    meshData.uvs[vIdx] = uv;

                    vIdx++;
                }

                meshData.vertices[vIdx] = pathData.positions[0];
                meshData.normals[vIdx] = normal;
                meshData.uvs[vIdx++] = new Vector2(0.5f, 0.5f * pipeMeshSettings.lengthToUV);
            }
        }
    }
}