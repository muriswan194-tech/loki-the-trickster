using UnityEngine;

// Fast original comic snapshots give way to one short, real-time 3D Deadpool interruption.
public class LokabrennaEpilogue : MonoBehaviour {
    enum FinaleState { Montage, Live, End }
    static readonly Color Ink=new Color(.075f,.045f,.04f), Paper=new Color(.9f,.79f,.61f), Gold=new Color(.85f,.63f,.29f), Emerald=new Color(.06f,.29f,.19f), DeadpoolRed=new Color(.54f,.055f,.075f);
    readonly string[] montage={
        "THE APPLE WAS UNDER ODIN’S THRONE.",
        "NEXT: SIX LOKIS. ONE VERY CONFUSED ROYAL CLERK.",
        "THEN: AN UNAUTHORIZED DRAKKAR RACE.",
        "THE FEASTING HALL BECAME A MAZE.",
        "THE PIGEONS FILED A FORMAL COMPLAINT.",
        "AND THE WORLD KEPT SPINNING…"
    };
    readonly float[] lineTimes={.15f,1.45f,3.6f,5.6f,7.6f,9.55f};
    readonly string[] liveLines={
        "ROYAL WATCH: Odin’s golden apple. Again. Where did you find it?",
        "LOKI: In a pocket. A different pocket than last time.",
        "DEADPOOL: Surprise! I heard there was trouble. I came for the finale.",
        "LOKI: Your portal interrupted my apology.",
        "DEADPOOL: Great. I was told this is the ending. No bonus mission, please."
    };
    FinaleState state=FinaleState.Montage;
    float montageClock,liveClock,endClock;
    int panel,lineIndex=-1;
    Transform loki,guard,deadpool,portal,apple,lokiHead,deadpoolRoot;
    Camera sceneCamera;
    GUIStyle captionStyle,titleStyle,smallStyle;

    void Start(){
        Time.timeScale=1f;
        sceneCamera=Camera.main;
        if(sceneCamera==null){GameObject c=new GameObject("Main Camera");c.tag="MainCamera";sceneCamera=c.AddComponent<Camera>();}
        sceneCamera.orthographic=false;sceneCamera.fieldOfView=48f;sceneCamera.transform.position=new Vector3(0,3.4f,-8.4f);sceneCamera.transform.LookAt(new Vector3(0,1.2f,.4f));
        Light sun=FindObjectOfType<Light>();if(sun==null){GameObject l=new GameObject("Warm finale key");sun=l.AddComponent<Light>();}sun.type=LightType.Directional;sun.color=new Color(1f,.81f,.59f);sun.intensity=1.45f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(42,-28,0);
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.38f,.48f,.54f);RenderSettings.ambientEquatorColor=new Color(.58f,.48f,.37f);RenderSettings.ambientGroundColor=new Color(.16f,.17f,.16f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.34f,.4f,.43f);RenderSettings.fogStartDistance=18;RenderSettings.fogEndDistance=34;
        BuildLiveStage();
    }

    void BuildLiveStage(){
        Make("Final scene · pale Asgard stone",new Vector3(0,-.22f,1f),new Vector3(18,.42f,15f),new Color(.72f,.66f,.54f));
        Make("Palace approach · gold threshold",new Vector3(0,.025f,4.2f),new Vector3(10,.12f,3.4f),new Color(.74f,.57f,.3f));
        Mesh arch=LokabrennaMeshLibrary.Load("AsgardArch");MakeMesh("Palace · open gold arch",new Vector3(0,0,5.1f),new Vector3(1.75f,1.72f,1f),Gold,arch);
        foreach(float x in new[]{-6.2f,-4.2f,4.2f,6.2f}){
            Make("Palace · fluted column",new Vector3(x,2f,4.2f),new Vector3(.42f,2f,.42f),new Color(.77f,.69f,.52f),PrimitiveType.Cylinder);
            Make("Palace · capital",new Vector3(x,3.05f,4.2f),new Vector3(.76f,.18f,.72f),Gold,PrimitiveType.Cylinder);
        }
        loki=BuildLoki(new Vector3(-1.25f,.95f,.15f));
        guard=BuildGuard(new Vector3(1.15f,.95f,.55f));
        apple=Make("Odin’s recovered golden apple",new Vector3(-.55f,1.28f,.55f),new Vector3(.3f,.38f,.3f),new Color(.9f,.57f,.12f),PrimitiveType.Sphere).transform;
        Make("Apple · leaf",apple.position+new Vector3(0,.2f,0),new Vector3(.11f,.08f,.06f),Emerald,PrimitiveType.Sphere);
        deadpool=BuildDeadpool(new Vector3(2.8f,.05f,.75f));deadpool.gameObject.SetActive(false);
        portal=MakeTorus("Finale · original amber interruption portal",new Vector3(2.65f,1.65f,1.15f),new Vector3(1.8f,1.8f,.18f),new Color(1f,.35f,.045f));portal.gameObject.SetActive(false);
        Make("Toppled apple basket",new Vector3(3.55f,.4f,-.35f),new Vector3(.78f,.72f,.7f),new Color(.31f,.18f,.1f),PrimitiveType.Cylinder).transform.rotation=Quaternion.Euler(0,0,71);
        for(int i=0;i<4;i++)Make("Loose golden apple",new Vector3(2.9f+i*.44f,.25f,-.55f+Mathf.Sin(i*2.2f)*.18f),Vector3.one*.27f,new Color(.86f,.48f,.1f),PrimitiveType.Sphere);
    }

    Transform BuildLoki(Vector3 position){
        Transform root=new GameObject("Loki · original Asgardian trickster").transform;root.position=position;
        Material coat=Mat("Deep emerald tailored coat",new Color(.055f,.23f,.15f),.12f,.28f),cloak=Mat("Night green cape",new Color(.035f,.09f,.075f),.05f,.3f),gold=Mat("Brushed warm gold",Gold,.62f,.34f),skin=Mat("Warm skin",new Color(.61f,.4f,.29f),0,.4f),hair=Mat("Dark hair",new Color(.025f,.025f,.03f),.02f,.38f);
        AddMesh(root,"Tailored long coat · shaped hem",new Vector3(0,.8f,.03f),Vector3.one,coat,LokabrennaMeshLibrary.Load("TailoredRobe"));
        AddMesh(root,"Draped dark cape",new Vector3(0,.85f,-.23f),Vector3.one,cloak,LokabrennaMeshLibrary.Load("FlowingCape"));
        AddMesh(root,"Gold circlet detail",new Vector3(0,.38f,.12f),new Vector3(.82f,.82f,.82f),gold,LokabrennaMeshLibrary.Load("GoldBeltRing"));
        MakeChild(root,"Loki · face",new Vector3(0,1.73f,.11f),new Vector3(.4f,.49f,.39f),skin,PrimitiveType.Sphere);
        MakeChild(root,"Loki · swept dark hair",new Vector3(0,1.94f,-.02f),new Vector3(.44f,.2f,.4f),hair,PrimitiveType.Sphere);
        for(int side=-1;side<=1;side+=2){
            MakeChild(root,"Loki · long hair lock",new Vector3(side*.21f,1.48f,-.015f),new Vector3(.11f,.53f,.15f),hair,PrimitiveType.Capsule);
            Transform horn=MakeChildMesh(root,"Loki · original swept crown horn",new Vector3(side*.2f,2.08f,.01f),new Vector3(side<0?-1f:1f,1f,1f),gold,LokabrennaMeshLibrary.Load("SweptCrownHorn"));
            horn.localRotation=Quaternion.Euler(0,0,side<0?13f:-13f);
            MakeChild(root,"Coat · sculpted shoulder mantle",new Vector3(side*.34f,1.18f,-.01f),new Vector3(.29f,.22f,.32f),gold,PrimitiveType.Sphere);
        }
        Transform arm=MakeChild(root,"Loki · expressive arm",new Vector3(.39f,1f,.17f),new Vector3(.16f,.55f,.16f),coat,PrimitiveType.Capsule);arm.localRotation=Quaternion.Euler(0,0,-32);
        lokiHead=root;
        return root;
    }

    Transform BuildGuard(Vector3 position){
        Transform root=new GameObject("Royal watch · bronze and crimson armor").transform;root.position=position;
        Material bronze=Mat("Watch bronze",new Color(.48f,.3f,.12f),.52f,.32f),red=Mat("Watch crimson",new Color(.31f,.045f,.05f),.13f,.38f),dark=Mat("Watch dark steel",new Color(.12f,.14f,.14f),.38f,.34f);
        MakeChild(root,"Guard · plated cuirass",new Vector3(0,.94f,.02f),new Vector3(.54f,.68f,.36f),bronze,PrimitiveType.Sphere);
        MakeChild(root,"Guard · helmet",new Vector3(0,1.58f,0),new Vector3(.49f,.45f,.47f),bronze,PrimitiveType.Sphere);
        MakeChild(root,"Guard · crimson crest",new Vector3(0,1.88f,-.03f),new Vector3(.14f,.32f,.17f),red,PrimitiveType.Capsule);
        MakeChild(root,"Guard · dark visor",new Vector3(0,1.55f,.24f),new Vector3(.34f,.095f,.04f),dark);
        MakeChild(root,"Guard · left pauldron",new Vector3(-.39f,1.22f,0),new Vector3(.32f,.2f,.33f),bronze,PrimitiveType.Sphere);
        MakeChild(root,"Guard · right pauldron",new Vector3(.39f,1.22f,0),new Vector3(.32f,.2f,.33f),bronze,PrimitiveType.Sphere);
        Transform shield=MakeChild(root,"Guard · round embossed shield",new Vector3(-.55f,.85f,.2f),new Vector3(.53f,.63f,.12f),bronze,PrimitiveType.Cylinder);shield.localRotation=Quaternion.Euler(90,0,0);
        MakeChild(root,"Guard · crimson shield boss",new Vector3(-.55f,.85f,.29f),new Vector3(.17f,.17f,.06f),red,PrimitiveType.Sphere);
        return root;
    }

    Transform BuildDeadpool(Vector3 position){
        Transform root=new GameObject("Unexpected red-masked guest · original placeholder").transform;root.position=position;
        Material red=Mat("Cameo · deep red fabric",DeadpoolRed,.12f,.43f),black=Mat("Cameo · matte black panels",new Color(.035f,.04f,.045f),.18f,.4f),white=Mat("Cameo · pale eye lenses",new Color(.88f,.84f,.72f),.05f,.25f);
        AddMesh(root,"Cameo · fitted red tunic",new Vector3(0,.8f,0),Vector3.one,red,LokabrennaMeshLibrary.Load("TailoredRobe"));
        MakeChild(root,"Cameo · mask",new Vector3(0,1.72f,.04f),new Vector3(.43f,.48f,.39f),red,PrimitiveType.Sphere);
        MakeChild(root,"Cameo · dark eye surround",new Vector3(0,1.75f,.32f),new Vector3(.36f,.2f,.08f),black,PrimitiveType.Sphere);
        for(int side=-1;side<=1;side+=2)MakeChild(root,"Cameo · expressive pale lens",new Vector3(side*.105f,1.77f,.38f),new Vector3(.09f,.055f,.025f),white,PrimitiveType.Sphere);
        MakeChild(root,"Cameo · cross-body utility strap",new Vector3(0,1.12f,.22f),new Vector3(.12f,.8f,.07f),black);
        return root;
    }

    void Update(){
        if(state==FinaleState.Montage){montageClock+=Time.unscaledDeltaTime;if(montageClock>=.68f){montageClock=0;panel++;if(panel>=montage.Length){BeginLive();}}return;}
        if(state==FinaleState.Live){UpdateLive();return;}
        endClock+=Time.unscaledDeltaTime;
    }

    void BeginLive(){
        state=FinaleState.Live;liveClock=0;lineIndex=-1;
        sceneCamera.transform.position=new Vector3(0,3.3f,-8.4f);sceneCamera.transform.LookAt(new Vector3(.25f,1.1f,.5f));
    }

    void UpdateLive(){
        liveClock+=Time.deltaTime;
        Vector3 focus=new Vector3(.15f,1.15f,.2f)+new Vector3(Mathf.Sin(liveClock*.16f)*.28f,0,0);
        Vector3 desired=focus+new Vector3(0,2.6f,-7.8f);
        sceneCamera.transform.position=Vector3.Lerp(sceneCamera.transform.position,desired,1f-Mathf.Exp(-1.6f*Time.deltaTime));sceneCamera.transform.LookAt(focus);
        if(loki!=null){loki.position+=new Vector3(0,Mathf.Sin(liveClock*2f)*.002f,0);if(liveClock<2.5f){Vector3 look=guard.position-loki.position;look.y=0;if(look.sqrMagnitude>.01f)loki.rotation=Quaternion.Slerp(loki.rotation,Quaternion.LookRotation(look),Time.deltaTime*2f);}}
        if(liveClock>=3.1f&&!portal.gameObject.activeSelf){portal.gameObject.SetActive(true);deadpool.gameObject.SetActive(true);deadpool.position=new Vector3(2.7f,.05f,1.1f);}
        if(liveClock>=3.1f&&liveClock<4.7f){float t=Mathf.Clamp01((liveClock-3.1f)/1.2f);deadpool.position=Vector3.Lerp(new Vector3(2.7f,.05f,1.1f),new Vector3(1.65f,.05f,.45f),t);deadpool.rotation=Quaternion.Euler(0,-24,0);}
        if(liveClock>=3.1f){Vector3 to=deadpool.position-guard.position;to.y=0;if(to.sqrMagnitude>.01f)guard.rotation=Quaternion.Slerp(guard.rotation,Quaternion.LookRotation(to),Time.deltaTime*3f);Vector3 lokiTo=deadpool.position-loki.position;lokiTo.y=0;if(lokiTo.sqrMagnitude>.01f)loki.rotation=Quaternion.Slerp(loki.rotation,Quaternion.LookRotation(lokiTo),Time.deltaTime*3f);}
        int next=-1;for(int i=0;i<lineTimes.Length;i++)if(liveClock>=lineTimes[i])next=i;
        if(next!=lineIndex){lineIndex=next;if(lineIndex>=liveLines.Length){state=FinaleState.End;endClock=0;}}
    }

    Material Mat(string name,Color color,float metallic,float roughness){Material m=new Material(Shader.Find("Standard"));m.name=name;m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",1f-roughness);return m;}
    Transform MakeChild(Transform parent,string name,Vector3 position,Vector3 scale,Material material,PrimitiveType type=PrimitiveType.Cube){Transform t=GameObject.CreatePrimitive(type).transform;t.name=name;t.SetParent(parent,false);t.localPosition=position;t.localScale=scale;t.GetComponent<Renderer>().material=material;return t;}
    Transform MakeChild(Transform parent,string name,Vector3 position,Vector3 scale,Color color,PrimitiveType type=PrimitiveType.Cube){return MakeChild(parent,name,position,scale,Mat(name,color,.08f,.4f),type);}
    Transform MakeChildMesh(Transform parent,string name,Vector3 position,Vector3 scale,Material material,Mesh mesh){GameObject g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().material=material;return g.transform;}
    void AddMesh(Transform parent,string name,Vector3 position,Vector3 scale,Material material,Mesh mesh){GameObject g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;MeshFilter f=g.AddComponent<MeshFilter>();f.sharedMesh=mesh;g.AddComponent<MeshRenderer>().material=material;}
    Transform Make(string name,Vector3 position,Vector3 scale,Color color,PrimitiveType type=PrimitiveType.Cube){Transform t=GameObject.CreatePrimitive(type).transform;t.name=name;t.position=position;t.localScale=scale;t.GetComponent<Renderer>().material=Mat(name,color,color==Gold?.42f:.05f,.4f);return t;}
    Transform MakeMesh(string name,Vector3 position,Vector3 scale,Color color,Mesh mesh){GameObject g=new GameObject(name);g.transform.position=position;g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().material=Mat(name,color,.43f,.32f);return g.transform;}
    Transform MakeTorus(string name,Vector3 position,Vector3 scale,Color color){return MakeMesh(name,position,scale,color,LokabrennaMeshLibrary.Load("GoldBeltRing"));}

    void EnsureStyles(){if(captionStyle!=null)return;captionStyle=new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,wordWrap=true};captionStyle.normal.textColor=Paper;titleStyle=new GUIStyle(captionStyle){fontSize=56};titleStyle.normal.textColor=Gold;smallStyle=new GUIStyle(captionStyle){fontSize=14};smallStyle.normal.textColor=Paper;}
    void Fill(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    void DrawPanel(int index){
        Color[] colors={new Color(.57f,.22f,.09f),new Color(.17f,.3f,.25f),new Color(.42f,.15f,.09f),new Color(.19f,.25f,.28f),new Color(.58f,.38f,.14f),new Color(.18f,.14f,.18f)};
        Rect panelRect=new Rect(Screen.width*(index%2==0?.08f:.15f),Screen.height*(index%2==0?.18f:.14f),Screen.width*(index%2==0?.84f:.7f),Screen.height*(index%2==0?.54f:.62f));
        Fill(new Rect(panelRect.x-11,panelRect.y-11,panelRect.width+22,panelRect.height+22),Paper);Fill(panelRect,colors[index]);
        for(int i=0;i<13;i++){float x=panelRect.x+((i*83+index*37)%(int)panelRect.width);Fill(new Rect(x,panelRect.y+12,2,panelRect.height-24),new Color(.05f,.04f,.04f,.15f));}
        string[] icons={"✦","?!","WHOOSH","↗","CAW","…"};GUI.Label(new Rect(panelRect.x+panelRect.width*.08f,panelRect.y+panelRect.height*.1f,panelRect.width*.84f,panelRect.height*.36f),icons[index],new GUIStyle(titleStyle){fontSize=index==2?72:60,normal={textColor=index==1?Gold:Paper}});
        Fill(new Rect(Screen.width*.055f,Screen.height*.77f,Screen.width*.89f,Screen.height*.12f),Ink);
        GUI.Label(new Rect(Screen.width*.075f,Screen.height*.765f,Screen.width*.85f,Screen.height*.13f),montage[index],captionStyle);
        GUI.Label(new Rect(Screen.width*.84f,Screen.height*.93f,Screen.width*.13f,24),(index+1)+" / "+montage.Length,smallStyle);
        // Fast comic-panel cuts use restrained slashes rather than a full-screen effect.
        for(int i=0;i<7;i++)Fill(new Rect(Screen.width*(.08f+i*.13f),Screen.height*.08f+(i%2)*8,Screen.width*.045f,3),Gold);
    }
    void OnGUI(){
        EnsureStyles();
        if(state==FinaleState.Montage){Fill(new Rect(0,0,Screen.width,Screen.height),Ink);DrawPanel(panel);return;}
        if(state==FinaleState.Live){Fill(new Rect(0,0,Screen.width,64),new Color(.035f,.055f,.05f,.68f));GUI.Label(new Rect(28,15,Screen.width-56,34),"ASGARD · ROYAL APPLE INCIDENT",smallStyle);if(lineIndex>=0&&lineIndex<liveLines.Length){Fill(new Rect(Screen.width*.1f,Screen.height*.82f,Screen.width*.8f,Screen.height*.105f),new Color(.055f,.04f,.035f,.88f));GUI.Label(new Rect(Screen.width*.12f,Screen.height*.825f,Screen.width*.76f,Screen.height*.095f),liveLines[lineIndex],captionStyle);}return;}
        Fill(new Rect(0,0,Screen.width,Screen.height),Ink);GUI.Label(new Rect(Screen.width*.08f,Screen.height*.28f,Screen.width*.84f,Screen.height*.2f),"THE END",titleStyle);GUI.Label(new Rect(Screen.width*.12f,Screen.height*.5f,Screen.width*.76f,Screen.height*.13f),"THE WORLD DOESN’T REVOLVE AROUND LOKI.\nTHE TRICKSTER GETS THE LAST WORD.",captionStyle);GUI.Label(new Rect(Screen.width*.15f,Screen.height*.82f,Screen.width*.7f,34),"LOKABRENNA · A GAME JAM TALE",smallStyle);
    }
}
