"""Original procedural ship art. Python 3 standard library only.
Build: python tools/ship_model_pipeline.py build
Check: python tools/ship_model_pipeline.py validate
OBJ:   python tools/ship_model_pipeline.py import-obj FILE --ship-id ID --lod 0
"""
import argparse,base64,hashlib,json,math,struct
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
def read(p):return json.loads(p.read_text())
def sub(a,b):return [a[i]-b[i] for i in range(3)]
def cross(a,b):return [a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]]
def normal(a,b,c):
 n=cross(sub(b,a),sub(c,a));l=math.sqrt(sum(x*x for x in n))
 if l<1e-8:raise ValueError('Degenerate triangle')
 return [x/l for x in n]
class Mesh:
 def __init__(self):self.groups={}
 def tri(self,a,b,c,material):
  n=normal(a,b,c);g=self.groups.setdefault(material,[])
  g.extend([(a,n),(b,n),(c,n)])
 def tube(self,p,lod):
  center=p['Center'];rings=p['Rings'];sides=max(4,p.get('Sides',16)//(1+lod))
  pts=[[[center[0]+r[3]+r[1]*math.cos(i*math.tau/sides),center[1]+r[2]*math.sin(i*math.tau/sides),center[2]+r[0]] for i in range(sides)] for r in rings]
  for j in range(len(pts)-1):
   for i in range(sides):
    k=(i+1)%sides;self.tri(pts[j][i],pts[j][k],pts[j+1][k],p['Material']);self.tri(pts[j][i],pts[j+1][k],pts[j+1][i],p['Material'])
  for end in [0,-1]:
   mid=[sum(v[c] for v in pts[end])/sides for c in range(3)]
   for i in range(sides):
    a,b=pts[end][i],pts[end][(i+1)%sides]
    self.tri(mid,b,a,p['Material']) if end==0 else self.tri(mid,a,b,p['Material'])
 def box(self,p):
  c=p['Center'];x,y,z=p['Size'];v=[[c[0]+a*x,c[1]+b*y,c[2]+d*z] for a,b,d in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
  for face in [(0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(0,1,5,4),(3,7,6,2)]:
   a,b,c,d=[v[i] for i in face];self.tri(a,b,c,p['Material']);self.tri(a,c,d,p['Material'])
 def export(self,path,profile,scale=.2):
  data=bytearray();views=[];accessors=[];primitives=[];materials=[]
  def acc(values,typ,components,ctype):
   offset=len(data);flat=[c for v in values for c in v] if components>1 else values
   data.extend(struct.pack('<'+('f' if ctype==5126 else 'I')*len(flat),*flat));vi=len(views);views.append(dict(buffer=0,byteOffset=offset,byteLength=len(data)-offset));a=dict(bufferView=vi,componentType=ctype,count=len(values),type=typ)
   if typ=='VEC3' and ctype==5126:a.update(min=[min(v[i] for v in values) for i in range(3)],max=[max(v[i] for v in values) for i in range(3)])
   ai=len(accessors);accessors.append(a);return ai
  for material,g in sorted(self.groups.items()):
   positions=[[x*scale for x in v] for v,n in g];normals=[n for v,n in g];col=profile['Materials'][material];rgb=[int(col[i:i+2],16)/255 for i in (1,3,5)]
   mi=len(materials);materials.append(dict(name=material,pbrMetallicRoughness=dict(baseColorFactor=rgb+[1],metallicFactor=profile['Metallic'],roughnessFactor=profile['Roughness']),emissiveFactor=[x*.75 for x in rgb] if material=='engine' else [0,0,0],doubleSided=False))
   primitives.append(dict(attributes=dict(POSITION=acc(positions,'VEC3',3,5126),NORMAL=acc(normals,'VEC3',3,5126)),indices=acc(list(range(len(g))),'SCALAR',1,5125),material=mi,mode=4))
  result=dict(asset=dict(version='2.0',generator='Starlight model pipeline 20 / original geometry'),scene=0,scenes=[dict(nodes=[0])],nodes=[dict(mesh=0)],meshes=[dict(primitives=primitives)],materials=materials,buffers=[dict(uri='data:application/octet-stream;base64,'+base64.b64encode(data).decode(),byteLength=len(data))],bufferViews=views,accessors=accessors)
  path.parent.mkdir(parents=True,exist_ok=True);path.write_text(json.dumps(result,separators=(',',':'))+'\n');return validate_gltf(path)
def validate_gltf(path):
 j=read(path)
 if j['asset']['version']!='2.0' or len(j['buffers'])!=1 or len(j['meshes'])!=1:raise ValueError('Expected one mesh and one embedded glTF 2 buffer')
 if len(j['nodes'])!=1 or j['nodes'][0]!={'mesh':0}:raise ValueError('Bake transforms and export one root mesh')
 uri=j['buffers'][0]['uri']
 if not uri.startswith('data:application/octet-stream;base64,'):raise ValueError('Embed buffers; external resources are unsupported')
 data=base64.b64decode(uri.split(',')[1],validate=True)
 if len(data)!=j['buffers'][0]['byteLength']:raise ValueError('Buffer size mismatch')
 def values(index,typ,ctype):
  a=j['accessors'][index];v=j['bufferViews'][a['bufferView']];n={'VEC3':3,'SCALAR':1}[typ];size=2 if ctype==5123 else 4;stride=v.get('byteStride',n*size);off=v.get('byteOffset',0)+a.get('byteOffset',0)
  if a['type']!=typ or a['componentType']!=ctype or 'sparse' in a or a['count']<=0 or v['buffer']!=0:raise ValueError('Unsupported accessor')
  if stride<n*size or off<0 or off+(a['count']-1)*stride+n*size>v.get('byteOffset',0)+v['byteLength'] or v.get('byteOffset',0)+v['byteLength']>len(data):raise ValueError('Accessor out of bounds')
  fmt='<' + ('f' if ctype==5126 else 'H' if ctype==5123 else 'I')*n
  return [struct.unpack_from(fmt,data,off+i*stride) for i in range(a['count'])]
 points=[];triangles=0
 for p in j['meshes'][0]['primitives']:
  if p.get('mode',4)!=4:raise ValueError('Triangle lists only')
  pos=values(p['attributes']['POSITION'],'VEC3',5126);norms=values(p['attributes']['NORMAL'],'VEC3',5126);ia=j['accessors'][p['indices']];indices=[v[0] for v in values(p['indices'],'SCALAR',ia['componentType'])]
  if ia['componentType'] not in (5123,5125) or len(pos)!=len(norms) or len(indices)%3:raise ValueError('Invalid vertex/index counts')
  if any(not all(math.isfinite(x) for x in v) for v in pos+norms):raise ValueError('Nonfinite vertex')
  if any(abs(sum(x*x for x in n)-1)>0.02 for n in norms):raise ValueError('Normals must have unit length')
  if max(indices)>=len(pos):raise ValueError('Index outside vertex array')
  for i in range(0,len(indices),3):normal(*(pos[indices[i+k]] for k in range(3)))
  mat=j['materials'][p['material']]
  if 'baseColorTexture' in mat.get('pbrMetallicRoughness',{}) or 'textures' in j:raise ValueError('This runtime supports flat material colours, not textures')
  for c in mat['pbrMetallicRoughness']['baseColorFactor']+mat.get('emissiveFactor',[0,0,0]):
   if not math.isfinite(c) or not 0<=c<=1:raise ValueError('Invalid material colour')
  points+=pos;triangles+=len(indices)//3
 if triangles>2500:raise ValueError('Per-model 2500 triangle budget exceeded')
 lo=[min(p[i] for p in points) for i in range(3)];hi=[max(p[i] for p in points) for i in range(3)]
 if hi[2]-lo[2]<.1 or max(hi[i]-lo[i] for i in range(3))>30:raise ValueError('Model size outside runtime profile; use render units, +Z nose')
 return dict(Triangles=triangles,Min=lo,Max=hi,Sha256=hashlib.sha256(path.read_bytes()).hexdigest())
def export_obj(mesh,path,profile):
 lines=['# Original Starlight Syndicate geometry; +Y up, +Z nose','mtllib '+path.with_suffix('.mtl').name];ml=[];index=1
 for material,g in sorted(mesh.groups.items()):
  rgb=[int(profile['Materials'][material][i:i+2],16)/255 for i in (1,3,5)];ml.extend(['newmtl '+material,'Kd '+' '.join(map(str,rgb)),'Ks 0.3 0.3 0.3','Ns 24']);lines.append('usemtl '+material)
  for p,n in g:lines+=['v '+' '.join(str(round(x*.2,6)) for x in p),'vn '+' '.join(str(round(x,6)) for x in n)]
  for i in range(0,len(g),3):lines.append('f '+' '.join(f'{index+i+k}//{index+i+k}' for k in range(3)))
  index+=len(g)
 path.write_text('\n'.join(lines)+'\n');path.with_suffix('.mtl').write_text('\n'.join(ml)+'\n')
def context():
 fleet=read(ROOT/'Assets/Fleet.json');profiles=read(ROOT/'Art/FactionArtDirection.json')['Profiles'];recipes=read(ROOT/'Art/ShipRecipes.json')['Ships']
 if {x['Faction'] for x in profiles}!={x['Name'] for x in read(ROOT/'Assets/Factions.json')}:raise ValueError('Faction art coverage mismatch')
 if len({x['Faction'] for x in profiles})!=len(profiles):raise ValueError('Duplicate faction profile')
 if len({r['ShipId'] for r in recipes})!=len(recipes):raise ValueError('Duplicate recipe')
 byid={h['Id']:h for h in fleet};byf={f['Faction']:f for f in profiles}
 for r in recipes:
  if r['ShipId'] not in byid or r['Faction']!=byid[r['ShipId']]['Faction']:raise ValueError('Unknown ship or incorrect faction')
  for a in r['Anchors']:
   if len(a['Position'])!=3 or not all(math.isfinite(x) for x in a['Position']):raise ValueError('Invalid anchor')
 return fleet,byf,recipes
def build():
 fleet,profiles,recipes=context();ready={};out=ROOT/'Assets/FleetModels';obj=ROOT/'Art/Exports';obj.mkdir(exist_ok=True)
 for r in recipes:
  lods=[]
  for lod in range(3):
   mesh=Mesh()
   for p in r['Parts']:
    if p.get('MaxLod',2)<lod:continue
    if p['Material'] not in profiles[r['Faction']]['Materials']:raise ValueError('Unknown material')
    mesh.tube(p,lod) if p['Type']=='tube' else mesh.box(p) if p['Type']=='box' else (_ for _ in ()).throw(ValueError('Unknown primitive'))
   path=out/(r['ShipId']+f'_lod{lod}.gltf');info=mesh.export(path,profiles[r['Faction']]);lods.append(dict(File='FleetModels/'+path.name,**info))
   if lod==0:export_obj(mesh,obj/(r['ShipId']+'.obj'),profiles[r['Faction']])
  ready[r['ShipId']]=dict(Status='ready',Faction=r['Faction'],Family=profiles[r['Faction']]['Family'],Lods=lods,Anchors=r['Anchors'],Design=r['Design'])
 entries=[dict(ShipId=h['Id'],Name=h['Name'],**ready.get(h['Id'],dict(Status='planned',Faction=h['Faction'],Family=profiles[h['Faction']]['Family'],Lods=[],Anchors=[],Design='No dedicated model yet.'))) for h in fleet]
 manifest=dict(Version=1,Up='+Y',Nose='+Z',Units='render units, not physical metres',LodDistances=[750,2400],TriangleBudget=2500,Entries=entries)
 (ROOT/'Assets/ShipModelManifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
 validate();print(f'Built {len(recipes)} original ships / {len(recipes)*3} LOD assets / {len(recipes)} OBJ + MTL exports.')
def validate():
 fleet,profiles,recipes=context();j=read(ROOT/'Assets/ShipModelManifest.json');entries=j['Entries']
 if len(entries)!=len(fleet) or {x['ShipId'] for x in entries}!={h['Id'] for h in fleet}:raise ValueError('Registry coverage mismatch')
 for e in entries:
  if e['Status']=='ready':
   if len(e['Lods'])!=3:raise ValueError('Need three explicit LODs')
   for lod in e['Lods']:
    path=ROOT/'Assets'/lod['File']
    if not path.resolve().is_relative_to((ROOT/'Assets/FleetModels').resolve()):raise ValueError('Unsafe path')
    info=validate_gltf(path)
    if info!= {k:lod[k] for k in info}:raise ValueError('Stale bounds/count/hash; rebuild or import again')
   if not e['Lods'][0]['Triangles']>=e['Lods'][1]['Triangles']>=e['Lods'][2]['Triangles']:raise ValueError('LOD triangle counts must decrease')
 print(f'Validated {len(profiles)} faction profiles, {len(entries)} registry entries and {sum(len(e["Lods"]) for e in entries)} dedicated LODs.')
def import_obj(path,shipid,lod):
 fleet,profiles,recipes=context();entry=next((h for h in fleet if h['Id']==shipid),None)
 if entry is None:raise ValueError('Unknown registry ID')
 manifest=read(ROOT/'Assets/ShipModelManifest.json');e=next(x for x in manifest['Entries'] if x['ShipId']==shipid)
 if e['Status']!='ready':raise ValueError('Create a reviewed recipe and three LODs before importing replacements for a new hull')
 vertices=[];mesh=Mesh();material='hull'
 for line in Path(path).read_text().splitlines():
  p=line.split()
  if not p:continue
  if p[0]=='v':vertices.append([float(v) for v in p[1:4]])
  elif p[0]=='usemtl':material=p[1] if p[1] in profiles[entry['Faction']]['Materials'] else 'hull'
  elif p[0]=='f':
   face=[]
   for token in p[1:]:
    n=int(token.split('/')[0]);index=n-1 if n>0 else len(vertices)+n
    if n==0 or index<0 or index>=len(vertices):raise ValueError('OBJ face index out of range')
    face.append(vertices[index])
   # Convex faces only; author triangulation for concave faces.
   for i in range(1,len(face)-1):mesh.tri(face[0],face[i],face[i+1],material)
 target=ROOT/'Assets'/e['Lods'][lod]['File'];temp=target.with_suffix('.candidate.gltf')
 try:
  info=mesh.export(temp,profiles[entry['Faction']],scale=1);newlod=dict(File=e['Lods'][lod]['File'],**info)
  counts=[newlod['Triangles'] if i==lod else x['Triangles'] for i,x in enumerate(e['Lods'])]
  if not counts[0]>=counts[1]>=counts[2]:raise ValueError('Imported LOD breaks detail ordering')
  temp.replace(target);e['Lods'][lod]=newlod;e['Design']+=' / artist OBJ override';(ROOT/'Assets/ShipModelManifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
 finally:temp.unlink(missing_ok=True)
 validate();print('Imported reviewed OBJ replacement:',entry['Name'],'LOD',lod)
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);subp=p.add_subparsers(dest='command',required=True)
 subp.add_parser('build');subp.add_parser('validate');imp=subp.add_parser('import-obj');imp.add_argument('file');imp.add_argument('--ship-id',required=True);imp.add_argument('--lod',type=int,choices=[0,1,2],required=True)
 args=p.parse_args()
 if args.command=='build':build()
 elif args.command=='validate':validate()
 else:import_obj(args.file,args.ship_id,args.lod)
