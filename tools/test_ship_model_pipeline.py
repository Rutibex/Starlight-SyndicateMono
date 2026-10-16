"""Quick procedural-model regression on four representative recipe fixtures.

All generated files live in a disposable copy; the full asset tree is never altered.
"""
import copy,contextlib,io,json,shutil,tempfile,unittest
from pathlib import Path
import ship_model_pipeline as p
class PipelineTests(unittest.TestCase):
 def setUp(self):
  self.saved=p.ROOT;self.temp=tempfile.TemporaryDirectory();p.ROOT=Path(self.temp.name)
  (p.ROOT/'Assets').mkdir();(p.ROOT/'Art').mkdir()
  for name in ['Fleet.json','Factions.json']:shutil.copyfile(self.saved/'Assets'/name,p.ROOT/'Assets'/name)
  for name in ['FactionArtDirection.json','ShipRecipes.json']:shutil.copyfile(self.saved/'Art'/name,p.ROOT/'Art'/name)
  recipes=p.ROOT/'Art/ShipRecipes.json'
  selected=json.loads(recipes.read_text());selected['Ships']=selected['Ships'][:4]
  recipes.write_text(json.dumps(selected))
  with contextlib.redirect_stdout(io.StringIO()):p.build()
 def tearDown(self):p.ROOT=self.saved;self.temp.cleanup()
 def first(self):return next((p.ROOT/'Assets/FleetModels').glob('*lod0.gltf'))
 def test_deterministic(self):
  before={f.name:f.read_bytes() for f in (p.ROOT/'Assets/FleetModels').glob('*.gltf')}
  with contextlib.redirect_stdout(io.StringIO()):p.build()
  self.assertEqual(before,{f.name:f.read_bytes() for f in (p.ROOT/'Assets/FleetModels').glob('*.gltf')})
 def test_obj_roundtrip(self):
  m=p.read(p.ROOT/'Assets/ShipModelManifest.json');e=next(x for x in m['Entries'] if x['Status']=='ready');tri=e['Lods'][0]['Triangles']
  with contextlib.redirect_stdout(io.StringIO()):p.import_obj(p.ROOT/'Art/Exports'/(e['ShipId']+'.obj'),e['ShipId'],0)
  self.assertEqual(tri,p.read(p.ROOT/'Assets/ShipModelManifest.json')['Entries'][m['Entries'].index(e)]['Lods'][0]['Triangles'])
 def test_bad_assets(self):
  valid=p.read(self.first());cases=[]
  j=copy.deepcopy(valid);j['buffers'][0]['uri']='file.bin';cases.append(j)
  j=copy.deepcopy(valid);j['accessors'][0]['count']=999999999;cases.append(j)
  j=copy.deepcopy(valid);j['nodes'][0]['scale']=[2,2,2];cases.append(j)
  j=copy.deepcopy(valid);j['meshes'][0]['primitives'][0]['mode']=1;cases.append(j)
  for j in cases:
   with self.subTest(case=j):
    f=p.ROOT/'bad.gltf';f.write_text(json.dumps(j))
    with self.assertRaises(ValueError):p.validate_gltf(f)
 def test_bad_manifest_hash(self):
  f=self.first();f.write_text(f.read_text()+' ')
  with self.assertRaises(ValueError):p.validate()
 def test_bad_obj_keeps_asset(self):
  m=p.read(p.ROOT/'Assets/ShipModelManifest.json');e=next(x for x in m['Entries'] if x['Status']=='ready');f=p.ROOT/'Assets'/e['Lods'][0]['File'];before=f.read_bytes();obj=p.ROOT/'bad.obj';obj.write_text('v 0 0 0\nf 1 2 3\n')
  with self.assertRaises(ValueError):p.import_obj(obj,e['ShipId'],0)
  self.assertEqual(before,f.read_bytes())
 def test_faction_guard(self):
  f=p.ROOT/'Art/ShipRecipes.json';j=p.read(f);j['Ships'][0]['Faction']='Wrong';f.write_text(json.dumps(j))
  with self.assertRaises(ValueError):p.context()
if __name__=='__main__':unittest.main()
