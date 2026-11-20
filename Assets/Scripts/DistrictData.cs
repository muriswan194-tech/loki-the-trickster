using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class DistrictPoint { public float x, z; }
[Serializable] public class MapTrigger { public string id, type; public float x, z, radius; }
[Serializable] public class PatrolData { public List<DistrictPoint> points = new List<DistrictPoint>(); public float speed = 1.2f; }
[Serializable] public class InteractionData { public string id, type, label; public float x, z, radius, hearing, distraction; public bool noisy, movable, solid; }
[Serializable] public class LandmarkData { public string id, type; public float x,z; }
[Serializable] public class MapRouteData { public string id; public float width=1f; public List<DistrictPoint> points = new List<DistrictPoint>(); }
[Serializable] public class DistrictBounds { public float minX, maxX, minZ, maxZ; }
[Serializable] public class DistrictData {
    public DistrictBounds bounds;
    public DistrictPoint playerStart, bifrost;
    public PatrolData guard;
    public List<InteractionData> interactions = new List<InteractionData>();
    public List<LandmarkData> landmarks = new List<LandmarkData>();
    public List<MapTrigger> triggers = new List<MapTrigger>();
    public List<MapRouteData> routes = new List<MapRouteData>();
}

public static class SimpleJson {
    public static object Parse(string source) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var parser = new Parser(source);
        object value = parser.ReadValue(); parser.SkipWhite();
        if (!parser.AtEnd) throw new FormatException("Unexpected text after JSON value.");
        return value;
    }
    public static Dictionary<string, object> Object(object value) { return value as Dictionary<string, object>; }
    public static List<object> Array(object value) { return value as List<object>; }
    public static string String(Dictionary<string, object> obj, string key, string fallback = "") { object value; return obj != null && obj.TryGetValue(key, out value) && value is string ? (string)value : fallback; }
    public static float Number(Dictionary<string, object> obj, string key, float fallback = 0f) { object value; return obj != null && obj.TryGetValue(key, out value) && value is double ? (float)(double)value : fallback; }
    public static bool Boolean(Dictionary<string, object> obj,string key,bool fallback=false){object value;return obj!=null&&obj.TryGetValue(key,out value)&&value is bool?(bool)value:fallback;}
    sealed class Parser {
        readonly string s; int i;
        public Parser(string source) { s = source; }
        public bool AtEnd { get { return i >= s.Length; } }
        public void SkipWhite() { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }
        public object ReadValue() {
            SkipWhite(); if (AtEnd) throw new FormatException("Unexpected end of JSON.");
            char c=s[i]; if(c=='{') return ReadObject(); if(c=='[') return ReadArray(); if(c=='\"') return ReadString();
            if(c=='-' || char.IsDigit(c)) return ReadNumber();
            if(Match("true")) return true; if(Match("false")) return false; if(Match("null")) return null;
            throw new FormatException("Unexpected JSON token at character " + i + ".");
        }
        Dictionary<string, object> ReadObject() {
            i++; var result=new Dictionary<string, object>(); SkipWhite(); if(Take('}')) return result;
            while(true) { SkipWhite(); string key=ReadString(); SkipWhite(); Require(':'); result[key]=ReadValue(); SkipWhite(); if(Take('}')) return result; Require(','); }
        }
        List<object> ReadArray() {
            i++; var result=new List<object>(); SkipWhite(); if(Take(']')) return result;
            while(true) { result.Add(ReadValue()); SkipWhite(); if(Take(']')) return result; Require(','); }
        }
        string ReadString() {
            Require('\"'); var result=new System.Text.StringBuilder();
            while(i<s.Length) { char c=s[i++]; if(c=='\"') return result.ToString(); if(c!='\\') { result.Append(c); continue; }
                if(i>=s.Length) break; char e=s[i++]; switch(e) { case '\"': result.Append('\"'); break; case '\\': result.Append('\\'); break; case '/': result.Append('/'); break; case 'b': result.Append('\b'); break; case 'f': result.Append('\f'); break; case 'n': result.Append('\n'); break; case 'r': result.Append('\r'); break; case 't': result.Append('\t'); break;
                    case 'u': if(i+4>s.Length) throw new FormatException("Incomplete unicode escape."); result.Append((char)Convert.ToInt32(s.Substring(i,4),16)); i+=4; break; default: throw new FormatException("Invalid JSON escape."); }
            } throw new FormatException("Unterminated JSON string.");
        }
        double ReadNumber() { int start=i; if(s[i]=='-') i++; while(i<s.Length&&char.IsDigit(s[i])) i++; if(i<s.Length&&s[i]=='.'){i++;while(i<s.Length&&char.IsDigit(s[i]))i++;} if(i<s.Length&&(s[i]=='e'||s[i]=='E')){i++;if(i<s.Length&&(s[i]=='+'||s[i]=='-'))i++;while(i<s.Length&&char.IsDigit(s[i]))i++;} double n; if(!double.TryParse(s.Substring(start,i-start),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out n)) throw new FormatException("Invalid JSON number."); return n; }
        bool Match(string token) { if(i+token.Length>s.Length||string.CompareOrdinal(s,i,token,0,token.Length)!=0)return false;i+=token.Length;return true; }
        bool Take(char c) { if(i<s.Length&&s[i]==c){i++;return true;}return false; }
        void Require(char c) { SkipWhite(); if(!Take(c)) throw new FormatException("Expected '"+c+"' at character "+i+"."); }
    }
}

public class DistrictDataLoader {
    public DistrictData Data { get; private set; }
    public DistrictData Load() {
        TextAsset json=Resources.Load<TextAsset>("asgard-district");
        if(json==null){Debug.LogError("Missing Assets/Resources/asgard-district.json");return null;}
        try {
            var root=SimpleJson.Object(SimpleJson.Parse(json.text)); if(root==null) throw new FormatException("Root must be an object.");
            Data=new DistrictData { bounds=ReadBounds(root,"bounds"), playerStart=ReadPoint(root,"playerStart"), bifrost=ReadPoint(root,"bifrost"), guard=ReadPatrol(root,"guard") };
            object raw; if(root.TryGetValue("interactions",out raw)) foreach(var entry in SimpleJson.Array(raw)) { var o=SimpleJson.Object(entry); if(o==null) continue; Data.interactions.Add(new InteractionData { id=SimpleJson.String(o,"id"),type=SimpleJson.String(o,"type"),label=SimpleJson.String(o,"label"),x=SimpleJson.Number(o,"x"),z=SimpleJson.Number(o,"z"),radius=SimpleJson.Number(o,"radius",1.6f),hearing=SimpleJson.Number(o,"hearing"),distraction=SimpleJson.Number(o,"distraction"),noisy=SimpleJson.Boolean(o,"noisy"),movable=SimpleJson.Boolean(o,"movable"),solid=SimpleJson.Boolean(o,"solid") }); }
            object triggerRaw; if(root.TryGetValue("triggers",out triggerRaw)) foreach(var entry in SimpleJson.Array(triggerRaw)) { var o=SimpleJson.Object(entry); if(o==null)continue;Data.triggers.Add(new MapTrigger{id=SimpleJson.String(o,"id"),type=SimpleJson.String(o,"type"),x=SimpleJson.Number(o,"x"),z=SimpleJson.Number(o,"z"),radius=SimpleJson.Number(o,"radius",1.7f)}); }
            object landmarkRaw; if(root.TryGetValue("landmarks",out landmarkRaw)) foreach(var entry in SimpleJson.Array(landmarkRaw)) { var o=SimpleJson.Object(entry); if(o==null)continue;Data.landmarks.Add(new LandmarkData{id=SimpleJson.String(o,"id"),type=SimpleJson.String(o,"type"),x=SimpleJson.Number(o,"x"),z=SimpleJson.Number(o,"z")}); }
            object routeRaw; if(root.TryGetValue("routes",out routeRaw)) foreach(var entry in SimpleJson.Array(routeRaw)){var o=SimpleJson.Object(entry);if(o==null)continue;var route=new MapRouteData{id=SimpleJson.String(o,"id"),width=SimpleJson.Number(o,"width",1f)};object points;if(o.TryGetValue("points",out points)&&SimpleJson.Array(points)!=null)foreach(object item in SimpleJson.Array(points)){var p=SimpleJson.Object(item);if(p!=null)route.points.Add(new DistrictPoint{x=SimpleJson.Number(p,"x"),z=SimpleJson.Number(p,"z")});}if(route.points.Count>=2)Data.routes.Add(route);}
            foreach(LandmarkData point in Data.landmarks)if(point.id=="bifrost-approach")Data.bifrost=new DistrictPoint{x=point.x,z=point.z};
            if(Data.bounds==null||Data.playerStart==null||Data.bifrost==null||Data.guard==null) throw new FormatException("Required district fields are missing.");
            return Data;
        } catch(Exception ex) { Debug.LogError("District JSON is invalid: "+ex.Message); return null; }
    }
    static DistrictPoint ReadPoint(Dictionary<string,object> root,string key) { object raw; if(!root.TryGetValue(key,out raw)) return null; var o=SimpleJson.Object(raw); return o==null?null:new DistrictPoint{x=SimpleJson.Number(o,"x"),z=SimpleJson.Number(o,"z")}; }
    static DistrictBounds ReadBounds(Dictionary<string,object> root,string key) { object raw; if(!root.TryGetValue(key,out raw))return null;var o=SimpleJson.Object(raw);return o==null?null:new DistrictBounds{minX=SimpleJson.Number(o,"minX"),maxX=SimpleJson.Number(o,"maxX"),minZ=SimpleJson.Number(o,"minZ"),maxZ=SimpleJson.Number(o,"maxZ")}; }
    static PatrolData ReadPatrol(Dictionary<string,object> root,string key) { object raw; if(!root.TryGetValue(key,out raw))return null;var o=SimpleJson.Object(raw);if(o==null)return null;var result=new PatrolData{speed=SimpleJson.Number(o,"speed",1.25f)};object points;if(o.TryGetValue("points",out points))foreach(var v in SimpleJson.Array(points)){var p=SimpleJson.Object(v);if(p!=null)result.points.Add(new DistrictPoint{x=SimpleJson.Number(p,"x"),z=SimpleJson.Number(p,"z")});}return result.points.Count>=2?result:null; }
}

[Serializable] public class EncounterDefinition {
    public string id, title, guestCharacterId, guestDisplayName, triggerId, firstLine, alarmLine, epilogueScene;
    public float triggerX, triggerZ, triggerRadius, portalX, portalZ, selectionWeight;
    public bool jamActive;
    public List<string> epilogue = new List<string>();
}
public static class EncounterRegistry {
    public static List<EncounterDefinition> LoadAll(DistrictData district=null) {
        TextAsset json=Resources.Load<TextAsset>("trouble-encounter");
        if(json==null){Debug.LogError("Missing Assets/Resources/trouble-encounter.json");return new List<EncounterDefinition>();}
        try {
            var root=SimpleJson.Object(SimpleJson.Parse(json.text));object raw;
            if(root==null||!root.TryGetValue("encounters",out raw))throw new FormatException("Expected an encounters array.");
            var entries=SimpleJson.Array(raw);if(entries==null)throw new FormatException("Encounters must be an array.");
            var results=new List<EncounterDefinition>();
            foreach(object entry in entries){var item=SimpleJson.Object(entry);if(item==null)continue;object triggerRaw,portalRaw;var trigger=item.TryGetValue("trigger",out triggerRaw)?SimpleJson.Object(triggerRaw):null;var portal=item.TryGetValue("portal",out portalRaw)?SimpleJson.Object(portalRaw):null;
                var result=new EncounterDefinition { id=SimpleJson.String(item,"id"),title=SimpleJson.String(item,"title"),guestCharacterId=SimpleJson.String(item,"guestCharacterId"),guestDisplayName=SimpleJson.String(item,"guestDisplayName"),triggerId=SimpleJson.String(item,"triggerId"),firstLine=SimpleJson.String(item,"firstLine"),alarmLine=SimpleJson.String(item,"alarmLine"),epilogueScene=SimpleJson.String(item,"epilogueScene"),triggerX=SimpleJson.Number(trigger,"x"),triggerZ=SimpleJson.Number(trigger,"z"),triggerRadius=SimpleJson.Number(trigger,"radius",1.75f),portalX=SimpleJson.Number(portal,"x"),portalZ=SimpleJson.Number(portal,"z"),selectionWeight=SimpleJson.Number(item,"selectionWeight",1f),jamActive=item.ContainsKey("jamActive")&&item["jamActive"] is bool&&(bool)item["jamActive"] };
                object lines;if(item.TryGetValue("epilogue",out lines)&&SimpleJson.Array(lines)!=null)foreach(object line in SimpleJson.Array(lines))if(line is string)result.epilogue.Add((string)line);
                if(!string.IsNullOrEmpty(result.triggerId)&&district!=null){foreach(MapTrigger mapTrigger in district.triggers)if(mapTrigger.id==result.triggerId){result.triggerX=mapTrigger.x;result.triggerZ=mapTrigger.z;result.triggerRadius=mapTrigger.radius;break;}}
                if(string.IsNullOrEmpty(result.id)||string.IsNullOrEmpty(result.guestCharacterId))throw new FormatException("Encounter id and guestCharacterId are required.");results.Add(result);
            }
            return results;
        } catch(Exception ex){Debug.LogError("Encounter JSON is invalid: "+ex.Message);return new List<EncounterDefinition>();}
    }
    public static EncounterDefinition LoadById(string id){foreach(var item in LoadAll())if(item.id==id)return item;return null;}
    public static EncounterDefinition LoadJamEncounter(){foreach(var item in LoadAll())if(item.jamActive)return item;var all=LoadAll();return all.Count>0?all[0]:null;
    }
}
