using CobbleBuild.CobblemonClasses;
using Newtonsoft.Json;
using static CobbleBuild.ConversionTechnology.SpawnConversion;
using static CobbleBuild.JavaClasses.JavaData;

namespace CobbleBuild.BedrockClasses {
   public class SpawnRulesJson : BedrockJson {
      [JsonProperty("minecraft:spawn_rules")]
      public SpawnRules spawn_rules;
      public SpawnRulesJson(SpawnRules spawnRules) : base("1.18.0") //Not sure why newer format version don't work; this component must be outdated.
      {
         spawn_rules = spawnRules;
      }
   }
   public class SpawnRules {

      public DescriptionClass description;
      public List<ConditionClass> conditions;
      public SpawnRules(string identifier, string populationType, List<ConditionClass> condition) {
         description = new DescriptionClass(identifier, populationType);
         conditions = condition;
      }
      //This seems really silly but should be fine and is just the easiest solution without some sort of library
      public SpawnRules Clone() {
         return JsonConvert.DeserializeObject<SpawnRules>(JsonConvert.SerializeObject(this))!;
      }
      public class DescriptionClass {
         public string identifier;
         public string population_control;
         public DescriptionClass(string identify, string populationType) {
            identifier = identify;
            population_control = populationType;
         }
      }
      public class ConditionClass {
         [JsonProperty("minecraft:biome_filter")]
         public List<Filter>? biome_filter;
         [JsonProperty("minecraft:spawns_on_block_filter")]
         public List<string>? valid_spawn_blocks;
         [JsonProperty("minecraft:spawns_on_block_prevented_filter")]
         public List<string>? prevented_blocks;
         [JsonProperty("minecraft:height_filter")]
         public MinMiaxValue? height_filter;
         [JsonProperty("minecraft:density_limit")]
         public DensityLimit? density_limit;
         [JsonProperty("minecraft:weight")]
         public SpawnWeight? weight;
         [JsonProperty("minecraft:herd")]
         public MinMaxSize? herding;
         [JsonProperty("minecraft:permute_type")]
         public List<PermuteType>? permute_types;
         //to enable these just make them a new object (denulify them but leave them blank)
         [JsonProperty("minecraft:spawns_on_surface")]
         public object? surface;
         [JsonProperty("minecraft:spawns_underground")]
         public object? underground;
         [JsonProperty("minecraft:spawns_underwater")]
         public object? underwater;
         [JsonProperty("minecraft:spawns_lava")]
         public object? lava;

         public class DensityLimit {
            public int surface;
            public int underground;
            public DensityLimit(int surface, int underground = 0) {
               this.surface = surface;
               this.underground = underground;
            }
         }

         public class PermuteType {
            public int weight;
            public string? entity_type; // can be minecraft:entity<minecraft:spawn_event>
            public int? garunteed_count;
            /// <summary>
            /// Creates a permute type specifying a specific entity
            /// </summary>
            /// <param name="weight">Odds of specified permute type. Overall the weight must add up to 100.</param>
            /// <param name="entity">Entity to spawn. Leaving this null will result in the normal entity of the spawn condition spawning.</param>
            /// <param name="spawnEvent">Allows you to specify a specific spawn event for that entity</param>
            public PermuteType(int weight, string? entity = null, string? spawnEvent = null) {
               this.weight = weight;
               this.entity_type = entity;
               if (spawnEvent != null && entity != null)
                  this.entity_type += $"<{spawnEvent}>";
            }
         }

         public ConditionClass() { }
         //This seems really silly but should be fine and is just the easiest solution without some sort of library
         public ConditionClass Clone() {
            return JsonConvert.DeserializeObject<ConditionClass>(JsonConvert.SerializeObject(this))!;
         }
         /// <summary>
         /// Applies a cobblemon spawn condition to this bedrock condition
         /// </summary>
         /// <param name="definite">Whether or not this is combining with the other condition or applying on top. (if blocks need to be added to the list or use an .Intersect())</param>
         /// <param name="antiCondition">Whether or not this condition will behave like an anticondition</param>
         public void ApplyCondition(CobblemonSpawnCondition condition, bool definite = false, bool antiCondition = false) {
            //Filtering by Biome
            if (condition.biomes != null) {
               List<Filter> any_of_filter = new List<Filter>();
               foreach (string s in condition.biomes) {
                  //If nether biome is specified, the pokemon must be able to spawn underground for it to work
                  if (!antiCondition && netherBiomes.Contains(s))
                     this.underground = new object();

                  List<string[]> biomes = resolveBiome(s);
                  foreach (string[] biomeTags in biomes) {
                     if (biomeTags.Length == 0)
                        continue;
                     else if (biomeTags.Length == 1) {
                        any_of_filter.Add(new Filter("has_biome_tag") { @operator = "==", value = biomeTags[0] });
                     }
                     else {
                        any_of_filter.Add(new Filter() {
                           all_of = biomeTags.Select(x => new Filter("has_biome_tag") { @operator = "==", value = x }).ToList()
                        });
                     }


                  }
               }
               if (any_of_filter != null && any_of_filter.Count > 0) {
                  if (this.biome_filter == null)
                     this.biome_filter = [];
                  this.biome_filter.Add(antiCondition ? new Filter { none_of = any_of_filter } : new Filter { any_of = any_of_filter });
               }
               //If the filter is blank, the condition is probably only meant for modded biomes or something and should be disabled.
               else if (antiCondition == false) {
                  this.weight = new SpawnWeight(0);
               }
            }

            //Block Filtering
            //Only overwrite if not an anticondition
            if (condition.neededNearbyBlocks != null) {
               this.addBlocksToListIfValid(condition.neededNearbyBlocks, ref (antiCondition ? ref this.prevented_blocks : ref this.valid_spawn_blocks), definite && !antiCondition);
            }

            if (condition.neededBaseBlocks != null) {
               this.addBlocksToListIfValid(condition.neededBaseBlocks, ref (antiCondition ? ref this.prevented_blocks : ref this.valid_spawn_blocks), definite && !antiCondition);
            }

            //Height
            if (condition.minY != null)
               this.height_filter = new MinMiaxValue((int)condition.minY, 320);

            //Hopefully this is equivalent
            if (condition.canSeeSky == false) {
               if (antiCondition) {
                  this.surface = new object();
                  this.underground = null;
               }
               else {
                  this.surface = null;
                  this.underground = new object();
               }
            }

            //Fluid
            if (!antiCondition) { //If anticondition no action should be needed
               switch (condition.fluid) {
                  case "#minecraft:water":
                     this.underwater = new object();
                     break;
                  case "#minecraft:lava":
                     this.lava = new object();
                     break;
                  case null:
                  default:
                     break;
               }
            }
         }

         public void ApplyAnticondition(CobblemonSpawnCondition condition, bool definite = false) {
            this.ApplyCondition(condition, definite, true);
         }
         /// <param name="overwrite">Overwrites the list entirely</param>
         private void addBlocksToListIfValid(string[] blockIds, ref List<string>? listToApplyTo, bool overwrite) {
            bool preexistingList = listToApplyTo != null;
            var newList = new List<string>();
            foreach (string block in blockIds) { //Refrences to other block sets are ignored for now
               var item = resolveBlock(block);
               item = item.Select(x => { //Replace sugar cane because it is a unique case where the item and block have different ids
                  if (x == "minecraft:sugar_cane")
                     return "minecraft:reeds";
                  else
                     return x;
               }).ToList();
               newList = newList.Union(item).ToList();
            }
            //If multiple requirements, 
            listToApplyTo = (preexistingList && !overwrite) ? listToApplyTo!.Union(newList).ToList() : newList;
         }
      }

      public class SpawnWeight {
         [JsonProperty("default")]
         public float @default;
         public SpawnWeight(float weight) {
            @default = weight;
         }
      }

   }
}
