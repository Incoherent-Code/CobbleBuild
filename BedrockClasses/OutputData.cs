using CobbleBuild.CobblemonClasses;
using CobbleBuild.ConversionTechnology;
using CobbleBuild.JavaClasses;

namespace CobbleBuild.BedrockClasses {
   /// <summary>
   /// This Class is an extension of CobblemonData that includes extra data that the bedrock code does not have access to.
   /// </summary>
   public class OutputData : SpeciesData {
      //Only use these as a fallback, scriptedConditions holds these per spawncondition which is better
      public int minLevel;
      public int maxLevel;

      public Dictionary<int, List<string>> variationMap = new Dictionary<int, List<string>>();
      public Dictionary<string, ScriptedConditions> spawnConditionsMap = new Dictionary<string, ScriptedConditions>();

      public OutputData(Pokemon pokemon) {
         foreach (var prop in pokemon.data.GetType().GetFields()) {
            this.GetType().GetField(prop.Name).SetValue(this, prop.GetValue(pokemon.data));
         }
         //Included In case I ever encapsulate anything
         foreach (var prop in pokemon.data.GetType().GetProperties()) {
            this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(pokemon.data, null), null);
         }
         //CobbleBuild will assume that the level range of the first spawn is good enough to use when something spawns in
         if (pokemon.spawnData != null && pokemon.spawnData.spawns.Length > 0) {
            int[] minMaxLevels = pokemon.spawnData.spawns[0].level.Split("-").Select(n => int.Parse(n)).ToArray();
            minLevel = minMaxLevels[0];
            maxLevel = minMaxLevels[1];
            for (int i = 0; i < pokemon.spawnData.spawns.Length; i++) {
               if (pokemon.spawnData.spawns[i].condition == null)
                  continue;
               //Processing Anticonditions needs to happen at some point
               var spawn = pokemon.spawnData.spawns[i];
               var spawnCondition = spawn.condition!;
               int[] minMaxArray = spawn.level.Split("-").Select(n => int.Parse(n)).ToArray();

               //Handling neededNearbyBlocks
               List<string[]>? neededNearbyBlocks = [];
               List<string[]>? preventedNearbyBlocks = [];
               if (spawn.presets != null) {
                  foreach (var presetID in spawn.presets) {
                     if (!SpawnConversion.presetDefinitions.TryGetValue(presetID, out var preset))
                        continue;
                     if (preset.condition?.neededNearbyBlocks != null) {
                        var resolvedBlocks = preset.condition.neededNearbyBlocks.SelectMany(x => JavaData.resolveBlock(x)).ToArray();
                        neededNearbyBlocks.Add(resolvedBlocks);
                     }
                     if (preset.antiCondition?.neededNearbyBlocks != null) {
                        var resolvedBlocks = preset.antiCondition.neededNearbyBlocks.SelectMany(x => JavaData.resolveBlock(x)).ToArray();
                        preventedNearbyBlocks.Add(resolvedBlocks);
                     }
                  }
               }
               var neededNearBlocks = spawn.condition?.neededNearbyBlocks?.SelectMany(x => JavaData.resolveBlock(x)).ToArray();
               if (neededNearBlocks != null && neededNearBlocks.Length > 0)
                  neededNearbyBlocks.Add(neededNearBlocks);
               var preventedNearBlocks = spawn.anticondition?.neededNearbyBlocks?.SelectMany(x => JavaData.resolveBlock(x)).ToArray();
               if (preventedNearBlocks != null && preventedNearBlocks.Length > 0)
                  neededNearbyBlocks.Add(preventedNearBlocks);
               if (neededNearbyBlocks.Count < 1)
                  neededNearbyBlocks = null;
               if (preventedNearbyBlocks.Count < 1)
                  preventedNearbyBlocks = null;

               var newScriptedCondition = new ScriptedConditions(
                  minMaxArray[0],
                  minMaxArray[1],
                  spawnCondition.isRaining,
                  spawnCondition.isThundering,
                  spawnCondition.timeRange,
                  neededNearbyBlocks?.ToArray(),
                  preventedNearbyBlocks?.ToArray()
               );
               spawnConditionsMap[i.ToString()] = newScriptedCondition;
               if (spawn.weightMultiplier != null && spawn.weightMultiplier.multiplier != 0) {
                  var multipliedCondition = spawn.weightMultiplier.condition;
                  var multipliedNeededBlocks = spawn.weightMultiplier.condition.neededNearbyBlocks?.SelectMany(x => JavaData.resolveBlock(x)).ToArray();
                  //If being applied as an anticondition
                  if (spawn.weightMultiplier.multiplier < 1) {
                     spawnConditionsMap[i.ToString() + 'm'] = newScriptedCondition;
                     var preventedBlocks =
                        (multipliedNeededBlocks != null && preventedNearbyBlocks != null)
                        ? [multipliedNeededBlocks, .. preventedNearbyBlocks]
                        : ((multipliedNeededBlocks != null) ? [multipliedNeededBlocks] : preventedNearbyBlocks?.ToArray());
                     spawnConditionsMap[i.ToString()] = new ScriptedConditions(
                        minMaxArray[0],
                        minMaxArray[1],
                        (multipliedCondition.isRaining != null) ? !multipliedCondition.isRaining : spawnCondition.isRaining,
                        (multipliedCondition.isThundering != null) ? !multipliedCondition.isThundering : spawnCondition.isThundering,
                        InvertTimeRange(multipliedCondition.timeRange) ?? spawnCondition.timeRange,
                        neededNearbyBlocks?.ToArray(),
                        preventedBlocks
                     );
                  }
                  else {
                     var nearbyBlocks =
                        (multipliedNeededBlocks != null && neededNearbyBlocks != null)
                        ? [multipliedNeededBlocks, .. neededNearbyBlocks]
                        : ((multipliedNeededBlocks != null) ? [multipliedNeededBlocks] : neededNearbyBlocks?.ToArray());
                     spawnConditionsMap[i.ToString() + 'm'] = new ScriptedConditions(
                        minMaxArray[0],
                        minMaxArray[1],
                        multipliedCondition.isRaining ?? spawnCondition.isRaining,
                        multipliedCondition.isThundering ?? spawnCondition.isThundering,
                        multipliedCondition.timeRange ?? spawnCondition.timeRange,
                        nearbyBlocks,
                        preventedNearbyBlocks?.ToArray()
                     );
                  }
               }
            }
         }
         else {
            //Default
            minLevel = 5;
            maxLevel = 30;
         }
         for (int i = 0; i < pokemon.Variations.Count; i++) {
            variationMap.Add(i, pokemon.Variations[i].aspects);
         }
      }

      private static string? InvertTimeRange(string? timeRange) {
         if (timeRange == null)
            return null;
         switch (timeRange) {
            case "day":
               return "night";
            case "night":
               return "day";
            default:
               Misc.warn($"Could not invert time range '${timeRange}'");
               return null;
         }
      }
      /// <summary>
      /// This class is meant to hold checks that the javascript code will have to check upon spawning because
      /// they cannot be checked using spawn rules
      /// </summary>
      public class ScriptedConditions {
         public bool? isRaining;
         public bool? isThundering;
         public string? timeRange;
         public int minLevel;
         public int maxLevel;
         public string[][]? neededNearbyBlocks;
         public string[][]? preventedNearbyBlocks;
         /// <param name="timeRange">The same timerange format that exists in the cobblemon spawn condition</param>
         public ScriptedConditions(int minLevel, int maxLevel, bool? isRaining, bool? isThundering, string? timeRange, string[][]? neededNearbyBlocks, string[][]? preventedNearbyBlocks) {
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
            this.isRaining = isRaining;
            this.isThundering = isThundering;
            this.timeRange = timeRange;
            this.preventedNearbyBlocks = preventedNearbyBlocks;
            this.neededNearbyBlocks = neededNearbyBlocks;
         }
      }
   }
}
