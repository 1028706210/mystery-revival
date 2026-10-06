using System;
using UnityEngine;

namespace MysteryRevival.Hybrid25D
{
    /// <summary>The P1 door opens by hiding its leaf and disabling its blocker.</summary>
    public sealed class HybridDoor : MonoBehaviour
    {
        public Transform interactor;

        private const float InteractionRadius = 1.6f;
        [SerializeField] private Renderer leaf;
        [SerializeField] private BoxCollider blocker;
        [SerializeField] private bool isOpen;

        public bool IsOpen { get { return isOpen; } }

        internal void Initialize(Renderer leafRenderer, BoxCollider leafCollider)
        {
            if (leafRenderer == null) throw new ArgumentNullException("leafRenderer");
            if (leafCollider == null) throw new ArgumentNullException("leafCollider");
            leaf = leafRenderer;
            blocker = leafCollider;
            SetOpen(false);
        }

        public void SetOpen(bool open)
        {
            isOpen = open;
            ApplyState();
        }

        public bool TryInteract()
        {
            if (interactor == null) return false;
            var offset = interactor.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > InteractionRadius * InteractionRadius) return false;
            SetOpen(!IsOpen);
            return true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        }

        private void OnEnable()
        {
            ApplyState();
        }

        private void ApplyState()
        {
            if (leaf != null) leaf.enabled = !isOpen;
            if (blocker != null) blocker.enabled = !isOpen;
        }
    }
}
