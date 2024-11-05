using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dalak.LineRenderer3D
{
    [System.Serializable]
    public class PathData
    {
        public List<Vector3> positions;
        [NonSerialized] public Vector3[] forwards;
        [NonSerialized] public Quaternion[] loopRotations;
        [NonSerialized] public float[] lengths;
        [NonSerialized] public float totalLength;


        public void UpdateData()
        {
            if (positions.Count < 2)
            {
                return;
            }

            int nSegment = positions.Count - 1;
            
            if (forwards == null || forwards.Length != nSegment)
            {
                forwards = new Vector3[nSegment];
                loopRotations = new Quaternion[nSegment];
                lengths = new float[nSegment];
            }


            Vector3 forward = positions[1] - positions[0];
            float length = forward.magnitude;
            forward /= length;
            Vector3 up = MathUtils.GetOrthogonal(forward);

            lengths[0] = length;
            forwards[0] = forward;
            if (forward.sqrMagnitude > Mathf.Epsilon)
            {
                loopRotations[0] = Quaternion.LookRotation(forward, up);
            }
            totalLength = length;
            
            for (int segmentIdx = 1; segmentIdx < positions.Count - 1; segmentIdx++)
            {
                Vector3 newForward = positions[segmentIdx + 1] - positions[segmentIdx];
                length = newForward.magnitude;
                newForward /= length;
                up = Quaternion.FromToRotation(forward, newForward) * up;
                forward = newForward;

                totalLength += length;

                lengths[segmentIdx] = length;
                forwards[segmentIdx] = forward;
                if(forward.sqrMagnitude>Mathf.Epsilon)
                    loopRotations[segmentIdx] = Quaternion.LookRotation(forward, up);
            }
            
        }
    }
    
}