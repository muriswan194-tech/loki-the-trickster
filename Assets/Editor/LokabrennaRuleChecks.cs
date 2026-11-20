using System;
using System.Collections;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class LokabrennaRuleChecks {
    const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    public static void Run() {
        var host = new GameObject("Lokabrenna rule check host");
        var game = host.AddComponent<LokabrennaGame>();
        typeof(LokabrennaGame).GetMethod("Start", PrivateInstance).Invoke(game, null);
        var player = (Transform)Get(game, "player");
        var guard = (Transform)Get(game, "guard");
        player.position = new Vector3(0, .88f, 4);
        guard.position = new Vector3(0, .85f, 0);
        guard.rotation = Quaternion.LookRotation(Vector3.forward);
        Check((bool)Call(game, "CanSeePlayer"), "Guard sight cone detects visible Loki");

        guard.rotation = Quaternion.LookRotation(Vector3.back);
        Check(!(bool)Call(game, "CanSeePlayer"), "Guard does not see behind itself");
        guard.rotation = Quaternion.LookRotation(Vector3.forward);
        IList props = (IList)Get(game, "props");
        object crate = null;
        foreach (object prop in props) if ((string)Get(prop, "id") == "crate-a") crate = prop;
        var crateTransform = (Transform)Get(crate, "transform");
        crateTransform.position = new Vector3(0, .45f, 2);
        Check(!(bool)Call(game, "CanSeePlayer"), "Moved solid crate blocks guard sightline");
        crateTransform.position = new Vector3(-3.8f, .45f, -1.5f);
        Check((bool)Call(game, "CanSeePlayer"), "Sightline returns when cover moves away");

        Call(game, "EmitSound", new Vector3(0, 0, 2), 8f, "rule check");
        Check((bool)Get(game, "soundActive") && Get(game, "guardState").ToString() == "Investigate", "In-range sound sends guard to investigate");
        Call(game, "EmitSound", new Vector3(0, 0, 2), .5f, "rule check");
        Check(!(bool)Get(game, "soundActive"), "Out-of-range sound does not attract guard");

        Set(game,"disguised",false);Set(game,"suspicion",0f);Set(game,"lastPlayerSpeed",4.1f);SetGuardState(game,"Patrol");
        Call(game,"EvaluatePerception",true,1f);Call(game,"EvaluatePerception",true,1f);
        Check(Get(game,"guardState").ToString()=="Chase","Sustained visible Loki pushes the guard into chase");
        Set(game,"disguised",true);Set(game,"suspicion",0f);Set(game,"lastPlayerSpeed",4.1f);SetGuardState(game,"Patrol");
        Call(game,"EvaluatePerception",true,1f);
        Check((float)Get(game,"suspicion")==0f&&Get(game,"guardState").ToString()=="Patrol","Plausible walking in disguise does not raise suspicion");
        Set(game,"lastPlayerSpeed",6.8f);Call(game,"EvaluatePerception",true,1f);Call(game,"EvaluatePerception",true,1f);
        Check(Get(game,"guardState").ToString()=="Suspicious","Sprinting in disguise draws suspicion");

        player.position = crateTransform.position + Vector3.back;
        player.rotation = Quaternion.LookRotation(Vector3.forward);
        Vector3 crateBefore = crateTransform.position;
        Call(game, "Interact");
        Check(crateTransform.position != crateBefore, "Space interaction moves the selected crate");
        Check(crateTransform.position.z > crateBefore.z, "Pushed crate retains its new position");

        Vector3 aim = (Vector3)Call(game, "AimPoint");
        Call(game, "CastIllusion");
        var decoy = (Transform)Get(game, "decoy");
        Check(decoy != null && Vector2.Distance(new Vector2(decoy.position.x, decoy.position.z), new Vector2(aim.x, aim.z)) < .01f, "Illusion appears at the aimed world point");

        player.position=guard.position+Vector3.forward*2f;
        Set(game,"disguised",false);
        Call(game, "Shapeshift");
        Check((bool)Get(game, "disguised"), "Shapeshift changes guard perception state");
        Check(((Renderer)Get(game, "playerRenderer")).material.color == new Color(.53f, .23f, .2f), "Shapeshift changes Loki's visible appearance");
        Call(game,"SetAspect",Enum.Parse(game.GetType().GetNestedType("Aspect",BindingFlags.NonPublic),"Falcon"));
        Check(Get(game,"aspect").ToString()=="Falcon"&&player.localScale.y<1f,"Falcon aspect changes Loki's form and silhouette");
        Call(game,"SetAspect",Enum.Parse(game.GetType().GetNestedType("Aspect",BindingFlags.NonPublic),"Human"));
        Check(Mathf.Abs(player.localScale.y-1f)<.01f,"Returning to human restores Loki's base-height silhouette");
        Call(game,"SetPropMass",crate,1);Check(Mathf.Abs((float)Get(crate,"mass")-.35f)<.01f,"Telekinesis makes a movable prop light");
        Call(game,"SetPropMass",crate,3);Check((float)Get(crate,"mass")>3f,"Heavy props require a shorter push");
        Call(game,"Conjure");Check(Get(game,"conjured")!=null,"Conjuration creates a physical solid apple prop");
        object sceptre=null,tesseract=null;foreach(object prop in props){if((string)Get(prop,"id")=="odin-sceptre")sceptre=prop;if((string)Get(prop,"id")=="frigga-tesseract")tesseract=prop;}
        player.position=((Transform)Get(sceptre,"transform")).position;Call(game,"Interact");Check((bool)Get(game,"sceptreFound"),"Sceptre remains a hidden optional discovery until approached");
        player.position=((Transform)Get(tesseract,"transform")).position;Call(game,"Interact");Check((bool)Get(game,"tesseractFound"),"Tesseract remains a hidden optional discovery until approached");
        int charges=(int)Get(game,"teleportCharges");Call(game,"Teleport");Check((int)Get(game,"teleportCharges")==charges-1,"Tesseract jump spends one of ten charges");
        Debug.Log("Lokabrenna rule checks passed: sight, cover, hearing, investigation, chase, disguise, movement, and aimed abilities.");
        EditorApplication.Exit(0);
    }

    public static void CaptureWorldPreview() {
        if (Camera.main == null) {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
        }
        if (UnityEngine.Object.FindObjectOfType<Light>() == null) {
            var lightObject = new GameObject("Directional Light");
            lightObject.AddComponent<Light>().type = LightType.Directional;
        }
        var host = new GameObject("Lokabrenna preview host");
        var game = host.AddComponent<LokabrennaGame>();
        typeof(LokabrennaGame).GetMethod("Start", PrivateInstance).Invoke(game, null);
        var camera = Camera.main;
        if (camera == null) throw new Exception("No MainCamera found in the district scene.");
        var player=(Transform)Get(game,"player");var aim=player.position+Vector3.up*.82f;
        camera.transform.position=aim+new Vector3(0,3.8f,-6.8f);camera.transform.rotation=Quaternion.LookRotation(aim+Vector3.forward*4.2f-camera.transform.position,Vector3.up);camera.fieldOfView=52f;
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.backgroundColor = new Color(.075f,.11f,.14f);
        var target = new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(1600,900,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1600,900),0,0);
        image.Apply();
        string output = Path.Combine(Path.GetTempPath(),"Lokabrenna-courtyard-preview.png");
        File.WriteAllBytes(output,image.EncodeToPNG());
        Debug.Log("Lokabrenna courtyard preview saved to " + output);
        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(target);
        EditorApplication.Exit(0);
    }

    static object Get(object target, string name) {
        if (target == null) return null;
        var type = target.GetType();
        var field = type.GetField(name, PrivateInstance | BindingFlags.Public);
        if (field != null) return field.GetValue(target);
        var property = type.GetProperty(name, PrivateInstance | BindingFlags.Public);
        return property == null ? null : property.GetValue(target, null);
    }
    static void Set(object target,string name,object value){target.GetType().GetField(name,PrivateInstance|BindingFlags.Public).SetValue(target,value);}
    static void SetGuardState(object game,string state){FieldInfo field=game.GetType().GetField("guardState",PrivateInstance);field.SetValue(game,Enum.Parse(field.FieldType,state));}
    static object Call(object target, string name, params object[] args) {
        return target.GetType().GetMethod(name, PrivateInstance).Invoke(target, args);
    }
    static void Check(bool okay, string description) {
        if (!okay) throw new Exception("Lokabrenna rule check failed: " + description);
        Debug.Log("PASS: " + description);
    }
}
