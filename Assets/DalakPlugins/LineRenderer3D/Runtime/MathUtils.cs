using UnityEngine;

namespace Dalak.LineRenderer3D
{
    public static class MathUtils
    {
        /// <summary>
        /// Calculates a point on Bezier 4-Point-Curve which has the following formula:
        /// (1−t)^3*POINT1 + 3*(1−t)^2*t*POINT2 +3*(1−t)*t^2*POINT3 + t^3*POINT4
        /// </summary>
        /// <param name="startPos"> POINT1</param>
        /// <param name="startPosHandle">POINT2</param>
        /// <param name="endPos">POINT4</param>
        /// <param name="endPosHandle">POINT3</param>
        /// <param name="t">Input value t, between 0 and 1</param>
        /// <returns>(Vector3)Point</returns>
        public static Vector3 CalculateBezier(Vector3 startPos, Vector3 startPosHandle,Vector3 endPos, Vector3 endPosHandle, float t)
        {
            return Mathf.Pow((1 - t), 3) * startPos 
                   + 3 * Mathf.Pow(1 - t, 2) * t * startPosHandle
                   + 3 * (1 - t) * t * t * endPosHandle 
                   + t * t * t * endPos;
        }
        /// <summary>
        /// Finds perpendicular Vector3 to the given Vector3
        /// </summary>
        public static Vector3 GetOrthogonal(Vector3 vec)
        {
            var v1 = new Vector3(vec.z, vec.z, -vec.x - vec.y).normalized;
            var v2 = new Vector3(-vec.y - vec.z, vec.x, vec.x).normalized;
            if (v1.magnitude > Mathf.Epsilon)
            {
                return v1;
            }
            return v2;
        }
    }
}