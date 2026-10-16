using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;

public sealed partial class DemoGame
{
 bool fittingWorkshop,fitOwnedOnly,fitBankFilter=true,fitSearchFocus,fitPreviewDragging;
 int fitBrowseTab=1,fitBrowsePage,fitSystem=0;string fitSearch="",fitSelectedItem="",fitSelectedSocket="",fitNotice="";
 List<FittingSocket> fitSockets=[];readonly HashSet<string> fitCollapsed=[];
 RenderTarget2D? fittingPreview;float fitYaw=.7f,fitPitch=-.25f,fitZoom=.85f;Vector2 fitPreviewLast;
 const string FitPreviewTip="Fitting preview / drag to rotate, wheel to zoom";
 FittingSocket? SelectedFitSocket=>fitSockets.FirstOrDefault(s=>s.Key==fitSelectedSocket);
 FittingModule? SelectedFitModule=>SelectedFitSocket?.Module;
 void PrepareFittingPreview()
 {
  if(!desktop.Any(w=>w.Kind==ViewMode.Fitting&&!w.Minimized)||fleetBrowser||fittingWorkshop)return;
  fittingPreview??=new RenderTarget2D(GraphicsDevice,400,420,false,SurfaceFormat.Color,DepthFormat.Depth24);
  GraphicsDevice.SetRenderTarget(fittingPreview);GraphicsDevice.Clear(Color.Transparent);
  renderer.DrawPreview(FittingWorld.Player.IsFleetHull?FittingWorld.Player.CombatHull:"",fitYaw,fitPitch,fitZoom,FittingWorld.Player);
 }
 void SelectFitSocket(FittingSocket socket)
 {
  fitSelectedSocket=socket.Key;fitBrowsePage=0;fitNotice="";
  if(socket.Module is {} m){SetSpecialistSlot(m);fitSelectedItem=m.Id;}
 }
 void SetSpecialistSlot(FittingModule m)
 {
  switch(m.Kind){case "weapon":case "legacy":FittingWorld.FittingWeaponSlot=m.Index;break;case "ewar":FittingWorld.FittingEwarSlot=m.Index;break;case "defense":FittingWorld.FittingDefenseSlot=m.Index;break;case "support":FittingWorld.FittingSupportSlot=m.Index;break;case "enhancement":FittingWorld.FittingEnhancementSlot=m.Index;break;case "mobility":FittingWorld.FittingMobilitySlot=m.Index;break;case "utility":FittingWorld.FittingUtilitySlot=m.Index;break;case "special":FittingWorld.FittingSpecialSlot=m.Index;break;case "rig":FittingWorld.FittingRigSlot=m.Index;break;}
 }
 void FitText(char c)
 {
  if(!fitSearchFocus)return;
  if(fitSaveDialog){FitDesignText(c);return;}
  if(c=='\b')fitSearch=fitSearch.Length>0?fitSearch[..^1]:"";
  else if(!char.IsControl(c)&&fitSearch.Length<40)fitSearch+=c;
  fitBrowsePage=0;
 }
 bool FitLayoutInput(MouseState m,KeyboardState keys)
 {
  var top=WindowAt(mouse);bool downNow=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released,upNow=m.LeftButton==ButtonState.Released&&oldMouse.LeftButton==ButtonState.Pressed;
  if(fitSearchFocus&&(!desktop.Any(w=>w.Kind==ViewMode.Fitting)||fittingWorkshop||fleetBrowser))fitSearchFocus=false;
  if(fitSearchFocus&&(keys.IsKeyDown(Keys.LeftControl)||keys.IsKeyDown(Keys.RightControl))&&Press(Keys.A,keys))fitSelectText=true;
  if(fitSaveDialog&&Press(Keys.Escape,keys)){fitSaveDialog=fitSearchFocus=false;return true;}
  if(fitSaveDialog&&Press(Keys.Enter,keys)){SaveFitDesign(false);return true;}
  if(fitSearchFocus&&Press(Keys.Enter,keys))fitSearchFocus=false;
  if(downNow&&!fitSaveDialog){var h=ui.Hits.LastOrDefault(h=>h.Owner==top?.Owner&&h.Rect.Contains(mouse.ToPoint()));if(h?.Tip!="Search fitting catalog")fitSearchFocus=false;}
  if(top?.Kind==ViewMode.Fitting&&!fittingWorkshop&&!fleetBrowser&&!context)
  {
   var preview=ui.Hits.LastOrDefault(h=>h.Owner==top.Owner&&h.Tip==FitPreviewTip);
   if(preview?.Rect.Contains(mouse.ToPoint())==true&&!itemDragging&&itemDrag==null){
    if(downNow){fitPreviewDragging=true;fitPreviewLast=mouse;}
    int wheel=m.ScrollWheelValue-oldMouse.ScrollWheelValue;if(wheel!=0){fitZoom=Math.Clamp(fitZoom*MathF.Pow(.84f,wheel/120f),.65f,2);return true;}
   }
   if(m.ScrollWheelValue!=oldMouse.ScrollWheelValue&&ui.Hits.Any(h=>h.Owner==top.Owner&&h.Tip=="Fitting catalog area"&&h.Rect.Contains(mouse.ToPoint()))){fitBrowsePage=Math.Max(0,fitBrowsePage-(m.ScrollWheelValue-oldMouse.ScrollWheelValue)/120);return true;}
  }
  if(fitPreviewDragging){var delta=mouse-fitPreviewLast;fitYaw+=delta.X*.01f;fitPitch=Math.Clamp(fitPitch+delta.Y*.01f,-1.3f,1.3f);fitPreviewLast=mouse;if(upNow||m.LeftButton==ButtonState.Released)fitPreviewDragging=false;return true;}
  return false;
 }
 string[] FitPayloads(FittingModule? m)
 {
  if(m==null)return [];
  return m.Kind switch{
   "weapon"=>ChargeCatalog.For(WeaponCatalog.Get(m.Id)).Select(p=>p.Id).ToArray(),
   "ewar"=>EwarPayloads.For(EwarCatalog.ById[m.Id]).Select(p=>p.Id).ToArray(),
   "defense"=>DefensePayloads.For(DefenseCatalog.ById[m.Id]).Select(p=>p.Id).ToArray(),
   "support"=>SupportPayloads.For(SupportCatalog.ById[m.Id]).Select(p=>p.Id).ToArray(),
   "enhancement"=>EnhancementCatalog.Scripts.Where(p=>EnhancementCatalog.Fits(EnhancementCatalog.ById[m.Id],p)).Select(p=>p.Id).ToArray(),
   "utility"=>UtilityPayloads.All.Where(p=>UtilityPayloads.Fits(UtilityCatalog.ById[m.Id],p)).Select(p=>p.Id).ToArray(),_=>[]};
 }
 bool LoadFitPayload(FittingModule m,string id,bool? hangar=null)
 {
  if(fittingSimulation!=null){fittingSimulation.Stock();hangar=true;}
  if(!FitPayloads(m).Contains(id)){fitNotice="Payload is incompatible with the selected module.";return false;}
  bool loaded=m.Kind switch{"weapon"=>FittingWorld.LoadWeapon(m.Index,id),"ewar"=>FittingWorld.LoadEwarPayload(m.Index,id),"defense"=>FittingWorld.LoadDefensePayload(m.Index,id),"support"=>FittingWorld.LoadSupportPayload(m.Index,id),"enhancement"=>FittingWorld.LoadEnhancementScript(m.Index,id),"utility"=>FittingWorld.LoadUtilityPayload(m.Index,id,hangar),_=>false};
  fitNotice=loaded?"Loaded "+DemoWorld.ItemName(id)+".":"Load refused: check stock, cycle and reload state.";return loaded;
 }
 bool FitFromLayout(string id,bool? explicitHangar=null)
 {
  if(fitBrowseTab==2&&SelectedFitModule is {} payloadTarget)return LoadFitPayload(payloadTarget,id,explicitHangar);
  if(!FittingLayoutModel.ById.TryGetValue(id,out var spec)){fitNotice="Select a module from the catalog.";return false;}
  var selected=SelectedFitSocket;
  if(selected!=null&&selected.Bank!=spec.Bank){fitNotice="Choose a "+spec.Bank+" socket for this module.";return false;}
  int slot=FittingLayoutModel.Destination(FittingWorld.Player,spec,selected?.Module);
  string error=fittingSimulation!=null?fittingSimulation.Error(spec,slot):FittingLayoutModel.Error(FittingWorld,spec,slot);
  if(error.Length>0){fitNotice=error;return false;}
  if(fittingSimulation!=null){fittingSimulation.Stock();explicitHangar=true;}
  bool hangar=explicitHangar??(FittingWorld.Docked&&FittingWorld.ItemCount(id,true)>=1);
  if(FittingWorld.ItemCount(id,hangar)<1){fitNotice="No owned module in "+(hangar?"station hangar":"ship cargo")+". View market to buy it.";return false;}
  bool fitted=fittingSimulation!=null?fittingSimulation.Fit(spec,slot):FittingLayoutModel.Fit(FittingWorld,spec,hangar,slot);
  if(fitted){fitSelectedSocket=spec.Kind+":"+slot;fitSelectedItem=id;fitSockets=FittingLayoutModel.Sockets(FittingWorld.Player);fitNotice="Fitted "+DemoWorld.ItemName(id)+".";}
  else fitNotice="Fitting refused by the existing module rules.";
  return fitted;
 }
 void UnfitLayoutSelection()
 {
  var m=SelectedFitModule;if(m==null)return;
  if(!FittingWorld.Docked){fitNotice="Dock before removing modules.";return;}
  if(m.Kind=="rig"&&fittingSimulation!=null){fitNotice=FittingWorld.RemoveRig(m.Index,true)?"Simulated rig removed.":"Removal would invalidate this fit.";return;}
  if(m.Kind=="rig"){FittingWorld.RemoveRig(m.Index);FittingWorld.FittingRigSlot=m.Index;EnsureWindow(ViewMode.Rigs);return;}
  bool result=m.Kind switch{"weapon" or "legacy"=>FittingWorld.UnfitLayoutWeapon(m.Index),"defense"=>FittingWorld.UnfitDefense(m.Index),"support"=>FittingWorld.UnfitSupport(m.Index),"enhancement"=>FittingWorld.UnfitEnhancement(m.Index),"mobility"=>FittingWorld.UnfitMobility(m.Index),"utility"=>FittingWorld.UnfitUtility(m.Index),"special"=>FittingWorld.UnfitSpecial(m.Index),"ewar"=>UnfitLayoutEwar(m.Index),_=>false};
  fitNotice=result?"Module returned to station hangar.":"Removal refused: stop active cycles or resolve dependent fitting/cargo limits.";
  if(result){fitSelectedSocket="";fitSockets=FittingLayoutModel.Sockets(FittingWorld.Player);}
 }
 bool UnfitLayoutEwar(int index){FittingWorld.UnfitEwar(index);return FittingWorld.Player.Ewar[index].Definition==null;}
 void SetFitOnline(FittingModule m)
 {
  if(m.Online==null)return;bool online=m.Online!=true;
  bool result=m.Kind switch{"defense" or "support" or "ewar"=>FittingWorld.SetBasicModuleOnlineB(m,online),"weapon"=>FittingWorld.SetWeaponOnline(m.Index,online),"enhancement"=>FittingWorld.SetEnhancementOnline(m.Index,online),"mobility"=>FittingWorld.SetMobilityOnline(m.Index,online),"utility"=>FittingWorld.SetUtilityOnline(m.Index,online),"special"=>FittingWorld.SetSpecialOnline(m.Index,online),_=>false};
  fitNotice=result?(online?"Module online.":"Module offline."):"Online change refused by capacitor, cycle or fitting limits.";
 }
 void OpenFitSpecialist(FittingModule m)
 {
  if(fittingSimulation!=null){fitNotice=fittingSimulation.SetActive(m,!m.Active)?"Simulated cycling assumption changed; no combat or ammunition consumption.":"This module is passive, offline, burnt, or has no cycling model.";return;}
  SetSpecialistSlot(m);
  if(m.Kind is "weapon" or "legacy" or "ewar"){fittingWorkshop=true;fittingEwar=m.Kind=="ewar";return;}
  var view=m.Kind switch{"defense"=>ViewMode.Defense,"support"=>ViewMode.Support,"enhancement"=>ViewMode.Enhancements,"mobility"=>ViewMode.Mobility,"utility"=>ViewMode.Utilities,"special"=>ViewMode.Special,_=>ViewMode.Rigs};EnsureWindow(view);
 }
 void FittingLayoutMenu()
 {
  var hit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Tip.StartsWith("Slot / "));
  if(hit!=null){var s=fitSockets.FirstOrDefault(s=>FitSocketTip(s)==hit.Tip);if(s!=null)SelectFitSocket(s);}
  var catalogHit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Key.StartsWith("fit-item:"));
  if(catalogHit!=null){string id=catalogHit.Key[9..];fitSelectedItem=id;ShowPopup(DemoWorld.ItemName(id),[("Fit / simulate module",()=>FitPlannerItem(id)),("Show Info",()=>ExecuteCommandJ(new(id,true),"info")),("View market details",()=>ExecuteCommandJ(new(id,true),"market"))]);return;}
  var actions=new List<(string,Action)>{("Ship information",ShowFittingShipInfo),("Open inventory",()=>EnsureWindow(ViewMode.Inventory)),("Fleet registry",()=>{if(fittingSimulation!=null){fitBrowseTab=0;fitBrowsePage=0;}else fleetBrowser=true;}),("Reset fitting window",()=>{var w=desktop.Single(w=>w.Kind==ViewMode.Fitting);var r=LogicalWindow(ViewMode.Fitting);w.X=r.X;w.Y=r.Y;w.Width=r.Width;w.Height=r.Height;SaveDesktop();})};
  FitPlannerMenuExtras(actions);
  if(SelectedFitModule is {} m){actions.Insert(0,("Show Info",()=>ShowItemInfo(m.Id)));actions.Insert(0,("Module controls",()=>OpenFitSpecialist(m)));actions.Insert(1,(m.Kind=="rig"?"Request rig destruction":"Unfit to hangar",UnfitLayoutSelection));if(m.Online!=null)actions.Insert(2,(m.Online==true?"Offline module":"Online module",()=>SetFitOnline(m)));if(m.Payload.Length>0)actions.Add(("Unload charges / script",()=>fitNotice=FittingWorld.UnloadFittingPayload(m)?"Charges returned to station hangar.":"Stop cycles and dock before unloading."));if(FitPayloads(m).Length>0)actions.Add(("Compatible charges / scripts",()=>{fitBrowseTab=2;fitBrowsePage=0;}));}
  ShowPopup("Ship fitting",actions);
 }
 void DropIntoFittingLayout(InventoryHit source)
 {
  var hit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Tip.StartsWith("Slot / "));
  if(hit!=null){var s=fitSockets.FirstOrDefault(s=>FitSocketTip(s)==hit.Tip);if(s!=null)SelectFitSocket(s);}
  if(!FittingLayoutModel.ById.ContainsKey(source.Id)&&SelectedFitModule is {} m){LoadFitPayload(m,source.Id,source.Hangar);return;}
  if(hit==null)fitSelectedSocket="";
  if(DroneCatalog.ById.ContainsKey(source.Id)){FitPlannerItem(source.Id);return;}
  int oldTab=fitBrowseTab;fitBrowseTab=1;FitFromLayout(source.Id,source.Hangar);fitBrowseTab=oldTab;
 }
 static float FitChargeFraction(FittingModule m){if(m.Payload.Length==0)return 0;int loaded=m.Kind=="enhancement"?1:m.Loaded;double maximum=m.Kind switch{"weapon"=>ChargeCatalog.ById.GetValueOrDefault(m.Payload)?.Crystal==true?1:WeaponCatalog.Get(m.Id).Magazine,"ewar"=>EwarPayloads.ById.GetValueOrDefault(m.Payload)?.Consumable==true?EwarCatalog.ById[m.Id].Magazine:1,"defense"=>DefenseCatalog.ById[m.Id].MagazineVolume/Math.Max(.000001,DemoWorld.ItemVolume(m.Payload)),"support"=>SupportCatalog.ById[m.Id].MagazineVolume/Math.Max(.000001,DemoWorld.ItemVolume(m.Payload)),"utility"=>UtilityPayloads.Get(m.Payload)?.Kind is "Core" or "Combat"?8:UtilityPayloads.Get(m.Payload)?.Kind is "Crystal" or "Survey"?1:3,_=>1};return Math.Clamp(loaded/(float)Math.Max(1,Math.Floor(maximum)),0,1);}
 static string FitSocketTip(FittingSocket s)=>ShortInfo("Slot / "+s.Bank.ToUpper()+" "+(s.Position+1)+" / "+(s.Module?.Name??"empty"),65);
 static Color FitBankColor(string bank)=>bank switch{"high"=>Cyan,"medium"=>new Color(126,166,215),"low"=>Gold,_=>new Color(171,145,201)};
 static string FitAbbreviation(string kind)=>kind switch{"weapon"=>"W","defense"=>"D","ewar"=>"E","support"=>"S","enhancement"=>"+","mobility"=>">>","utility"=>"U","special"=>"C","rig"=>"R",_=>"L"};
 static Vector2 FitSocketPoint(string bank,int position,int count)
 {
  (float a,float b)=bank switch{"high"=>(218,312),"medium"=>(-34,36),"low"=>(56,134),_=>(153,195)};
  float degrees=count<=1?(a+b)/2:a+(b-a)*position/(count-1);float angle=MathHelper.ToRadians(degrees);
  return new Vector2(765,414)+new Vector2(MathF.Cos(angle),MathF.Sin(angle))*246;
 }
 void FitGlyph(Vector2 center,string kind,Color color)
 {
  int x=(int)center.X,y=(int)center.Y;
  if(kind=="weapon"){ui.Line(new(x-9,y+9),new(x+10,y-10),color,3);ui.Line(new(x-10,y+1),new(x-1,y+10),color,2);ui.Line(new(x+4,y-10),new(x+10,y-4),color,2);}
  else if(kind=="defense"){ui.Line(new(x-10,y-9),new(x+10,y-9),color,2);ui.Line(new(x-10,y-9),new(x-7,y+5),color,2);ui.Line(new(x+10,y-9),new(x+7,y+5),color,2);ui.Line(new(x-7,y+5),new(x,y+11),color,2);ui.Line(new(x+7,y+5),new(x,y+11),color,2);}
  else if(kind=="mobility"){for(int i=-1;i<=1;i++){ui.Line(new(x+i*6-3,y-9),new(x+i*6+3,y),color,2);ui.Line(new(x+i*6+3,y),new(x+i*6-3,y+9),color,2);}}
  else if(kind=="ewar"){ui.Circle(center,7,color,2);ui.Arc(center,12,-1.1f,2.2f,color,2);ui.Arc(center,12,2,2.2f,color,2);}
  else if(kind=="support"){ui.Line(new(x-10,y),new(x+10,y),color,3);ui.Line(new(x,y-10),new(x,y+10),color,3);}
  else if(kind=="utility"){ui.Hex(center,11,color,2);ui.Line(new(x-6,y+6),new(x+6,y-6),color,2);}
  else if(kind=="rig"){ui.Hex(center,10,color,2);ui.Circle(center,4,color,2);}
  else if(kind=="special"){ui.Circle(center,11,color,2);ui.Line(new(x-7,y+7),new(x+7,y-7),color,2);}
  else {ui.Line(new(x-9,y),new(x+9,y),color,2);ui.Line(new(x,y-9),new(x,y+9),color,2);}
 }
 void FittingLayout()
 {
  var ship=FittingWorld.Player;fitSockets=FittingLayoutModel.Sockets(ship);
  if(!fitSockets.Any(s=>s.Key==fitSelectedSocket))fitSelectedSocket=fitSockets.FirstOrDefault()?.Key??"";
  ui.Box(new(91,94,1398,740),new Color(9,16,22,250));
  ui.Box(new(103,141,324,596),new Color(17,26,33));ui.Box(new(440,141,655,596),new Color(7,14,21));ui.Box(new(1108,141,368,660),new Color(15,24,30));
  ui.Text(ShortInfo(ship.HullName,26),446,108,White,19);
  ui.Button(new(1119,103,345,28),fittingSimulation!=null?"SIMULATION / RETURN TO LIVE":"ENTER FITTING SIMULATION",ToggleFittingSimulation,"Toggle fitting simulation",active:fittingSimulation!=null);
  ui.Button(new(800,103,132,28),fittingSimulation!=null?"HULL INFO":"SHIP INFO",ShowFittingShipInfo);ui.Button(new(942,103,149,28),"FLEET REGISTRY",()=>{if(fittingSimulation!=null){fitBrowseTab=0;fitBrowsePage=0;}else fleetBrowser=true;});
  string[] tabs=["Hulls & Fits","Modules","Charges","Inventory"];
  for(int i=0;i<4;i++){int tab=i;ui.Button(new(104+i*81,148,79,28),tabs[i],()=>{fitBrowseTab=tab;fitShowDrones=false;fitBrowsePage=0;fitSearchFocus=false;},active:fitBrowseTab==i);}
  if(fitBrowserVisible)DrawFitBrowser();
  // Circular drafting grid beneath the separately rendered ship preview.
  var center=new Vector2(765,414);Color line=new(34,66,78);
  for(int dy=-212;dy<=212;dy+=2){int half=(int)Math.Sqrt(212*212-dy*dy);ui.Box(new(765-half,414+dy,half*2,2),new Color(14,30,40));}
  for(int grid=-180;grid<=180;grid+=30){float span=MathF.Sqrt(212*212-grid*grid);ui.Line(new(765-span,414+grid),new(765+span,414+grid),line);ui.Line(new(765+grid,414-span),new(765+grid,414+span),line);}
  ui.Circle(center,215,new Color(49,95,110),2);ui.Circle(center,273,new Color(38,59,69),2);
  for(int i=0;i<80;i++){float a=i*MathF.Tau/80;ui.Line(center+new Vector2(MathF.Cos(a),MathF.Sin(a))*266,center+new Vector2(MathF.Cos(a),MathF.Sin(a))*(i%5==0?272:269),line);}
  if(fittingPreview!=null)ui.Image(fittingPreview,new(567,206,396,416),fittingSimulation!=null?new Color(125,205,235,190):Color.White);
  if(fittingSimulation!=null){ui.Text("SIMULATION",694,219,Cyan,12);for(int scan=250;scan<615;scan+=12)ui.Line(new(635,scan),new(900,scan),new Color(75,150,180,28));}
  ui.Hits.Add(new(new(596,266,338,296),()=>{},FitPreviewTip));
  foreach(var bank in FittingLayoutModel.Banks){var sockets=fitSockets.Where(s=>s.Bank==bank).ToArray();var tint=FitBankColor(bank);for(int i=0;i<sockets.Length;i++){
   var socket=sockets[i];var point=FitSocketPoint(bank,i,sockets.Length);var r=new Rectangle((int)point.X-21,(int)point.Y-21,42,42);var module=socket.Module;
   ui.Box(r,fitSelectedSocket==socket.Key?new Color(51,72,80):new Color(20,34,44));ui.Border(r,socket.Position>=FittingLayoutModel.Capacity(ship,bank)||module?.Burnt==true?Red:fitSelectedSocket==socket.Key?White:module!=null?tint:new Color(62,82,92));
   if(module!=null){FitGlyph(point,module.Kind,module.Online==false?Muted:module.Burnt?Red:tint);ui.Box(new(r.X+4,r.Bottom-5,r.Width-8,2),module.Active?Green:module.Online==false?Muted:tint);}
   else{ui.Text("+",r.X+17,r.Y+10,Muted,18);}
   if(module?.Payload.Length>0){ui.Box(new(r.X+2,r.Y-5,38,3),Muted);ui.Bar(new(r.X+2,r.Y-5,38,3),FitChargeFraction(module),Cyan);}
   ui.Text((i+1).ToString(),r.X+2,r.Y+2,Muted,8);ui.Hits.Add(new(r,()=>SelectFitSocket(socket),FitSocketTip(socket)));
  }}
  int Used(string bank)=>fitSockets.Count(s=>s.Bank==bank&&s.Module!=null);
  ui.Text($"HIGH {Used("high")}/{FittingLayoutModel.Capacity(ship,"high")}",449,177,Cyan,11);ui.Text($"MID {Used("medium")}/{FittingLayoutModel.Capacity(ship,"medium")}",1042,410,FitBankColor("medium"),10);ui.Text($"LOW {Used("low")}/{FittingLayoutModel.Capacity(ship,"low")}",450,158,Gold,10);ui.Text($"RIG {Used("rig")}/{FittingLayoutModel.Capacity(ship,"rig")}",455,453,FitBankColor("rig"),10);
  int turrets=FittingLayoutModel.Installed(ship).Count(m=>m.Kind=="weapon"&&WeaponCatalog.Get(m.Id).UsesTurret),launchers=FittingLayoutModel.Installed(ship).Count(m=>m.Kind=="weapon"&&WeaponCatalog.Get(m.Id).UsesLauncher);
  ui.Text($"Turrets {turrets}/{ship.Stat("turretSlotsLeft"):0}",450,143,Muted,10);ui.Text($"Launchers {launchers}/{ship.Stat("launcherSlotsLeft"):0}",958,154,Muted,10);
  ui.Button(new(453,609,73,25),"Reset 3D",()=>{fitYaw=.7f;fitPitch=-.25f;fitZoom=.85f;},"Reset fitting preview");
  ui.Button(new(453,579,104,25),renderer.PreviewWireframe?"SOLID":"WIREFRAME",()=>{renderer.PreviewWireframe=!renderer.PreviewWireframe;fitNotice=renderer.PreviewSummary(FittingWorld.Player.CombatHull);},"Toggle model wireframe");
  ui.Button(new(453,549,106,25),"MODEL LOD "+renderer.PreviewLod,()=>{renderer.PreviewLod=(renderer.PreviewLod+1)%3;fitNotice=renderer.PreviewSummary(FittingWorld.Player.CombatHull);},"Cycle model preview LOD");
  ui.Text($"Cargo {FittingWorld.CargoUsed:0.#}/{ship.CargoMax:0} m3",451,649,Muted,11);ui.Text($"Drone bay {ship.DroneBayUsed:0}/{ship.Stat("droneCapacity"):0} m3",898,649,Muted,11);
  DrawFitSelection();DrawFitStatistics();DrawTacticalFitControlsB();
  if(fittingSimulation==null&&SelectedFitModule is {} hm){if(world.Player.HeatState(hm.Kind,hm.Index)!=null)ui.Button(new(950,579,135,25),"NANITE REPAIR",()=>world.RepairWithNanites(hm.Kind,hm.Index),"Nanite repair selected module");if(hm.Kind=="ewar")ui.Button(new(950,549,135,25),world.Player.Ewar[hm.Index].Overheat?"EWAR HEAT ON":"EWAR HEAT OFF",()=>world.Player.Ewar[hm.Index].Overheat=!world.Player.Ewar[hm.Index].Overheat,"Toggle selected EWAR overload");if(hm.Kind=="ewar"&&world.Docked)ui.Button(new(950,609,135,25),"REPAIR EWAR",()=>world.RepairEwar(hm.Index),"Station repair selected EWAR");}
  if(fittingSimulation!=null)ui.Button(new(950,519,135,25),fitMaxProficiency?"PROFICIENCY V":"PROF: CURRENT",ToggleFitProficiency,"Toggle simulated hull proficiency");
  if(ship.StrategicCruiser)ui.Button(new(453,519,159,25),"STRATEGIC SUBSYSTEMS",()=>subsystemPanel=!subsystemPanel,"Open strategic subsystems");
  ui.Button(new(104,759,152,29),"FIT / LOAD SELECTED",()=>{if(fitShowDrones)FitPlannerItem(fitSelectedItem);else FitFromLayout(fitSelectedItem);},"Fit / load selected catalog item");
  if(fittingSimulation!=null)ui.Button(new(265,759,160,29),"RESET FROM LIVE",ResetFittingSimulation);
  else ui.Button(new(265,759,160,29),"VIEW MARKET",()=>{int index=Array.FindIndex(ItemCatalog.All,d=>d.Id==fitSelectedItem);if(index>=0)catalogSelection=index;EnsureWindow(ViewMode.Market);});
  ui.Text(ShortInfo(fitNotice.Length>0?fitNotice:fittingSimulation!=null?"SIMULATION / Free stock. Select modules to assume cycling. Save As keeps designs; Save state keeps simulation snapshots. Campaign unchanged.":FittingWorld.Docked?"Owned modules only. Fit changes use shared socket, CPU and power limits.":"Dock to change modules. Compatible payload loading remains available in space.",135),105,809,fitNotice.Length>0?Gold:Muted,11);
  if(subsystemPanel&&ship.StrategicCruiser)DrawSubsystemPanel();
  DrawFitPlannerFooter();
 }
 void DrawFitBrowser()
 {
  if(fitBrowseTab==0){DrawFitHullBrowser();return;}
  if(fitBrowseTab==3){DrawFitInventoryBrowser();return;}
  DrawFitModuleBrowser();
 }
 float FitStock(string id)=>fittingSimulation!=null?100000:FittingWorld.ItemCount(id,false)+(FittingWorld.Docked?FittingWorld.ItemCount(id,true):0);
 void DrawFitBrowserPaging(int pages,int count){ui.Button(new(113,676,70,25),"Previous",()=>fitBrowsePage=Math.Max(0,fitBrowsePage-1),"Previous fitting catalog page");ui.Center($"{fitBrowsePage+1}/{pages} / {count}",268,684,Muted,10);ui.Button(new(347,676,70,25),"Next",()=>fitBrowsePage=Math.Min(pages-1,fitBrowsePage+1),"Next fitting catalog page");}
 void DrawFitSelection()
 {
  var selected=SelectedFitSocket;var m=selected?.Module;
  ui.Text(m==null?$"{selected?.Bank.ToUpper()??"SELECT"} / EMPTY SOCKET":ShortInfo(m.Name,63),449,710,m==null?Muted:Gold,15);
  ui.Text(m==null?"Choose a compatible module on the left, then Fit selected.":$"{m.Bank.ToUpper()} / {m.Kind} / "+(m.Burnt?"BURNT":m.Online==false?"OFFLINE":m.Active?"ACTIVE":"FITTED")+(m.Payload.Length>0?$" / {ShortInfo(DemoWorld.ItemName(m.Payload),25)} x{m.Loaded}":""),450,733,Muted,11);
  if(m!=null){ui.Button(new(1008,710,83,25),"SHOW INFO",()=>ShowItemInfo(m.Id),"Installed module Show Info");ui.Button(new(449,763,135,28),m.Kind=="rig"?"REMOVE RIG":"UNFIT",UnfitLayoutSelection,"Unfit selected fitting module");ui.Button(new(594,763,143,28),fittingSimulation!=null?(m.Active?"STOP ASSUMED CYCLE":"ASSUME CYCLING"):"MODULE CONTROLS",()=>OpenFitSpecialist(m),"Open selected module controls");if(m.Online!=null)ui.Button(new(747,763,120,28),m.Online==true?"OFFLINE":"ONLINE",()=>SetFitOnline(m),"Toggle selected fitting module online");if(FitPayloads(m).Length>0)ui.Button(new(877,763,214,28),"COMPATIBLE CHARGES",()=>{fitBrowseTab=2;fitBrowsePage=0;fitSearch="";},"Browse compatible fitting charges");}
 }
 void DrawFitStatistics()
 {
  var s=FittingWorld.Player;int y=149;
  bool Group(string label,string summary){bool open=!fitCollapsed.Contains(label);ui.Button(new(1119,y,345,25),(open?"- ":"+ ")+label.ToUpper()+"  "+summary,()=>{if(!fitCollapsed.Add(label))fitCollapsed.Remove(label);},"Toggle fitting stats / "+label);y+=29;return open;}
  void Row(string label,string value){ui.Text(label,1127,y+4,Muted,11);ui.Text(value,1305,y+4,White,11);y+=22;}
  if(Group("Capacitor",FitCapStatus(s))){Row("Capacity / recharge",$"{s.MaxCap:0} GJ / {s.Stat("rechargeRate")/1000:0}s");Row("Use / peak recharge",$"{s.CapacitorUse:0.##} / {s.PeakCapRecharge:0.##} GJ/s");}
  if(Group("Offense",$"{s.TotalDps:0.#} potential DPS")){Row("Weapons / drones",$"{s.WeaponDps:0.#} / {s.BayDroneDps:0.#}");Row("Volley",$"{s.Stat("volleyDamage"):0.#}");}
  if(Group("Defense",$"{s.EffectiveHitpoints:0} minimum-resist EHP")){
   ui.Text("HP                 EM       TH       KI       EX",1127,y+4,Muted,9);y+=21;
   foreach(string layer in new[]{"shield","armor","hull"}){float hp=layer=="shield"?s.MaxShield:layer=="armor"?s.MaxArmor:s.MaxHull;ui.Text(layer.ToUpper(),1127,y+3,Muted,10);ui.Text(hp.ToString("0"),1200,y+3,White,11);int x=1265;foreach(var type in Enum.GetValues<DamageType>()){float resist=s.Resistance(layer,type);Color tint=type==DamageType.EM?Cyan:type==DamageType.Thermal?Red:type==DamageType.Kinetic?new Color(170,180,190):Gold;ui.Box(new(x,y,43,18),new Color(27,36,44));ui.Bar(new(x,y+16,43,2),resist,tint);ui.Text($"{resist*100:0}%",x+5,y+3,tint,10);x+=48;}y+=23;}
   Row("Local shield / armor",$"{s.DefenseRepairRate("shield"):0.#} / {s.DefenseRepairRate("armor"):0.#} HP/s");
  }
  if(Group("Targeting",$"{s.Stat("maxTargetRange")/1000:0.#} km")){Row("Resolution / targets",$"{s.Stat("scanResolution"):0} mm / {s.Stat("maxLockedTargets"):0}");Row("Sensor / signature",$"{s.Stat("scan"+s.NativeSensor+"Strength"):0.#} pts / {s.Stat("signatureRadius"):0}m");}
  if(Group("Navigation",$"{s.Stat("maxVelocity"):0.#} m/s")){Row("Mass / inertia",$"{s.Stat("mass")/1000:0}t / {s.Stat("agility"):0.###}");Row("Warp / align",$"{s.Stat("baseWarpSpeed")*s.Stat("warpSpeedMultiplier"):0.#} AU/s / {s.AlignTime:0.##}s");}
  if(Group("Drones",$"{s.BayDroneDps:0.#} bay DPS")){Row("Bandwidth / control",$"{s.Stat("droneBandwidth"):0} / {s.Stat("droneControlDistance")/1000:0.#} km");ui.Button(new(1127,y,328,25),"MANAGE DRONES / FIGHTERS",()=>{if(fittingSimulation!=null){fitBrowseTab=1;fitShowDrones=true;fitSearch="";fitBrowsePage=0;}else EnsureWindow(ViewMode.Drones);});y+=29;}
  double cpu=FittingWorld.TotalFitCpu,power=FittingWorld.TotalFitPower,maxCpu=s.Stat("cpuOutput"),maxPower=s.Stat("powerOutput");
  ui.Text($"CPU {cpu:0.#} / {maxCpu:0.#} tf",1120,690,cpu>maxCpu+.001?Red:Cyan,12);ui.Bar(new(1120,712,343,6),(float)(cpu/Math.Max(1,maxCpu)),cpu>maxCpu+.001?Red:Cyan);
  ui.Text($"POWER GRID {power:0.#} / {maxPower:0.#} MW",1120,728,power>maxPower+.001?Red:Gold,12);ui.Bar(new(1120,750,343,6),(float)(power/Math.Max(1,maxPower)),power>maxPower+.001?Red:Gold);
  ui.Button(new(1120,807,343,23),$"Value {FittingDesignStore.Equipment(s).Sum(p=>ItemCatalog.Get(p.Key).Price*p.Value)+(decimal)Math.Max(0,s.Stat("estimatedHullValue")):0} CR / Buy missing",OpenFitPurchase,"Buy missing fitting equipment");
  ui.Text($"CALIBRATION {s.RigCalibration} / {s.Stat("upgradeCapacity"):0}",1120,769,FitBankColor("rig"),12);ui.Bar(new(1120,791,343,6),(float)(s.RigCalibration/Math.Max(1,s.Stat("upgradeCapacity"))),FitBankColor("rig"));
 }
}
