using Newtonsoft.Json;

namespace CobbleBuild.CobblemonClasses {
   public class CobblemonSpawnPresetJson {
      public string? context;
      public CobblemonSpawnCondition? condition;
      public CobblemonSpawnCondition? antiCondition;
      /// <summary>
      /// THIS CONSTRUCTOR IS FOR JSON PARSING ONLY
      /// </summary>
      [JsonConstructor]
      public CobblemonSpawnPresetJson() { }
      public CobblemonSpawnPresetJson(CobblemonSpawnCondition? condition, CobblemonSpawnCondition? antiCondition = null) {
         this.condition = condition;
         this.antiCondition = antiCondition;
      }
   }
}
