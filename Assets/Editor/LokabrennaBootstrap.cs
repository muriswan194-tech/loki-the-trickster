using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class LokabrennaBootstrap {
 const string MainScene="Assets/Scenes/Lokabrenna.unity";
 const string EpilogueScene="Assets/Scenes/LokabrennaEpilogue.unity";
 [InitializeOnLoadMethod] static void Delayed(){EditorApplication.delayCall+=EnsureScenes;}
 public static void EnsureScenes(){
  if(Application.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  bool made=AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScene)==null;
  if(made){var s=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);new GameObject("Lokabrenna · Game").AddComponent<LokabrennaGame>();EditorSceneManager.SaveScene(s,MainScene);}
  if(AssetDatabase.LoadAssetAtPath<SceneAsset>(EpilogueScene)==null){var s=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);new GameObject("Lokabrenna · Epilogue").AddComponent<LokabrennaEpilogue>();EditorSceneManager.SaveScene(s,EpilogueScene);}
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(MainScene,true),new EditorBuildSettingsScene(EpilogueScene,true)};AssetDatabase.SaveAssets();AssetDatabase.Refresh();
  if(made||string.IsNullOrEmpty(SceneManager.GetActiveScene().path))EditorSceneManager.OpenScene(MainScene,OpenSceneMode.Single);
 }
 public static void StartPlay(){if(!Application.isPlaying){EditorApplication.isPlaying=true;}}
}
