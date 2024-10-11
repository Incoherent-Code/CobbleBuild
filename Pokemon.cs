using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using static CobbleBuild.Misc;

namespace CobbleBuild {
   /// <summary>
   /// Overarching class to pass data around with.
   /// Uses the extracted species data to initalize
   /// </summary>
   public class Pokemon(SpeciesData data) {
      public string Name = data.name;
      public string shortName = toID(data.name);
      public string identifier = toIdentifier(data.name);
      public string folder_name = data.nationalPokedexNumber.ToString("0000") + "_" + toID(data.name);
      public int id = data.nationalPokedexNumber;
      public SpeciesData data = data;
      public Dictionary<string, Animation> animationData { get; set; } = [];
      public CobblemonSpawnJson? spawnData { get; set; }
      public bool modelIsGendered = false;
      public bool hasLootTable { get; set; } = false;
      public bool passedWithoutErrors { get; set; } = false;
      public List<Variation> Variations { get; set; } = [];
   }
}
