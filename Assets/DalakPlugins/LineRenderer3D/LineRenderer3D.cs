using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Dalak.LineRenderer3D
{
    public class LineRenderer3D : MonoBehaviour
    {
        public MeshFilter meshFilter = null;
        Mesh mesh;

        MeshData meshData = new MeshData();
        PipeMeshGenerator smoothPipeGenerator = new PipeMeshGenerator();
        PipeMeshGeneratorFlat flatPipeGenerator = new PipeMeshGeneratorFlat();

        
        public enum CapType
        {
            Smooth = 0,
            Flat = 1
        }

        public enum PipeType
        {
            Smooth = 0,
            Flat = 1
        }

        public enum ColliderType
        {
            None,
            CapsuleCollider,
            MeshCollider
        }
        [Tooltip("The line will be created by given path data")]
        public PathData pathData;
        public PipeMeshSettings pipeMeshSettings;
        [Tooltip("Affects how meshes are created")]
        public PipeType pipeType = PipeType.Flat;
        [Header("Smooth cap does not work with flat pipe")]
        [Tooltip("Affects how meshes are created for the caps(beginning and end of the line)")]
        public CapType capType = CapType.Flat;
        public ColliderType colliderType = ColliderType.None;
        
        [Tooltip("Creates the 3d line on Awake according to the given path data and mesh settings")]
        public bool generateOnAwake = true;
        [Header("Check this if you are going to update line frequently")]
        [Tooltip("To get better performance in case path data positions are changed and mesh updated frequently")]
        public bool markDynamic = false;

        CapsuleColliderGenerator capsuleColliderGenerator = new CapsuleColliderGenerator();

        void Awake()
        {
#if UNITY_EDITOR
            if (meshFilter == null)
            {
                Debug.LogError("[LineRenderer3D] Please assign a mesh filter");
            }
            else
            {
                var meshRenderer = meshFilter.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    Debug.LogError("[LineRenderer3D] Please add MeshRenderer component  ");
                }
                else
                {
                    if (meshRenderer.sharedMaterial == null)
                    {
                        Debug.LogError("[LineRenderer3D] Please assign a material to the MeshRenderer");
                    }
                }
            }
            
#endif
            if (generateOnAwake)
            {
                UpdateMesh();
            }
        }

        /// <summary>
        /// Generates meshes according to path data, mesh data and pipe mesh settings
        /// </summary>
        public void UpdateMesh()
        {
            if (pathData.positions.Count <= 1)
            {
                return;
            }
            
            
            if (mesh == null)
            {
                mesh = new Mesh();
                meshFilter.sharedMesh = mesh;
                if (markDynamic)
                {
                    mesh.MarkDynamic();
                }
            }
            
            pathData.UpdateData();

            switch (pipeType)
            {
                case PipeType.Flat:
                    flatPipeGenerator.CreatePipeFlatEnded(pathData, meshData, pipeMeshSettings);
                    break;
                case PipeType.Smooth:
                    switch (capType)
                    {
                        case CapType.Flat:
                            smoothPipeGenerator.CreatePipeFlatEnded(pathData, meshData, pipeMeshSettings);
                            break;
                        case CapType.Smooth:
                            smoothPipeGenerator.CreateMeshSmoothEnded(pathData, meshData, pipeMeshSettings);
                            break;
                        default:
                            Debug.LogError("Not implemented");
                            break;
                    }
                    break;

                default:
                    Debug.LogError("Not implemented");
                    break;
            }

            mesh.Clear();
            mesh.vertices = meshData.vertices;
            mesh.uv = meshData.uvs;
            mesh.triangles = meshData.triangles;
            mesh.normals = meshData.normals;
                
            mesh.RecalculateBounds();

            switch (colliderType)
            {
                case ColliderType.CapsuleCollider:
                    capsuleColliderGenerator.UpdateCollider(transform, pathData, pipeMeshSettings);
                    break;
                case ColliderType.MeshCollider:
                    capsuleColliderGenerator.DisableCollider();
                    var meshCollider = GetComponent<MeshCollider>();
                    if (meshCollider == null)
                    {
                        meshCollider = gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = mesh;
                    }
                    break;
                case ColliderType.None:
                    break;
                default:
                    Debug.LogError("Not implemented");
                    break;
            }
        }


        void OnDrawGizmos()
        {
            if (pathData?.positions == null)
            {
                return;
            }

            Gizmos.color = Color.magenta;
            for (int i = 0; i < pathData.positions.Count - 1; i++)
            {
                Gizmos.DrawLine(transform.TransformPoint(pathData.positions[i]), transform.TransformPoint(pathData.positions[i + 1]));
            }

            foreach (var pos in pathData.positions)
            {
                Gizmos.DrawSphere(transform.TransformPoint(pos), 0.05f);
            }
        }
    }
}