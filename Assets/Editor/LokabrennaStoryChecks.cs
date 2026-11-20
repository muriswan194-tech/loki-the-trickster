using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class LokabrennaStoryChecks {
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    public static void Run(){
        var district=new DistrictDataLoader().Load();var encounters=EncounterRegistry.LoadAll(district);
        Check(encounters.Count==1,"Exactly one data-driven playable trouble is registered");
        Check(encounters[0].id=="odin-apple-lockdown"&&encounters[0].guestCharacterId!="deadpool","Playable trouble is the apple lockdown; Deadpool is reserved for the ending");
        Check(district.triggers.Count==1&&district.triggers[0].id==encounters[0].triggerId,"One courtyard trigger starts the single trouble");
        Check(district.bounds.maxX-district.bounds.minX>=30&&district.bounds.minZ<=-11,"The district has expanded into a larger wanderable area");
        Check(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Lokabrenna.unity")!=null&&AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/LokabrennaEpilogue.unity")!=null,"Opening and finale scenes exist");

        EnsureCameraAndLight();
        GameObject host=new GameObject("Story-flow verification");var game=host.AddComponent<LokabrennaGame>();Invoke(game,"Start");
        Check(Read(game,"screen").ToString()=="Menu","Game opens at the main menu");
        Invoke(game,"StartGame",false);Check(Read(game,"screen").ToString()=="Playing","New Game enters the playable courtyard");
        var player=(Transform)Read(game,"player");var crisis=district.triggers[0];player.position=new Vector3(crisis.x,1,crisis.z);Invoke(game,"TryStartEncounter");
        Check((bool)Read(game,"encounterStarted")&&(bool)Read(game,"alarmRaised"),"Apple-search trouble starts and locks the Bifrost");
        Set(game,"introPlaying",false);
        var guard=(Transform)Read(game,"guard");
        player.position=new Vector3(0,1,district.bifrost.z+1f);guard.position=new Vector3(12,.85f,-5);
        Invoke(game,"CheckGate");
        Check((bool)Read(game,"escaped")&&(bool)Read(game,"epilogueTransitionRequested"),"Moving the guard away provides an escape and advances to the finale");

        GameObject finaleHost=new GameObject("Finale-flow verification");var finale=finaleHost.AddComponent<LokabrennaEpilogue>();Invoke(finale,"Start");
        Check(Read(finale,"state").ToString()=="Montage","Escape opens the rapid comic montage");
        Invoke(finale,"BeginLive");Set(finale,"liveClock",3.2f);Invoke(finale,"UpdateLive");
        Check(Read(finale,"state").ToString()=="Live"&&((Transform)Read(finale,"portal")).gameObject.activeSelf&&((Transform)Read(finale,"deadpool")).gameObject.activeSelf,"The montage ends in a live 3D portal interruption with Deadpool");
        Set(finale,"liveClock",9.6f);Invoke(finale,"UpdateLive");
        Check(Read(finale,"state").ToString()=="End","The live joke resolves to a terminal end card, not another mission");
        Debug.Log("Lokabrenna story-flow checks passed: menu, single trouble, escape, montage, live cameo, terminal ending.");
        Object.DestroyImmediate(host);Object.DestroyImmediate(finaleHost);EditorApplication.Exit(0);
    }

    static void EnsureCameraAndLight(){
        if(Camera.main==null){var c=new GameObject("Main Camera");c.tag="MainCamera";c.AddComponent<Camera>();}
        if(Object.FindObjectOfType<Light>()==null)new GameObject("Directional Light").AddComponent<Light>();
    }
    static object Read(object target,string name){return target.GetType().GetField(name,Private).GetValue(target);}
    static void Set(object target,string name,object value){target.GetType().GetField(name,Private).SetValue(target,value);}
    static object Invoke(object target,string name,params object[] args){return target.GetType().GetMethod(name,Private).Invoke(target,args);}
    static void Check(bool value,string description){if(!value)throw new System.Exception("Story-flow check failed: "+description);Debug.Log("PASS: "+description);}
}
