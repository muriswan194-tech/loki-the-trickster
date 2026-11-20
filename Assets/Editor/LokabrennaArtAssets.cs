using System.IO;
using UnityEditor;
using UnityEngine;

// Generates the project's small, reusable custom mesh kit on first open.
public static class LokabrennaArtAssets {
    const string Folder="Assets/Resources/Models";
    [InitializeOnLoadMethod] static void Queue(){EditorApplication.delayCall+=Build;}
    public static void Build(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        Directory.CreateDirectory(Folder);bool changed=false;
        Save("TailoredRobe",LokabrennaMeshLibrary.CreateRobe(),ref changed);
        Save("FlowingCape",LokabrennaMeshLibrary.CreateCape(),ref changed);
        Save("FeatheredWing",LokabrennaMeshLibrary.CreateWing(),ref changed);
        Save("AsgardArch",LokabrennaMeshLibrary.CreateArch(),ref changed);
        Save("AsgardPalaceSpire",LokabrennaMeshLibrary.CreatePalaceSpire(),ref changed);
        Save("AsgardClothAwning",LokabrennaMeshLibrary.CreateAwning(),ref changed);
        Save("AsgardFountainBowl",LokabrennaMeshLibrary.CreateFountainBowl(),ref changed);
        Save("AsgardPalaceDome",LokabrennaMeshLibrary.CreatePalaceDome(),ref changed);
        Save("SweptCrownHorn",LokabrennaMeshLibrary.CreateCurvedHorn(),ref changed);
        Save("GoldBeltRing",LokabrennaMeshLibrary.CreateTorus(),ref changed);
        if(changed){AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Lokabrenna original mesh kit ready: robe, cape, wing, crown and arch.");}
    }
    static void Save(string name,Mesh mesh,ref bool changed){string path=Folder+"/"+name+".asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(path)!=null){Object.DestroyImmediate(mesh);return;}AssetDatabase.CreateAsset(mesh,path);changed=true;}
}

public sealed class LokabrennaTextureImport : AssetPostprocessor {
    void OnPreprocessTexture(){
        if(!assetPath.EndsWith("_nor_gl_2k.jpg",System.StringComparison.OrdinalIgnoreCase))return;
        TextureImporter importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.NormalMap;importer.sRGBTexture=false;importer.maxTextureSize=2048;
    }
}
