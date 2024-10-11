using CobbleBuild.BedrockClasses;
using Newtonsoft.Json;

namespace CobbleBuild.CobblemonClasses {
   public class Berry {
      public int baseYeild;
      public string[] preferredBiomeTags;
      public MinMiaxValue growthTime;
      public MinMiaxValue refreshRate;
      public string[] favoriteMulches;
      public GrowthFactor[] growthFactors;
      public BerrySpawnCondition[] spawnConditions;
      public GrowthPoint[] growthPoints;
      public bool randomizedGrowthPoints = true;
      public Dictionary<string, string> mutations;
      public string sproutShape;
      public string matureShape;
      public Dictionary<string, int> flavors;
      //Come back to these later, they are unused anyways
      //public Dictionary<int, Color> tintIndexes;
      [JsonProperty("flowerModel")]
      public string flowerModelIdentifier;
      public string flowerTexture;
      [JsonProperty("fruitModel")]
      public string fruitModelIdentifier;
      public string fruitTexture;
      public float weight;
   }
   public class GrowthFactor {
      /// <summary>
      /// Type of growth factor
      /// </summary>
      public string variant;
      //Populated with all
      public MinMiaxValue? bonusYeild;
      //Used with biome_downfall and biome_temperature
      public MinMiaxValue? range;
   }
   public class BerrySpawnCondition {
      /// <summary>
      /// Type of spawn condition
      /// </summary>
      public string variant;
      public int minGroveSize;
      public int maxGroveSize;
   }
   //We cannot use our Vector classes because they serialize to array
   public class GrowthPoint {
      public GrowthPointVector3 position;
      public GrowthPointVector3 rotation;
   }
   public class GrowthPointVector3(float x, float y, float z) {
      public float x = x;
      public float y = y;
      public float z = z;
   }
}
