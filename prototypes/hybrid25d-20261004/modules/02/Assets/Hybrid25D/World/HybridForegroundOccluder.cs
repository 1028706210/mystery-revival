using System;
using UnityEngine;

namespace MysteryRevival.Hybrid25D
{
    /// <summary>Hides one foreground visual when it obstructs the target's centre.</summary>
    public sealed class HybridForegroundOccluder : MonoBehaviour
    {
        public Transform target;

        [SerializeField] private Camera viewCamera;
        [SerializeField] private Renderer visual;
        [SerializeField] private BoxCollider obstruction;
        [SerializeField] private float targetHeight = 0.85f;
        private bool originalVisibility = true;

        public bool IsHidden { get; private set; }

        internal void Initialize(Camera camera, Renderer renderer, BoxCollider collider)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            if (renderer == null) throw new ArgumentNullException("renderer");
            if (collider == null) throw new ArgumentNullException("collider");
            viewCamera = camera;
            visual = renderer;
            obstruction = collider;
            originalVisibility = visual.enabled;
        }

        private void LateUpdate()
        {
            RefreshVisibility();
        }

        // Also lets 06 exercise the same path deterministically after positioning the actor.
        public void RefreshVisibility()
        {
            bool hidden = false;
            if (target != null && viewCamera != null && obstruction != null)
            {
                var point = target.position + Vector3.up * targetHeight;
                var viewport = viewCamera.WorldToViewportPoint(point);
                if (viewport.z > 0f)
                {
                    // An orthographic ray must start at this viewport point, not at the camera centre.
                    var ray = viewCamera.ViewportPointToRay(viewport);
                    float distance = Vector3.Dot(point - ray.origin, ray.direction);
                    RaycastHit hit;
                    hidden = distance > 0f && obstruction.Raycast(ray, out hit, distance);
                }
            }
            SetHidden(hidden);
        }

        private void OnDisable()
        {
            SetHidden(false);
        }

        private void SetHidden(bool hidden)
        {
            IsHidden = hidden;
            if (visual != null) visual.enabled = originalVisibility && !hidden;
            // The wall collider stays enabled even while its visual is hidden.
        }
    }
}
