using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using CobbleBuild.ConversionTechnology;
using CobbleBuild.DataCreator;
using Newtonsoft.Json;
using static CobbleBuild.Config;
using static CobbleBuild.Misc;
using static CobbleBuild.Program;

namespace CobbleBuild {
   public static class Tasks {
      public static async Task ImportPokemon(Pokemon pokemon) {
         //Console.WriteLine("Importing Cobblemon: " + pokemon.Name);
         //Variation Processing from Resolver
         string[] resolvers = Directory.GetFiles(Path.Combine(config.resourcesPath, "assets/cobblemon/bedrock/pokemon/resolvers/", pokemon.folder_name));
         pokemon.Variations = VariationConversion.convertVariations(resolvers
             .Select(x => JsonConvert.DeserializeObject<ResolverJson>(File.ReadAllText(x))!)
             .ToArray(), pokemon);

         //Creating Natural Spawning Files after reading spawn data
         if (File.Exists(Path.Combine(config.resourcesPath, @"data\cobblemon\spawn_pool_world\", pokemon.folder_name + ".json"))) {
            CobblemonSpawnJson spawnData = await LoadFromJsonAsync<CobblemonSpawnJson>(Path.Combine(config.resourcesPath, @"data\cobblemon\spawn_pool_world\", pokemon.folder_name + ".json"));
            pokemon.spawnData = spawnData;
            var newSpawnData = SpawnConversion.convertToBedrock(spawnData);
            if (newSpawnData != null) {
               await SaveToJsonAsync(newSpawnData, Path.Combine(config.behaviorPath, @"spawn_rules\cobblemon\", pokemon.shortName + ".json"));
            }
            else {
               warn($"Spawn File Conversion for {pokemon.shortName} Failed and returned null.");
            }
         }
         //Does not need a warning
         //else {
         //   warn("No spawn file found for " + pokemon.shortName);
         //}

         //Creates Loot Table for Cobblemon
         LootTableJson? loot = LootConversion.convertToBedrock(pokemon.data.drops);
         if (loot != null) {
            await SaveToJsonAsync(loot, Path.Combine(config.behaviorPath, @$"loot_tables\cobblemon\{pokemon.shortName}.json"));
            pokemon.hasLootTable = true;
         }
         //else {
         //   warn($"Loot Conversion for {pokemon.shortName} returned Null Value");
         //}

         //Creates Client Entity
         RenderControllerJson? renderer;
         AnimationControllerJson? animationController;
         ClientEntityJson clientEntity = ClientEntityCreator.Create(ref pokemon, out renderer, out animationController);
         await SaveToJsonAsync(clientEntity, Path.Combine(config.resourcePath, @$"entity\cobblemon\{pokemon.shortName}.json"));

         if (renderer != null) {
            await SaveToJsonAsync(renderer, Path.Combine(config.resourcePath, $"render_controllers/cobblemon/{pokemon.shortName}.json"));
         }

         if (animationController != null) {
            await SaveToJsonAsync(animationController, Path.Combine(config.resourcePath, $"animation_controllers/cobblemon/{pokemon.shortName}.json"));
         }

         //Creates Server Entity
         await SaveToJsonAsync(ServerEntityCreator.Create(pokemon), Path.Combine(config.behaviorPath, @$"entities\cobblemon\{pokemon.shortName}.json"));

         //Creates Animation Json
         if (pokemon.animationData != null)
            await SaveToJsonAsync(new AnimationJson("1.10.0", pokemon.animationData), Path.Combine(config.resourcePath, $@"animations/cobblemon/{pokemon.shortName}.animation.json"));

         //Copy Pokemon Data to /scripts to be compiled with
         await SaveToJsonAsync(new OutputData(pokemon), Path.Combine(config.projectPath, "scripts", "pokemon_data", $"{pokemon.shortName}.json"), false); //Esbuild will not accept jsonc

         //Sucess, Yipee
         sucessCounter++;
         pokemon.passedWithoutErrors = true;
      }
      public static async Task ImportPokeball(string file, AnimationJson pokeballAnimations) {
         PokeballResourceData variation = await LoadFromJsonAsync<PokeballResourceData>(file);
         string identifier = variation.pokeball;
         string name = identifier.Split(":")[1];

         //Create Server Entity
         ServerEntityJson serverEntity = ServerEntityCreator.Create(variation);
         await SaveToJsonAsync(serverEntity, Path.Combine(config.behaviorPath, @$"entities\pokeballs\{name}.json"));

         //Copy Texture
         await Misc.CopyAsync(Path.Combine(config.resourcesPath, @$"assets\cobblemon\textures\poke_balls\{name}.png"), Path.Combine(config.resourcePath, @$"textures\pokeballs\{name}.png"));

         //Loot Tables
         LootTable loot = new LootTable() {
            entries = new List<LootTable.LootTableEntry>(),
            rolls = 1
         };
         loot.entries.Add(new LootTable.LootTableEntry(identifier, 100));
         await SaveToJsonAsync(new LootTableJson(loot), Path.Combine(config.behaviorPath, "loot_tables/pokeballs", name + ".json"));

         //Create Client Entity
         ClientEntityJson clientEntity = ClientEntityCreator.Create(variation, pokeballAnimations);
         await SaveToJsonAsync(clientEntity, Path.Combine(config.resourcePath, @$"entity/pokeballs/{name}.json"));

         //Creates Item Json
         ItemJson item = ItemCreator.Create(variation);
         await SaveToJsonAsync(item, Path.Combine(config.behaviorPath, @$"items\pokeballs\{name}.json"));

         //Creates Dummy Pokeball for display on healing machine
         await SaveToJsonAsync(ServerEntityCreator.CreatePokeballDummy(variation), Path.Combine(config.behaviorPath, "entities/pokeballs/", name + "_dummy.json"));
         await SaveToJsonAsync(ClientEntityCreator.CreatePokeballDummy(variation, pokeballAnimations), Path.Combine(config.resourcePath, "entity/pokeballs/", name + "_dummy.json"));
      }
      public static async Task createApricorn(string apricornName) {
         int percentToGetSeed = 10;
         //Imports blocks using premade black apricorn block as template
         if (apricornName != "black_apricorn") {
            //Apricorn block, sapling, and generated apricorn block
            await Import.ImportUsingTemplate(Path.Combine(config.projectPath, "behavior_packs/CobblemonBedrock/blocks/black_apricorn.block.json"), "black_apricorn", apricornName, Path.Combine(config.behaviorPath, $"blocks/apricorns/{apricornName}.block.json"));
            await Import.ImportUsingTemplate(Path.Combine(config.projectPath, "behavior_packs/CobblemonBedrock/blocks/black_apricorn_generated.block.json"), "black_apricorn", apricornName, Path.Combine(config.behaviorPath, $"blocks/apricorns/{apricornName}_generated.block.json"));
            await Import.ImportUsingTemplate(Path.Combine(config.projectPath, "behavior_packs/CobblemonBedrock/blocks/black_apricorn_seed.block.json"), "black_apricorn", apricornName, Path.Combine(config.behaviorPath, $"blocks/apricorns/{apricornName}_seed.block.json"));
            //Worldgen
            await Import.ImportUsingTemplate(Path.Combine(config.projectPath, "behavior_packs/CobblemonBedrock/features/black_apricorn_prefab.json"), "black_apricorn", apricornName, Path.Combine(config.behaviorPath, $"features/{apricornName}_prefab.json"));
         }

         //Creates Placers
         ItemJson saplingPlacer;
         ItemJson apricorn = ItemCreator.Create(apricornName, out saplingPlacer);
         await SaveToJsonAsync(apricorn, Path.Combine(config.behaviorPath, $"items/apricorns/{apricornName}.json"));
         await Import.ImportTexture(Path.Combine(config.resourcesPath, $"assets/cobblemon/textures/item/{apricornName}.png"), Path.Combine(config.resourcePath, $"textures/item/{apricornName}.png"), Import.TextureType.Item);
         await SaveToJsonAsync(saplingPlacer, Path.Combine(config.behaviorPath, $"items/apricorns/{apricornName}_seed.json"));
         await Import.ImportTexture(Path.Combine(config.resourcesPath, $"assets/cobblemon/textures/item/{apricornName}_seed.png"), Path.Combine(config.resourcePath, $"textures/item/{apricornName}_seed.png"), Import.TextureType.Item);

         //Loot Tables
         LootTable apricotAdultLoot = new LootTable() {
            entries = new List<LootTable.LootTableEntry>(),
            rolls = 1
         };
         apricotAdultLoot.entries.Add(new LootTable.LootTableEntry($"cobblemon:{apricornName}", 100));
         LootTable apricotAdultLoot2 = new LootTable() {
            entries = new List<LootTable.LootTableEntry>(),
            rolls = 1
         };
         apricotAdultLoot2.entries.Add(new LootTable.LootTableEntry($"cobblemon:{apricornName}_sapling", percentToGetSeed));
         apricotAdultLoot2.entries.Add(new LootTable.LootTableEntry(100 - percentToGetSeed));
         LootTable saplingLoot = new LootTable() {
            entries = new List<LootTable.LootTableEntry>(),
            rolls = 1
         };
         saplingLoot.entries.Add(new LootTable.LootTableEntry($"cobblemon:{apricornName}_sapling", 100));
         await SaveToJsonAsync(new LootTableJson(apricotAdultLoot, apricotAdultLoot2), Path.Combine(config.behaviorPath, $"loot_tables/blocks/apricorns/{apricornName}.json"));
         await SaveToJsonAsync(new LootTableJson(saplingLoot), Path.Combine(config.behaviorPath, $"loot_tables/blocks/apricorns/{apricornName}_sapling.json"));
      }
      public static async Task CreateBerry(string pathToFile) {
         if (!File.Exists(pathToFile)) {
            throw new ArgumentException("File Path Provided did not get a valid file");
         }
         var berryData = await LoadFromJsonAsync<Berry>(pathToFile);
         var berryName = Path.GetFileNameWithoutExtension(pathToFile);
         var texturePath = Path.Combine(config.resourcesPath, "assets/cobblemon/textures/item/berries", $"{berryName}.png");
         //Import.ImportTexture(texturePath, Path.Combine(config.resourcePath, getPathFrom(texturePath, "textures")), Import.TextureType.Item);
         ItemJson item = ItemCreator.CreateBerry(berryName, berryData);
         await SaveToJsonAsync(item, Path.Combine(config.behaviorPath, $"items/berries/{berryName}.json"));
      }
      public static async Task CreateGemericItemFromTexture(string texturePath) {
         string itemName = Path.GetFileNameWithoutExtension(texturePath);
         ItemJson item = ItemCreator.CreateGeneric(itemName);
         await SaveToJsonAsync(item, Path.Combine(config.behaviorPath, $"items/generic/{itemName}.json"));
      }
   }
}
