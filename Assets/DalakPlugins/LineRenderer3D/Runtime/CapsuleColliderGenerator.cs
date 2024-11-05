using System.Collections.Generic;
using UnityEngine;

namespace Dalak.LineRenderer3D
{
    public class CapsuleColliderGenerator
    {
        public Transform colliderContainer;
        public List<CapsuleCollider> colliders = new List<CapsuleCollider>();

        /// <summary>
        ///  Updates capsule colliders considering path data and pipe mesh settings,
        ///  deactivate current ones or creates depending on the requirement
        /// </summary>
        public void UpdateCollider(Transform lineTransform,PathData pathData, PipeMeshSettings pipeMeshSettings)
        {
            if (colliderContainer == null)
            {
                colliderContainer = new GameObject("Colliders").transform;
                colliderContainer.parent = lineTransform;
                colliderContainer.localPosition = Vector3.zero;
                colliderContainer.localRotation = Quaternion.identity;
                colliderContainer.localScale = Vector3.one;
            }
            colliderContainer.gameObject.SetActive(true);

            int nColliders = pathData.positions.Count - 1;
            int nExistingCollider = colliders.Count;

            int i = 0;
            for (; i < Mathf.Min(nColliders,nExistingCollider); i++)
            {
                // Use existing colliders
                Vector3 p1 = pathData.positions[i];
                Vector3 p2 = pathData.positions[i + 1];

                Vector3 colliderPos = (p1 + p2) * 0.5f;

                var capsuleCollider = colliders[i];
                capsuleCollider.radius = pipeMeshSettings.radius;
                capsuleCollider.height = pathData.lengths[i] + pipeMeshSettings.radius;
                capsuleCollider.direction = 2;
                
                var colliderTransform = capsuleCollider.transform;
                colliderTransform.gameObject.SetActive(true);
                colliderTransform.localPosition = colliderPos;
                colliderTransform.localRotation = Quaternion.LookRotation(p2 - p1);
            }

            for (; i < nColliders; i++)
            {
                // Create new colliders
                Vector3 p1 = pathData.positions[i];
                Vector3 p2 = pathData.positions[i + 1];

                Vector3 colliderPos = (p1 + p2) * 0.5f;
                
                var colliderTransform = new GameObject().transform;
                colliderTransform.parent = colliderContainer;
                colliderTransform.localPosition = colliderPos;
                colliderTransform.localRotation = Quaternion.LookRotation(p2 - p1);

                
                var capsuleCollider = colliderTransform.gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = pipeMeshSettings.radius;
                capsuleCollider.height = pathData.lengths[i] + pipeMeshSettings.radius;
                capsuleCollider.direction = 2;
                
                colliders.Add(capsuleCollider);
            }

            for (; i < nExistingCollider; i++)
            {
                // Set active false, colliders those already created, unless necessary at current state
                colliders[i].gameObject.SetActive(false);
            }
          
        }

        public void DisableCollider()
        {
            if (colliderContainer != null)
            {
                colliderContainer.gameObject.SetActive(false);
            }
        }
        
        
    }
}
