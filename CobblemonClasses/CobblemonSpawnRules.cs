namespace CobbleBuild.CobblemonClasses {
   public class CobblemonSpawnJson {
      public bool enabled;
      public CobblemonSpawn[] spawns;
   }
   public class CobblemonSpawn {
      public string id;
      public string pokemon;
      public string[]? presets;
      public string type;
      public string? context;
      public string bucket;
      public string level;
      public float weight;
      public CobblemonSpawnCondition? condition;
      public CobblemonSpawnCondition? anticondition;
      public SpawnWeightMultiplier? weightMultiplier;
   }
   public class CobblemonSpawnCondition {
      public bool? canSeeSky;
      public string[]? biomes;
      public bool? isRaining;
      public bool? isThundering;
      public string[]? structures;
      public int? minY;
      public string[]? neededBaseBlocks;
      public string[]? neededNearbyBlocks;
      public string? fluid;
      public string? timeRange;
   }

   public class SpawnWeightMultiplier {
      public float multiplier;
      public CobblemonSpawnCondition condition;
   }
}
