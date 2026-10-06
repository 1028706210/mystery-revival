using UnityEngine;

namespace MysteryRevival.Hybrid25D
{
    internal static class HybridPlayerMovement
    {
        internal static Vector3 GroundVelocity(Vector2 input, Vector3 cameraForward, Vector3 cameraRight, float speed)
        {
            if (!Finite(input.x) || !Finite(input.y) || !Finite(speed) || speed <= 0)
                return Vector3.zero;

            input = Vector2.ClampMagnitude(input, 1);
            Vector3 forward = new Vector3(cameraForward.x, 0, cameraForward.z);
            if (forward.sqrMagnitude < 0.000001f)
            {
                Vector3 projectedRight = new Vector3(cameraRight.x, 0, cameraRight.z);
                forward = Vector3.Cross(projectedRight, Vector3.up);
            }
            if (forward.sqrMagnitude < 0.000001f)
                forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            return (right * input.x + forward * input.y) * speed;
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
