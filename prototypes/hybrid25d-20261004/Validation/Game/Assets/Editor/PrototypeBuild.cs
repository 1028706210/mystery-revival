using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MysteryRevival
{
    public static class PrototypeBuild
    {
        public static void SmokeEditor()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity");
            EditorApplication.isPlaying=true;
        }
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var type = typeof(PrototypeGame);
            new GameObject("Prototype").AddComponent(type);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Prototype.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Prototype.unity", true) };
            PlayerSettings.companyName = "Independent Prototype";
            PlayerSettings.productName = "神秘复苏 · 调查原型";
            PlayerSettings.defaultScreenWidth = 1366;
            PlayerSettings.defaultScreenHeight = 768;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            // The prototype uses the built-in input manager, no external input package.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            var output = Environment.GetEnvironmentVariable("MYSTERY_BUILD_PATH");
            if (string.IsNullOrEmpty(output)) output = Path.GetFullPath("../artifacts/build/MysteryRevival.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Prototype.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            Debug.Log("PROTOTYPE_BUILD_RESULT=" + report.summary.result);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("Windows build failed: " + report.summary.result);
        }
    }
}
