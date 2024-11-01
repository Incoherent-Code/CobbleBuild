using CobbleBuild.CobblemonClasses;

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

               var spawn = pokemon.spawnData.spawns[i];
               var spawnCondition = spawn.condition;
               int[] minMaxArray = spawn.level.Split("-").Select(n => int.Parse(n)).ToArray();
               var newScriptedCondition = new ScriptedConditions(minMaxArray[0], minMaxArray[1], spawnCondition.isRaining, spawnCondition.isThundering, spawnCondition.timeRange);
               spawnConditionsMap[i.ToString()] = newScriptedCondition;
               if (spawn.weightMultiplier != null && spawn.weightMultiplier.multiplier != 0) {
                  var multipliedCondition = spawn.weightMultiplier.condition;
                  //If being applied as an anticondition
                  if (spawn.weightMultiplier.multiplier < 1) {
                     spawnConditionsMap[i.ToString() + 'm'] = newScriptedCondition;
                     spawnConditionsMap[i.ToString()] = new ScriptedConditions(
                        minMaxArray[0],
                        minMaxArray[1],
                        (multipliedCondition.isRaining != null) ? !multipliedCondition.isRaining : spawnCondition.isRaining,
                        (multipliedCondition.isThundering != null) ? !multipliedCondition.isThundering : spawnCondition.isThundering,
                        InvertTimeRange(multipliedCondition.timeRange) ?? spawnCondition.timeRange
                     );
                  }
                  else {
                     spawnConditionsMap[i.ToString() + 'm'] = new ScriptedConditions(
                        minMaxArray[0],
                        minMaxArray[1],
                        multipliedCondition.isRaining ?? spawnCondition.isRaining,
                        multipliedCondition.isThundering ?? spawnCondition.isThundering,
                        multipliedCondition.timeRange ?? spawnCondition.timeRange
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
         /// <param name="timeRange">The same timerange format that exists in the cobblemon spawn condition</param>
         public ScriptedConditions(int minLevel, int maxLevel, bool? isRaining, bool? isThundering, string? timeRange) {
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
            this.isRaining = isRaining;
            this.isThundering = isThundering;
            this.timeRange = timeRange;
         }
      }
   }
}
