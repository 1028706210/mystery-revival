using UnityEngine;

namespace MysteryRevival.Hybrid25D
{
    /// <summary>Creates the P1 greybox in metres. The caller owns the returned root.</summary>
    public static class HybridWorldFactory
    {
        public static GameObject Create(Transform parent)
        {
            var root = new GameObject("HybridWorld");
            root.transform.SetParent(parent, false);

            CreateCube(root.transform, "Ground", new Vector3(0f, -0.1f, 0f),
                new Vector3(12f, 0.2f, 10f), new Color(0.31f, 0.34f, 0.32f));

            // Two instances of the same 4 x 3 x 0.2 m wall leave a 1.2 m gap.
            CreateWall(root.transform, "WallLeft", new Vector3(-2.6f, 1.5f, 1.5f));
            CreateWall(root.transform, "WallRight", new Vector3(2.6f, 1.5f, 1.5f));

            var doorRoot = new GameObject("Door");
            doorRoot.transform.SetParent(root.transform, false);
            doorRoot.transform.localPosition = new Vector3(0f, 0f, 1.5f);
            var frameColor = new Color(0.52f, 0.49f, 0.42f);
            CreateCube(doorRoot.transform, "FrameLeft", new Vector3(-0.675f, 1.1f, 0f),
                new Vector3(0.15f, 2.2f, 0.24f), frameColor);
            CreateCube(doorRoot.transform, "FrameRight", new Vector3(0.675f, 1.1f, 0f),
                new Vector3(0.15f, 2.2f, 0.24f), frameColor);
            CreateCube(doorRoot.transform, "FrameTop", new Vector3(0f, 2.3f, 0f),
                new Vector3(1.5f, 0.2f, 0.24f), frameColor);
            CreateCube(doorRoot.transform, "WallAboveDoor", new Vector3(0f, 2.7f, 0f),
                new Vector3(1.2f, 0.6f, 0.2f), new Color(0.62f, 0.61f, 0.55f));
            var leaf = CreateCube(doorRoot.transform, "DoorLeaf", new Vector3(0f, 1.1f, 0f),
                new Vector3(1.2f, 2.2f, 0.18f), new Color(0.25f, 0.17f, 0.11f));
            doorRoot.AddComponent<HybridDoor>().Initialize(
                leaf.GetComponent<Renderer>(), leaf.GetComponent<BoxCollider>());

            var cameraObject = new GameObject("ViewCamera");
            cameraObject.transform.SetParent(root.transform, false);
            var cameraRotation = Quaternion.Euler(35f, 45f, 0f);
            cameraObject.transform.localRotation = cameraRotation;
            cameraObject.transform.localPosition = new Vector3(0f, 0.6f, 0f)
                - cameraRotation * Vector3.forward * 14f;
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
            cameraObject.AddComponent<AudioListener>();

            var spawn = new GameObject("Spawn");
            spawn.transform.SetParent(root.transform, false);
            spawn.transform.localPosition = new Vector3(0f, 0f, -3f);

            // This short wall can be walked around at either end.
            var foreground = CreateCube(root.transform, "Foreground", new Vector3(2f, 1.25f, -1.5f),
                new Vector3(2f, 2.5f, 0.2f), new Color(0.43f, 0.48f, 0.45f));
            foreground.AddComponent<HybridForegroundOccluder>().Initialize(
                camera, foreground.GetComponent<Renderer>(), foreground.GetComponent<BoxCollider>());

            var lightObject = new GameObject("ViewLight");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(50f, -25f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;

            return root;
        }

        private static GameObject CreateWall(Transform parent, string name, Vector3 position)
        {
            return CreateCube(parent, name, position, new Vector3(4f, 3f, 0.2f),
                new Color(0.62f, 0.61f, 0.55f));
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position,
            Vector3 size, Color color)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = size;
            // Reuse the primitive material; no per-rebuild material lifetime to manage.
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", color);
            cube.GetComponent<Renderer>().SetPropertyBlock(properties);
            return cube;
        }
    }
}
