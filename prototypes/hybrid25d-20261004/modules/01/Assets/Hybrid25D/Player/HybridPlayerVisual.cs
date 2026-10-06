using UnityEngine;

namespace MysteryRevival.Hybrid25D
{
    public sealed class HybridPlayerVisual : MonoBehaviour
    {
        private Camera view;

        internal void Initialize(Camera camera)
        {
            view = camera;
            FaceView();
        }

        private void LateUpdate()
        {
            FaceView();
        }

        private void FaceView()
        {
            if (view != null)
                transform.rotation = view.transform.rotation;
        }
    }
}
