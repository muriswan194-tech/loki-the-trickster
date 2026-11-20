using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LokabrennaGame : MonoBehaviour {
    enum ScreenState { Menu, Playing, Paused, Caught }
    enum GuardState { Patrol, Investigate, Suspicious, Chase, Return }
    sealed class WorldProp { public string id, label; public Transform transform; public Rigidbody body; public float radius, hearing, distraction, mass=1f; public bool noisy, movable, solid; }
    enum Aspect { Human, Hawk, Falcon, Ant, Wildfire }
    const string MainScene = "Lokabrenna";
    const string ContinueKey = "lokabrenna-started";
    readonly Color moss = new Color(.16f,.3f,.29f), stone = new Color(.33f,.39f,.4f), brass = new Color(.77f,.58f,.29f), lokiGreen = new Color(.24f,.55f,.39f), guardRed = new Color(.53f,.23f,.2f);
    DistrictData district; EncounterDefinition encounter; List<EncounterDefinition> encounterCatalog; Transform player, guard, guardHelmet, playerHorns, disguiseHelmet, playerCape, playerArmL, playerArmR, lokiHumanVisual, disguiseVisual, hawkVisual, falconVisual, antVisual, wildfireVisual, decoy;
    Renderer playerRenderer, guardRenderer; readonly List<GameObject> gateBars = new List<GameObject>();
    readonly List<WorldProp> props = new List<WorldProp>();
    ScreenState screen=ScreenState.Menu; GuardState guardState=GuardState.Patrol; string toast="", menuPage="main"; float toastUntil, disguiseUntil, suspicion, introHoldUntil, caughtTimer, decoyUntil, soundUntil, lastPlayerSpeed, lastSKey, cameraYaw, cameraPitch=22f, cameraDistance=5.8f, walkPhase; int patrolIndex, teleportCharges=10; Vector3 investigatePoint, soundPoint, moveVelocity, cameraFollowVelocity; bool encounterStarted, introPlaying, alarmRaised, escaped, disguised, soundActive, sceptreFound, tesseractFound, epilogueTransitionRequested, mapOpen, cursorCaptured; Aspect aspect=Aspect.Human; Transform conjured;
    GUIStyle titleStyle, subtitleStyle, bodyStyle, smallStyle, buttonStyle, buttonDimStyle, toastStyle;

    void Start() {
        district=new DistrictDataLoader().Load(); encounterCatalog=EncounterRegistry.LoadAll(district);
        if(district==null||encounterCatalog==null||encounterCatalog.Count==0) { enabled=false; return; }
        BuildWorld();
        SetCursorForCurrentScreen();
        toast="Take a breath. Asgard is yours to wander."; toastUntil=Time.time+4f;
    }
    void Update() {
        if(!enabled)return;
        SetCursorForCurrentScreen();
        if(screen==ScreenState.Menu){if(Input.GetKeyDown(KeyCode.Escape))menuPage="main";return;}
        if(screen==ScreenState.Paused){if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P))screen=ScreenState.Playing;return;}
        if(screen==ScreenState.Caught){caughtTimer-=Time.deltaTime;if(caughtTimer<=0)SceneManager.LoadScene(MainScene);return;}
        if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P)){screen=ScreenState.Paused;return;}
        if(Input.GetKeyDown(KeyCode.R)){SceneManager.LoadScene(MainScene);return;}
        if(Input.GetKeyDown(KeyCode.M))mapOpen=!mapOpen;
        if(mapOpen)return;
        MovePlayer();
        UpdateCamera();
        ReadAbilityInput();
        if(!encounterStarted)TryStartEncounter();
        if(encounterStarted&&!introPlaying&&!escaped)UpdateGuard();
        if(encounterStarted&&!escaped)CheckGate();
        if(disguised&&Time.time>=disguiseUntil)EndDisguise();
        if(decoy!=null&&Time.time>=decoyUntil){Destroy(decoy.gameObject);decoy=null;}
    }
    void BuildWorld() {
        Camera cam=Camera.main;if(cam!=null){cam.transform.position=new Vector3(0,18,-21);cam.transform.LookAt(new Vector3(0,0,1));cam.orthographic=false;cam.fieldOfView=58f;cam.clearFlags=CameraClearFlags.Skybox;cam.backgroundColor=new Color(.075f,.11f,.14f);}
        Shader skyShader=Shader.Find("Skybox/Procedural");if(skyShader!=null){Material sky=new Material(skyShader);sky.SetFloat("_Exposure",1.1f);sky.SetFloat("_AtmosphereThickness",1.05f);sky.SetColor("_SkyTint",new Color(.47f,.61f,.68f));sky.SetColor("_GroundColor",new Color(.38f,.31f,.24f));RenderSettings.skybox=sky;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.35f,.43f,.48f);RenderSettings.fogStartDistance=25f;RenderSettings.fogEndDistance=52f;}
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.31f,.41f,.48f);RenderSettings.ambientEquatorColor=new Color(.57f,.49f,.37f);RenderSettings.ambientGroundColor=new Color(.14f,.18f,.2f);RenderSettings.ambientIntensity=1.05f;
        Light sun=FindObjectOfType<Light>();if(sun!=null){sun.type=LightType.Directional;sun.color=new Color(1f,.86f,.69f);sun.intensity=1.35f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.68f;sun.transform.rotation=Quaternion.Euler(46,-28,0);RenderSettings.sun=sun;QualitySettings.shadowDistance=42f;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.antiAliasing=2;}
        GameObject ground=Make("Asgard · continuous wanderable flagstone district",new Vector3(0,-.22f,-.7f),new Vector3(32,.42f,24f),new Color(.75f,.68f,.53f));
        ApplyPbrFlagstones(ground);
        Make("District edge · west",new Vector3(-16.05f,.02f,-.7f),new Vector3(.22f,.08f,24f),new Color(.55f,.43f,.29f));
        Make("District edge · east",new Vector3(16.05f,.02f,-.7f),new Vector3(.22f,.08f,24f),new Color(.55f,.43f,.29f));
        Make("District edge · south",new Vector3(0,.02f,-12.45f),new Vector3(32f,.08f,.22f),new Color(.55f,.43f,.29f));
        DistrictPoint square=MapPoint("asgard-square",0,-.2f),gate=MapPoint("bifrost-approach",0,8.15f);
        GameObject squareInset=Make("Square · inset",new Vector3(square.x,.012f,square.z),new Vector3(12.8f,.06f,8.4f),new Color(.42f,.4f,.35f));ApplyPbrFlagstones(squareInset);
        GameObject approach=Make("Main approach",new Vector3(gate.x,.075f,(gate.z+square.z)*.5f+1.3f),new Vector3(5.7f,.12f,7.6f),new Color(.59f,.49f,.34f));ApplyPbrFlagstones(approach);
        for(int x=-5;x<=5;x+=2)Make("Square joint",new Vector3(square.x+x,.055f,square.z),new Vector3(.035f,.02f,8.2f),new Color(.42f,.43f,.39f));
        float gateX=gate.x,gateZ=gate.z+.5f;
        MakeMesh("Bifrost · carved pale-stone surround",new Vector3(gateX,0,gateZ+.09f),new Vector3(1.38f,1.08f,.92f),new Color(.76f,.66f,.49f),LokabrennaMeshLibrary.Load("AsgardArch"));
        MakeMesh("Bifrost · hand-finished gold arch",new Vector3(gateX,0,gateZ-.02f),new Vector3(1.13f,1f,1.08f),brass,LokabrennaMeshLibrary.Load("AsgardArch"));
        Make("Gate pier west",new Vector3(gateX-2.04f,.79f,gateZ),new Vector3(.56f,1.58f,.56f),brass,PrimitiveType.Cylinder);
        Make("Gate pier east",new Vector3(gateX+2.04f,.79f,gateZ),new Vector3(.56f,1.58f,.56f),brass,PrimitiveType.Cylinder);
        Make("Bifrost threshold",new Vector3(gate.x,.18f,gate.z-.3f),new Vector3(4.3f,.12f,.9f),new Color(.25f,.72f,.78f));
        Make("Bifrost threshold · inner light",new Vector3(gate.x,.255f,gate.z-.3f),new Vector3(3.5f,.025f,.11f),new Color(.52f,.91f,.91f));
        gateBars.Add(Make("Closed gate · left leaf",new Vector3(gate.x-.73f,1.45f,gate.z-.13f),new Vector3(.13f,2.35f,.13f),guardRed));
        gateBars.Add(Make("Closed gate · right leaf",new Vector3(gate.x+.73f,1.45f,gate.z-.13f),new Vector3(.13f,2.35f,.13f),guardRed));
        for(int i=0;i<7;i++)gateBars.Add(Make("Gate grille",new Vector3(gate.x-1.5f+i*.5f,1.5f,gate.z-.13f),new Vector3(.07f,2.4f,.12f),brass));
        BuildPalaceHorizon();
        BuildSquareLandmarks();
        BuildPlazaDetails();
        foreach(InteractionData item in district.interactions)BuildInteraction(item);
        BuildHomes();
        BuildOuterDistrictPaths();
        BuildStatueLandmark();
        player=Make("Loki · original green-and-gold trickster",new Vector3(district.playerStart.x,1f,district.playerStart.z),new Vector3(.72f,1f,.72f),new Color(.15f,.34f,.23f),PrimitiveType.Capsule).transform;
        playerRenderer=player.GetComponent<Renderer>();
        BuildLokiAppearance();
        playerRenderer.enabled=false;
        BuildAlternateAspects();
        BuildDisguiseAppearance();
        guard=Make("Bifrost watch · guard",new Vector3(district.guard.points[0].x,.85f,district.guard.points[0].z),new Vector3(.8f,1.4f,.8f),guardRed,PrimitiveType.Capsule).transform;
        guardRenderer=guard.GetComponent<Renderer>();guardHelmet=Make("Guard · helmet",guard.position+Vector3.up*.82f,new Vector3(.91f,.3f,.91f),brass,PrimitiveType.Sphere).transform;guardHelmet.SetParent(guard,false);guardHelmet.localPosition=new Vector3(0,.85f,0);
        Transform crest=Make("Guard · crest",guard.position,new Vector3(.15f,.32f,.14f),new Color(.8f,.69f,.42f)).transform;crest.SetParent(guard,false);crest.localPosition=new Vector3(0,1.08f,.12f);
        BuildRoyalGuardAppearance();
        Make("Watch post · rounded plinth",new Vector3(2.65f,.38f,3.5f),new Vector3(.72f,.38f,.72f),new Color(.27f,.31f,.3f),PrimitiveType.Cylinder);
        Make("Alarm lens",new Vector3(2.65f,1.2f,3.5f),new Vector3(.34f,.36f,.34f),new Color(.72f,.22f,.2f),PrimitiveType.Sphere);
        foreach(float x in new[]{-8.6f,-4.5f,4.5f,8.6f}){Make("Square colonnade · round plinth",new Vector3(x,.15f,-6.35f),new Vector3(.9f,.3f,.9f),new Color(.3f,.35f,.36f),PrimitiveType.Cylinder);Make("Fluted colonnade shaft",new Vector3(x,1.35f,-6.35f),new Vector3(.34f,2.35f,.34f),new Color(.49f,.52f,.48f),PrimitiveType.Cylinder);Make("Column capital",new Vector3(x,2.65f,-6.35f),new Vector3(.72f,.24f,.72f),brass,PrimitiveType.Cylinder);}
        BuildTrees();
        if(cam!=null&&player!=null){Vector3 cameraTarget=player.position+Vector3.up*1.02f;cam.transform.position=cameraTarget+Quaternion.Euler(cameraPitch,cameraYaw,0)*new Vector3(0,0,-cameraDistance);cam.transform.LookAt(cameraTarget);}
    }
    Transform AddPart(Transform parent,string name,Vector3 localPosition,Vector3 size,Color color,PrimitiveType type=PrimitiveType.Cube){Transform part=Make(name,parent.position+localPosition,size,color,type).transform;part.SetParent(parent,false);part.localPosition=localPosition;return part;}
    GameObject MakeMesh(string name,Vector3 position,Vector3 scale,Color color,Mesh mesh){GameObject go=new GameObject(name);go.transform.position=position;go.transform.localScale=scale;var filter=go.AddComponent<MeshFilter>();filter.sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.material=new Material(Shader.Find("Standard"));renderer.material.color=color;renderer.material.SetFloat("_Glossiness",.36f);renderer.material.SetFloat("_Metallic",color==brass?.55f:.08f);return go;}
    Transform AddMeshPart(Transform parent,string name,Vector3 localPosition,Vector3 scale,Color color,string assetName){Transform part=MakeMesh(name,parent.position+localPosition,scale,color,LokabrennaMeshLibrary.Load(assetName)).transform;part.SetParent(parent,false);part.localPosition=localPosition;return part;}
    void BuildLokiAppearance(){
        lokiHumanVisual=new GameObject("Loki · original green-and-gold attire").transform;lokiHumanVisual.SetParent(player,false);lokiHumanVisual.localPosition=Vector3.zero;
        playerHorns=new GameObject("Loki · original horned crown").transform;playerHorns.SetParent(lokiHumanVisual,false);playerHorns.localPosition=Vector3.zero;
        Color hair=new Color(.035f,.028f,.035f),hairLight=new Color(.09f,.07f,.075f),skin=new Color(.76f,.56f,.43f),darkGreen=new Color(.035f,.12f,.09f),coat=new Color(.055f,.29f,.19f),coatLight=new Color(.09f,.39f,.26f),goldLine=new Color(.93f,.66f,.25f),goldHi=new Color(1f,.82f,.43f),undersuit=new Color(.075f,.085f,.085f);
        // Layered silhouette: long tailored coat over a dark fitted underlayer, with a cloth cape behind it.
        AddMeshPart(lokiHumanVisual,"Tailored emerald coat · sculpted flared hem",new Vector3(0,-.04f,.16f),Vector3.one,coat,"TailoredRobe");
        AddPart(lokiHumanVisual,"Dark fitted collar and underlayer",new Vector3(0,.39f,.12f),new Vector3(.48f,.47f,.3f),undersuit,PrimitiveType.Sphere);
        playerCape=AddMeshPart(lokiHumanVisual,"Flowing dark-green cape",new Vector3(0,0,-.18f),Vector3.one,darkGreen,"FlowingCape");
        Renderer coatRenderer=lokiHumanVisual.Find("Tailored emerald coat · sculpted flared hem").GetComponent<Renderer>();
        if(coatRenderer!=null){coatRenderer.material.SetInt("_Cull",0);coatRenderer.material.SetFloat("_Glossiness",.23f);}
        Renderer capeRenderer=playerCape.GetComponent<Renderer>();if(capeRenderer!=null){capeRenderer.material.SetInt("_Cull",0);capeRenderer.material.SetFloat("_Glossiness",.24f);capeRenderer.material.color=new Color(.045f,.17f,.13f);}
        AddLine(playerCape,"Cape · fine gold selvedge",new[]{new Vector3(-.39f,-.48f,-.39f),new Vector3(-.56f,-1.03f,-.47f),new Vector3(-.74f,-1.48f,-.36f)},goldLine,.018f);
        AddLine(playerCape,"Cape · fine gold selvedge",new[]{new Vector3(.39f,-.48f,-.39f),new Vector3(.56f,-1.03f,-.47f),new Vector3(.74f,-1.48f,-.36f)},goldLine,.018f);
        Transform belt=AddMeshPart(lokiHumanVisual,"Hand-finished gold belt",new Vector3(0,-.39f,.06f),Vector3.one,goldHi,"GoldBeltRing");belt.localRotation=Quaternion.Euler(90,0,0);
        AddPart(lokiHumanVisual,"Engraved belt clasp",new Vector3(0,-.39f,.31f),new Vector3(.14f,.14f,.075f),goldHi,PrimitiveType.Sphere);
        AddPart(lokiHumanVisual,"Emerald clasp inset",new Vector3(0,-.39f,.371f),new Vector3(.057f,.07f,.027f),new Color(.04f,.62f,.31f),PrimitiveType.Sphere);
        // Sculpted coat lapels and a fine double gold seam keep the front readable at game-camera distance.
        AddLine(lokiHumanVisual,"Left gold coat piping",new[]{new Vector3(-.3f,.46f,.30f),new Vector3(-.18f,.27f,.37f),new Vector3(-.1f,.02f,.42f),new Vector3(-.13f,-.25f,.43f),new Vector3(-.2f,-.62f,.38f)},goldLine,.025f);
        AddLine(lokiHumanVisual,"Right gold coat piping",new[]{new Vector3(.3f,.46f,.30f),new Vector3(.18f,.27f,.37f),new Vector3(.1f,.02f,.42f),new Vector3(.13f,-.25f,.43f),new Vector3(.2f,-.62f,.38f)},goldLine,.025f);
        AddLine(lokiHumanVisual,"Tailored chest chevron",new[]{new Vector3(-.24f,.28f,.35f),new Vector3(0,.18f,.43f),new Vector3(.24f,.28f,.35f)},goldHi,.034f);
        AddPart(lokiHumanVisual,"High sculpted coat collar · left",new Vector3(-.22f,.48f,.17f),new Vector3(.19f,.33f,.16f),coatLight,PrimitiveType.Sphere);
        AddPart(lokiHumanVisual,"High sculpted coat collar · right",new Vector3(.22f,.48f,.17f),new Vector3(.19f,.33f,.16f),coatLight,PrimitiveType.Sphere);
        AddPart(lokiHumanVisual,"Gold collar clasp",new Vector3(0,.43f,.336f),new Vector3(.11f,.13f,.055f),goldHi,PrimitiveType.Sphere);
        for(int side=-1;side<=1;side+=2){
            AddPart(lokiHumanVisual,side<0?"Left shaped shoulder mantle":"Right shaped shoulder mantle",new Vector3(side*.4f,.32f,.04f),new Vector3(.31f,.26f,.32f),coatLight,PrimitiveType.Sphere);
            AddPart(lokiHumanVisual,"Gold shoulder plate",new Vector3(side*.43f,.33f,.19f),new Vector3(.25f,.2f,.16f),goldLine,PrimitiveType.Sphere);
            Transform arm=AddPart(lokiHumanVisual,side<0?"Loki left tailored sleeve":"Loki right tailored sleeve",new Vector3(side*.52f,-.04f,.08f),new Vector3(.2f,.59f,.21f),coatLight,PrimitiveType.Capsule);
            if(side<0)playerArmL=arm;else playerArmR=arm;
            AddPart(lokiHumanVisual,"Forearm leather bracer",new Vector3(side*.53f,-.3f,.12f),new Vector3(.16f,.23f,.16f),undersuit,PrimitiveType.Sphere);
            AddPart(lokiHumanVisual,"Gold wrist cuff",new Vector3(side*.53f,-.38f,.11f),new Vector3(.22f,.1f,.21f),goldHi,PrimitiveType.Sphere);
            AddPart(lokiHumanVisual,"Expressive hand",new Vector3(side*.53f,-.52f,.13f),new Vector3(.14f,.18f,.13f),skin,PrimitiveType.Sphere);
            AddPart(lokiHumanVisual,side<0?"Left leg · dark fitted trouser":"Right leg · dark fitted trouser",new Vector3(side*.2f,-.69f,.01f),new Vector3(.19f,.45f,.2f),undersuit,PrimitiveType.Capsule);
            AddPart(lokiHumanVisual,side<0?"Left shaped riding boot":"Right shaped riding boot",new Vector3(side*.22f,-.78f,.1f),new Vector3(.22f,.37f,.27f),darkGreen,PrimitiveType.Capsule);
            AddPart(lokiHumanVisual,"Boot gold filigree",new Vector3(side*.22f,-.91f,.216f),new Vector3(.2f,.07f,.045f),goldLine,PrimitiveType.Sphere);
        }
        AddPart(lokiHumanVisual,"Loki neck",new Vector3(0,.59f,.09f),new Vector3(.2f,.3f,.2f),skin,PrimitiveType.Capsule);
        AddPart(lokiHumanVisual,"Loki face",new Vector3(0,.79f,.13f),new Vector3(.43f,.49f,.39f),skin,PrimitiveType.Sphere);
        AddPart(lokiHumanVisual,"Chin and jaw",new Vector3(0,.62f,.23f),new Vector3(.29f,.22f,.23f),skin,PrimitiveType.Sphere);
        // Distinct swept hair framing the cheekbones and tapering down the back of the neck.
        AddPart(lokiHumanVisual,"Long dark hair · crown",new Vector3(0,1.015f,-.015f),new Vector3(.49f,.24f,.46f),hair,PrimitiveType.Sphere);
        AddPart(lokiHumanVisual,"Swept fringe · left",new Vector3(-.12f,.99f,.25f),new Vector3(.29f,.12f,.19f),hairLight,PrimitiveType.Sphere).localRotation=Quaternion.Euler(0,0,-16f);
        AddPart(lokiHumanVisual,"Swept fringe · right",new Vector3(.1f,1.005f,.24f),new Vector3(.3f,.1f,.18f),hair,PrimitiveType.Sphere).localRotation=Quaternion.Euler(0,0,12f);
        AddPart(lokiHumanVisual,"Long hair · back fall",new Vector3(0,.78f,-.19f),new Vector3(.33f,.36f,.17f),hair,PrimitiveType.Capsule);
        for(int side=-1;side<=1;side+=2){
            Transform hairLock=AddPart(lokiHumanVisual,"Long dark hair · cheek-framing lock",new Vector3(side*.3f,.72f,.02f),new Vector3(.14f,.48f,.18f),hair,PrimitiveType.Capsule);hairLock.localRotation=Quaternion.Euler(0,0,side<0?-7f:7f);
            AddPart(lokiHumanVisual,"Eye · warm ivory",new Vector3(side*.105f,.82f,.316f),new Vector3(.073f,.043f,.024f),new Color(.94f,.84f,.67f),PrimitiveType.Sphere);
            AddPart(lokiHumanVisual,"Eye · dark iris",new Vector3(side*.105f,.82f,.338f),new Vector3(.027f,.036f,.015f),new Color(.2f,.13f,.055f),PrimitiveType.Sphere);
            AddPart(lokiHumanVisual,"Expressive dark brow",new Vector3(side*.11f,.905f,.319f),new Vector3(.13f,.034f,.035f),hair,PrimitiveType.Sphere).localRotation=Quaternion.Euler(0,0,side<0?8f:-4f);
            Transform horn=AddMeshPart(playerHorns,"Swept gold crown horn",new Vector3(side*.24f,1.0f,-.005f),new Vector3(side<0?-1.06f:1.06f,1.04f,1.04f),goldLine,"SweptCrownHorn");horn.localRotation=Quaternion.Euler(0,0,side<0?12f:-12f);
        }
        AddPart(lokiHumanVisual,"Loki nose",new Vector3(0,.755f,.348f),new Vector3(.061f,.09f,.063f),skin,PrimitiveType.Sphere);
        AddPart(lokiHumanVisual,"Sly half-smile",new Vector3(.035f,.682f,.34f),new Vector3(.12f,.022f,.018f),new Color(.25f,.1f,.075f),PrimitiveType.Sphere).localRotation=Quaternion.Euler(0,0,-7f);
    }
    void AddLine(Transform parent,string label,Vector3[] points,Color color,float width){
        GameObject go=new GameObject(label);go.transform.SetParent(parent,false);LineRenderer line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);line.startWidth=width;line.endWidth=width*.68f;line.numCapVertices=3;line.numCornerVertices=3;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;line.material=new Material(Shader.Find("Sprites/Default"));line.startColor=color;line.endColor=color;
    }
    void BuildDisguiseAppearance(){disguiseVisual=new GameObject("Guard disguise · armor and borrowed helm").transform;disguiseVisual.SetParent(player,false);disguiseVisual.localPosition=Vector3.zero;disguiseVisual.gameObject.SetActive(false);Color steel=new Color(.43f,.46f,.44f);AddPart(disguiseVisual,"Borrowed royal cuirass",new Vector3(0,0,.25f),new Vector3(.66f,.82f,.2f),guardRed);AddPart(disguiseVisual,"Borrowed shoulder armor",new Vector3(-.4f,.3f,.1f),new Vector3(.3f,.23f,.3f),brass,PrimitiveType.Sphere);AddPart(disguiseVisual,"Borrowed shoulder armor",new Vector3(.4f,.3f,.1f),new Vector3(.3f,.23f,.3f),brass,PrimitiveType.Sphere);AddPart(disguiseVisual,"Borrowed left greave",new Vector3(-.2f,-.7f,.08f),new Vector3(.22f,.53f,.25f),steel,PrimitiveType.Capsule);AddPart(disguiseVisual,"Borrowed right greave",new Vector3(.2f,-.7f,.08f),new Vector3(.22f,.53f,.25f),steel,PrimitiveType.Capsule);disguiseHelmet=AddPart(disguiseVisual,"Borrowed guard helm",new Vector3(0,1.02f,0),new Vector3(.76f,.48f,.76f),brass,PrimitiveType.Sphere);AddPart(disguiseVisual,"Helm visor",new Vector3(0,.99f,.35f),new Vector3(.55f,.12f,.13f),steel);}
    void BuildAlternateAspects(){
        hawkVisual=BuildBird("Hawk scout",new Color(.39f,.3f,.2f));falconVisual=BuildBird("Falcon swift form",new Color(.58f,.26f,.15f));
        antVisual=new GameObject("Ant · tiny form").transform;antVisual.SetParent(player,false);antVisual.localPosition=Vector3.zero;Color ant=new Color(.15f,.2f,.13f);AddPart(antVisual,"Ant abdomen",new Vector3(0,0,-.18f),new Vector3(.42f,.32f,.46f),ant,PrimitiveType.Sphere);AddPart(antVisual,"Ant thorax",new Vector3(0,.02f,.08f),new Vector3(.27f,.24f,.3f),new Color(.23f,.28f,.17f),PrimitiveType.Sphere);AddPart(antVisual,"Ant head",new Vector3(0,.06f,.27f),new Vector3(.25f,.24f,.23f),ant,PrimitiveType.Sphere);
        for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++){float z=.15f-i*.16f;Transform leg=AddPart(antVisual,"Ant leg",new Vector3(side*.23f,-.02f,z),new Vector3(.34f,.045f,.045f),ant,PrimitiveType.Capsule);leg.localRotation=Quaternion.Euler(0,0,side*23f);}
        wildfireVisual=new GameObject("Wildfire · living emerald flame").transform;wildfireVisual.SetParent(player,false);wildfireVisual.localPosition=Vector3.zero;Transform flame=AddPart(wildfireVisual,"Emerald flame core",new Vector3(0,.12f,0),new Vector3(.74f,1.06f,.72f),new Color(.04f,.72f,.24f),PrimitiveType.Sphere);Renderer glow=flame.GetComponent<Renderer>();glow.material.EnableKeyword("_EMISSION");glow.material.SetColor("_EmissionColor",new Color(.02f,.52f,.12f));for(int side=-1;side<=1;side+=2)AddPart(wildfireVisual,"Flame lick",new Vector3(side*.22f,.72f,0),new Vector3(.14f,.42f,.14f),new Color(.05f,.95f,.31f),PrimitiveType.Sphere);
        hawkVisual.gameObject.SetActive(false);falconVisual.gameObject.SetActive(false);antVisual.gameObject.SetActive(false);wildfireVisual.gameObject.SetActive(false);
    }
    Transform BuildBird(string label,Color feather){Transform form=new GameObject(label).transform;form.SetParent(player,false);form.localPosition=Vector3.zero;AddPart(form,label+" body",new Vector3(0,0,.02f),new Vector3(.42f,.48f,.74f),feather,PrimitiveType.Sphere);AddPart(form,label+" head",new Vector3(0,.23f,.34f),new Vector3(.31f,.3f,.32f),feather,PrimitiveType.Sphere);AddPart(form,label+" beak",new Vector3(0,.18f,.56f),new Vector3(.19f,.13f,.23f),brass,PrimitiveType.Sphere);for(int side=-1;side<=1;side+=2){Transform wing=AddMeshPart(form,label+" feathered gliding wing",new Vector3(side*.23f,.06f,-.02f),new Vector3(side<0?-1f:1f,1f,1f),feather,"FeatheredWing");wing.localRotation=Quaternion.Euler(0,side*4f,side*3f);AddPart(form,label+" eye",new Vector3(side*.11f,.28f,.49f),new Vector3(.045f,.045f,.04f),Color.black,PrimitiveType.Sphere);}return form;}
    void BuildRoyalGuardAppearance(){
        Color steel=new Color(.48f,.51f,.5f),darkSteel=new Color(.2f,.24f,.24f);
        AddPart(guard,"Royal guard rounded cuirass",new Vector3(0,.03f,.38f),new Vector3(.62f,.76f,.25f),steel,PrimitiveType.Sphere);
        AddPart(guard,"Breastplate gold crest",new Vector3(0,.1f,.47f),new Vector3(.14f,.37f,.05f),brass);
        AddPart(guard,"Helmet visor",new Vector3(0,.78f,.41f),new Vector3(.55f,.13f,.13f),darkSteel);
        Transform shield=AddPart(guard,"Asgard royal shield",new Vector3(-.64f,.02f,.24f),new Vector3(.65f,.75f,.12f),brass,PrimitiveType.Cylinder);shield.localRotation=Quaternion.Euler(90,0,0);
        AddPart(guard,"Shield boss",new Vector3(-.64f,.02f,.34f),new Vector3(.18f,.18f,.08f),steel,PrimitiveType.Sphere);
        Transform spear=AddPart(guard,"Ceremonial spear · no combat",new Vector3(.54f,.4f,.22f),new Vector3(.045f,1.55f,.045f),darkSteel);spear.localRotation=Quaternion.Euler(0,0,-4f);
        AddPart(guard,"Spear tip",new Vector3(.65f,1.18f,.22f),new Vector3(.13f,.2f,.13f),brass,PrimitiveType.Cylinder);
    }
    void BuildSquareLandmarks(){
        DistrictPoint fountain=MapPoint("east-fountain",6.2f,-3.8f),alley=MapPoint("side-alley",-7.1f,.8f),market=MapPoint("market-square",6.9f,-.15f),gate=MapPoint("bifrost-approach",0,8.15f);
        MakeMesh("East fountain · carved scalloped basin",new Vector3(fountain.x,.04f,fountain.z),new Vector3(1.52f,.9f,1.52f),new Color(.66f,.61f,.5f),LokabrennaMeshLibrary.Load("AsgardFountainBowl"));
        Transform water=Make("Fountain · still teal water",new Vector3(fountain.x,.24f,fountain.z),new Vector3(1.19f,.035f,1.19f),new Color(.22f,.52f,.53f),PrimitiveType.Cylinder).transform;
        water.GetComponent<Renderer>().material.SetFloat("_Metallic",.38f);water.GetComponent<Renderer>().material.SetFloat("_Glossiness",.78f);
        Make("Fountain · stepped pedestal",new Vector3(fountain.x,.52f,fountain.z),new Vector3(.55f,.55f,.55f),brass,PrimitiveType.Cylinder);
        Make("Fountain · sculpted finial",new Vector3(fountain.x,.94f,fountain.z),new Vector3(.37f,.45f,.37f),new Color(.77f,.66f,.43f),PrimitiveType.Sphere);
        MakeMesh("Side alley · carved arched entrance",new Vector3(alley.x,0,alley.z+5.55f),new Vector3(1.12f,1f,1f),brass,LokabrennaMeshLibrary.Load("AsgardArch"));
        for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++){float z=alley.z-3.6f+i*2.2f;float x=alley.x+side*2.05f;Make("Side arcade · round column",new Vector3(x,1.15f,z),new Vector3(.26f,1.15f,.26f),new Color(.42f,.4f,.36f),PrimitiveType.Cylinder);Make("Side arcade · capital",new Vector3(x,2.38f,z),new Vector3(.47f,.14f,.47f),brass,PrimitiveType.Cylinder);}
        MakeMesh("Market canopy · tailored saffron awning",new Vector3(market.x,2.48f,market.z),Vector3.one,new Color(.62f,.29f,.13f),LokabrennaMeshLibrary.Load("AsgardClothAwning"));
        for(int side=-1;side<=1;side+=2){Make("Market canopy · bronze upright",new Vector3(market.x+side*1.17f,1.32f,market.z-.78f),new Vector3(.11f,2.42f,.11f),new Color(.43f,.31f,.2f),PrimitiveType.Cylinder);Make("Awning edge · stitched brass piping",new Vector3(market.x+side*1.17f,2.51f,market.z),new Vector3(.045f,.045f,1.65f),brass,PrimitiveType.Sphere);}
        Make("Market counter · carved walnut",new Vector3(market.x,.7f,market.z),new Vector3(1.08f,.37f,.39f),new Color(.32f,.19f,.12f),PrimitiveType.Cylinder);
        Make("Market counter · bronze worktop",new Vector3(market.x,1.06f,market.z),new Vector3(1.2f,.045f,.47f),brass,PrimitiveType.Cylinder);
        for(int i=0;i<3;i++)Make("Market spice jar",new Vector3(market.x-.65f+i*.65f,1.28f,market.z),new Vector3(.25f,.26f,.25f),i==1?new Color(.74f,.29f,.19f):new Color(.38f,.62f,.47f),PrimitiveType.Cylinder);
        Make("Bifrost path marker",new Vector3(gate.x,.14f,gate.z-1.55f),new Vector3(3.1f,.1f,.08f),new Color(.78f,.65f,.42f));
    }
    void BuildPlazaDetails(){
        DistrictPoint center=MapPoint("asgard-square",0,-.2f);
        Make("Plaza medallion · stone ring",new Vector3(center.x,.095f,center.z),new Vector3(2.35f,.12f,2.35f),new Color(.42f,.44f,.41f),PrimitiveType.Cylinder);
        Make("Plaza medallion · bronze inset",new Vector3(center.x,.17f,center.z),new Vector3(1.68f,.06f,1.68f),new Color(.61f,.43f,.25f),PrimitiveType.Cylinder);
        Make("Plaza medallion · heart",new Vector3(center.x,.215f,center.z),new Vector3(.88f,.055f,.88f),new Color(.28f,.5f,.48f),PrimitiveType.Cylinder);
        for(int i=0;i<8;i++){float angle=i*Mathf.PI/4f;Vector3 p=new Vector3(center.x+Mathf.Cos(angle)*1.08f,.18f,center.z+Mathf.Sin(angle)*1.08f);Transform mark=Make("Medallion compass inlay",p,new Vector3(.08f,.045f,.48f),new Color(.86f,.72f,.47f)).transform;mark.rotation=Quaternion.Euler(0,-angle*Mathf.Rad2Deg,0);}
        foreach(Vector3 p in new[]{new Vector3(center.x-4.4f,.06f,center.z-4.9f),new Vector3(center.x+4.4f,.06f,center.z-4.9f),new Vector3(center.x-4.4f,.06f,center.z+5.8f),new Vector3(center.x+4.4f,.06f,center.z+5.8f)}){
            Make("Lantern plinth",p+Vector3.up*.18f,new Vector3(.48f,.36f,.48f),new Color(.35f,.34f,.3f),PrimitiveType.Cylinder);
            Make("Lantern post",p+Vector3.up*1.2f,new Vector3(.13f,1.8f,.13f),brass,PrimitiveType.Cylinder);
            Make("Lantern glow",p+Vector3.up*2.2f,new Vector3(.42f,.5f,.42f),new Color(1f,.65f,.27f),PrimitiveType.Sphere);
            Make("Lantern cap",p+Vector3.up*2.53f,new Vector3(.62f,.12f,.62f),new Color(.28f,.3f,.29f));
        }
        foreach(float x in new[]{-4.2f,4.2f}){
            Make("Carved plaza bench · curved stone seat",new Vector3(x,.48f,1f),new Vector3(2.2f,.18f,.6f),new Color(.43f,.29f,.19f),PrimitiveType.Cylinder);
            for(int leg=-1;leg<=1;leg+=2)Make("Bench · brass support",new Vector3(x+leg*.78f,.22f,1f),new Vector3(.12f,.46f,.24f),brass,PrimitiveType.Cylinder);
        }
    }
    void BuildTrees(){
        foreach(Vector3 p in new[]{new Vector3(-11.5f,.0f,-5.8f),new Vector3(11.5f,.0f,-5.8f),new Vector3(-11.5f,.0f,4.2f),new Vector3(11.5f,.0f,5.8f)}){
            Make("Courtyard olive · warm-barked trunk",p+Vector3.up*1.08f,new Vector3(.39f,2.16f,.39f),new Color(.31f,.2f,.13f),PrimitiveType.Cylinder);
            Transform limb=Make("Courtyard olive · angled bough",p+new Vector3(.28f,1.72f,0),new Vector3(.18f,1.35f,.18f),new Color(.37f,.25f,.16f),PrimitiveType.Cylinder).transform;limb.rotation=Quaternion.Euler(0,0,-38f);
            Transform crown=MakeMesh("Olive crown · wind-shaped clipped canopy",p+Vector3.up*2.72f,new Vector3(2.45f,1.62f,2.05f),new Color(.2f,.36f,.26f),LokabrennaMeshLibrary.Load("AsgardOliveCanopy")).transform;
            crown.rotation=Quaternion.Euler(0,17f,0);
            MakeMesh("Olive crown · sunlit canopy",p+new Vector3(-.52f,3.03f,.15f),new Vector3(1.15f,.85f,1.04f),new Color(.28f,.43f,.3f),LokabrennaMeshLibrary.Load("AsgardOliveCanopy"));
        }
    }
    void BuildStatueLandmark(){
        GameObject model=Resources.Load<GameObject>("Models/PolyHavenGothic/gothic_statue_1k");
        if(model==null){Make("Asgard lion relief · placeholder",new Vector3(-4.6f,.72f,1.15f),new Vector3(.75f,1.35f,.55f),brass,PrimitiveType.Sphere);return;}
        GameObject statue=Instantiate(model,new Vector3(-4.6f,0,1.15f),Quaternion.Euler(0,28f,0));statue.name="Asgard sentinel · Poly Haven CC0 gothic statue";statue.transform.localScale=Vector3.one;
        Renderer[] renderers=statue.GetComponentsInChildren<Renderer>();Bounds bounds=new Bounds(statue.transform.position,Vector3.zero);bool found=false;
        foreach(Renderer r in renderers){if(!found){bounds=r.bounds;found=true;}else bounds.Encapsulate(r.bounds);foreach(Material m in r.materials){m.color=new Color(.78f,.66f,.47f);m.SetFloat("_Metallic",.28f);m.SetFloat("_Glossiness",.3f);}}
        if(found)statue.transform.position+=Vector3.up*(.12f-bounds.min.y);
        Make("Sentinel · carved round dais",new Vector3(-4.6f,.18f,1.15f),new Vector3(1.5f,.36f,1.5f),brass,PrimitiveType.Cylinder);
    }
    void BuildPalaceHorizon(){
        // Five broad, structurally aligned arcades lead the eye through the gate to the palace.
        Color stoneGold=new Color(.72f,.63f,.47f),shadowStone=new Color(.18f,.24f,.25f),warmStone=new Color(.62f,.53f,.4f);
        for(int i=-2;i<=2;i++){
            float x=i*2.58f;float scale=i==0?1.11f:1f;float z=16.55f;
            MakeMesh("Palace gallery · sculpted arch",new Vector3(x,0,z),new Vector3(.68f*scale,1.04f*scale,.58f),i==0?new Color(.83f,.69f,.46f):stoneGold,LokabrennaMeshLibrary.Load("AsgardArch"));
            for(int side=-1;side<=1;side+=2){
                float px=x+side*1.28f;
                Make("Palace gallery · shaped pier",new Vector3(px,1.57f,z),new Vector3(.36f,1.58f,.42f),warmStone,PrimitiveType.Cylinder);
                Make("Palace gallery · carved foot",new Vector3(px,.22f,z),new Vector3(.7f,.34f,.68f),new Color(.6f,.49f,.35f),PrimitiveType.Cylinder);
                Make("Palace gallery · gold impost",new Vector3(px,3.07f,z),new Vector3(.62f,.18f,.54f),brass,PrimitiveType.Cylinder);
            }
            Make("Palace gallery · deep teal lancet",new Vector3(x,1.45f,z+.28f),new Vector3(.63f,1.8f,.08f),shadowStone,PrimitiveType.Capsule);
            Make("Palace gallery · amber window light",new Vector3(x,1.43f,z+.19f),new Vector3(.26f,1.05f,.045f),new Color(.63f,.4f,.2f),PrimitiveType.Capsule);
        }
        Make("Palace skyline · lower cornice",new Vector3(0,3.35f,16.72f),new Vector3(13.8f,.18f,1.15f),new Color(.53f,.43f,.31f));
        Make("Palace skyline · gold entablature",new Vector3(0,3.51f,16.72f),new Vector3(14.15f,.13f,1.25f),brass);
        Make("Palace · rotunda drum",new Vector3(0,3.95f,17.4f),new Vector3(2.45f,.95f,2.25f),new Color(.72f,.62f,.44f),PrimitiveType.Cylinder);
        Make("Palace · drum moulding",new Vector3(0,4.4f,17.4f),new Vector3(2.58f,.12f,2.38f),new Color(.91f,.7f,.4f),PrimitiveType.Cylinder);
        MakeMesh("Palace · crowned central dome",new Vector3(0,4.42f,17.4f),new Vector3(3.35f,3.8f,3f),new Color(.78f,.67f,.47f),LokabrennaMeshLibrary.Load("AsgardPalaceDome"));
        MakeMesh("Palace · central ceremonial arch",new Vector3(0,2.98f,16.06f),new Vector3(.86f,.9f,.5f),new Color(.92f,.76f,.47f),LokabrennaMeshLibrary.Load("AsgardArch"));
        foreach(float x in new[]{-5.2f,-3.6f,3.6f,5.2f}){
            Make("Palace · slender fluted tower",new Vector3(x,3.45f,17.55f),new Vector3(.38f,3.25f,.38f),new Color(.7f,.59f,.42f),PrimitiveType.Cylinder);
            Make("Palace · tower collar",new Vector3(x,5.03f,17.55f),new Vector3(.59f,.17f,.59f),new Color(.91f,.69f,.38f),PrimitiveType.Cylinder);
            MakeMesh("Palace · leaf-shaped roof spire",new Vector3(x,5.1f,17.55f),new Vector3(.86f,2.2f,.86f),new Color(.54f,.39f,.23f),LokabrennaMeshLibrary.Load("AsgardPalaceSpire"));
        }
        foreach(float x in new[]{-8.1f,8.1f}){
            Make("Palace wing · round pavilion base",new Vector3(x,.74f,17.9f),new Vector3(1.06f,1.5f,1.06f),new Color(.58f,.51f,.39f),PrimitiveType.Cylinder);
            Make("Palace wing · bronze roof collar",new Vector3(x,1.43f,17.1f),new Vector3(2.2f,.14f,1.2f),brass,PrimitiveType.Cylinder);
            foreach(float side in new[]{-1f,1f})Make("Palace wing · slender paired pier",new Vector3(x+side*.84f,1.65f,16.85f),new Vector3(.23f,1.45f,.23f),stoneGold,PrimitiveType.Cylinder);
            MakeMesh("Palace wing · crown spire",new Vector3(x,1.48f,17.9f),new Vector3(1.22f,2.3f,1.22f),new Color(.57f,.42f,.25f),LokabrennaMeshLibrary.Load("AsgardPalaceSpire"));
        }
    }
    void ApplyPbrFlagstones(GameObject surface){
        if(surface==null)return;Texture2D color=Resources.Load<Texture2D>("Art/Marble01/marble_01_diff_2k");Texture2D normal=Resources.Load<Texture2D>("Art/Marble01/marble_01_nor_gl_2k");
        if(color==null)return;Material material=surface.GetComponent<Renderer>().material;material.mainTexture=color;material.mainTextureScale=new Vector2(surface.transform.localScale.x/1.5f,surface.transform.localScale.z/1.5f);material.color=new Color(.9f,.84f,.73f);material.SetFloat("_Glossiness",.4f);
        if(normal!=null){material.SetTexture("_BumpMap",normal);material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",.42f);}
    }
    void BuildHomes(){
        // Paired round residences use the same stone, bronze and emerald accents as the palace.
        DistrictPoint odin=MapPoint("odin-study",-8.8f,-5.55f),frigga=MapPoint("frigga-house",8.8f,-5.55f);
        foreach(float x in new[]{odin.x,frigga.x}){
            float z=x<0?odin.z:frigga.z;
            Color plaster=x<0?new Color(.58f,.5f,.39f):new Color(.49f,.55f,.49f);
            Make("Residence · carved circular base",new Vector3(x,.2f,z),new Vector3(3.4f,.4f,3.4f),new Color(.39f,.34f,.27f),PrimitiveType.Cylinder);
            Make(x<0?"Odin’s round garden study":"Frigga’s circular house",new Vector3(x,1.27f,z),new Vector3(2.8f,2.1f,2.8f),plaster,PrimitiveType.Cylinder);
            Make("Residence · upper bronze moulding",new Vector3(x,2.23f,z),new Vector3(2.93f,.19f,2.93f),brass,PrimitiveType.Cylinder);
            MakeMesh("Residence · fluted domed roof",new Vector3(x,2.31f,z),new Vector3(2.85f,1.7f,2.85f),new Color(.52f,.37f,.23f),LokabrennaMeshLibrary.Load("AsgardPalaceDome"));
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4f;Vector3 pilaster=new Vector3(x+Mathf.Sin(a)*1.34f,1.23f,z+Mathf.Cos(a)*1.34f);Make("Residence · slim fluted pilaster",pilaster,new Vector3(.16f,1.75f,.16f),new Color(.79f,.67f,.49f),PrimitiveType.Cylinder);}
            Transform entry=Make("Residence · recessed arched entry",new Vector3(x,.86f,z+1.405f),new Vector3(.58f,1.18f,.1f),new Color(.1f,.17f,.17f),PrimitiveType.Capsule).transform;
            MakeMesh("Residence · carved entry surround",new Vector3(x,.24f,z+1.49f),new Vector3(.24f,.45f,.18f),brass,LokabrennaMeshLibrary.Load("AsgardArch"));
            Make("Residence · entry handle",new Vector3(x+.18f,.81f,z+1.48f),new Vector3(.05f,.1f,.04f),new Color(.95f,.74f,.38f),PrimitiveType.Sphere);
            for(int side=-1;side<=1;side+=2){
                Make("Residence · glowing oval window",new Vector3(x+side*1.03f,1.43f,z+1.12f),new Vector3(.25f,.48f,.08f),new Color(.92f,.58f,.29f),PrimitiveType.Capsule);
                Make("Residence · bronze window rim",new Vector3(x+side*1.03f,1.43f,z+1.18f),new Vector3(.34f,.57f,.07f),new Color(.86f,.65f,.39f),PrimitiveType.Capsule);
            }
        }
        Make("Frigga · low garden border",new Vector3(frigga.x+2.1f,.28f,frigga.z+1.75f),new Vector3(.55f,.56f,2.2f),new Color(.13f,.34f,.26f));
    }
    void BuildOuterDistrictPaths(){
        Color pathStone=new Color(.64f,.56f,.43f),inlay=new Color(.82f,.69f,.45f),deepStone=new Color(.38f,.35f,.29f);
        DistrictPoint westGallery=MapPoint("west-gallery",-13.15f,-1.35f),eastGallery=MapPoint("east-orchard",13.15f,-1.35f),gardenWalk=MapPoint("south-garden-walk",0,-10.15f);
        if(district.routes!=null)foreach(MapRouteData route in district.routes){
            if(route.id=="bifrost-approach")continue;
            for(int i=1;i<route.points.Count;i++){
                DistrictPoint a=route.points[i-1],b=route.points[i];Vector3 delta=new Vector3(b.x-a.x,0,b.z-a.z);float length=delta.magnitude;
                GameObject path=Make("Wayfinding route · "+route.id,new Vector3((a.x+b.x)*.5f,-.035f,(a.z+b.z)*.5f),new Vector3(route.width,.16f,length),pathStone);
                path.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,0);ApplyPbrFlagstones(path);
            }
        }
        foreach(float x in new[]{westGallery.x-1.5f,westGallery.x+1.5f,eastGallery.x-1.5f,eastGallery.x+1.5f})Make("Promenade · bronze paving inlay",new Vector3(x,.052f,-1.3f),new Vector3(.055f,.025f,17.7f),inlay);
        foreach(float z in new[]{gardenWalk.z-1.15f,gardenWalk.z+1.1f})Make("Garden walk · bronze paving inlay",new Vector3(0,.052f,z),new Vector3(26f,.025f,.055f),inlay);
        foreach(float side in new[]{-1f,1f}){
            float x=side<0?westGallery.x:eastGallery.x;
            for(int i=0;i<4;i++){
                float z=-8f+i*4.1f;
                Make("Promenade · carved lamp plinth",new Vector3(x+side*1.35f,.18f,z),new Vector3(.58f,.36f,.58f),deepStone,PrimitiveType.Cylinder);
                Make("Promenade · bronze lamp stem",new Vector3(x+side*1.35f,1.18f,z),new Vector3(.11f,1.65f,.11f),brass,PrimitiveType.Cylinder);
                Transform lamp=Make("Promenade · warm glass lantern",new Vector3(x+side*1.35f,2.12f,z),new Vector3(.34f,.42f,.34f),new Color(.96f,.62f,.29f),PrimitiveType.Sphere).transform;
                Renderer glow=lamp.GetComponent<Renderer>();glow.material.EnableKeyword("_EMISSION");glow.material.SetColor("_EmissionColor",new Color(.5f,.2f,.055f));
            }
            Make("Promenade · garden planter",new Vector3(x,.15f,3.2f),new Vector3(2.1f,.3f,1.25f),deepStone,PrimitiveType.Cylinder);
            for(int i=0;i<5;i++){float a=i*Mathf.PI*2f/5f;Make("Promenade · clipped evergreen",new Vector3(x+Mathf.Cos(a)*.62f,.78f,3.2f+Mathf.Sin(a)*.4f),new Vector3(.72f,1.2f,.72f),new Color(.15f,.3f,.23f),PrimitiveType.Sphere);}
            Make("Promenade · curved resting bench",new Vector3(x-side*.3f,.52f,-3.1f),new Vector3(1.65f,.18f,.55f),new Color(.38f,.26f,.17f),PrimitiveType.Cylinder);
        }
        Make("South walk · round memorial basin",new Vector3(gardenWalk.x,.18f,gardenWalk.z),new Vector3(2.2f,.35f,2.2f),deepStone,PrimitiveType.Cylinder);
        Make("South walk · water surface",new Vector3(gardenWalk.x,.37f,gardenWalk.z),new Vector3(1.72f,.035f,1.72f),new Color(.22f,.48f,.5f),PrimitiveType.Cylinder);
        MakeMesh("South walk · small original crown marker",new Vector3(gardenWalk.x,.35f,gardenWalk.z),new Vector3(.54f,.85f,.54f),brass,LokabrennaMeshLibrary.Load("AsgardPalaceSpire"));
    }
    void MakeCone(string name,Vector3 position,float radius,float height,Color color){
        const int sides=9;var vertices=new Vector3[sides+2];var triangles=new int[sides*6];vertices[0]=new Vector3(0,height,0);vertices[sides+1]=Vector3.zero;
        for(int i=0;i<sides;i++){float angle=i*Mathf.PI*2f/sides;vertices[i+1]=new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);int next=(i+1)%sides;triangles[i*6]=0;triangles[i*6+1]=i+1;triangles[i*6+2]=next+1;triangles[i*6+3]=sides+1;triangles[i*6+4]=next+1;triangles[i*6+5]=i+1;}
        var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();var go=new GameObject(name);go.transform.position=position;var filter=go.AddComponent<MeshFilter>();filter.sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.material=new Material(Shader.Find("Standard"));renderer.material.color=color;renderer.material.SetFloat("_Glossiness",.18f);
    }
    void TryStartEncounter(){
        foreach(EncounterDefinition candidate in encounterCatalog){
            if(Distance(player.position,new DistrictPoint{x=candidate.triggerX,z=candidate.triggerZ})<=candidate.triggerRadius){
                encounter=candidate;PlayerPrefs.SetString("lokabrenna-encounter",encounter.id);PlayerPrefs.Save();
                if(Application.isPlaying)StartCoroutine(GuestArrival());else {IEnumerator intro=GuestArrival();intro.MoveNext();}
                return;
            }
        }
    }
    void BuildInteraction(InteractionData item){
        Vector3 p=new Vector3(item.x,0,item.z);
        if(item.type=="bell"){
            Make("Bell stand · carved dark-stone pedestal",p+Vector3.up*.47f,new Vector3(.68f,.84f,.68f),new Color(.34f,.29f,.23f),PrimitiveType.Cylinder);
            Make("Bell stand · bronze neck",p+Vector3.up*.99f,new Vector3(.29f,.22f,.29f),brass,PrimitiveType.Cylinder);
            Transform bell=MakeMesh("Court bell · flared hand-cast bronze",p+Vector3.up*1.2f,new Vector3(1.08f,1.03f,1.08f),brass,LokabrennaMeshLibrary.Load("AsgardCourtBell")).transform;
            Transform crown=MakeMesh("Court bell · crown loop",p+Vector3.up*1.7f,new Vector3(.34f,.34f,.34f),new Color(.9f,.7f,.38f),LokabrennaMeshLibrary.Load("GoldBeltRing")).transform;crown.rotation=Quaternion.Euler(90,0,0);
            Make("Court bell · clapper",p+Vector3.up*1.13f,new Vector3(.1f,.16f,.1f),new Color(.24f,.2f,.16f),PrimitiveType.Sphere);
            props.Add(new WorldProp{id=item.id,label=item.label,transform=bell,radius=item.radius,noisy=item.noisy||item.type=="bell",movable=item.movable,solid=item.solid,hearing=item.hearing>0?item.hearing:12f,distraction=item.distraction});
        }else if(item.id=="signal-lamp"){
            Make("Signal lamp · octagonal stone foot",p+Vector3.up*.18f,new Vector3(.96f,.32f,.96f),new Color(.37f,.34f,.28f),PrimitiveType.Cylinder);
            Make("Signal lamp · slender bronze post",p+Vector3.up*.82f,new Vector3(.14f,1.22f,.14f),brass,PrimitiveType.Cylinder);
            Transform lamp=Make("Signal lamp · warning glass",p+Vector3.up*1.47f,new Vector3(.4f,.56f,.4f),new Color(.76f,.3f,.17f),PrimitiveType.Capsule).transform;
            Renderer lampRenderer=lamp.GetComponent<Renderer>();lampRenderer.material.EnableKeyword("_EMISSION");lampRenderer.material.SetColor("_EmissionColor",new Color(.42f,.095f,.025f));
            for(int side=-1;side<=1;side+=2)Make("Signal lamp · protective bronze strut",p+new Vector3(side*.29f,1.45f,0),new Vector3(.055f,.72f,.055f),brass,PrimitiveType.Cylinder);
            Make("Lamp finial",p+Vector3.up*1.82f,new Vector3(.16f,.22f,.16f),brass,PrimitiveType.Sphere);
            props.Add(new WorldProp{id=item.id,label=item.label,transform=lamp,radius=item.radius,noisy=item.noisy, movable=item.movable,solid=item.solid,hearing=item.hearing>0?item.hearing:9f,distraction=item.distraction});
        }else if(item.type=="illusion"){
            Make("Hall of mirrors · sculpted round foot",p+Vector3.up*.2f,new Vector3(1.1f,.28f,.72f),new Color(.37f,.34f,.28f),PrimitiveType.Cylinder);
            Transform glass=Make("Hall of mirrors · circular enchanted glass",p+Vector3.up*1.34f+Vector3.forward*.04f,new Vector3(1.18f,1.18f,.12f),new Color(.19f,.52f,.58f),PrimitiveType.Sphere).transform;
            Transform frame=MakeMesh("Hall of mirrors · fine bronze ring",p+Vector3.up*1.34f+Vector3.forward*.12f,new Vector3(2.25f,2.25f,2.25f),brass,LokabrennaMeshLibrary.Load("GoldBeltRing")).transform;frame.rotation=Quaternion.Euler(90,0,0);
            props.Add(new WorldProp{id=item.id,label=item.label,transform=glass,radius=item.radius,noisy=item.noisy,movable=item.movable,solid=item.solid,hearing=item.hearing,distraction=item.distraction});
        }else if(item.type=="crate"){
            Transform crate=MakeMesh("Market basket · coopered walnut barrel",p+Vector3.up*.55f,new Vector3(1.02f,1.12f,1.02f),new Color(.39f,.24f,.14f),LokabrennaMeshLibrary.Load("AsgardMarketBarrel")).transform;
            crate.gameObject.AddComponent<MeshCollider>().convex=true;
            for(int ring=-1;ring<=1;ring+=2){Transform hoop=MakeMesh("Barrel · bronze binding",Vector3.zero,new Vector3(.99f,.99f,.99f),new Color(.52f,.37f,.21f),LokabrennaMeshLibrary.Load("GoldBeltRing")).transform;hoop.SetParent(crate,false);hoop.localPosition=new Vector3(0,ring*.29f,0);}
            Rigidbody body=crate.gameObject.AddComponent<Rigidbody>();body.mass=1f;body.drag=.65f;body.angularDrag=1.1f;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            props.Add(new WorldProp{id=item.id,label=item.label,transform=crate,body=body,radius=item.radius,noisy=item.noisy,movable=item.movable,solid=item.solid,hearing=item.hearing,distraction=item.distraction});
        }else if(item.type=="hidden"){
            Color relic=item.id=="odin-sceptre"?brass:new Color(.22f,.66f,.9f);
            Transform discovery=Make(item.id=="odin-sceptre"?"Sceptre glint · tucked behind study":"Tesseract glint · hidden in garden",p+Vector3.up*.23f,new Vector3(.24f,.34f,.24f),relic,PrimitiveType.Sphere).transform;
            discovery.GetComponent<Renderer>().enabled=false;
            props.Add(new WorldProp{id=item.id,label=item.label,transform=discovery,radius=item.radius,noisy=false,movable=false,solid=false});
        }else if(item.type=="flame"){
            Make("Hearth plinth",p+Vector3.up*.35f,new Vector3(.85f,.7f,.8f),new Color(.35f,.28f,.22f),PrimitiveType.Cylinder);
            Transform fire=Make("Hearth ember",p+Vector3.up*.95f,new Vector3(.48f,.72f,.48f),new Color(1f,.36f,.08f),PrimitiveType.Sphere).transform;
            props.Add(new WorldProp{id=item.id,label=item.label,transform=fire,radius=item.radius,noisy=true,hearing=item.hearing,distraction=1.2f});
        }
    }
    GameObject Make(string name,Vector3 position,Vector3 scale,Color color,PrimitiveType type=PrimitiveType.Cube){
        GameObject go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=position;go.transform.localScale=scale;
        Renderer r=go.GetComponent<Renderer>();if(r!=null){r.material=new Material(Shader.Find("Standard"));r.material.color=color;r.material.SetFloat("_Glossiness",.34f);r.material.SetFloat("_Metallic",color==brass?.48f:.08f);}
        return go;
    }
    IEnumerator GuestArrival(){
        if(encounterStarted)yield break;
        encounterStarted=true;introPlaying=true;introHoldUntil=Time.time+3.2f;
        alarmRaised=true;OpenGateLook(false);guardState=GuardState.Suspicious;investigatePoint=guard.position;
        SetToast("ROYAL WATCH: “The Bifrost is sealed while we search for Odin’s missing golden apple.”",4f);
        yield return new WaitForSeconds(2.35f);
        SetToast("LOKI: “A tragic day for apples. And gates.”",3.5f);
        yield return new WaitForSeconds(.85f);introPlaying=false;
    }
    void SetCursorForCurrentScreen(){
        bool shouldCapture=screen==ScreenState.Playing&&!mapOpen;
        if(cursorCaptured==shouldCapture)return;
        cursorCaptured=shouldCapture;Cursor.lockState=shouldCapture?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!shouldCapture;
    }
    void MovePlayer(){
        float x=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1f:0f)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1f:0f);
        float z=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1f:0f)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1f:0f);
        float moveSpeed=(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)?6.8f:4.1f)*(aspect==Aspect.Falcon?1.65f:aspect==Aspect.Ant?.62f:aspect==Aspect.Hawk?1.2f:1f);
        Camera cam=Camera.main;Vector3 camForward=cam!=null?cam.transform.forward:Vector3.forward;camForward.y=0;camForward.Normalize();Vector3 camRight=cam!=null?cam.transform.right:Vector3.right;camRight.y=0;camRight.Normalize();
        Vector3 desiredVelocity=(camRight*x+camForward*z).normalized*moveSpeed;float response=desiredVelocity.sqrMagnitude>.001f?28f:34f;moveVelocity=Vector3.MoveTowards(moveVelocity,desiredVelocity,response*Time.deltaTime);Vector3 delta=moveVelocity*Time.deltaTime;
        if(delta.sqrMagnitude<.000001f){lastPlayerSpeed=0;walkPhase=0;if(playerArmL!=null)playerArmL.localRotation=Quaternion.identity;if(playerArmR!=null)playerArmR.localRotation=Quaternion.identity;player.position=new Vector3(player.position.x,aspect==Aspect.Ant?.24f:aspect==Aspect.Hawk?1.3f:1f,player.position.z);return;}
        Vector3 p=player.position+delta;p.x=Mathf.Clamp(p.x,district.bounds.minX+.45f,district.bounds.maxX-.45f);p.z=Mathf.Clamp(p.z,district.bounds.minZ+.5f,district.bounds.maxZ-.5f);
        foreach(WorldProp prop in props){if(!prop.solid||prop.transform==null)continue;Vector2 separation=new Vector2(p.x-prop.transform.position.x,p.z-prop.transform.position.z);if(separation.magnitude<.7f){Vector2 old=new Vector2(player.position.x-prop.transform.position.x,player.position.z-prop.transform.position.z);if(old.sqrMagnitude>.01f){old.Normalize();p.x=prop.transform.position.x+old.x*.7f;p.z=prop.transform.position.z+old.y*.7f;}else p=player.position;}}
        if(encounterStarted&&Mathf.Abs(p.x-district.bifrost.x)<2.05f&&p.z>district.bifrost.z-.9f&&!disguised&&Vector3.Distance(guard.position,new Vector3(district.bifrost.x,.85f,district.bifrost.z))<2.5f)p.z=district.bifrost.z-.9f;
        lastPlayerSpeed=moveVelocity.magnitude;
        walkPhase+=Time.deltaTime*Mathf.Lerp(5.5f,15f,Mathf.Clamp01(lastPlayerSpeed/6.8f));p.y=aspect==Aspect.Ant?.24f:aspect==Aspect.Hawk?1.3f:1f;player.position=p;player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(moveVelocity),1f-Mathf.Exp(-15f*Time.deltaTime));
        if(playerArmL!=null)playerArmL.localRotation=Quaternion.Euler(Mathf.Sin(walkPhase)*24f,0,0);if(playerArmR!=null)playerArmR.localRotation=Quaternion.Euler(-Mathf.Sin(walkPhase)*24f,0,0);if(playerCape!=null)playerCape.localRotation=Quaternion.Euler(0,0,Mathf.Sin(walkPhase*.5f)*4f);
    }
    void UpdateCamera(){
        Camera cam=Camera.main;if(cam==null||player==null)return;
        if(Input.GetMouseButton(1)){cameraYaw+=Input.GetAxis("Mouse X")*3.2f;cameraPitch=Mathf.Clamp(cameraPitch-Input.GetAxis("Mouse Y")*2.1f,12f,52f);}
        cameraDistance=Mathf.Clamp(cameraDistance-Input.GetAxis("Mouse ScrollWheel")*3.2f,4.2f,8.5f);
        Vector3 target=player.position+Vector3.up*1.02f;Vector3 offset=Quaternion.Euler(cameraPitch,cameraYaw,0)*new Vector3(0,0,-cameraDistance);float safeDistance=offset.magnitude;
        foreach(RaycastHit hit in Physics.SphereCastAll(target,.24f,offset.normalized,safeDistance,~0,QueryTriggerInteraction.Ignore)){
            if(hit.transform==null||hit.transform==player||hit.transform.IsChildOf(player)||hit.transform==guard||hit.transform.IsChildOf(guard))continue;
            safeDistance=Mathf.Min(safeDistance,Mathf.Max(.55f,hit.distance-.18f));
        }
        Vector3 desired=target+offset.normalized*safeDistance;cam.transform.position=Vector3.SmoothDamp(cam.transform.position,desired,ref cameraFollowVelocity,.075f,100f,Time.deltaTime);
        Quaternion look=Quaternion.LookRotation(target-cam.transform.position,Vector3.up);cam.transform.rotation=Quaternion.Slerp(cam.transform.rotation,look,1f-Mathf.Exp(-16f*Time.deltaTime));cam.orthographic=false;cam.fieldOfView=53f;
    }
    void ReadAbilityInput(){
        if(Input.GetKeyDown(KeyCode.S))lastSKey=Time.time;
        if(Input.GetKeyDown(KeyCode.S)&&Input.GetKey(KeyCode.E)){Shapeshift();return;}
        if(Input.GetKeyDown(KeyCode.Alpha1)||Input.GetKeyDown(KeyCode.Alpha2)||Input.GetKeyDown(KeyCode.Alpha3)||Input.GetKeyDown(KeyCode.Alpha4)||Input.GetKeyDown(KeyCode.Alpha5)){
            int n=Input.GetKeyDown(KeyCode.Alpha1)?1:Input.GetKeyDown(KeyCode.Alpha2)?2:Input.GetKeyDown(KeyCode.Alpha3)?3:Input.GetKeyDown(KeyCode.Alpha4)?4:5;
            if(Time.time-lastSKey<.7f)SetAspect((Aspect)(n-1));
            else if(Input.GetKey(KeyCode.T))ChangeMass(n);
        }
        if(Input.GetKeyDown(KeyCode.E)){if(Input.GetKey(KeyCode.S))Shapeshift();else MisdirectSound();}
        if(Input.GetKeyDown(KeyCode.Q))CastIllusion();
        if(Input.GetKeyDown(KeyCode.F))Shapeshift();
        if(Input.GetKeyDown(KeyCode.C))Conjure();
        if(Input.GetKeyDown(KeyCode.Space))Interact();
        if(Input.GetKeyDown(KeyCode.T)&&Input.GetKey(KeyCode.Alpha1))ChangeMass(1);
        if(Input.GetKeyDown(KeyCode.T)&&Input.GetKey(KeyCode.Alpha2))ChangeMass(2);
        if(Input.GetKeyDown(KeyCode.T)&&Input.GetKey(KeyCode.Alpha3))ChangeMass(3);
        if(Input.GetKeyDown(KeyCode.G)){if(sceptreFound){soundPoint=player.position+player.forward*3.5f;EmitSound(soundPoint,11f,"Odin’s sceptre");SetToast("The sceptre draws the guard’s attention to its glimmer.",2.5f);}else SetToast("The Allfather’s sceptre is somewhere in the district. No marker, naturally.",2.4f);}
    }
    void SetAspect(Aspect value){
        aspect=value;disguised=false;disguiseVisual.gameObject.SetActive(false);lokiHumanVisual.gameObject.SetActive(value==Aspect.Human);hawkVisual.gameObject.SetActive(value==Aspect.Hawk);falconVisual.gameObject.SetActive(value==Aspect.Falcon);antVisual.gameObject.SetActive(value==Aspect.Ant);wildfireVisual.gameObject.SetActive(value==Aspect.Wildfire);playerRenderer.enabled=false;playerRenderer.material.color=new Color(.15f,.34f,.23f);
        playerHorns.gameObject.SetActive(value==Aspect.Human);player.localScale=value==Aspect.Ant?new Vector3(.3f,.34f,.3f):value==Aspect.Hawk||value==Aspect.Falcon?new Vector3(.58f,.68f,.58f):value==Aspect.Wildfire?new Vector3(.62f,.76f,.62f):new Vector3(.72f,1f,.72f);player.position=new Vector3(player.position.x,value==Aspect.Ant?.24f:value==Aspect.Hawk?1.3f:1f,player.position.z);
        if(Camera.main!=null)cameraDistance=value==Aspect.Hawk?12.2f:8.6f;
        StartCoroutine(MagicBurst(player.position, new Color(.05f,.88f,.28f),.6f));
        SetToast("Aspect: "+value+". The same world rules still apply.",2.6f);
    }
    WorldProp AimedProp(){WorldProp nearest=null;float d=4.5f;Vector3 point=AimPoint();foreach(WorldProp p in props){if(p.transform==null||!p.movable)continue;float next=Vector2.Distance(new Vector2(point.x,point.z),new Vector2(p.transform.position.x,p.transform.position.z));if(next<d){d=next;nearest=p;}}return nearest;}
    void ChangeMass(int mode){WorldProp p=AimedProp();if(p==null){SetToast("Aim at a movable object within reach first.",2.5f);return;}SetPropMass(p,mode);SetToast(p.label+" now feels "+(mode==1?"light as a feather.":mode==2?"like itself again.":"absurdly heavy."),2.8f);}
    void SetPropMass(WorldProp p,int mode){p.mass=mode==1?.35f:mode==2?1f:3.2f;if(p.body!=null)p.body.mass=p.mass;p.transform.localScale=new Vector3(.9f,.9f,.9f)*Mathf.Lerp(1.1f,.86f,Mathf.Clamp01(p.mass/3.2f));StartCoroutine(ShowTether(p.transform));}
    void Conjure(){if(conjured!=null)Destroy(conjured.gameObject);conjured=MakeMesh("Conjured solid golden apple",player.position+player.forward*.55f+Vector3.up*.35f,new Vector3(.82f,.82f,.82f),new Color(.93f,.58f,.13f),LokabrennaMeshLibrary.Load("AsgardGoldenApple")).transform;conjured.gameObject.AddComponent<SphereCollider>().radius=.31f;Transform stem=Make("Apple · dark stem",conjured.position+Vector3.up*.24f,new Vector3(.08f,.19f,.08f),new Color(.19f,.18f,.11f),PrimitiveType.Cylinder).transform;stem.SetParent(conjured,false);stem.localPosition=new Vector3(0,.29f,0);Rigidbody body=conjured.gameObject.AddComponent<Rigidbody>();body.mass=.25f;body.drag=.15f;body.isKinematic=true;conjured.SetParent(player,false);conjured.localPosition=new Vector3(.4f,.34f,.58f);StartCoroutine(ConjureFlicker(conjured));SetToast("A solid golden apple flickers into your hand. SPACE sends it clattering.",3f);}
    IEnumerator ConjureFlicker(Transform item){if(item==null)yield break;Vector3 target=item.localScale;item.localScale=Vector3.zero;float t=0;while(t<.2f&&item!=null){t+=Time.deltaTime;item.localScale=Vector3.Lerp(Vector3.zero,target,t/.2f);yield return null;}}
    IEnumerator MagicBurst(Vector3 point,Color color,float duration){
        GameObject ring=GameObject.CreatePrimitive(PrimitiveType.Sphere);ring.name="Loki magic · brief emerald shimmer";ring.transform.position=point+Vector3.up*.18f;ring.transform.localScale=Vector3.one*.16f;Collider collider=ring.GetComponent<Collider>();if(collider!=null)Destroy(collider);
        Renderer renderer=ring.GetComponent<Renderer>();Material material=new Material(Shader.Find("Standard"));material.color=color;material.SetFloat("_Metallic",.18f);material.SetFloat("_Glossiness",.8f);material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*.65f);renderer.material=material;
        float t=0;while(t<duration&&ring!=null){t+=Time.deltaTime;float q=Mathf.Clamp01(t/duration);ring.transform.localScale=Vector3.one*Mathf.Lerp(.16f,1.65f,q);Color c=color;c.a=1f-q;renderer.material.color=c;yield return null;}if(ring!=null)Destroy(ring);
    }
    IEnumerator ShowTether(Transform target){
        if(target==null||player==null)yield break;GameObject go=new GameObject("Telekinesis · green energy thread");LineRenderer line=go.AddComponent<LineRenderer>();line.positionCount=3;line.useWorldSpace=true;line.startWidth=.065f;line.endWidth=.025f;line.material=new Material(Shader.Find("Sprites/Default"));Color c=new Color(.12f,1f,.44f,.86f);line.startColor=c;line.endColor=new Color(.12f,1f,.44f,.08f);
        float t=0;while(t<.32f&&target!=null&&player!=null){t+=Time.deltaTime;Vector3 a=player.position+Vector3.up*.85f,b=target.position;line.SetPosition(0,a);line.SetPosition(1,(a+b)*.5f+Vector3.up*.35f);line.SetPosition(2,b);yield return null;}if(go!=null)Destroy(go);
    }
    void Teleport(){if(!tesseractFound){SetToast("No Tesseract yet. Frigga keeps her secrets in the west garden.",2.5f);return;}if(teleportCharges<=0){SetToast("The Tesseract is recharging very, very slowly.",2.5f);return;}Vector3 p=AimPoint();Vector3 jump=p-player.position;if(jump.magnitude>4.5f)jump=jump.normalized*4.5f;player.position+=jump;teleportCharges--;StartCoroutine(MagicBurst(player.position,new Color(.22f,.62f,1f),.45f));SetToast("Tesseract jump · "+teleportCharges+" charges remain.",2.5f);StartCoroutine(RechargeTeleport());}
    IEnumerator RechargeTeleport(){yield return new WaitForSeconds(75f);teleportCharges=Mathf.Min(10,teleportCharges+1);}
    void UpdateGuard(){
        if(Time.time<introHoldUntil){suspicion=0;return;}
        UpdateWorldProps();
        bool seesPlayer=CanSeePlayer();
        if(decoy!=null&&CanSeePoint(decoy.position,7.5f)&&guardState!=GuardState.Chase){investigatePoint=decoy.position;guardState=GuardState.Investigate;}
        EvaluatePerception(seesPlayer,Time.deltaTime);
        if(soundActive&&Time.time<soundUntil){investigatePoint=soundPoint;if(guardState!=GuardState.Chase)guardState=GuardState.Investigate;}
        else if(guardState==GuardState.Investigate&&Vector3.Distance(guard.position,investigatePoint)<.45f){guardState=GuardState.Suspicious;}
        if(guardState==GuardState.Suspicious&&suspicion<.08f&&!soundActive)guardState=GuardState.Return;
        if(guardState==GuardState.Chase&&Vector3.Distance(guard.position,player.position)<1.25f&&!disguised)CaughtNow();
        Vector3 target;float speed=district.guard.speed;
        if(guardState==GuardState.Chase){target=player.position;target.y=.85f;speed=2.75f;}
        else if(guardState==GuardState.Investigate||guardState==GuardState.Suspicious){target=investigatePoint;target.y=.85f;speed+=.25f;}
        else {DistrictPoint waypoint=district.guard.points[patrolIndex];target=new Vector3(waypoint.x,.85f,waypoint.z);if(guardState==GuardState.Return)guardState=GuardState.Patrol;}
        guard.position=Vector3.MoveTowards(guard.position,target,speed*Time.deltaTime);
        Vector3 face=target-guard.position;if(face.sqrMagnitude>.01f)guard.rotation=Quaternion.Slerp(guard.rotation,Quaternion.LookRotation(new Vector3(face.x,0,face.z)),Time.deltaTime*3f);
        if(guardState==GuardState.Patrol&&Vector3.Distance(guard.position,target)<.16f){patrolIndex=1-patrolIndex;}
        if(decoy!=null&&Vector3.Distance(guard.position,decoy.position)<.7f&&guardState==GuardState.Investigate){SetToast("The guard circles the illusion. It is almost convincing.",2.5f);soundActive=false;guardState=GuardState.Suspicious;}
    }
    bool CanSeePlayer(){
        if(!CanSeePoint(player.position,7.5f))return false;
        return true;
    }
    void EvaluatePerception(bool seesPlayer,float deltaTime){
        if(seesPlayer&&!disguised&&aspect!=Aspect.Ant&&aspect!=Aspect.Wildfire){suspicion=Mathf.MoveTowards(suspicion,1f,deltaTime*(lastPlayerSpeed>5.5f?.8f:.48f));if(suspicion>.62f)guardState=GuardState.Chase;else guardState=GuardState.Suspicious;}
        else if(seesPlayer&&disguised&&lastPlayerSpeed>5.5f){suspicion=Mathf.MoveTowards(suspicion,.72f,deltaTime*.42f);if(suspicion>.68f)guardState=GuardState.Suspicious;}
        else suspicion=Mathf.MoveTowards(suspicion,0f,deltaTime*(guardState==GuardState.Chase?.08f:.2f));
        if(disguised&&guardState==GuardState.Chase&&lastPlayerSpeed<=5.5f)guardState=GuardState.Suspicious;
        if((aspect==Aspect.Ant||aspect==Aspect.Wildfire)&&seesPlayer&&guardState==GuardState.Chase)guardState=GuardState.Suspicious;
    }
    bool CanSeePoint(Vector3 point,float range){
        Vector3 delta=point-guard.position;delta.y=0;float distance=delta.magnitude;
        if(distance>range||distance<.01f)return false;
        if(Vector3.Dot(guard.forward,delta.normalized)<.38f)return false;
        foreach(WorldProp prop in props){if(!prop.solid||prop.transform==null)continue;Vector2 a=new Vector2(guard.position.x,guard.position.z),b=new Vector2(point.x,point.z),c=new Vector2(prop.transform.position.x,prop.transform.position.z);Vector2 ab=b-a;float t=Mathf.Clamp01(Vector2.Dot(c-a,ab)/Mathf.Max(.01f,ab.sqrMagnitude));if(Vector2.Distance(a+ab*t,c)<.75f)return false;}
        return true;
    }
    Vector3 AimPoint(){
        Camera cam=Camera.main;if(cam!=null){Ray ray=cam.ScreenPointToRay(Input.mousePosition);Plane ground=new Plane(Vector3.up,Vector3.zero);float distance;if(ground.Raycast(ray,out distance)){Vector3 point=ray.GetPoint(distance);point.x=Mathf.Clamp(point.x,district.bounds.minX+.7f,district.bounds.maxX-.7f);point.z=Mathf.Clamp(point.z,district.bounds.minZ+.7f,district.bounds.maxZ-.7f);return point;}}
        return player.position+player.forward*3f;
    }
    void EmitSound(Vector3 point,float radius,string source){
        float distance=Vector3.Distance(guard.position,point);
        if(distance<radius){soundPoint=point;soundUntil=Time.time+4.2f;soundActive=true;investigatePoint=point;if(guardState!=GuardState.Chase)guardState=GuardState.Investigate;SetToast("A sound carries across the square. The guard investigates "+source+".",3.2f);}
        else {soundActive=false;SetToast("The sound echoes somewhere beyond the guard's hearing.",2.4f);}
    }
    void MisdirectSound(){Vector3 point=AimPoint();EmitSound(point,10f,"the misdirection");}
    void UpdateWorldProps(){
        if(soundActive&&Time.time>=soundUntil)soundActive=false;
        foreach(WorldProp prop in props){if(prop.transform==null)continue;if(prop.noisy&&Vector3.Distance(guard.position,prop.transform.position)<.65f&&guardState==GuardState.Investigate){investigatePoint=prop.transform.position;}}
    }
    void CastIllusion(){
        if(decoy!=null)Destroy(decoy.gameObject);
        Vector3 point=AimPoint();GameObject copy=Instantiate(player.gameObject,new Vector3(point.x,player.position.y,point.z),player.rotation);copy.name="Loki’s green-shimmer illusion";decoy=copy.transform;foreach(Collider c in copy.GetComponentsInChildren<Collider>())c.enabled=false;foreach(Renderer r in copy.GetComponentsInChildren<Renderer>()){r.material.color=Color.Lerp(r.material.color,new Color(.05f,.92f,.34f),.48f);r.material.EnableKeyword("_EMISSION");r.material.SetColor("_EmissionColor",new Color(.02f,.25f,.06f));}
        decoyUntil=Time.time+8f;investigatePoint=point;
        StartCoroutine(MagicBurst(point, new Color(.05f,.88f,.28f),.7f));
        if(CanSeePoint(point,7.5f)&&guardState!=GuardState.Chase){guardState=GuardState.Investigate;SetToast("The guard notices a strange figure at your aimed location.",3.4f);}
        else SetToast("An illusion shimmers into place. It is outside the guard's view for now.",3f);
    }
    void Shapeshift(){
        if(Vector3.Distance(player.position,guard.position)>4.5f){SetToast("No nearby face to borrow. Get closer to a person.",2.5f);return;}
        if(disguised){SetToast("The borrowed uniform is already doing its best. "+Mathf.CeilToInt(disguiseUntil-Time.time)+" seconds remain.",3f);return;}
        disguised=true;disguiseUntil=Time.time+9f;aspect=Aspect.Human;lokiHumanVisual.gameObject.SetActive(false);disguiseVisual.gameObject.SetActive(true);playerRenderer.enabled=false;playerRenderer.material.color=guardRed;player.localScale=new Vector3(.72f,1f,.72f);playerHorns.gameObject.SetActive(false);if(disguiseHelmet!=null)disguiseHelmet.gameObject.SetActive(true);StartCoroutine(MagicBurst(player.position,new Color(.05f,.88f,.28f),.45f));
        SetToast("You look like the watch. Sprinting or ringing props can still draw attention.",4f);
    }
    void EndDisguise(){disguised=false;disguiseVisual.gameObject.SetActive(false);SetAspect(Aspect.Human);playerHorns.gameObject.SetActive(true);SetToast("The disguise fades. Loki returns the uniform in spirit only.",3f);}
    void Interact(){
        foreach(WorldProp hidden in props){if(hidden.transform==null||!hidden.id.StartsWith("odin-")&&!hidden.id.StartsWith("frigga-"))continue;float d=Vector2.Distance(new Vector2(player.position.x,player.position.z),new Vector2(hidden.transform.position.x,hidden.transform.position.z));if(d<=hidden.radius){if(hidden.id=="odin-sceptre")sceptreFound=true;else tesseractFound=true;hidden.transform.gameObject.SetActive(false);SetToast(hidden.id=="odin-sceptre"?"Hidden discovery: Odin’s sceptre. Its quiet glint can steal a guard’s attention.":"Hidden discovery: a Tesseract with ten slow-recharging jumps. Click TELEPORT to spend one.",4f);return;}}
        WorldProp nearest=null;float best=float.MaxValue;
        foreach(WorldProp prop in props){float d=Vector2.Distance(new Vector2(player.position.x,player.position.z),new Vector2(prop.transform.position.x,prop.transform.position.z));if(d<best){best=d;nearest=prop;}}
        if(nearest==null||best>nearest.radius){if(conjured!=null&&Vector3.Distance(player.position,conjured.position)<1.6f){Vector3 landing=player.position+player.forward*1.3f;landing.y=.42f;conjured.SetParent(null);conjured.position=landing;Rigidbody rb=conjured.GetComponent<Rigidbody>();if(rb!=null){rb.isKinematic=false;rb.AddForce(player.forward*4.5f,ForceMode.Impulse);}EmitSound(landing,10f,"the conjured apple");SetToast("The apple skitters across the stones with a suspicious clonk.",2.7f);return;}SetToast("Nothing within reach. Move close to a prop, or aim E / Q toward the world.",3f);return;}
        if(nearest.id=="odin-sceptre"&&!sceptreFound){sceptreFound=true;nearest.transform.gameObject.SetActive(false);SetToast("Hidden discovery: Odin’s sceptre. Its quiet glint can steal a guard’s attention.",4f);return;}
        if(nearest.id=="frigga-tesseract"&&!tesseractFound){tesseractFound=true;nearest.transform.gameObject.SetActive(false);SetToast("Hidden discovery: a Tesseract with ten slow-recharging jumps. Click TELEPORT to spend one.",4f);return;}
        if(nearest.movable){Vector3 push=player.forward;Vector3 delta=new Vector3(push.x,0,push.z)*(1.1f/Mathf.Max(.35f,nearest.mass));Vector3 next=nearest.transform.position+delta;next.x=Mathf.Clamp(next.x,district.bounds.minX,district.bounds.maxX);next.z=Mathf.Clamp(next.z,district.bounds.minZ,district.bounds.maxZ);nearest.transform.position=next;if(nearest.body!=null){nearest.body.position=next;nearest.body.AddForce(push*2.1f,ForceMode.Impulse);}SetToast(nearest.label+" scrapes across the stones. Weight changes how far it yields.",3f);if(nearest.noisy)EmitSound(nearest.transform.position,nearest.hearing,nearest.label);return;}
        if(nearest.id=="hearth-flame"&&aspect==Aspect.Wildfire){EmitSound(nearest.transform.position,nearest.hearing,"a friendly flame");SetToast("Wildfire answers the hearth; the guard looks toward its flicker.",3f);return;}
        if(nearest.noisy){nearest.transform.Rotate(0,0,22f);EmitSound(nearest.transform.position,nearest.hearing,nearest.label);soundUntil+=nearest.distraction;}
        else SetToast("You shift the "+nearest.label+". It catches the lantern light.",2.5f);
    }
    void CheckGate(){
        if(Mathf.Abs(player.position.x-district.bifrost.x)>2.05f||player.position.z<district.bifrost.z+.2f)return;
        float guardDistance=Vector2.Distance(new Vector2(player.position.x,player.position.z),new Vector2(guard.position.x,guard.position.z));
        if(disguised||aspect==Aspect.Falcon||guardDistance>2.7f){escaped=true;OpenGateLook(true);SetToast("Loki slips through while the watch is elsewhere. The guard is still investigating.",5f);if(Application.isPlaying)StartCoroutine(GoToEpilogue());else epilogueTransitionRequested=true;}
        else {player.position=new Vector3(player.position.x,player.position.y,7.25f);if(Time.time>toastUntil)SetToast("The guard is holding the passage. Break their sightline, draw them away, or blend in.",3.2f);}
    }
    IEnumerator GoToEpilogue(){yield return new WaitForSeconds(1.5f);string[] parts=encounter.epilogueScene.Split('/');string scene=parts[parts.Length-1].Replace(".unity","");SceneManager.LoadScene(scene);}
    void OpenGateLook(bool open){foreach(GameObject bar in gateBars)if(bar!=null)bar.SetActive(!open);}
    void CaughtNow(){if(screen==ScreenState.Caught)return;screen=ScreenState.Caught;caughtTimer=1.8f;SetToast("Caught! The guard asks Loki to explain the alarm. Loki requests a rain check.",3f);}
    void SetToast(string message,float seconds){toast=message;toastUntil=Time.time+seconds;}
    DistrictPoint MapPoint(string id,float fallbackX,float fallbackZ){foreach(LandmarkData point in district.landmarks)if(point.id==id)return new DistrictPoint{x=point.x,z=point.z};return new DistrictPoint{x=fallbackX,z=fallbackZ};}
    static float Distance(Vector3 p,DistrictPoint q){return Vector2.Distance(new Vector2(p.x,p.z),new Vector2(q.x,q.z));}

    void CreateStyles(){
        titleStyle=new GUIStyle(GUI.skin.label){fontSize=28,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft,wordWrap=true};titleStyle.normal.textColor=new Color(.95f,.88f,.72f);
        subtitleStyle=new GUIStyle(GUI.skin.label){fontSize=16,wordWrap=true};subtitleStyle.normal.textColor=new Color(.76f,.84f,.82f);
        bodyStyle=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};bodyStyle.normal.textColor=new Color(.85f,.88f,.83f);
        smallStyle=new GUIStyle(GUI.skin.label){fontSize=12,wordWrap=true};smallStyle.normal.textColor=new Color(.69f,.77f,.74f);
        buttonStyle=new GUIStyle(GUI.skin.button){fontSize=16,fontStyle=FontStyle.Bold};buttonStyle.normal.textColor=new Color(.95f,.87f,.7f);
        buttonDimStyle=new GUIStyle(buttonStyle);buttonDimStyle.normal.textColor=new Color(.45f,.49f,.46f);
        toastStyle=new GUIStyle(GUI.skin.box){fontSize=15,alignment=TextAnchor.MiddleCenter,wordWrap=true};toastStyle.normal.textColor=new Color(.97f,.94f,.84f);
    }
    void OnGUI(){
        if(titleStyle==null)CreateStyles();
        if(screen==ScreenState.Menu){DrawMenu();return;}
        if(screen==ScreenState.Caught){DrawCaught();return;}
        DrawHud();if(mapOpen)DrawWorldMap();if(screen==ScreenState.Paused)DrawPause();
    }
    void DrawMenu(){
        GUI.color=new Color(.035f,.07f,.09f,.97f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
        float w=Mathf.Min(860,Screen.width-60),left=(Screen.width-w)*.5f,top=Mathf.Max(24,Screen.height*.11f);
        GUI.Label(new Rect(left,top,w,24),"ASGARD  /  A TRICKSTER TALE",smallStyle);
        GUI.Label(new Rect(left,top+32,w,86),"LOKI THE TRICKSTER",titleStyle);
        GUI.Label(new Rect(left,top+105,w,56),"THE WORLD DOESN’T REVOLVE AROUND LOKI",subtitleStyle);
        if(menuPage=="main"){
            GUI.Label(new Rect(left,top+170,w*.57f,88),"One Asgard district to wander. One missing apple. One very suspicious guard.\nNo combat. Plenty of ways to make a bad situation interesting.",bodyStyle);
            float bx=left,by=top+275,bw=300,bh=42,gap=11;
            MenuButton(new Rect(bx,by,bw,bh),"NEW GAME",()=>StartGame(false),true);
            bool saved=PlayerPrefs.GetInt(ContinueKey,0)==1;
            MenuButton(new Rect(bx,by+bh+gap,bw,bh),"CONTINUE",()=>StartGame(true),saved);
            MenuButton(new Rect(bx,by+2*(bh+gap),bw,bh),"OPTIONS",()=>menuPage="options",true);
            MenuButton(new Rect(bx,by+3*(bh+gap),bw,bh),"CHARACTER PROFILE",()=>menuPage="profile",true);
            GUI.Label(new Rect(left+w*.57f,top+182,w*.4f,106),"LOKI",new GUIStyle(titleStyle){fontSize=70,alignment=TextAnchor.MiddleCenter,normal={textColor=lokiGreen}});
            GUI.Label(new Rect(left+w*.57f,top+274,w*.4f,52),"THE TRICKSTER\nSilver tongue · borrowed faces · questionable alibis",smallStyle);
            GUI.Label(new Rect(left,Screen.height-45,w,25),"WASD MOVE  ·  SHIFT SPRINT  ·  RIGHT MOUSE ORBIT  ·  WHEEL ZOOM  ·  SPACE USE",smallStyle);
        }else{
            GUI.Label(new Rect(left,top+170,w,145),menuPage=="options"?"CONTROLS\nWASD / arrows · move relative to camera · Shift · sprint\nRight mouse drag · orbit · scroll wheel · zoom\nQ · illusion · E · misdirect sound · E+S or F · disguise\nS then 1–5 · aspects · T then 1–3 · mass · C · conjure\nP / ESC · pause · R · restart":"LOKI\nAn original Asgardian trickster silhouette in tailored emerald, gold trim, long dark hair and a swept horned crown. The game uses original geometry and no film art.",bodyStyle);
            MenuButton(new Rect(left,top+300,220,42),"BACK",()=>menuPage="main",true);
        }
    }
    void StartGame(bool continueGame){
        if(continueGame&&PlayerPrefs.GetInt(ContinueKey,0)==0){menuPage="main";SetToast("No saved stroll yet. Even Loki can't continue an adventure he hasn't started.",4f);return;}
        PlayerPrefs.SetInt(ContinueKey,1);PlayerPrefs.Save();screen=ScreenState.Playing;
        SetToast(continueGame?"Welcome back to the district.":"Wander the district. The watch’s trouble begins near the Bifrost.",5f);
    }
    void MenuButton(Rect r,string text,System.Action action,bool enabled){Color old=GUI.color;GUI.color=enabled?new Color(.12f,.22f,.23f,.98f):new Color(.08f,.12f,.13f,.8f);if(GUI.Button(r,text,enabled?buttonStyle:buttonDimStyle)&&enabled)action();GUI.color=old;}
    void DrawHud(){
        GUI.color=new Color(.045f,.08f,.1f,.85f);GUI.DrawTexture(new Rect(0,0,Screen.width,64),Texture2D.whiteTexture);GUI.color=Color.white;
        GUI.Label(new Rect(24,12,Screen.width*.46f,30),encounterStarted?"THE BIFROST  /  GET THROUGH BEFORE SUNRISE":"ASGARD SQUARE  /  BEFORE SUNRISE",new GUIStyle(bodyStyle){fontSize=17,fontStyle=FontStyle.Bold});
        GUI.Label(new Rect(24,39,Screen.width*.8f,20),encounterStarted?"Q illusion · E sound · F disguise · C conjure · M district map · RMB orbit · wheel zoom":"Explore freely. M opens the district map; RMB orbits the camera.",smallStyle);
        if(encounterStarted&&alarmRaised){GUI.Label(new Rect(Screen.width-260,17,236,24),"GUARD  ·  "+guardState.ToString().ToUpperInvariant(),smallStyle);GUI.color=new Color(.31f,.13f,.12f,.9f);GUI.DrawTexture(new Rect(Screen.width-260,41,220,9),Texture2D.whiteTexture);GUI.color=new Color(.91f,.51f,.32f,.95f);GUI.DrawTexture(new Rect(Screen.width-260,41,220*suspicion,9),Texture2D.whiteTexture);GUI.color=Color.white;}
        if(!encounterStarted)GUI.Label(new Rect(Screen.width*.5f-160,Screen.height-86,320,28),"Explore with WASD / arrows",smallStyle);
        else if(!introPlaying){string nearby=NearbyPrompt();if(!string.IsNullOrEmpty(nearby))GUI.Label(new Rect(Screen.width*.5f-280,Screen.height-112,560,36),nearby,toastStyle);}
        if(disguised)GUI.Label(new Rect(Screen.width*.5f-150,78,300,30),"DISGUISE  ·  "+Mathf.CeilToInt(disguiseUntil-Time.time)+"s",toastStyle);
        if(!string.IsNullOrEmpty(toast)&&Time.time<toastUntil)GUI.Label(new Rect(Screen.width*.5f-330,82,660,58),toast,toastStyle);
        DrawAbilityDock();
    }
    void DrawAbilityDock(){
        float x=Screen.width-222,y=Screen.height-328;GUI.color=new Color(.035f,.07f,.08f,.88f);GUI.DrawTexture(new Rect(x-12,y-12,214,316),Texture2D.whiteTexture);GUI.color=Color.white;
        GUI.Label(new Rect(x,y,190,22),"TRICKSTER’S TOOLKIT",smallStyle);
        if(GUI.Button(new Rect(x,y+27,184,30),"ILLUSION  ·  Q"))CastIllusion();
        if(GUI.Button(new Rect(x,y+62,184,30),"SOUND  ·  E"))MisdirectSound();
        if(GUI.Button(new Rect(x,y+97,184,30),"CONJURE APPLE  ·  C"))Conjure();
        if(GUI.Button(new Rect(x,y+132,184,30),"CHANGE FORM  ·  1–5"))SetAspect((Aspect)(((int)aspect+1)%5));
        if(GUI.Button(new Rect(x,y+167,184,25),"LIGHT AIMED PROP"))ChangeMass(1);
        if(GUI.Button(new Rect(x,y+195,184,25),"RESTORE NORMAL WEIGHT"))ChangeMass(2);
        if(GUI.Button(new Rect(x,y+223,184,25),"MAKE PROP HEAVY"))ChangeMass(3);
        if(GUI.Button(new Rect(x,y+251,184,30),"TELEPORT  ·  "+(tesseractFound?teleportCharges+"/10":"?")))Teleport();
    }
    void DrawWorldMap(){
        float pw=Mathf.Min(590f,Screen.width-36f),ph=Mathf.Min(620f,Screen.height-36f);Rect panel=new Rect((Screen.width-pw)*.5f,(Screen.height-ph)*.5f,pw,ph);
        GUI.color=new Color(.035f,.065f,.075f,.97f);GUI.DrawTexture(panel,Texture2D.whiteTexture);GUI.color=Color.white;GUI.Box(panel,"",GUI.skin.box);
        GUI.Label(new Rect(panel.x+22,panel.y+16,pw-60,32),"ASGARD  /  DISTRICT AT A GLANCE",new GUIStyle(bodyStyle){fontSize=18,fontStyle=FontStyle.Bold});
        GUI.Label(new Rect(panel.x+22,panel.y+44,pw-44,22),"A field map · M closes · locations follow the editable district file",smallStyle);
        Rect map=new Rect(panel.x+26,panel.y+78,pw-52,ph-142);GUI.color=new Color(.64f,.56f,.42f,1);GUI.DrawTexture(map,Texture2D.whiteTexture);GUI.color=Color.white;
        GUI.color=new Color(.4f,.33f,.24f,.8f);GUI.DrawTexture(new Rect(map.x,map.y,map.width,2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(map.x,map.yMax-2,map.width,2),Texture2D.whiteTexture);GUI.color=Color.white;
        if(district.routes!=null)foreach(MapRouteData route in district.routes)for(int i=1;i<route.points.Count;i++)DrawMapRoute(map,route.points[i-1],route.points[i],route.width,new Color(.85f,.76f,.61f,.95f));
        DrawMapLandmark(map,"asgard-square","SQUARE",new Color(.73f,.62f,.42f));
        foreach(LandmarkData landmark in district.landmarks){
            if(landmark.id=="asgard-square"||landmark.type=="promenade")continue;
            string label=landmark.id=="bifrost-approach"?"BIFROST":landmark.id=="east-fountain"?"FOUNTAIN":landmark.id=="side-alley"?"WEST ALLEY":landmark.id=="market-square"?"MARKET":landmark.id=="odin-study"?"ODIN’S STUDY":landmark.id=="frigga-house"?"FRIGGA’S HOUSE":landmark.id.ToUpperInvariant();
            Color c=landmark.type=="gate"?new Color(.1f,.68f,.77f):landmark.type=="residence"?new Color(.34f,.47f,.35f):new Color(.86f,.73f,.45f);DrawMapLandmark(map,landmark.id,label,c);
        }
        if(district.guard.points!=null&&district.guard.points.Count>1){for(int i=1;i<district.guard.points.Count;i++)DrawMapRoute(map,district.guard.points[i-1],district.guard.points[i],.18f,new Color(.55f,.21f,.19f,.95f));foreach(DistrictPoint patrol in district.guard.points)DrawMapDot(map,patrol.x,patrol.z,new Color(.7f,.25f,.2f),"G");}
        foreach(WorldProp prop in props){if(prop.transform==null||prop.id.StartsWith("odin-")||prop.id.StartsWith("frigga-"))continue;DrawMapDot(map,prop.transform.position.x,prop.transform.position.z,new Color(.22f,.32f,.31f),"");}
        if(player!=null)DrawMapDot(map,player.position.x,player.position.z,new Color(.08f,.73f,.39f),"L");
        GUI.Label(new Rect(panel.x+25,panel.yMax-52,pw-50,24),"N  ↑      GOLD PATHS  ·  GREEN LOKI  ·  RED GUARD / PATROL",smallStyle);
        GUI.Label(new Rect(panel.x+25,panel.yMax-30,pw-50,22),"Watch the patrol and improvise; no route is required.",smallStyle);
    }
    void DrawMapLandmark(Rect map,string id,string label,Color color){foreach(LandmarkData landmark in district.landmarks)if(landmark.id==id){DrawMapDot(map,landmark.x,landmark.z,color,"•");Vector2 p=MapToScreen(map,landmark.x,landmark.z);GUI.Label(new Rect(p.x+7,p.y-9,130,18),label,new GUIStyle(smallStyle){fontSize=10,fontStyle=FontStyle.Bold});return;}}
    void DrawMapDot(Rect map,float x,float z,Color color,string text){Vector2 p=MapToScreen(map,x,z);float size=string.IsNullOrEmpty(text)?8f:19f;GUI.color=new Color(.04f,.07f,.07f,.95f);GUI.DrawTexture(new Rect(p.x-size*.5f-1,p.y-size*.5f-1,size+2,size+2),Texture2D.whiteTexture);GUI.color=color;GUI.DrawTexture(new Rect(p.x-size*.5f,p.y-size*.5f,size,size),Texture2D.whiteTexture);GUI.color=Color.white;if(!string.IsNullOrEmpty(text))GUI.Label(new Rect(p.x-size*.5f,p.y-size*.55f-1,size,size),text,new GUIStyle(smallStyle){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold,fontSize=10,normal={textColor=Color.white}});}
    Vector2 MapToScreen(Rect map,float x,float z){float nx=Mathf.InverseLerp(district.bounds.minX,district.bounds.maxX,x);float nz=Mathf.InverseLerp(district.bounds.minZ,district.bounds.maxZ,z);return new Vector2(map.x+nx*map.width,map.y+(1f-nz)*map.height);}
    void DrawMapRoute(Rect map,DistrictPoint a,DistrictPoint b,float worldWidth,Color color){Vector2 p=MapToScreen(map,a.x,a.z),q=MapToScreen(map,b.x,b.z),d=q-p;float length=d.magnitude;if(length<1f)return;float thickness=Mathf.Clamp(worldWidth/(district.bounds.maxX-district.bounds.minX)*map.width,3f,16f);Matrix4x4 old=GUI.matrix;GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg,(p+q)*.5f);GUI.color=color;GUI.DrawTexture(new Rect((p.x+q.x-length)*.5f,(p.y+q.y-thickness)*.5f,length,thickness),Texture2D.whiteTexture);GUI.color=Color.white;GUI.matrix=old;}
    string NearbyPrompt(){WorldProp best=null;float distance=float.MaxValue;foreach(var item in props){float d=Vector2.Distance(new Vector2(player.position.x,player.position.z),new Vector2(item.transform.position.x,item.transform.position.z));if(d<distance){distance=d;best=item;}}return best!=null&&distance<best.radius?"[ SPACE ]  "+best.label+"  ·  [E] sound at cursor":"[ Q ] illusion at cursor   ·   [ E ] sound at cursor   ·   [ F ] shapeshift";}
    void DrawPause(){GUI.color=new Color(.02f,.04f,.05f,.87f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;float x=Screen.width*.5f-150,y=Screen.height*.38f;GUI.Label(new Rect(x,y,300,55),"PAUSED",titleStyle);MenuButton(new Rect(x,y+80,300,46),"RESUME",()=>screen=ScreenState.Playing,true);MenuButton(new Rect(x,y+138,300,46),"RESTART DISTRICT",()=>SceneManager.LoadScene(MainScene),true);MenuButton(new Rect(x,y+196,300,46),"MAIN MENU",()=>SceneManager.LoadScene(MainScene),true);}
    void DrawCaught(){GUI.color=new Color(.03f,.06f,.07f,.9f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;GUI.Label(new Rect(Screen.width*.2f,Screen.height*.39f,Screen.width*.6f,60),"CAUGHT. AWKWARD.",titleStyle);GUI.Label(new Rect(Screen.width*.2f,Screen.height*.49f,Screen.width*.6f,55),toast,subtitleStyle);GUI.Label(new Rect(Screen.width*.2f,Screen.height*.58f,Screen.width*.6f,32),"Back to the district in a moment…",smallStyle);}
}
