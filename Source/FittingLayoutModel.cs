namespace StarlightSyndicateDemo;

// The layout is a projection of existing systems, not a second fitting inventory.
public sealed record FittingModule(string Kind,int Index,string Id,string Bank,bool? Online=null,bool Active=false,bool Burnt=false,string Payload="",int Loaded=0)
{
 public string Key=>Kind+":"+Index;
 public string Name=>ItemCatalog.Get(Id).Name;
}
public sealed record FittingSocket(string Bank,int Position,FittingModule? Module)
{
 public string Key=>Module?.Key??Bank+":empty:"+Position;
}
public sealed record FittingSpec(string Id,string Kind,string Bank,double Cpu=0,double Power=0,int Size=0);
public static class FittingLayoutModel
{
 public static readonly string[] Banks=["high","medium","low","rig"];
 public static readonly string[] Systems=["All systems","weapon","defense","ewar","support","enhancement","mobility","utility","special","rig","legacy"];
 public static readonly FittingSpec[] Catalog=WeaponCatalog.ById.Values.Select(d=>new FittingSpec(d.Id,"weapon","high",d.Cpu,d.Power,d.Size))
 .Concat(DefenseCatalog.ById.Values.Select(d=>new FittingSpec(d.Id,"defense",d.Slot,d.Cpu,d.Power,d.Size)))
 .Concat(EwarCatalog.ById.Values.Select(d=>new FittingSpec(d.Id,"ewar",d.Slot,d.Cpu,d.Power,d.Size)))
 .Concat(SupportCatalog.All.Select(d=>new FittingSpec(d.Id,"support",d.Slot,d.Cpu,d.Power,d.Size)))
 .Concat(EnhancementCatalog.All.Select(d=>new FittingSpec(d.Id,"enhancement",d.Slot,d.Cpu,d.Power)))
 .Concat(MobilityCatalog.All.Select(d=>new FittingSpec(d.Id,"mobility",d.Slot,d.Cpu,d.Power,d.Size)))
 .Concat(UtilityCatalog.All.Select(d=>new FittingSpec(d.Id,"utility",d.Slot,d.Cpu,d.Power,d.Size)))
 .Concat(SpecialCatalog.All.Select(d=>new FittingSpec(d.Id,"special","high",d.Cpu,d.Power)))
 .Concat(RigCatalog.All.Select(d=>new FittingSpec(d.Id,"rig","rig",Size:d.Size)))
 .Concat(new[]{"miner","analyzer","salvager"}.Select(id=>new FittingSpec(id,"legacy","high")))
 .OrderBy(d=>ItemCatalog.Get(d.Id).Name).ToArray();
 public static readonly Dictionary<string,FittingSpec> ById=Catalog.ToDictionary(d=>d.Id);
 public static List<FittingModule> Installed(Ship ship)
 {
  var a=new List<FittingModule>();
  for(int i=0;i<ship.Weapons.Length;i++)if(ship.WeaponAt(i) is {} s)a.Add(new("weapon",i,s.ModuleId,"high",s.Online,Active:s.Active,Burnt:s.Burnt,Payload:s.ChargeId,Loaded:s.Loaded));
  if(ship.LegacyUtilityOccupied)a.Add(new("legacy",1,ship.UtilityFit,"high",Active:ship.MinerOn));
  for(int i=0;i<ship.Ewar.Length;i++){var s=ship.Ewar[i];if(s.Definition is {} d)a.Add(new("ewar",i,s.ModuleId,d.Slot,s.Online,Active:s.Active,Burnt:s.Burnt,Payload:s.PayloadId,Loaded:s.Loaded));}
  for(int i=0;i<ship.Defense.Length;i++){var s=ship.Defense[i];if(s.Definition is {} d)a.Add(new("defense",i,s.ModuleId,d.Slot,s.Online,Active:s.Active,Burnt:s.Burnt,Payload:s.PayloadId,Loaded:s.Loaded));}
  for(int i=0;i<ship.Support.Length;i++){var s=ship.Support[i];if(s.Definition is {} d)a.Add(new("support",i,s.ModuleId,d.Slot,s.Online,Active:s.Active,Burnt:s.Burnt,Payload:s.PayloadId,Loaded:s.Loaded));}
  for(int i=0;i<ship.Enhancements.Length;i++){var s=ship.Enhancements[i];if(s.Definition is {} d)a.Add(new("enhancement",i,s.ModuleId,d.Slot,s.Online,s.Active,s.Burnt,s.ScriptId));}
  for(int i=0;i<ship.Mobility.Length;i++){var s=ship.Mobility[i];if(s.Definition is {} d)a.Add(new("mobility",i,s.ModuleId,d.Slot,s.Online,s.Active,s.Burnt));}
  for(int i=0;i<ship.Utilities.Length;i++){var s=ship.Utilities[i];if(s.Definition is {} d)a.Add(new("utility",i,s.ModuleId,d.Slot,s.Online,s.Active,s.Burnt,s.PayloadId,s.Loaded));}
  for(int i=0;i<ship.Special.Length;i++){var s=ship.Special[i];if(s.Definition!=null)a.Add(new("special",i,s.ModuleId,"high",s.Online,s.Active));}
  for(int i=0;i<ship.Rigs.Length;i++)if(RigCatalog.ById.ContainsKey(ship.Rigs[i]))a.Add(new("rig",i,ship.Rigs[i],"rig"));
  return a;
 }
 public static int Capacity(Ship ship,string bank)=>(int)Math.Max(0,bank=="rig"?ship.Attributes.GetValueOrDefault("upgradeSlotsLeft")??0:ship.Stat(bank=="high"?"hiSlots":bank=="medium"?"medSlots":"lowSlots"));
 public static List<FittingSocket> Sockets(Ship ship)
 {
  var installed=Installed(ship);var result=new List<FittingSocket>();
  foreach(var bank in Banks){var a=installed.Where(m=>m.Bank==bank).ToArray();int capacity=Capacity(ship,bank);for(int i=0;i<Math.Max(capacity,a.Length);i++)result.Add(new(bank,i,i<a.Length?a[i]:null));}
  return result;
 }
 public static int Destination(Ship ship,FittingSpec spec,FittingModule? selected=null)
 {
  if(selected!=null)return selected.Kind==spec.Kind&&selected.Bank==spec.Bank?selected.Index:-1;
  IEnumerable<int> slots=spec.Kind switch{
   "legacy"=>ship.Weapons.Length>1&&ship.WeaponAt(1)==null&&!ship.LegacyUtilityOccupied?new[]{1}:[],
   "weapon"=>Enumerable.Range(0,ship.Weapons.Length).Where(i=>ship.WeaponAt(i)==null&&!(i==1&&ship.LegacyUtilityOccupied)),
   "ewar"=>Enumerable.Range(0,ship.Ewar.Length).Where(i=>(i==0?"high":"medium")==spec.Bank&&ship.Ewar[i].Definition==null),
   "defense"=>Enumerable.Range(0,ship.Defense.Length).Where(i=>Ship.DefenseSlot(i)==spec.Bank&&ship.Defense[i].Definition==null),
   "support"=>Enumerable.Range(0,ship.Support.Length).Where(i=>DemoWorld.SupportSlot(i)==spec.Bank&&ship.Support[i].Definition==null),
   "enhancement"=>Enumerable.Range(0,ship.Enhancements.Length).Where(i=>Ship.EnhancementSlot(i)==spec.Bank&&ship.Enhancements[i].Definition==null),
   "mobility"=>Enumerable.Range(0,ship.Mobility.Length).Where(i=>Ship.MobilitySlot(i)==spec.Bank&&ship.Mobility[i].Definition==null),
   "utility"=>Enumerable.Range(0,ship.Utilities.Length).Where(i=>Ship.UtilitySlot(i)==spec.Bank&&ship.Utilities[i].Definition==null),
   "special"=>Enumerable.Range(0,ship.Special.Length).Where(i=>ship.Special[i].Definition==null),
   "rig"=>Enumerable.Range(0,ship.Rigs.Length).Where(i=>i<Capacity(ship,"rig")&&ship.Rigs[i].Length==0),_=>[]};
  return slots.DefaultIfEmpty(-1).First();
 }
 public static string Error(DemoWorld world,FittingSpec spec,int index)
 {
  if(!world.Docked)return "Dock before fitting modules.";
  if(index<0)return "Unfit the selected module or choose an empty compatible socket.";
  return spec.Kind switch{
   "legacy"=>world.Player.WeaponSocketCount+world.SpecialCount("high")+world.UtilityCount("high")+world.EnhancementCount("high")+world.SupportHighCount+world.DefenseHighCount+(world.Player.Ewar[0].Definition!=null?1:0)-(world.Player.LegacyUtilityOccupied?1:0)+1>world.Player.Stat("hiSlots")?"No free shared high socket.":"",
   "weapon"=>world.FitError(WeaponCatalog.Get(spec.Id),index),"ewar"=>world.EwarFitError(EwarCatalog.ById[spec.Id],index),
   "defense"=>world.DefenseFitError(DefenseCatalog.ById[spec.Id],index),"support"=>world.SupportFitError(SupportCatalog.ById[spec.Id],index),
   "enhancement"=>world.EnhancementFitError(EnhancementCatalog.ById[spec.Id],index),"mobility"=>world.MobilityFitError(MobilityCatalog.ById[spec.Id],index),
   "utility"=>world.UtilityFitError(UtilityCatalog.ById[spec.Id],index),"special"=>world.SpecialFitError(SpecialCatalog.ById[spec.Id],index),"rig"=>world.RigFitError(RigCatalog.ById[spec.Id],index),_=>"Unsupported module."};
 }
 public static bool Fit(DemoWorld world,FittingSpec spec,bool hangar,int index)=>spec.Kind switch{
  "legacy"=>world.FitItem(spec.Id,hangar),"weapon"=>world.FitWeapon(spec.Id,hangar,index),"ewar"=>world.FitEwar(spec.Id,hangar,index),"defense"=>world.FitDefense(spec.Id,hangar,index),
  "support"=>world.FitSupport(spec.Id,hangar,index),"enhancement"=>world.FitEnhancement(spec.Id,hangar,index),"mobility"=>world.FitMobility(spec.Id,hangar,index),
  "utility"=>world.FitUtility(spec.Id,hangar,index),"special"=>world.FitSpecial(spec.Id,hangar,index),"rig"=>world.FitRig(spec.Id,hangar,index),_=>false};
}
public sealed partial class DemoWorld
{
 // Same accounting as fleet unfit, also available for the legacy weapon workshop hulls.
 public bool UnfitLayoutWeapon(int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||slot<0||slot>=Player.Weapons.Length)return false;
  string id=slot==1&&Player.LegacyUtilityOccupied?Player.UtilityFit:Player.WeaponAt(slot)?.ModuleId??"";
  if(id.Length==0)return false;ReturnMagazine(Player.Weapons[slot],true);StoreWeapon(Player.Weapons[slot]);ChangeStack(id,true,(decimal)(1));Player.Weapons[slot]=new(){ModuleId=""};
  if(slot==0)Player.TurretOn=false;
  if(slot==1){Player.UtilityFit="";Player.MinerOn=Player.MissileOn=false;}RecomputeWeaponFit();return true;
 }
}
