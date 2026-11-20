using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class DistrictPoint { public float x, z; }
[Serializable] public class PatrolData { public List<DistrictPoint> points = new List<DistrictPoint>(); public float speed = 1.2f; }
[Serializable] public class InteractionData { public string id, type, label; public float x, z, radius; }
[Serializable] public class DistrictBounds { public float minX, maxX, minZ, maxZ; }
[Serializable] public class DistrictMeta { public string title, note; }
[Serializable] public class DistrictData {
    public int version; public DistrictBounds bounds; public DistrictPoint playerStart, gate;
    public PatrolData patrol; public List<InteractionData> interactions = new List<InteractionData>(); public DistrictMeta district;
}

public class DistrictDataLoader : MonoBehaviour {
    public DistrictData Data { get; private set; }
    public DistrictData Load() {
        TextAsset json = Resources.Load<TextAsset>("asgard-district");
        if (json == null) { Debug.LogError("Missing Assets/Resources/asgard-district.json"); return null; }
        Data = JsonUtility.FromJson<DistrictData>(json.text);
        if (Data == null || Data.bounds == null || Data.playerStart == null || Data.gate == null) Debug.LogError("District data is missing required fields.");
        return Data;
    }
}
