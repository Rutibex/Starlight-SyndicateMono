using System.Diagnostics;
using System.Text.Json;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld {internal SpaceTarget BenchmarkTargetI(int index){var t=MakeRaider(new Vector3(6000+(index%20)*300,0,(index/20)*350));t.Id="benchmark:"+index;t.Name="Benchmark hostile "+index;return t;}}
public static class PerformanceTestsI
{
 public static void Run()
 {
  SkillCatalog.VerificationAllV=false;var results=new List<object>();
  foreach(int count in new[]{8,64,256})
  {
   var w=new DemoWorld();w.NewGame(0);w.Population.Enabled=false;w.Undock();w.EnterGrid("belt",false);w.Targets.Clear();for(int n=0;n<count;n++)w.Targets.Add(w.BenchmarkTargetI(n));
   var ids=w.Targets.Select(t=>t.Id).ToArray();var clock=w.Now;var times=new List<double>();
   for(int n=0;n<40;n++){w.Player.Shield=w.Player.MaxShield;w.Player.Armor=w.Player.MaxArmor;w.Player.Hull=w.Player.MaxHull;var watch=Stopwatch.StartNew();w.Update(.05f);times.Add(watch.Elapsed.TotalMilliseconds);}
   if(!w.Targets.Select(t=>t.Id).SequenceEqual(ids)||w.Now-clock<TimeSpan.FromSeconds(1.99))throw new Exception("Benchmark state or clock loss.");times.Sort();results.Add(new{Targets=count,Steps=40,MeanMs=times.Average(),P95Ms=times[(int)(times.Count*.95)-1],MaximumMs=times.Max(),ManagedBytes=GC.GetTotalMemory(false),StatePreserved=true});
  }
  Console.WriteLine(JsonSerializer.Serialize(new{Runtime=Environment.Version.ToString(),OS=Environment.OSVersion.ToString(),LogicalCores=Environment.ProcessorCount,Fixture="Active-grid raiders with existing finite tactical provisioning; player health restored before each sample to isolate step cost",Results=results},new JsonSerializerOptions{WriteIndented=true}));
 }
}
