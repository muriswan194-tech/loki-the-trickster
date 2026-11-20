using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class LokabrennaBootstrap {
    static LokabrennaBootstrap() {
        EditorApplication.delayCall += EnsureScene;
    }
    static void EnsureScene() {
        if (Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string scenePath = "Assets/Scenes/Lokabrenna.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject game = new GameObject("Lokabrenna Game");
        game.AddComponent<LokabrennaGame>();
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        Debug.Log("Lokabrenna: created and opened the playable starter scene.");
    }
}
