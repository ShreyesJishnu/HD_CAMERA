using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Dalak.LineRenderer3D
{
    [System.Serializable]
    public class PipeMeshSettings
    {
        [Tooltip("radius of the pipe")]
        public float radius = 0.25f;

        [Tooltip("By increasing number of vertex per loop, smoother pipe is generated. However it affects performance")]
        [Min(3)]public int nVertexPerLoop = 10;
        [Tooltip("By increasing number of corner loops, smoother corners are generated. However it affects performance")]
        [Min(0)]public int nCornerLoops = 1;

        [Header("Decrease value if uvs have problems")]
        public float lengthToUV = 0.01f;
        [Header("Loop angle offset - Only works with flat pipes")]
        public float angleOffset = 0;
    }
}