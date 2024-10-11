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
      public Dictionary<int, ScriptedConditions> spawnConditionsMap = new Dictionary<int, ScriptedConditions>();

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

               var spawnCondition = pokemon.spawnData.spawns[i].condition;
               int[] minMaxArray = pokemon.spawnData.spawns[i].level.Split("-").Select(n => int.Parse(n)).ToArray();
               spawnConditionsMap.Add(i, new ScriptedConditions(minMaxArray[0], minMaxArray[1], spawnCondition.isRaining, spawnCondition.timeRange));
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
      /// <summary>
      /// This class is meant to hold checks that the javascript code will have to check upon spawning because
      /// they cannot be checked using spawn rules
      /// </summary>
      public class ScriptedConditions {
         public bool? isRaining;
         public string? timeRange;
         public int minLevel;
         public int maxLevel;
         public ScriptedConditions(int minLevel, int maxLevel, bool? isRaining, string? timeRange) {
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
            this.isRaining = isRaining;
            this.timeRange = timeRange;
         }
      }
   }
}
