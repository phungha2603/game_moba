using Project.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.EditorTools
{
    /// Headless verification that Bootstrap.unity actually opens in a real Unity Editor
    /// and GlobalLoader is present, since this project is normally edited with no Editor available.
    public static class SceneSmokeTest
    {
        private const string BootstrapScenePath = "Assets/_Project/Scenes/Bootstrap.unity";

        public static void OpenAndValidateBootstrapScene()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError($"SceneSmokeTest: FAILED to load '{BootstrapScenePath}'.");
                EditorApplication.Exit(1);
                return;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            GlobalLoader loader = Object.FindAnyObjectByType<GlobalLoader>();

            Debug.Log($"SceneSmokeTest: OK. Scene='{scene.name}' rootObjects={roots.Length} " +
                      $"GlobalLoaderFound={loader != null}");
            EditorApplication.Exit(0);
        }
    }
}
