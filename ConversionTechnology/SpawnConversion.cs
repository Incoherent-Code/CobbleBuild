using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using static CobbleBuild.Config;

namespace CobbleBuild.ConversionTechnology {
   public class SpawnConversion {

      //Preset Information
      public static Dictionary<string, CobblemonSpawnPresetJson> presetDefinitions = [];
      //Im just guessing with these values
      public static readonly Dictionary<string, float> bucketMultipliers = new Dictionary<string, float>() {
         { "common", 1f },
         { "uncommon", 0.5f },
         { "rare", 0.1f },
         { "ultra-rare", 0.03f }
      };
      /// <summary>
      /// This returns corresponding biome tags for each biome. We use biome tags because the docs suggest
      /// the "is_biome" comparison is not up to date with every current biome.
      /// </summary>
      public static Dictionary<string, string[]> biomeDictionary = new Dictionary<string, string[]>
      {
            {"animal", new string[] {"animal"}},
            {"badlands", new string[] {"mesa"}},//not called mesa in java anymore
            {"bamboo", new string[] {"bamboo"}},
            {"bamboo_jungle", new string[] {"bamboo","jungle"}},
            {"basalt_deltas", new string[] {"basalt_deltas"}},
            {"beach", new string[] {"beach"}},
            {"bee_habitat", new string[] {"bee_habitat"}},
            {"birch", new string[] {"birch"}},
            {"birch_forest", ["birch", "forest"] },
            {"caves", new string[] {"caves"}},
            {"cherry_grove", new string[] {"cherry_grove"}},
            {"cold", new string[] {"cold"}},
            {"cold_ocean", new string[] {"cold","ocean"}},
            {"crimson_forest", new string[] {"crimson_forest"}},
            {"dark_forest", new string[] {"roofed"}}, //bedrock really likes to use outdated biome names
            {"deep", new string[] {"deep"}},
            {"deep_cold_ocean", new string[] {"deep","cold","ocean"}},
            {"deep_dark", new string[] {"deep_dark"}},
            {"deep_frozen_ocean", new string[] {"deep","frozen","ocean"}},
            {"deep_lukewarm_ocean", new string[] {"deep","ocean","lukewarm"}},
            {"deep_ocean", new string[] {"deep","ocean"}},
            {"desert", new string[] {"desert"}},
            {"dripstone_caves", new string[] {"dripstone_caves"}},
            {"edge", new string[] {"edge"}},
            {"extreme_hills", new string[] {"extreme_hills"}},
            {"flower_forest", new string[] {"flower_forest"}},
            {"forest", new string[] {"forest"}},
            {"frozen", new string[] {"frozen"}},
            {"frozen_ocean", new string[] {"frozen","ocean"}},
            {"frozen_peaks", new string[] {"frozen_peaks"}},
            {"frozen_river", new string[] {"frozen","river"}},
            {"grove", new string[] {"grove"}},
            {"hill", new string[] {"hills"}},
            {"hills", new string[] {"hills"}},
            {"ice", new string[] {"ice"}},
            {"ice_plains", new string[] {"ice_plains"}},
            {"ice_spikes", new string[] {"ice_plains"}}, //Cannot Differenciate in Biome Tags
            {"jagged_peaks", new string[] {"jagged_peaks"}},
            {"jungle", new string[] {"jungle"}},
            {"lakes", new string[] {"lakes"}},
            {"lukewarm", new string[] {"lukewarm"}},
            {"lukewarm_ocean", new string[] {"ocean","lukewarm"}},
            {"lush_caves", new string[] {"lush_caves"}},
            {"mangrove_swamp", new string[] {"mangrove_swamp"}},
            {"meadow", new string[] {"meadow"}},
            {"mega", new string[] {"mega"}},
            {"mesa", new string[] {"mesa"}},
            {"monster", new string[] {"monster"}},
            {"mooshroom_island", new string[] {"mushroom_island"}},
            {"mountain", new string[] {"mountain"}},
            {"mushroom_fields", new string[] {"mushroom_island"}},
            {"mutated", new string[] {"mutated"}},
            {"nether", new string[] {"nether"}},
            {"nether_wastes", new string[] {"nether_wastes"}},
            {"netherwart_forest", new string[] {"netherwart_forest"}},
            {"no_legacy_worldgen", new string[] {"no_legacy_worldgen"}},
            {"ocean", new string[] {"ocean"}},
            {"old_growth_birch_forest", ["birch", "forest", "mutated"] },
            {"old_growth_pine_taiga", ["taiga", "mutated", "rare"] },
            {"old_growth_spruce_taiga", ["taiga", "mutated"] }, //Hard to narrow down
            {"overworld", new string[] {"overworld"}},
            {"overworld_generation", new string[] {"overworld_generation"}},
            {"plains", new string[] {"plains"}},
            {"plateau", new string[] {"plateau"}},
            {"rare", new string[] {"rare"}},
            {"river", new string[] {"river"}},
            {"roofed", new string[] {"roofed"}},
            {"savanna", new string[] {"savanna"}},
            {"savanna_plateau", new string[] {"savanna","plateau"}},
            {"shore", new string[] {"shore"}},
            {"snowy_beach", new string[] {"cold","beach"}},
            {"snowy_plains", new string[] {"ice_plains"}},
            {"snowy_slopes", new string[] {"snowy_slopes"}},
            {"snowy_taiga", new string[] {"cold","taiga"}},
            {"soulsand_valley", new string[] {"soulsand_valley"}},
            {"soul_sand_valley", new string[] {"soulsand_valley"}}, //Bruh
            {"sparse_jungle", ["jungle", "edge"] },
            {"spawn_endermen", new string[] {"spawn_endermen"}},
            {"spawn_few_piglins", new string[] {"spawn_few_piglins"}},
            {"spawn_few_zombified_piglins", new string[] {"spawn_few_zombified_piglins"}},
            {"spawn_ghast", new string[] {"spawn_ghast"}},
            {"spawn_magma_cubes", new string[] {"spawn_magma_cubes"}},
            {"spawn_many_magma_cubes", new string[] {"spawn_many_magma_cubes"}},
            {"spawn_piglin", new string[] {"spawn_piglin"}},
            {"spawn_zombified_piglin", new string[] {"spawn_zombified_piglin"}},
            {"stone", new string[] {"stone"}},
            {"stony_peaks", new string[] {"mountains"}}, //Not included in stone tag !?!?!?!?
            {"stony_shore", new string[] {"stone","beach"}},
            {"sunflower_plains", new string[] {"plains", "mutated"}},
            {"swamp", new string[] {"swamp"}},
            {"taiga", new string[] {"taiga"}},
            {"the_end", new string[] {"the_end"}},
            {"warm", new string[] {"warm"}},
            {"warm_ocean", new string[] {"warm","ocean"}},
            {"warped_forest", new string[] {"warped_forest"}},
            {"windswept_hills", ["extreme_hills"] },
            {"windswept_forest", ["extreme_hills", "forest"] },
            {"windswept_gravelly_hills", ["extreme_hills", "mutated"] },
            {"windswept_savanna", ["savanna","mutated"] },
            {"wooded_badlands", ["mesa", "plateau"] },
            {"eroded_badlands", ["mesa", "mutated"] } //Close enough
        };
      public static readonly List<string> netherBiomes = new List<string>()
      {
            "warped_forrest", "nether", "nether_wastes", "soulsand_valley", "basalt_deltas", "crimson_forrest"
        };
      /// <summary>
      /// Time ranges that are known and can be processed by the javascript.
      /// </summary>
      public static readonly List<string> knownTimeRanges = ["day", "night"];
      /// <summary>
      /// Scrapes source files for pertinent biome tag information as well as preset data
      /// </summary>
      public static void initValues() {
         //Loading Spawn Presets
         foreach (string file in Misc.getAllFilesInDirandSubDirs(Path.Combine(config.resourcesPath, @"data\cobblemon\spawn_detail_presets\"))) {
            var presetData = Misc.LoadFromJson<CobblemonSpawnPresetJson>(file);
            presetDefinitions.Add(Path.GetFileNameWithoutExtension(file), presetData);
         }
      }
      public static SpawnRulesJson? convertToBedrock(CobblemonSpawnJson json) {
         if (!(json.enabled == true))
            return null;
         string identifier = "cobblemon:" + json.spawns[0].pokemon.ToLower().Replace(" ", "_");
         List<SpawnRules.ConditionClass> conditions = new List<SpawnRules.ConditionClass>();
         for (int i = 0; i < json.spawns.Length; i++) {
            SpawnRules.ConditionClass newCondition = new SpawnRules.ConditionClass();
            var spawnClass = json.spawns[i];

            //Initialize Stuff
            //newCondition.valid_spawn_blocks = new List<string>();
            //newCondition.prevented_blocks = new List<string>();

            //Spawn Weight
            newCondition.weight = new SpawnRules.SpawnWeight(spawnClass.weight * (bucketMultipliers[spawnClass.bucket]));

            //Reads context from preset if valid (uses the first one it finds)
            if (spawnClass.context == null && spawnClass.presets != null && spawnClass.presets.Length > 0) {
               var validContexts = spawnClass.presets
                   .Where(x => presetDefinitions.ContainsKey(x) && presetDefinitions[x].context != null)
                   .Select(x => presetDefinitions[x].context)
                   .ToArray();
               if (validContexts.Length > 0)
                  spawnClass.context = validContexts[0];
            }

            //Taking context to set where it can spawn
            if (spawnClass.context == "grounded" || spawnClass.context == "surface") {
               newCondition.surface = new object();
            }
            else if (spawnClass.context == "submerged" || spawnClass.context == "seafloor") {
               newCondition.underwater = new object();
            }
            else if (spawnClass.context == null) {
               Misc.warn($"Skipping Spawn Condition {spawnClass.id} because Context is not implimented...");
               continue;
            }
            else {
               throw new NotImplementedException($"Context {spawnClass.context} is not implimented!");
            }

            //Using presets
            if (spawnClass.presets != null && spawnClass.presets.Length > 0) {
               foreach (var preset in spawnClass.presets) {
                  if (preset == null || !presetDefinitions.TryGetValue(preset, out var presetData))
                     continue;

                  if (presetData.condition != null) {
                     //If a structure is required, ignore the condition
                     if (presetData.condition.structures != null && presetData.condition.structures.Length > 0) {
                        goto StructureNotSupported;
                     }
                     newCondition.ApplyCondition(presetData.condition);
                  }
                  //Anticonditions
                  if (presetData.antiCondition != null) {
                     //Structures should be fine for anticonditions (no goto statement needed)
                     newCondition.ApplyAnticondition(presetData.antiCondition);
                  }
               }
            }
            //Handle conditions
            if (spawnClass.condition != null)
               newCondition.ApplyCondition(spawnClass.condition);
            if (spawnClass.anticondition != null)
               newCondition.ApplyAnticondition(spawnClass.anticondition);

            //If these lists are initialized but empty, then they are probably intended for modded blocks and the condition should be disabled
            if (newCondition.valid_spawn_blocks != null && newCondition.valid_spawn_blocks.Count < 1)
               continue;
            //Not necessary for prevented blocks tho
            if (newCondition.prevented_blocks != null && newCondition.prevented_blocks.Count < 1)
               newCondition.prevented_blocks = null;

            //Calls the entity with the correct spawn event for scripting to identify
            newCondition.permute_types = [new SpawnRules.ConditionClass.PermuteType(100, identifier, $"cobblemon:spawn_condition_{i}")];

            if (newCondition.weight.@default == 0)
               continue;



            //Our emulation of weightMultipliers
            if (spawnClass.weightMultiplier != null && spawnClass.weightMultiplier.condition != null) {
               //Skip multipliers with structure requirements
               if (spawnClass.weightMultiplier.condition.structures != null && spawnClass.weightMultiplier.condition.structures.Length > 0)
                  continue;
               var multipliedCondition = newCondition.Clone();
               //If the multiplier is increasing the odds
               if (spawnClass.weightMultiplier.multiplier > 1) {
                  multipliedCondition.weight = new SpawnRules.SpawnWeight((spawnClass.weightMultiplier.multiplier - 1) * newCondition.weight.@default);
                  multipliedCondition.ApplyCondition(spawnClass.weightMultiplier.condition);
               }
               //If the multiplier decreases the odds on that condition
               else {
                  var currentWeight = newCondition.weight.@default;
                  var multiplierWeight = currentWeight * spawnClass.weightMultiplier.multiplier;
                  multipliedCondition.weight = new SpawnRules.SpawnWeight(multiplierWeight);
                  newCondition.weight = new SpawnRules.SpawnWeight(currentWeight - multiplierWeight);
                  newCondition.ApplyAnticondition(spawnClass.weightMultiplier.condition);
               }
               multipliedCondition.permute_types = [new SpawnRules.ConditionClass.PermuteType(100, identifier, $"cobblemon:spawn_condition_{i}m")];
               conditions.Add(multipliedCondition);
            }

            conditions.Add(newCondition);
         //Skip spawn conditions with structure requirements because we cannot check those
         //I never thought I would actually use goto statements lol
         StructureNotSupported:
            {
               continue;
            }
         }
         SpawnRules rules = new SpawnRules(identifier, "animal", conditions);
         SpawnRulesJson output = new SpawnRulesJson(rules);
         return output;
      }
   }
}
