using UnityEngine;

// Small, reusable authored meshes for the jam art kit. These are saved as Unity Mesh assets by the editor builder.
public static class LokabrennaMeshLibrary {
    static Mesh Finish(string name, Vector3[] vertices, int[] triangles){var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
    public static Mesh CreateRobe(){
        const int around=24, rings=6;float[] ys={-.8f,-.66f,-.35f,.02f,.34f,.52f};float[] rs={.48f,.55f,.49f,.4f,.31f,.29f};var v=new Vector3[around*rings];var t=new int[(rings-1)*around*6+around*6];
        for(int r=0;r<rings;r++)for(int i=0;i<around;i++){float a=i*Mathf.PI*2f/around;float hemWave=r<2?Mathf.Sin(a*3f)*.035f:0;v[r*around+i]=new Vector3(Mathf.Cos(a)*(rs[r]+hemWave),ys[r]+hemWave,Mathf.Sin(a)*(rs[r]+hemWave));}
        int n=0;for(int r=0;r<rings-1;r++)for(int i=0;i<around;i++){int j=(i+1)%around,a=r*around+i,b=r*around+j,c=(r+1)*around+i,d=(r+1)*around+j;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("TailoredRobe",v,t);
    }
    public static Mesh CreateCape(){
        const int rows=9,cols=13;var v=new Vector3[rows*cols];var t=new int[(rows-1)*(cols-1)*6];
        for(int y=0;y<rows;y++){float q=(float)y/(rows-1),width=Mathf.Lerp(.38f,.76f,q);for(int x=0;x<cols;x++){float u=(float)x/(cols-1)*2-1;float sway=Mathf.Sin(q*4.5f+u*2.4f)*.055f;v[y*cols+x]=new Vector3(u*width,-.42f-q*1.17f+sway,-.42f-.16f*Mathf.Sin(q*Mathf.PI)+Mathf.Abs(u)*.11f+Mathf.Sin(u*5f+q*3f)*.025f);}}
        int n=0;for(int y=0;y<rows-1;y++)for(int x=0;x<cols-1;x++){int a=y*cols+x,b=a+1,c=a+cols,d=c+1;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("FlowingCape",v,t);
    }
    public static Mesh CreateWing(){
        const int across=15,span=9;var v=new Vector3[across*span*2];var t=new int[(across-1)*(span-1)*12];
        for(int side=0;side<2;side++)for(int x=0;x<across;x++){float u=(float)x/(across-1);float chord=Mathf.Sin(Mathf.Pow(u,.82f)*Mathf.PI)*.42f+.06f;float spanX=u*.98f;for(int y=0;y<span;y++){float q=(float)y/(span-1)*2-1;float feather=Mathf.Sin(u*17f)*.025f;v[side*across*span+x*span+y]=new Vector3(spanX,.02f+Mathf.Sin(u*Mathf.PI)*.12f+Mathf.Abs(q)*.05f,-.1f+q*(chord+feather));}}
        int n=0;for(int side=0;side<2;side++)for(int x=0;x<across-1;x++)for(int y=0;y<span-1;y++){int a=side*across*span+x*span+y,b=a+span,c=a+1,d=b+1;if(side==0){t[n++]=a;t[n++]=b;t[n++]=c;t[n++]=c;t[n++]=b;t[n++]=d;}else{t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=c;t[n++]=d;t[n++]=b;}}
        return Finish("FeatheredWing",v,t);
    }
    public static Mesh CreateArch(){
        const int segments=40,around=10;const float radius=1.7f,tube=.22f;var v=new Vector3[(segments+1)*around];var t=new int[segments*around*6];
        for(int s=0;s<=segments;s++){float a=Mathf.PI*s/segments;Vector3 radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);Vector3 center=radial*radius+Vector3.up*1.55f;for(int k=0;k<around;k++){float b=Mathf.PI*2f*k/around;v[s*around+k]=center+radial*(Mathf.Cos(b)*tube)+Vector3.forward*(Mathf.Sin(b)*tube);}}
        int n=0;for(int s=0;s<segments;s++)for(int k=0;k<around;k++){int kn=(k+1)%around,a=s*around+k,b=s*around+kn,c=(s+1)*around+k,d=(s+1)*around+kn;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("AsgardArch",v,t);
    }
    public static Mesh CreatePalaceSpire(){
        const int sides=12;float[] ys={0f,.12f,.2f,.88f,.94f,1f};float[] rs={.48f,.57f,.43f,.3f,.35f,.025f};int rings=ys.Length;var v=new Vector3[sides*rings];var t=new int[(rings-1)*sides*6];
        for(int y=0;y<rings;y++)for(int i=0;i<sides;i++){float a=(i+(y%2)*.5f)*Mathf.PI*2f/sides;float flute=1f+.055f*Mathf.Cos(a*6f);v[y*sides+i]=new Vector3(Mathf.Cos(a)*rs[y]*flute,ys[y],Mathf.Sin(a)*rs[y]*flute);}
        int n=0;for(int y=0;y<rings-1;y++)for(int i=0;i<sides;i++){int j=(i+1)%sides,a=y*sides+i,b=y*sides+j,c=(y+1)*sides+i,d=(y+1)*sides+j;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("AsgardPalaceSpire",v,t);
    }
    public static Mesh CreateAwning(){
        const int across=12,depth=8;var v=new Vector3[across*depth];var t=new int[(across-1)*(depth-1)*6];
        for(int z=0;z<depth;z++){float d=(float)z/(depth-1);for(int x=0;x<across;x++){float u=(float)x/(across-1)*2f-1f;float sag=Mathf.Sin(d*Mathf.PI)*.1f+Mathf.Pow(Mathf.Abs(u),5f)*.11f;v[z*across+x]=new Vector3(u*1.25f,-sag,-.86f+d*1.72f);}}
        int n=0;for(int z=0;z<depth-1;z++)for(int x=0;x<across-1;x++){int a=z*across+x,b=a+1,c=a+across,d=c+1;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("AsgardClothAwning",v,t);
    }
    public static Mesh CreateFountainBowl(){
        const int sides=48;float[] ys={0f,.04f,.14f,.24f,.29f};float[] rs={.78f,.92f,1f,.94f,.8f};int rings=ys.Length;var v=new Vector3[sides*rings];var t=new int[(rings-1)*sides*6];
        for(int y=0;y<rings;y++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2f/sides;float motif=1f+.018f*Mathf.Cos(a*12f);v[y*sides+i]=new Vector3(Mathf.Cos(a)*rs[y]*motif,ys[y],Mathf.Sin(a)*rs[y]*motif);}
        int n=0;for(int y=0;y<rings-1;y++)for(int i=0;i<sides;i++){int j=(i+1)%sides,a=y*sides+i,b=y*sides+j,c=(y+1)*sides+i,d=(y+1)*sides+j;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("AsgardFountainBowl",v,t);
    }
    public static Mesh CreateOliveCanopy(){
        // A single swept, subtly lobed canopy reads as clipped olive foliage at game distance.
        const int sides=40,rings=9;var v=new Vector3[sides*rings];var t=new int[(rings-1)*sides*6];
        for(int y=0;y<rings;y++){
            float q=(float)y/(rings-1),profile=Mathf.Sin(Mathf.PI*q);
            for(int i=0;i<sides;i++){
                float a=i*Mathf.PI*2f/sides;
                float lobe=1f+.055f*Mathf.Cos(a*7f+q*1.2f)+.022f*Mathf.Cos(a*14f-q*2f);
                float radius=(.12f+.88f*profile)*lobe;
                float lift=.035f*Mathf.Sin(a*5f+q*3f)*profile;
                v[y*sides+i]=new Vector3(Mathf.Cos(a)*radius,Mathf.Lerp(-.5f,.5f,q)+lift,Mathf.Sin(a)*radius);
            }
        }
        int n=0;for(int y=0;y<rings-1;y++)for(int i=0;i<sides;i++){int j=(i+1)%sides,a=y*sides+i,b=y*sides+j,c=(y+1)*sides+i,d=(y+1)*sides+j;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("AsgardOliveCanopy",v,t);
    }
    static Mesh CreateLathed(string name,float[] ys,float[] radii,int sides=28,float lobes=0f){
        var v=new Vector3[ys.Length*sides];var t=new int[(ys.Length-1)*sides*6];
        for(int y=0;y<ys.Length;y++)for(int i=0;i<sides;i++){
            float a=i*Mathf.PI*2f/sides,r=radii[y]*(1f+lobes*Mathf.Cos(a*5f));
            v[y*sides+i]=new Vector3(Mathf.Cos(a)*r,ys[y],Mathf.Sin(a)*r);
        }
        int n=0;for(int y=0;y<ys.Length-1;y++)for(int i=0;i<sides;i++){int j=(i+1)%sides,a=y*sides+i,b=y*sides+j,c=(y+1)*sides+i,d=(y+1)*sides+j;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish(name,v,t);
    }
    public static Mesh CreateCourtBell(){return CreateLathed("AsgardCourtBell",new[]{0f,.035f,.09f,.18f,.3f,.39f,.45f},new[]{.12f,.19f,.25f,.34f,.32f,.21f,.1f},32,.035f);}
    public static Mesh CreateMarketBarrel(){return CreateLathed("AsgardMarketBarrel",new[]{-.5f,-.44f,-.27f,0f,.27f,.44f,.5f},new[]{.34f,.42f,.47f,.49f,.47f,.42f,.34f},24,.025f);}
    public static Mesh CreateAsgardApple(){return CreateLathed("AsgardGoldenApple",new[]{-.26f,-.22f,-.13f,0f,.14f,.23f,.28f,.3f},new[]{.04f,.13f,.22f,.29f,.27f,.2f,.11f,.02f},32,.11f);}
    public static Mesh CreatePalaceDome(){
        const int sides=48;float[] ys={0f,.1f,.18f,.36f,.57f,.76f,.91f,1f};float[] rs={.84f,1f,.96f,.82f,.65f,.45f,.24f,.025f};int rings=ys.Length;var v=new Vector3[sides*rings];var t=new int[(rings-1)*sides*6];
        for(int y=0;y<rings;y++)for(int i=0;i<sides;i++){float a=(i+(y%2)*.5f)*Mathf.PI*2f/sides;float fluting=1f+.022f*Mathf.Cos(a*12f);v[y*sides+i]=new Vector3(Mathf.Cos(a)*rs[y]*fluting,ys[y],Mathf.Sin(a)*rs[y]*fluting);}
        int n=0;for(int y=0;y<rings-1;y++)for(int i=0;i<sides;i++){int j=(i+1)%sides,a=y*sides+i,b=y*sides+j,c=(y+1)*sides+i,d=(y+1)*sides+j;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("AsgardPalaceDome",v,t);
    }
    public static Mesh CreateCurvedHorn(){
        Vector3[] path={new Vector3(0,0,0),new Vector3(.01f,.18f,0),new Vector3(.1f,.37f,0),new Vector3(.25f,.52f,0),new Vector3(.43f,.63f,0)};float[] radius={.11f,.095f,.07f,.042f,.003f};const int sides=10;int count=path.Length;var v=new Vector3[count*sides];var t=new int[(count-1)*sides*6];
        for(int p=0;p<count;p++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2f/sides;v[p*sides+i]=path[p]+new Vector3(Mathf.Cos(a)*radius[p],0,Mathf.Sin(a)*radius[p]);}
        int n=0;for(int p=0;p<count-1;p++)for(int i=0;i<sides;i++){int j=(i+1)%sides,a=p*sides+i,b=p*sides+j,c=(p+1)*sides+i,d=(p+1)*sides+j;t[n++]=a;t[n++]=c;t[n++]=b;t[n++]=b;t[n++]=c;t[n++]=d;}
        return Finish("SweptCrownHorn",v,t);
    }
    public static Mesh CreateTorus(){const int major=32,minor=10;const float ring=.44f,tube=.055f;var v=new Vector3[major*minor];var t=new int[major*minor*6];for(int a=0;a<major;a++)for(int b=0;b<minor;b++){float u=a*Mathf.PI*2f/major,q=b*Mathf.PI*2f/minor;float r=ring+Mathf.Cos(q)*tube;v[a*minor+b]=new Vector3(Mathf.Cos(u)*r,Mathf.Sin(q)*tube,Mathf.Sin(u)*r);}int n=0;for(int a=0;a<major;a++)for(int b=0;b<minor;b++){int an=(a+1)%major,bn=(b+1)%minor,i=a*minor+b,j=an*minor+b,k=a*minor+bn,l=an*minor+bn;t[n++]=i;t[n++]=j;t[n++]=k;t[n++]=k;t[n++]=j;t[n++]=l;}return Finish("GoldBeltRing",v,t);}
    public static Mesh Load(string name){Mesh mesh=Resources.Load<Mesh>("Models/"+name);if(mesh!=null)return mesh;switch(name){case "TailoredRobe":return CreateRobe();case "FlowingCape":return CreateCape();case "FeatheredWing":return CreateWing();case "AsgardArch":return CreateArch();case "SweptCrownHorn":return CreateCurvedHorn();case "GoldBeltRing":return CreateTorus();case "AsgardOliveCanopy":return CreateOliveCanopy();case "AsgardCourtBell":return CreateCourtBell();case "AsgardMarketBarrel":return CreateMarketBarrel();case "AsgardGoldenApple":return CreateAsgardApple();default:return null;}}
}
