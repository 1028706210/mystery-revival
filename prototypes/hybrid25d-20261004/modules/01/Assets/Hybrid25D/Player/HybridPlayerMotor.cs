using UnityEngine;

namespace MysteryRevival.Hybrid25D
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class HybridPlayerMotor : MonoBehaviour
    {
        public bool InputEnabled = true;
        public float MoveSpeed = 3;

        private CharacterController controller;
        private Camera view;
        private float verticalVelocity;

        internal void Initialize(Camera camera)
        {
            controller = GetComponent<CharacterController>();
            view = camera;
        }

        private void Update()
        {
            var input = new Vector2(
                (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
                (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));
            Step(input, Time.deltaTime);
        }

        public void Step(Vector2 input, float deltaTime)
        {
            if (!InputEnabled)
            {
                verticalVelocity = 0;
                return;
            }
            if (view == null || controller == null || !controller.enabled ||
                deltaTime <= 0 || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                return;

            Vector3 velocity = HybridPlayerMovement.GroundVelocity(input, view.transform.forward,
                view.transform.right, MoveSpeed);
            if (velocity.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);

            if (controller.isGrounded && verticalVelocity < 0)
                verticalVelocity = -2;
            else
                verticalVelocity += Physics.gravity.y * deltaTime;

            velocity.y = verticalVelocity;
            CollisionFlags flags = controller.Move(velocity * deltaTime);
            if ((flags & CollisionFlags.Below) != 0 && verticalVelocity < 0)
                verticalVelocity = -2;
        }
    }
}
