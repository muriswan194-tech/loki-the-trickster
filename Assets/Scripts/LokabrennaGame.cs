using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LokabrennaGame : MonoBehaviour {
    const float PlayerSpeed = 5.2f;
    DistrictData data; Transform player, guard, gate, guardHelmet, playerHorns; Renderer playerRenderer; Color lokiGreen = new Color(.32f,.82f,.48f), gold = new Color(1f,.72f,.2f);
    Text hud, prompt, toast, controls; Image toastBg; Canvas canvas; GameObject menuRoot; Text menuStatus; bool menuOpen=true, crisis, caught, escaped, disguised, illusionActive, hasPlayed, appleDistracted; float disguiseUntil, suspicion, toastUntil, patrolWait, guardHoldUntil; Vector3 guardTarget; GameObject illusion;
    readonly Dictionary<string, Transform> props = new Dictionary<string, Transform>();

    void Start() {
        data = new DistrictDataLoader().Load(); if (data == null) return;
        BuildWorld(); BuildUI(); BuildMenu();
        hud.text = "GOLDEN COURT     ·     a quiet stroll"; Notify("Explore a little. Asgard's gates aren't going anywhere.");
    }
    void Update() {
        if (data == null) return;
        if (Input.GetKeyDown(KeyCode.R) && !menuOpen) { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); return; }
        if(menuOpen) return;
        if (caught || escaped) { UpdateUI(); return; }
        MovePlayer();
        if (crisis) { UpdateGuard(); if (Input.GetKeyDown(KeyCode.Space)) Interact(); }
        UpdateUI();
    }
    void BuildWorld() {
        Camera.main.transform.position = new Vector3(0,27,-11); Camera.main.transform.rotation = Quaternion.Euler(66,0,0); Camera.main.orthographic = true; Camera.main.orthographicSize = 12.5f; Camera.main.backgroundColor = new Color(.055f,.09f,.14f);
        Light light = FindObjectOfType<Light>(); if (light != null) { light.color = new Color(1f,.84f,.61f); light.intensity = 1.15f; light.transform.rotation = Quaternion.Euler(48,-32,0); }
        Create("Moonstone courtyard", new Vector3(0,-.18f,.4f), new Vector3(23,.35f,19), new Color(.16f,.25f,.32f));
        Create("North walk", new Vector3(0,-.03f,5.65f), new Vector3(6,.16f,6.3f), new Color(.24f,.34f,.39f));
        for(int i=-2;i<=2;i++) Create("Court paving",new Vector3(i*4,.012f,0),new Vector3(.035f,.018f,15),new Color(.37f,.46f,.48f));
        foreach(float x in new[]{-8f,8f}) foreach(float z in new[]{-5f,-1f,3f,7f}) Create("Court edge inlay",new Vector3(x,.025f,z),new Vector3(2.2f,.04f,.09f),new Color(.55f,.44f,.27f));
        Create("Bifrost gate frame",new Vector3(0,.55f,8.35f),new Vector3(4.7f,1.1f,.48f),new Color(.67f,.51f,.25f));
        Create("Gate left post",new Vector3(-2.05f,1.5f,8.35f),new Vector3(.34f,3.2f,.45f),gold); Create("Gate right post",new Vector3(2.05f,1.5f,8.35f),new Vector3(.34f,3.2f,.45f),gold);
        gate=Create("Bifrost threshold",new Vector3(0,.18f,7.05f),new Vector3(3.55f,.08f,.5f),new Color(.25f,.83f,.91f)).transform;
        Create("Fountain",new Vector3(6,.25f,4.1f),new Vector3(2.8f,.5f,2.8f),new Color(.26f,.6f,.65f),PrimitiveType.Cylinder); Create("Fountain water",new Vector3(6,.54f,4.1f),new Vector3(2.2f,.12f,2.2f),new Color(.35f,.84f,.84f),PrimitiveType.Cylinder);
        foreach(var p in new[]{new Vector3(-8,1,-5),new Vector3(8,1,-5),new Vector3(-8,1,6),new Vector3(8,1,6)}) { Create("Asgardian pillar",p,new Vector3(1.2f,2.1f,1.2f),new Color(.37f,.49f,.53f)); Create("Pillar cap",p+Vector3.up*1.1f,new Vector3(1.7f,.28f,1.7f),gold); }
        foreach (var item in data.interactions) {
            Vector3 pos=new Vector3(item.x,0,item.z); GameObject obj;
            if(item.type=="bell") { Create("Bell stand",pos+Vector3.up*.48f,new Vector3(.85f,.85f,.85f),new Color(.39f,.34f,.29f)); obj=Create("Bronze courtyard bell",pos+Vector3.up*1.28f,new Vector3(.55f,.64f,.55f),new Color(.91f,.59f,.2f),PrimitiveType.Sphere); Create("Bell crown",pos+Vector3.up*1.7f,new Vector3(.16f,.3f,.16f),gold); }
            else if(item.type=="prop") { Create("Apple plinth",pos+Vector3.up*.15f,new Vector3(1.4f,.3f,1.4f),new Color(.43f,.35f,.26f)); obj=Create("Suspicious golden fruit",pos+Vector3.up*.69f,new Vector3(.58f,.68f,.58f),gold,PrimitiveType.Sphere); Create("Apple leaf",pos+Vector3.up*1.12f,new Vector3(.12f,.28f,.08f),new Color(.36f,.63f,.29f)); }
            else { obj=Create("Illusion sigil",pos+Vector3.up*.04f,new Vector3(1.5f,.08f,1.5f),new Color(.32f,.7f,.82f),PrimitiveType.Cylinder); Create("Sigil inset",pos+Vector3.up*.09f,new Vector3(.8f,.035f,.8f),new Color(.12f,.28f,.37f),PrimitiveType.Cylinder); }
            props[item.id]=obj.transform;
        }
        player=Create("Loki",new Vector3(data.playerStart.x,.65f,data.playerStart.z),new Vector3(.72f,1.25f,.72f),lokiGreen,PrimitiveType.Capsule).transform; playerRenderer=player.GetComponent<Renderer>();
        playerHorns=Create("Loki horns",player.position+Vector3.up*.67f,new Vector3(.9f,.13f,.16f),gold).transform; playerHorns.SetParent(player);
        guard=Create("Sleepy gate guard",new Vector3(-1.5f,.75f,2.25f),new Vector3(.82f,1.45f,.82f),new Color(.78f,.39f,.32f),PrimitiveType.Capsule).transform;
        guardHelmet=Create("Guard helmet",guard.position+Vector3.up*.77f,new Vector3(.92f,.25f,.92f),gold,PrimitiveType.Sphere).transform; guardHelmet.SetParent(guard);
        for(int i=0;i<11;i++) { float x=(i%4)*4.8f-7.2f, z=(i/4)*4.7f-5.8f; Create("Blue spruce",new Vector3(x,.7f,z),new Vector3(1.05f,1.4f,1.05f),new Color(.18f,.35f,.32f),PrimitiveType.Sphere); }
        Create("Court lamp",new Vector3(-7,.55f,0),new Vector3(.4f,1.1f,.4f),gold); Create("Court lamp",new Vector3(7,.55f,-1),new Vector3(.4f,1.1f,.4f),gold);
    }
    void BuildUI() {
        GameObject c=new GameObject("Jam HUD"); canvas=c.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; c.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; c.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1280,720); c.AddComponent<GraphicRaycaster>();
        hud=Label(c.transform,"Heading",24,new Vector2(30,-22),new Vector2(0,1),new Vector2(.58f,1),TextAnchor.UpperLeft,new Color(.95f,.84f,.56f),true);
        prompt=Label(c.transform,"Interaction",19,new Vector2(0,32),new Vector2(.5f,0),new Vector2(.6f,0),TextAnchor.MiddleCenter,Color.white,true);
        GameObject bg=new GameObject("Toast panel"); bg.transform.SetParent(c.transform,false); RectTransform br=bg.AddComponent<RectTransform>(); br.anchorMin=new Vector2(.5f,1);br.anchorMax=new Vector2(.5f,1);br.anchoredPosition=new Vector2(0,-78);br.sizeDelta=new Vector2(690,56);toastBg=bg.AddComponent<Image>();toastBg.color=new Color(.07f,.12f,.17f,.93f);
        toast=Label(bg.transform,"Feedback",17,Vector2.zero,new Vector2(0,0),Vector2.one,TextAnchor.MiddleCenter,new Color(.97f,.93f,.8f),false);
        controls=Label(c.transform,"Controls",14,new Vector2(24,20),new Vector2(0,0),new Vector2(.5f,.1f),TextAnchor.LowerLeft,new Color(.83f,.87f,.82f),false);
        controls.text="WASD / ARROWS  MOVE     ·     SPACE  INTERACT     ·     R  PLAY AGAIN";
        Label(c.transform,"Studio",13,new Vector2(-22,20),new Vector2(1,0),new Vector2(.4f,.1f),TextAnchor.LowerRight,new Color(.62f,.7f,.69f),false).text="LOKABRENNA   /   THE WORLD HAS OTHER PLANS";
    }
    void BuildMenu() {
        menuRoot=new GameObject("Lokabrenna Main Menu"); menuRoot.transform.SetParent(canvas.transform,false);
        RectTransform full=menuRoot.AddComponent<RectTransform>(); full.anchorMin=Vector2.zero;full.anchorMax=Vector2.one;full.offsetMin=Vector2.zero;full.offsetMax=Vector2.zero;
        Image veil=menuRoot.AddComponent<Image>();veil.color=new Color(.035f,.07f,.105f,.94f);
        GameObject panel=new GameObject("Menu composition");panel.transform.SetParent(menuRoot.transform,false);RectTransform pr=panel.AddComponent<RectTransform>();pr.anchorMin=new Vector2(.09f,.1f);pr.anchorMax=new Vector2(.91f,.9f);pr.offsetMin=Vector2.zero;pr.offsetMax=Vector2.zero;
        Text eyebrow=Label(panel.transform,"Eyebrow",13,new Vector2(0,-10),new Vector2(0,1),new Vector2(.72f,1),TextAnchor.UpperLeft,new Color(.43f,.83f,.72f),true);eyebrow.text="ASGARD  /  A TRICKSTER TALE";
        Text title=Label(panel.transform,"Game title",39,new Vector2(0,-42),new Vector2(0,1),new Vector2(.72f,1),TextAnchor.UpperLeft,new Color(.96f,.88f,.7f),true);title.text="'LOKABRENNA'";
        Text subtitle=Label(panel.transform,"Game subtitle",20,new Vector2(0,-96),new Vector2(0,1),new Vector2(.68f,1),TextAnchor.UpperLeft,new Color(.77f,.86f,.84f),false);subtitle.text="The world doesn't revolve around Wildfire — the Trickster.";
        Text flavor=Label(panel.transform,"Menu flavor",16,new Vector2(0,-147),new Vector2(0,1),new Vector2(.48f,.53f),TextAnchor.UpperLeft,new Color(.72f,.78f,.76f),false);flavor.text="The Bifrost is locked. Odin has misplaced an apple.\nLoki has been blamed for both, somehow.\n\nNo swords. Three tricks. One very inconvenient guard.";
        Text portrait=Label(panel.transform,"Original character portrait",88,new Vector2(-15,-34),new Vector2(.67f,.53f),new Vector2(.98f,.95f),TextAnchor.MiddleCenter,new Color(.32f,.82f,.58f),true);portrait.text="LOKI";
        Text profile=Label(panel.transform,"Character profile",15,new Vector2(-6,4),new Vector2(.66f,.18f),new Vector2(.98f,.4f),TextAnchor.MiddleCenter,new Color(.88f,.77f,.5f),true);profile.text="LOKI  /  THE WILDFIRE TRICKSTER\nSilver tongue · borrowed faces · questionable alibis";
        AddMenuButton(panel.transform,"NEW GAME",new Vector2(0,-15),new Vector2(.4f,.078f),()=>StartGame(false));
        AddMenuButton(panel.transform,"CONTINUE",new Vector2(0,-62),new Vector2(.4f,.078f),()=>StartGame(true));
        AddMenuButton(panel.transform,"OPTIONS",new Vector2(0,-109),new Vector2(.4f,.078f),ShowOptions);
        AddMenuButton(panel.transform,"CHARACTER PROFILE",new Vector2(0,-156),new Vector2(.4f,.078f),ShowProfile);
        menuStatus=Label(panel.transform,"Menu status",13,new Vector2(0,22),new Vector2(0,0),new Vector2(.6f,.09f),TextAnchor.MiddleLeft,new Color(.66f,.81f,.75f),false);
        menuStatus.text="WASD / ARROWS MOVE     ·     SPACE INTERACT     ·     R RESTART";
        bool continueAvailable=PlayerPrefs.GetInt("lokabrenna-started",0)==1;
        Button continueButton=menuRoot.transform.GetChild(0).Find("CONTINUE Button").GetComponent<Button>();
        continueButton.interactable=continueAvailable;
        continueButton.GetComponent<Image>().color=continueAvailable?new Color(.11f,.2f,.23f,.96f):new Color(.075f,.11f,.13f,.78f);
    }
    void AddMenuButton(Transform parent,string label,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action) {
        GameObject button=new GameObject(label+" Button");button.transform.SetParent(parent,false);RectTransform r=button.AddComponent<RectTransform>();r.anchorMin=new Vector2(0,.48f);r.anchorMax=new Vector2(.42f,.48f);r.sizeDelta=new Vector2(0,52);r.anchoredPosition=position;
        Image image=button.AddComponent<Image>();image.color=new Color(.11f,.2f,.23f,.96f);Button b=button.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);
        Text text=Label(button.transform,label+" Label",16,new Vector2(22,0),Vector2.zero,Vector2.one,TextAnchor.MiddleLeft,new Color(.95f,.88f,.72f),true);text.text=label;
    }
    void StartGame(bool resume) { if(resume&&PlayerPrefs.GetInt("lokabrenna-started",0)==0){menuStatus.text="NO SAVE YET. Even Loki can't continue an adventure that hasn't started.";return;} menuOpen=false;menuRoot.SetActive(false);hud.text="GOLDEN COURT     ·     a quiet stroll";hasPlayed=true;PlayerPrefs.SetInt("lokabrenna-started",1);PlayerPrefs.Save();Notify(resume?"Continue the stroll. The apple is still missing, naturally.":"New game. Take a moment to explore before the gate crisis.");Invoke(nameof(BeginCrisis),resume?1f:9f); }
    void ShowOptions() { menuStatus.text="OPTIONS  ·  WASD / ARROWS TO MOVE  ·  SPACE TO TRICK  ·  R TO RETRY"; }
    void ShowProfile() { menuStatus.text="LOKI  ·  ASGARDIAN SHAPESHIFTER  ·  QUICK-WITTED  ·  VERY QUESTIONABLE ALIBIS"; }
    Text Label(Transform parent,string name,int size,Vector2 position,Vector2 anchorMin,Vector2 anchorMax,TextAnchor align,Color color,bool bold) {
        GameObject g=new GameObject(name);g.transform.SetParent(parent,false);RectTransform r=g.AddComponent<RectTransform>();r.anchorMin=anchorMin;r.anchorMax=anchorMax;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;r.anchoredPosition=position;
        Text t=g.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=size;t.alignment=align;t.color=color;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
    }
    GameObject Create(string name,Vector3 position,Vector3 scale,Color color,PrimitiveType type=PrimitiveType.Cube) {
        GameObject go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=position;go.transform.localScale=scale;Renderer r=go.GetComponent<Renderer>();r.material=new Material(Shader.Find("Standard"));r.material.color=color;r.material.SetFloat("_Glossiness",.22f);return go;
    }
    void BeginCrisis() { crisis=true;hud.text="GOLDEN COURT     ·     THE GATE IS LOCKED";Notify("Guards locked the Bifrost while hunting Odin's missing apple. Loki: “Of course it's a gate emergency.”"); }
    void MovePlayer() {
        float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical");Vector3 move=new Vector3(x,0,z).normalized*PlayerSpeed*Time.deltaTime;
        if(move.sqrMagnitude>0){Vector3 p=player.position+move;p.x=Mathf.Clamp(p.x,data.bounds.minX+.7f,data.bounds.maxX-.7f);p.z=Mathf.Clamp(p.z,data.bounds.minZ+.7f,data.bounds.maxZ-.7f);if(crisis&&Mathf.Abs(p.x)<2.15f&&p.z>6.35f&&!disguised){p.z=6.35f;}player.position=p;if(Mathf.Abs(x)+Mathf.Abs(z)>.01f)player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(new Vector3(x,0,z)),Time.deltaTime*12);}
    }
    void UpdateGuard() {
        if(Time.time<guardHoldUntil){
            if(illusionActive && illusion!=null) { guard.position=Vector3.MoveTowards(guard.position,illusion.transform.position,2.3f*Time.deltaTime); }
            else if(appleDistracted){guard.position=Vector3.MoveTowards(guard.position,guardTarget,data.patrol.speed*Time.deltaTime);}
            suspicion=Mathf.MoveTowards(suspicion,0,Time.deltaTime*.6f); return;
        }
        if(Time.time<patrolWait){ suspicion=Mathf.MoveTowards(suspicion,0,Time.deltaTime*.25f); return; }
        if(illusionActive&&illusion!=null){guardTarget=illusion.transform.position;}else if(Vector3.Distance(guard.position,guardTarget)<.14f||guardTarget==Vector3.zero){var points=data.patrol.points;int next=(Vector3.Distance(guard.position,new Vector3(points[0].x,.75f,points[0].z))<.4f)?1:0;guardTarget=new Vector3(points[next].x,.75f,points[next].z);}
        if(Vector3.Distance(guard.position,guardTarget)>.14f)guard.position=Vector3.MoveTowards(guard.position,guardTarget,((illusionActive)?2.1f:data.patrol.speed)*Time.deltaTime);
        Vector3 look=guardTarget-guard.position;if(look.sqrMagnitude>.01f)guard.rotation=Quaternion.LookRotation(new Vector3(look.x,0,look.z));
        bool near=Vector3.Distance(player.position,guard.position)<2.6f;bool disguisedNow=disguised&&Time.time<disguiseUntil;
        if(near&&!disguisedNow&&!illusionActive){suspicion+=Time.deltaTime*.95f;if(suspicion>2.4f)Caught();}else suspicion=Mathf.MoveTowards(suspicion,0,Time.deltaTime*.55f);
        if(disguised&&Time.time>=disguiseUntil){disguised=false;playerRenderer.material.color=lokiGreen;playerHorns.gameObject.SetActive(true);guardHelmet.gameObject.SetActive(true);Notify("Borrowed uniform expired. Please return helmet to its emotionally complicated owner.");}
        if(Vector3.Distance(player.position,new Vector3(data.gate.x,.6f,data.gate.z))<1.1f){if(disguisedNow){Escaped("The guard saluted you through. Loki saluted back, deeply unearned.");}else if(!illusionActive&&!appleDistracted&&patrolWait<Time.time)Caught();}
    }
    void Interact() {
        InteractionData nearest=null;float d=float.MaxValue;foreach(var i in data.interactions){float n=Vector3.Distance(player.position,new Vector3(i.x,.65f,i.z));if(n<d){d=n;nearest=i;}}
        if(nearest==null||d>nearest.radius){if(crisis&&Vector3.Distance(player.position,guard.position)<3.1f){disguised=true;disguiseUntil=Time.time+10f;playerRenderer.material.color=new Color(.78f,.39f,.32f);playerHorns.gameObject.SetActive(false);guardHelmet.gameObject.SetActive(false);Notify("Borrowed a guard's uniform. Look official, walk past, and reach the Bifrost before it wears off.");}else Notify("Nothing within arm's reach. Try the bell, apple, or glowing sigil.");return;}
        if(nearest.type=="bell"){guardTarget=new Vector3(nearest.x,.75f,nearest.z);patrolWait=Time.time+5f;guardHoldUntil=Time.time+2f;appleDistracted=true;Notify("DING! The guard investigates the bell. Loki claims it was a ghost with excellent timing.");}
        else if(nearest.type=="prop"){guardTarget=new Vector3(nearest.x,.75f,nearest.z);guardHoldUntil=Time.time+4.5f;appleDistracted=true;Notify("The guard spots Odin's apple and gets very, very busy writing a report.");}
        else {if(illusion!=null)Destroy(illusion);illusion=Create("Loki's convincing illusion",player.position+player.forward*2.5f+Vector3.up*.65f,new Vector3(.68f,1.15f,.68f),new Color(.38f,.85f,1f,.55f),PrimitiveType.Capsule);illusionActive=true;guardTarget=illusion.transform.position;guardHoldUntil=Time.time+3f;Notify("A flawless Loki double! (It is waving at the guard.) Run for the gate.");StartCoroutine(EndIllusion(4.2f));}
    }
    IEnumerator EndIllusion(float seconds){yield return new WaitForSeconds(seconds);illusionActive=false;if(illusion!=null)Destroy(illusion);}
    void UpdateUI(){
        if(suspicion>.1f&&!caught) { hud.text="GOLDEN COURT     ·     GUARD SUSPICION  "+new string('■',Mathf.Clamp(Mathf.CeilToInt(suspicion*3),1,5)); }
        if(!crisis){prompt.text="WANDER THE COURT";return;}
        InteractionData best=null;float bd=float.MaxValue;foreach(var i in data.interactions){float d=Vector3.Distance(player.position,new Vector3(i.x,.65f,i.z));if(d<bd){bd=d;best=i;}}
        prompt.text=best!=null&&bd<best.radius?"[ SPACE ]   "+best.label:(bd>best.radius&&Vector3.Distance(player.position,guard.position)<3.1f?"[ SPACE ]   SHAPESHIFT AS A GUARD":"");
        string mode=disguised&&Time.time<disguiseUntil?"GUARD DISGUISE  ·  "+Mathf.CeilToInt(disguiseUntil-Time.time)+"s":"THREE TRICKS   ·   ILLUSION   /   BELL   /   APPLE";
        if(suspicion<=.1f)hud.text="GOLDEN COURT     ·     "+mode;
    }
    void Notify(string message){if(toast==null)return;toast.text=message;toastUntil=Time.time+5f;toastBg.enabled=true;}
    void Caught(){caught=true;hud.text="CAUGHT     ·     THIS IS FINE";Notify("Caught! The guard mistakes you for the apple inspector. Embarrassing, not fatal. Press R to try another trick.");}
    void Escaped(string message){escaped=true;hud.text="BIFROST CROSSED     ·     LOKI WINS THIS ROUND";Notify(message+" Press R to play again.");}
    void OnGUI(){if(toast!=null&&Time.time>toastUntil&&!caught&&!escaped)toastBg.enabled=false;}
}











