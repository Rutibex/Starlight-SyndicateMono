using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace StarlightSyndicateDemo;
public sealed record ShipMaterialMesh(VertexPositionNormalTexture[] Vertices,Vector3 Color,Vector3 Emission,float Roughness);
public sealed record ShipRenderModel(ShipMaterialMesh[] Parts,BoundingBox Bounds)
{
 public int Triangles=>Parts.Sum(p=>p.Vertices.Length/3);
 public VertexPositionNormalTexture[] Flat=>Parts.SelectMany(p=>p.Vertices).ToArray();
}
public static class ShipModelAssets
{
 public static readonly List<string> Warnings=[];
 public static float NearDistance=750,FarDistance=2400;
 public static int SelectLod(float distance)=>distance>FarDistance?2:distance>NearDistance?1:0;
 public static Dictionary<string,ShipRenderModel[]> LoadAll(string assets)
 {
  var result=new Dictionary<string,ShipRenderModel[]>();
  try{
   using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(assets,"ShipModelManifest.json")));
   if(doc.RootElement.GetProperty("Version").GetInt32()!=1)throw new InvalidDataException("Unsupported model manifest version");
   var distances=doc.RootElement.GetProperty("LodDistances");float near=distances[0].GetSingle(),far=distances[1].GetSingle();if(!float.IsFinite(near)||!float.IsFinite(far)||near<=0||far<=near)throw new InvalidDataException("Invalid LOD distances");NearDistance=near;FarDistance=far;
   foreach(var entry in doc.RootElement.GetProperty("Entries").EnumerateArray().Where(e=>e.GetProperty("Status").GetString()=="ready")){
    string id=entry.GetProperty("ShipId").GetString()!;var lods=new List<ShipRenderModel>();
    foreach(var lod in entry.GetProperty("Lods").EnumerateArray()){
     try{string path=Path.GetFullPath(Path.Combine(assets,lod.GetProperty("File").GetString()!));string allowed=Path.GetFullPath(Path.Combine(assets,"FleetModels"))+Path.DirectorySeparatorChar;if(!path.StartsWith(allowed,StringComparison.Ordinal))throw new InvalidDataException("Unsafe model path");var model=Read(path);if(model.Triangles!=lod.GetProperty("Triangles").GetInt32())throw new InvalidDataException("Model manifest triangle count mismatch");lods.Add(model);}
     catch(Exception ex) when(ex is IOException or JsonException or FormatException or InvalidDataException or InvalidOperationException or IndexOutOfRangeException or ArgumentException or KeyNotFoundException or OverflowException){Warn(id+": "+ex.Message);if(lods.Count>0)lods.Add(lods[^1]);else break;}
    }
    if(lods.Count==3)result.Add(id,lods.ToArray());else Warn(id+": using legacy hull fallback");
   }
  }catch(Exception ex) when(ex is IOException or JsonException or InvalidDataException or InvalidOperationException or ArgumentException or KeyNotFoundException or OverflowException){Warn("Model manifest: "+ex.Message+"; using legacy hulls");}
  return result;
 }
 static void Warn(string text){Warnings.Add(text);Console.Error.WriteLine("MODEL WARNING: "+text);}
 public static ShipRenderModel Read(string path)
 {
  using var document=JsonDocument.Parse(File.ReadAllText(path));var root=document.RootElement;
  void Require(bool ok,string why){if(!ok)throw new InvalidDataException(why);}
  Require(root.GetProperty("asset").GetProperty("version").GetString()=="2.0","Expected glTF 2");
  Require(root.GetProperty("buffers").GetArrayLength()==1&&root.GetProperty("meshes").GetArrayLength()==1,"One buffer and mesh required");
  var node=root.GetProperty("nodes");Require(node.GetArrayLength()==1&&node[0].EnumerateObject().Count()==1&&node[0].GetProperty("mesh").GetInt32()==0,"Bake node transforms");
  string uri=root.GetProperty("buffers")[0].GetProperty("uri").GetString()!;Require(uri.StartsWith("data:application/octet-stream;base64,"),"Embedded buffers required");var data=Convert.FromBase64String(uri[(uri.IndexOf(',')+1)..]);Require(data.Length==root.GetProperty("buffers")[0].GetProperty("byteLength").GetInt32(),"Buffer length mismatch");Require(!root.TryGetProperty("textures",out _),"Textured models are outside this flat-material runtime profile");
  (int Offset,int Stride,int Count,int Type) Access(int index,string type,int components,bool position)
  {
   var a=root.GetProperty("accessors")[index];var v=root.GetProperty("bufferViews")[a.GetProperty("bufferView").GetInt32()];int ct=a.GetProperty("componentType").GetInt32();
   Require(a.GetProperty("type").GetString()==type&&!a.TryGetProperty("sparse",out _)&&v.GetProperty("buffer").GetInt32()==0,"Unsupported accessor");Require(position?ct==5126:ct is 5123 or 5125,"Unsupported component type");
   int size=ct==5123?2:4;int vo=v.TryGetProperty("byteOffset",out var vb)?vb.GetInt32():0;int ao=a.TryGetProperty("byteOffset",out var ab)?ab.GetInt32():0;int count=a.GetProperty("count").GetInt32();int stride=v.TryGetProperty("byteStride",out var st)?st.GetInt32():components*size;int length=v.GetProperty("byteLength").GetInt32();long offset=(long)vo+ao;
   Require(count>0&&vo>=0&&ao>=0&&stride>=components*size&&offset+(long)(count-1)*stride+components*size<=(long)vo+length&&(long)vo+length<=data.Length,"Accessor out of bounds");return ((int)offset,stride,count,ct);
  }
  Vector3 Vec((int Offset,int Stride,int Count,int Type) a,int i)=>new(BitConverter.ToSingle(data,a.Offset+i*a.Stride),BitConverter.ToSingle(data,a.Offset+i*a.Stride+4),BitConverter.ToSingle(data,a.Offset+i*a.Stride+8));
  bool Finite(Vector3 v)=>float.IsFinite(v.X)&&float.IsFinite(v.Y)&&float.IsFinite(v.Z);
  Vector3 ColorVector(JsonElement element){var a=element.EnumerateArray().Select(x=>x.GetSingle()).ToArray();Require(a.Length>=3&&a.All(x=>float.IsFinite(x)&&x>=0&&x<=1),"Invalid material color");return new(a[0],a[1],a[2]);}
  var parts=new List<ShipMaterialMesh>();Vector3 min=new(float.MaxValue),max=new(float.MinValue);int triangles=0;
  foreach(var primitive in root.GetProperty("meshes")[0].GetProperty("primitives").EnumerateArray()){
   Require(!primitive.TryGetProperty("mode",out var mode)||mode.GetInt32()==4,"Triangle lists required");var attrs=primitive.GetProperty("attributes");var pos=Access(attrs.GetProperty("POSITION").GetInt32(),"VEC3",3,true);var normal=Access(attrs.GetProperty("NORMAL").GetInt32(),"VEC3",3,true);var indices=Access(primitive.GetProperty("indices").GetInt32(),"SCALAR",1,false);Require(pos.Count==normal.Count&&indices.Count%3==0,"Invalid vertex counts");triangles+=indices.Count/3;Require(triangles<=2500,"Model triangle budget exceeded");var vertices=new VertexPositionNormalTexture[indices.Count];
   for(int i=0;i<indices.Count;i++){int at=indices.Offset+i*indices.Stride;uint ix=indices.Type==5123?BitConverter.ToUInt16(data,at):BitConverter.ToUInt32(data,at);Require(ix<pos.Count,"Index outside vertex array");var p=Vec(pos,(int)ix);var n=Vec(normal,(int)ix);Require(Finite(p)&&Finite(n)&&Math.Abs(n.LengthSquared()-1)<.02,"Invalid position or normal");min=Vector3.Min(min,p);max=Vector3.Max(max,p);vertices[i]=new(p,n,Vector2.Zero);}
   for(int i=0;i<vertices.Length;i+=3)Require(Vector3.Cross(vertices[i+1].Position-vertices[i].Position,vertices[i+2].Position-vertices[i].Position).LengthSquared()>1e-12,"Degenerate triangle");
   var material=root.GetProperty("materials")[primitive.GetProperty("material").GetInt32()];var pbr=material.GetProperty("pbrMetallicRoughness");Require(!pbr.TryGetProperty("baseColorTexture",out _),"Textures not supported");var color=ColorVector(pbr.GetProperty("baseColorFactor"));var emission=material.TryGetProperty("emissiveFactor",out var em)?ColorVector(em):Vector3.Zero;float rough=pbr.GetProperty("roughnessFactor").GetSingle();Require(float.IsFinite(rough)&&rough>=0&&rough<=1,"Invalid roughness");parts.Add(new(vertices,color,emission,rough));
  }
  Require(parts.Count>0&&(max-min).Z>.1f&&Math.Max((max-min).X,Math.Max((max-min).Y,(max-min).Z))<=30,"Model outside size profile");return new(parts.ToArray(),new(min,max));
 }
}
