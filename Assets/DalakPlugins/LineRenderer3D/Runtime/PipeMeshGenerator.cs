using UnityEngine;

namespace Dalak.LineRenderer3D
{
    public class PipeMeshGenerator
    {
        struct DirectionInfo
        {
            public Vector3 dir;
            public float t;
        }

        DirectionInfo[] directionLookUp;
        
        /// <summary>
        /// Generates smooth faces and smooth caps 
        /// </summary>
        public void CreateMeshSmoothEnded(PathData pathData, MeshData meshData,  PipeMeshSettings pipeMeshSettings)
        {
            const int nEndLoops = 4;

            int nMidLoops = (pathData.positions.Count - 2) * pipeMeshSettings.nCornerLoops;
            int nLoops = nMidLoops + 2; // + 2 end loops
            int nLoopVertices = nLoops * pipeMeshSettings.nVertexPerLoop;
            int nLoopTriangles = (nLoops - 1) * pipeMeshSettings.nVertexPerLoop * 2 * 3;

            // Add end caps

            int nVertices = nLoopVertices + (nEndLoops * pipeMeshSettings.nVertexPerLoop) * 2; // (loopVertices + midpoint) * 2
            int nTriangles = nLoopTriangles + (nEndLoops * pipeMeshSettings.nVertexPerLoop * 2 * 3) * 2; // 


            if (meshData.vertices == null || nVertices != meshData.vertices.Length 
                                          || nTriangles != meshData.triangles.Length)
            {
                meshData.vertices = new Vector3[nVertices];
                meshData.normals = new Vector3[nVertices];
                meshData.uvs = new Vector2[nVertices];
                meshData.triangles = new int[nTriangles];
                directionLookUp = new DirectionInfo[pipeMeshSettings.nVertexPerLoop];
                
                for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                {
                    float t = (float) s / (pipeMeshSettings.nVertexPerLoop - 1);
                    float angle = t * Mathf.PI * 2;
                    directionLookUp[s] = new DirectionInfo
                    {
                        dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0),
                        t = t
                    };
                }
            }

            int tIdx = 0;
            AddPipeLoopTriangles(meshData.triangles, ref tIdx,nLoops,pipeMeshSettings.nVertexPerLoop);
            AddSmoothCapTriangles(meshData, nLoopVertices, pipeMeshSettings.nVertexPerLoop, ref tIdx, nEndLoops);
            
            Debug.Assert(tIdx == meshData.triangles.Length, tIdx + ", " + meshData.triangles.Length);
            

            int vIdx = 0;
            float capLength = Mathf.PI * pipeMeshSettings.radius * 0.5f;
            
            if (pipeMeshSettings.nCornerLoops == 1)
            {
                AddPipeLoopsSingleCorner(pathData,meshData,pipeMeshSettings,directionLookUp,capLength,ref vIdx);
            }
            else
            {
                AddPipeLoops(pathData, meshData, pipeMeshSettings, directionLookUp,capLength, ref vIdx);
            }
            
            Debug.Assert(vIdx == nLoopVertices);


            AddSmoothCapVertices(pathData,meshData,pipeMeshSettings,directionLookUp,nEndLoops,ref vIdx);

            Debug.Assert(vIdx == nVertices, vIdx + ", " + nVertices);

        }
        /// <summary>
        /// Generates smooth faces, but flat caps 
        /// </summary>
        public void CreatePipeFlatEnded(PathData pathData, MeshData meshData, PipeMeshSettings pipeMeshSettings)
        {
            int nMidLoops = (pathData.positions.Count - 2) * pipeMeshSettings.nCornerLoops;
            int nLoops = nMidLoops + 2; // + 2 end loops
            int nLoopVertices = nLoops * pipeMeshSettings.nVertexPerLoop;
            int nLoopTriangles = (nLoops - 1) * pipeMeshSettings.nVertexPerLoop * 2 * 3;


            // Add end caps
            int nVertices = nLoopVertices + (pipeMeshSettings.nVertexPerLoop + 1) * 2;
            int nTriangles = nLoopTriangles + (pipeMeshSettings.nVertexPerLoop * 3) * 2;


            if (meshData.vertices == null || nVertices != meshData.vertices.Length)
            {
                meshData.vertices = new Vector3[nVertices];
                meshData.normals = new Vector3[nVertices];
                meshData.uvs = new Vector2[nVertices];
                meshData.triangles = new int[nTriangles];
                directionLookUp = new DirectionInfo[pipeMeshSettings.nVertexPerLoop];
                
                for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                {
                    float t = (float) s / (pipeMeshSettings.nVertexPerLoop - 1);
                    float angle = t * Mathf.PI * 2;
                    directionLookUp[s] = new DirectionInfo
                    {
                        dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0),
                        t = t
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
                int v1Start = segmentIdx * vertexPerLoop;
                int v2Start = (segmentIdx + 1) * vertexPerLoop;

                for (int s = 0; s < vertexPerLoop; s++)
                {
                    int ss = (s + 1) % vertexPerLoop;

                    triangles[tIdx++] = v1Start + s;
                    triangles[tIdx++] = v1Start + ss;
                    triangles[tIdx++] = v2Start + s;

                    triangles[tIdx++] = v1Start + ss;
                    triangles[tIdx++] = v2Start + ss;
                    triangles[tIdx++] = v2Start + s;
                }
            }
        }

        static float SmoothStop2(float normalizedTime)
        {
            float t = 1 - normalizedTime;
            return 1 - t * t * t;
        }
        
        static void AddSmoothCapVertices(PathData pathData, MeshData meshData,PipeMeshSettings pipeMeshSettings, DirectionInfo[] directionLookUp, int nEndLoops,ref int vIdx)
        {
            float totalArchLength = Mathf.PI * pipeMeshSettings.radius * 0.5f;
            float stepT = 1.0f / nEndLoops;

            {
                Quaternion rot = pathData.loopRotations[0];
                Vector3 sphereCenter = pathData.positions[0];
                Vector3 outDir = -pathData.forwards[0];

                for (int loopIdx = 0; loopIdx < nEndLoops; loopIdx++)
                {
                    float tt = SmoothStop2(stepT + loopIdx * stepT);
                    float distanceToCenter = tt * pipeMeshSettings.radius;
                    
                    float archLength = Mathf.Asin(distanceToCenter / pipeMeshSettings.radius) * pipeMeshSettings.radius;
                    float loopRadius = Mathf.Sqrt(pipeMeshSettings.radius * pipeMeshSettings.radius - distanceToCenter * distanceToCenter);
                    float uvY = (totalArchLength - archLength) * pipeMeshSettings.lengthToUV;

                    Vector3 loopCenter = sphereCenter + distanceToCenter * outDir;
                    
                    for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                    {
                        ref var directionInfo = ref directionLookUp[s];
                    
                        float t = 1 - directionInfo.t;
                        Vector3 dir = rot * directionInfo.dir;
                        Vector3 pp = loopCenter + dir * loopRadius;

                        meshData.uvs[vIdx] = new Vector2(t, uvY);
                        meshData.normals[vIdx] = (pp - sphereCenter);
                        meshData.vertices[vIdx++] = pp;
                    }
                }
            }
            
            
            {
                Quaternion rot = pathData.loopRotations[pathData.loopRotations.Length - 1];
                Vector3 sphereCenter = pathData.positions[pathData.positions.Count - 1];
                Vector3 outDir = pathData.forwards[pathData.forwards.Length - 1];
            
                for (int loopIdx = 0; loopIdx < nEndLoops; loopIdx++)
                {
                    float tt = SmoothStop2(stepT + loopIdx * stepT);
                    float distanceToCenter = tt * pipeMeshSettings.radius;
                    
                    float archLength = Mathf.Asin(distanceToCenter / pipeMeshSettings.radius) * pipeMeshSettings.radius;
                    float loopRadius = Mathf.Sqrt(pipeMeshSettings.radius * pipeMeshSettings.radius - distanceToCenter * distanceToCenter);
                    float uvY = (totalArchLength + pathData.totalLength + archLength) * pipeMeshSettings.lengthToUV;
                    
                    Vector3 loopCenter = sphereCenter + distanceToCenter * outDir;
                    
                    for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                    {
                        ref var directionInfo = ref directionLookUp[s];
                    
                        float t = 1 - directionInfo.t;
                        Vector3 dir = rot * directionInfo.dir;
                        Vector3 pp = loopCenter + dir * loopRadius;

                        meshData.uvs[vIdx] = new Vector2(t, uvY);
                        meshData.normals[vIdx] = (pp - sphereCenter);
                        meshData.vertices[vIdx++] = pp;
                    }
                }
            }
        }

        static void AddSmoothCapTriangles(MeshData meshData, int nLoopVertices, int nVertexPerLoop, ref int tIdx, int nEndLoops)
        {
            {
                // Start Cap

                {
                    // Mid loop to cap connection
                    int v1Start = 0;
                    int v2Start = nLoopVertices;
            
                    for (int s = 0; s < nVertexPerLoop; s++)
                    {
                        int ss = (s + 1) % nVertexPerLoop;
            
                        meshData.triangles[tIdx++] = v1Start + s;
                        meshData.triangles[tIdx++] = v2Start + s;
                        meshData.triangles[tIdx++] = v1Start + ss;
            
                        meshData.triangles[tIdx++] = v1Start + ss;
                        meshData.triangles[tIdx++] = v2Start + s;
                        meshData.triangles[tIdx++] = v2Start + ss;
                    }
                }
                
                for (int segmentIdx = 0; segmentIdx < nEndLoops - 1; segmentIdx++)
                {
                    int v1Start = nLoopVertices + segmentIdx * nVertexPerLoop;
                    int v2Start = nLoopVertices + (segmentIdx + 1) * nVertexPerLoop;
            
                    for (int s = 0; s < nVertexPerLoop; s++)
                    {
                        int ss = (s + 1) % nVertexPerLoop;
            
                        meshData.triangles[tIdx++] = v1Start + s;
                        meshData.triangles[tIdx++] = v2Start + s;
                        meshData.triangles[tIdx++] = v1Start + ss;
            
                        meshData.triangles[tIdx++] = v1Start + ss;
                        meshData.triangles[tIdx++] = v2Start + s;
                        meshData.triangles[tIdx++] = v2Start + ss;
                    }
                }
            }

            {
                
                // End cap
                int offset = nLoopVertices + nEndLoops * nVertexPerLoop;

                {
                    // Mid loop to cap connection
                    int v1Start = nLoopVertices - nVertexPerLoop;
                    int v2Start = offset;
            
                    for (int s = 0; s < nVertexPerLoop; s++)
                    {
                        int ss = (s + 1) % nVertexPerLoop;
            
                        meshData.triangles[tIdx++] = v1Start + s;
                        meshData.triangles[tIdx++] = v1Start + ss;
                        meshData.triangles[tIdx++] = v2Start + s;

                        meshData.triangles[tIdx++] = v1Start + ss;
                        meshData.triangles[tIdx++] = v2Start + ss;
                        meshData.triangles[tIdx++] = v2Start + s;
                    }
                }
                
                
                
                for (int segmentIdx = 0; segmentIdx < nEndLoops - 1; segmentIdx++)
                {
                    int v1Start = offset + segmentIdx * nVertexPerLoop;
                    int v2Start = offset + (segmentIdx + 1) * nVertexPerLoop;
                
                    for (int s = 0; s < nVertexPerLoop; s++)
                    {
                        int ss = (s + 1) % nVertexPerLoop;
                
                        meshData.triangles[tIdx++] = v1Start + s;
                        meshData.triangles[tIdx++] = v1Start + ss;
                        meshData.triangles[tIdx++] = v2Start + s;

                        meshData.triangles[tIdx++] = v1Start + ss;
                        meshData.triangles[tIdx++] = v2Start + ss;
                        meshData.triangles[tIdx++] = v2Start + s;
                    }
                }
            }
        }

        static void AddPipeLoops(PathData pathData, MeshData meshData, PipeMeshSettings pipeMeshSettings, DirectionInfo[] directionLookUp,float length, ref int vertexIdx)
        {
            int vIdx = vertexIdx;

            void AddLoop(Vector3 pos, Quaternion rot)
            {
                for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                {
                    ref var directionInfo = ref directionLookUp[s];
                    
                    float t = directionInfo.t;
                    Vector3 dir = rot * directionInfo.dir;
                    Vector3 p = pos + dir * pipeMeshSettings.radius;

                    meshData.uvs[vIdx] = new Vector2(1 - t, Mathf.Repeat(length * pipeMeshSettings.lengthToUV, 1));
                    meshData.normals[vIdx] = dir;
                    meshData.vertices[vIdx++] = p;
                }
            }


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
                }
            }

            length += pathData.lengths[pathData.lengths.Length - 1] - pipeMeshSettings.radius;
            AddLoop(pathData.positions[pathData.positions.Count - 1], pathData.loopRotations[pathData.loopRotations.Length - 1]);

            vertexIdx = vIdx;
        }
        
        static void AddPipeLoopsSingleCorner(PathData pathData, MeshData meshData, PipeMeshSettings pipeMeshSettings, DirectionInfo[] directionLookUp,float length, ref int vertexIdx)
        {
            int vIdx = vertexIdx;

            void AddLoop(Vector3 pos, Quaternion rot)
            {
                for (int s = 0; s < pipeMeshSettings.nVertexPerLoop; s++)
                {
                    ref var directionInfo = ref directionLookUp[s];
                    
                    float t = directionInfo.t;
                    Vector3 dir = rot * directionInfo.dir;
                    Vector3 p = pos + dir * pipeMeshSettings.radius;

                    // DalDebug.DrawCross(p, 0.02f, Color.red,0.5f);

                    meshData.uvs[vIdx] = new Vector2(1 - t, Mathf.Repeat(length * pipeMeshSettings.lengthToUV, 1));
                    meshData.normals[vIdx] = dir;
                    meshData.vertices[vIdx++] = p;
                }
            }

            AddLoop(pathData.positions[0], pathData.loopRotations[0]);


            for (int pointIdx = 1; pointIdx < pathData.positions.Count - 1; pointIdx++)
            {
                length += pathData.lengths[pointIdx - 1];
                Vector3 pos = pathData.positions[pointIdx];
                AddLoop(pos, Quaternion.Slerp(pathData.loopRotations[pointIdx - 1], pathData.loopRotations[pointIdx], 0.5f));

            }

            {
                length += pathData.lengths[pathData.lengths.Length - 1];
                Vector3 pos = pathData.positions[pathData.positions.Count - 1];
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