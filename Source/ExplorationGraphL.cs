namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public GraphRunL EnsureExplorationGraphL(EncounterF e)
 {
  if(e.GraphL!=null)return e.GraphL;
  var run=GraphCompilerL.Start(GraphCompilerL.Exploration((e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template))),e.Started==default?Now:e.Started,e.Expires,StableC(e.Id));e.GraphL=run;run.Pocket=e.Pocket;run.Entered[e.Pocket]=e.Started==default?Now:e.Started;
  for(int p=0;p<e.Pocket;p++)run.Fired.Add("ready/"+p);if(e.State=="Completed"){run.State="Completed";run.Fired.Add("done");}if(e.State is "Failed" or "Expired")run.State=e.State;
  return run;
 }
 bool SealEncounterKillL(SpaceTarget target)
 {
  var e=ActiveEncounterF;if(e==null||e.State!="Active"||!target.Id.StartsWith("f:"+e.Id+":")||target.Friendly)return false;
  var run=EnsureExplorationGraphL(e);if(!run.Drops.TryGetValue(target.Id,out var drop)){drop=new(){Target=target.Id,Pocket=e.Pocket,Position=target.Position,Salvage=2+(int)(run.Roll()*4),Loot=new(){{e.ProfileO?.Family=="Resident"?"o_blue":"salvage",1+(int)(run.Roll()*3)}}};if(e.ProfileN!=null&&target.Id.Contains(":commander:")){drop.Loot["n_gate_key"]=1;if(run.Roll()<.1)drop.Loot["e_mutator"]=1;}run.Drops[target.Id]=drop;}
  if(!Targets.Any(t=>t.Id=="wreck_"+target.Id))Targets.Add(new(){Id="wreck_"+target.Id,Name=target.Name+" wreck",Kind=TargetKind.Wreck,Position=drop.Position,OwnerId=drop.Owner,Contents=new(drop.Loot),Salvage=drop.Salvage});
  SettleBountyL(run,drop,target,e.RunP!=null||e.ProfileO!=null||e.ProfileN?.Family=="Ghost"?0:run.Npcs.GetValueOrDefault(target.Id)?.Bounty??70);return true;
 }
}
