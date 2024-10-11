namespace CobbleBuild {
   public class Cleaner {
      public static List<string> Directories { get; set; } = new List<string>(){
            "resource_packs/CobblemonBedrock/models/entity/cobblemon/",
            "resource_packs/CobblemonBedrock/animations/cobblemon/",
            "resource_packs/CobblemonBedrock/textures/pokemon/",
            "behavior_packs/CobblemonBedrock/spawn_rules/cobblemon/",
            "behavior_packs/CobblemonBedrock/entities/cobblemon/",
            "resource_packs/CobblemonBedrock/entity/cobblemon/",
            "resource_packs/CobblemonBedrock/sounds/cobblemon/",
            "behavior_packs/CobblemonBedrock/entities/pokeballs/",
            "resource_packs/CobblemonBedrock/entity/pokeballs/",
            "behavior_packs/CobblemonBedrock/entities/pokeballs/",
            "resource_packs/CobblemonBedrock/textures/pokeballs/",
            "resource_packs/CobblemonBedrock/models/entity/pokeballs/",
            "resource_packs/CobblemonBedrock/animations/pokeballs/",
            "behavior_packs/CobblemonBedrock/items/pokeballs/",
            "behavior_packs/CobblemonBedrock/loot_tables/cobblemon/",
            "behavior_packs/CobblemonBedrock/loot_tables/pokeballs/",
            "resource_packs/CobblemonBedrock/render_controllers/cobblemon",
            "resource_packs/CobblemonBedrock/animation_controllers/cobblemon",
            "behavior_packs/CobblemonBedrock/loot_tables/blocks/apricorns/",
            "behavior_packs/CobblemonBedrock/items/apricorns/",
            "behavior_packs/CobblemonBedrock/items/berries/",
            "behavior_packs/CobblemonBedrock/items/generic/",
            "behavior_packs/CobblemonBedrock/blocks/apricorns/",
            "behavior_packs/CobblemonBedrock/recipes/",
            "behavior_packs/CobblemonBedrock/features/apricorns/",
            "behavior_packs/CobblemonBedrock/feature_rules/apricorns/",
            "scripts/pokemon_data/",
            "behavior_packs/CobblemonBedrock/scripts/pokemon_data/",
            "resource_packs/CobblemonBedrock/textures/item/",
            "resource_packs/CobblemonBedrock/textures/block/"
        };
      public static List<string> DirectoriesDONOTCLEAN { get; set; } = new List<string>() //NOT AN EXCLUDE, just directories that should be verified but not cleaned
      {
            "behavior_packs/CobblemonBedrock/texts/",
            "resource_packs/CobblemonBedrock/texts/",
            "behavior_packs/CobblemonBedrock/blocks/"
        };
      public static void Verify() { //Makes sure they all exist
         foreach (var dir in Directories) {
            Directory.CreateDirectory(Path.Combine(Config.config.projectPath, dir));
         }
         foreach (var dir in DirectoriesDONOTCLEAN) {
            Directory.CreateDirectory(Path.Combine(Config.config.projectPath, dir));
         }
      }

      public static void Clean() {
         foreach (var dir in Directories) {
            Directory.Delete(Path.Combine(Config.config.projectPath, dir), true);
            Directory.CreateDirectory(Path.Combine(Config.config.projectPath, dir));
         }
      }
   }
}
