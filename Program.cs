using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using CobbleBuild.ConversionTechnology;
using CobbleBuild.JavaClasses;
using CobbleBuild.Kotlin;
using Newtonsoft.Json;
using System.Diagnostics;
using static CobbleBuild.Config;
using static CobbleBuild.Misc;
using static CobbleBuild.Wrappers;

namespace CobbleBuild {

   public class Program {

      public static List<string> EntityTextures = new List<string>();
      public static AtlasJson itemAtlasJson;
      public static AtlasJson blockAtlasJson;
      //public static LanguageFileOld lang = new LanguageFileOld();
      //public static Dictionary<string, SoundDefinition> sounds = new Dictionary<string, SoundDefinition>();
      /// <summary>
      /// List of folders to delete when the build finishes
      /// </summary>
      public static List<string> temporaryFolders = new List<string>();
      /// <summary>
      /// Global repository of animations.
      /// </summary>
      public static Dictionary<string, Animation> animations = [];

      //All Pokemon Identified as Implimented
      public static List<Pokemon> targetPokemon = new List<Pokemon>();

      public static int sucessCounter = 0;


      static async Task Main(string[] args) {
         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("CobbleBuild - Cobblemon Port Tool for Minecraft Bedrock Edition");

         init(args);
         //Tests if the required Directories exist.
         if (!Directory.Exists(config.projectPath) || !Directory.Exists(config.behaviorPath) || !Directory.Exists(config.resourcePath)) {
            error($"Be sure to run CobbleBuild in the root of the cobblemon-bedrock source code.");
         }
         if (!Directory.Exists(config.resourcesPath)) {
            var resoursesPath = Path.Combine(config.projectPath, "extractedResources", "cobblemon-src");
            Console.WriteLine("Downloading Cobblemon Source...");
            Directory.CreateDirectory(resoursesPath);
            RunCmd("git", "clone https://gitlab.com/cable-mc/cobblemon.git --branch v1.4.1 --single-branch ./", resoursesPath);
            config.cobblemonPath = resoursesPath;

            if (!config.temporairlyExtract)
               config.overwriteConfig();
            else
               temporaryFolders.Add(resoursesPath);
         }
         if (!Directory.Exists(config.minecraftJavaPath)) {
            //Default folder path
            string mcJarPath = handleEnvVariables(ref new Config().minecraftJavaPath);
            if (File.Exists(config.minecraftJavaPath) && config.minecraftJavaPath.EndsWith(".jar"))
               mcJarPath = config.minecraftJavaPath;

            if (File.Exists(mcJarPath) && mcJarPath.EndsWith(".jar")) {
               Console.WriteLine("Extracting Minecraft...");
               config.minecraftJavaPath = ExtractZipFile(mcJarPath);

               if (!config.temporairlyExtract)
                  config.overwriteConfig();
            }
            else {
               error($"Minecraft Java ({config.targetMinecraftVersion}) is required to run CobbleBuild. Please specify a valid path to the minecraft .jar using the -g flag or the config file.");
            }
         }
         Cleaner.Verify();
         if (config.buildTasks.Contains("clean")) {
            Console.WriteLine("Cleaning old build...");
            Cleaner.Clean();
         }

         //I know my attitude has been to not care about performance but damn this takes a long time.
         if (config.buildTasks.Contains("translate")) {
            Console.WriteLine("Copying Translations...");
            List<string> validLanguages = [];
            var USAdditions = JsonConvert.DeserializeObject<TranslationKey>(File.ReadAllText(Path.Combine(config.projectPath, "base_translations/en_US.json")))!;
            foreach (string file in Directory.GetFiles(Path.Combine(config.resourcesPath, "assets/cobblemon/lang"))) {
               var languageIDArray = Path.GetFileNameWithoutExtension(file).ToCharArray();
               //Capitalize characters 4 and 5 in the string.
               languageIDArray[3] = char.ToUpper(languageIDArray[3]);
               languageIDArray[4] = char.ToUpper(languageIDArray[4]);
               var languageID = new string(languageIDArray);

               var languageDictionary = DeserializeFromFile<TranslationKey>(file)!;
               //Adds in bedrock specific translations (starts with US, and is overwritten by other languages)
               languageDictionary.Merge(USAdditions);

               //Adds in bedrock specific translation for each language
               var languageAdditionsPath = Path.Combine(config.projectPath, $"base_translations/{languageID}.json");
               if (File.Exists(languageAdditionsPath)) {
                  var languageAdditions = DeserializeFromFile<TranslationKey>(languageAdditionsPath)!;
                  languageDictionary.Merge(languageAdditions);
               }


               //languageDictionary.Prepare();
               File.WriteAllText(Path.Combine(config.resourcePath, $"texts/{languageID}.lang"), languageDictionary.getBedrockKey());
               validLanguages.Add(languageID);
            }
            SaveToJson(validLanguages, Path.Combine(config.resourcePath, "texts/languages.json"));
         }

         if (config.buildTasks.Contains("posers")) {
            PoserRegistry.InitMappings();
            string cachePath = Path.Combine(config.projectPath, "poserCache");
            if (config.cachePosers && !Directory.Exists(cachePath))
               Directory.CreateDirectory(cachePath);

            Console.WriteLine("Parsing Kotlin Posers...");
            var kotlinPosersRoot = Path.Combine(config.kotlinBasePath, "client/render/models/blockbench/pokemon");
            string[] poserPaths = getAllFilesInDirandSubDirs(kotlinPosersRoot, kotlinPosersRoot);
            var poserTasks = new ActionGroup(config.multithreaded ? ActionGroupType.Async : ActionGroupType.Sync);
            foreach (string poserPath in poserPaths) {
               poserTasks.AddOrRun(() => printExceptionsToConsole(() => {
                  using (FileStream file = File.OpenRead(poserPath)) {
                     var poser = KotlinPoser.import(file);
                     string poserName = PoserRegistry.mappings![poser.poserName!];
                     PoserRegistry.posers[poserName] = poser;
                     if (config.cachePosers) {
                        SaveToJson(poser, Path.Combine(cachePath, poserName + ".json"));
                     }
                  }
               }));
            }
            var poserReadTime = poserTasks.ExecuteAll();
            Console.WriteLine($"Finished in {poserReadTime / 1000}s");
         }

         if (config.buildTasks.Contains("sounds") || config.buildTasks.Contains("importSounds")) {
            Console.WriteLine("Importing Sounds..");
            var javaSounds = LoadFromJson<JavaSoundJson>(Path.Combine(config.resourcesPath, "assets/cobblemon", "sounds.json"));
            var bedrockSounds = javaSounds.toBedrock();
            await Import.ImportAllSoundsFromSoundDef(bedrockSounds);
            await SaveToJsonAsync(bedrockSounds, Path.Combine(config.resourcePath, "sounds", "sound_definitions.json"));
         }

         if (config.buildTasks.Contains("build")) {
            if (PoserRegistry.posers.Count == 0) {
               string poserCachePath = Path.Combine(config.projectPath, "poserCache");
               if (config.cachePosers && Directory.Exists(poserCachePath) && Directory.GetFiles(poserCachePath).Length > 0) {
                  info("Using Poser Cache...");
                  var files = Directory.GetFiles(poserCachePath);
                  foreach (var file in files) {
                     if (!file.EndsWith(".json"))
                        return;
                     var poserName = Path.GetFileNameWithoutExtension(file);
                     var poser = await LoadFromJsonAsync<KotlinPoser>(file);
                     PoserRegistry.posers[poserName] = poser;
                  }
               }
               else {
                  warn("Posers have not been processed. Animation Controllers will not be created.");
               }
            }

            //Read Item Exture Json Base
            itemAtlasJson = LoadFromJson<AtlasJson>(Path.Combine(config.resourcePath, "textures/item_texture_base.json"));

            //Create Terrain (Block) Atlas 
            blockAtlasJson = LoadFromJson<AtlasJson>(Path.Combine(config.resourcePath, "textures/terrain_texture_base.json"));

            //Read Entity Textures List JSON (I found not including this in the pack meant that textures in subfolders won't load)
            string EntityTexturesJsonText = File.ReadAllText(Path.Combine(config.resourcePath, "textures/textures_list_base.json"));
            EntityTextures = JsonConvert.DeserializeObject<List<string>>(EntityTexturesJsonText)!;

            SpawnConversion.initValues();
            JavaData.populateData();

            Console.WriteLine("Analyzing Species Data...");
            //Scan files inside folders in common\src\main\resources\data\cobblemon\species\ to get all implimented pokemon and adds them to targetPokemon
            string[] genFolders = Directory.GetDirectories(Path.Combine(config.resourcesPath, @"data\cobblemon\species\"));
            foreach (string folder in genFolders) {
               string[] files = Directory.GetFiles(folder);
               foreach (string file in files) {
                  try {
                     string fileData = File.ReadAllText(file);
                     var data = JsonConvert.DeserializeObject<SpeciesData>(fileData);
                     if (data!.implemented == true) {
                        targetPokemon.Add(new Pokemon(data));
                     }
                  }
                  catch (Exception ex) {
                     softError("Could not parse json: " + ex.Message);
                  }
               }
            }
            Console.WriteLine(targetPokemon.Count.ToString() + " are implimented in the source code.");

            //I found that alot of the unused animations are broken, so I make a dictionary, and only the used animations are added to the pokemon.
            //Also saves on space, technically
            Console.WriteLine("Gathering Animations...");
            var animationsRoot = Path.Combine(config.resourcesPath, "assets/cobblemon/bedrock/pokemon/animations");
            foreach (string animationPath in getAllFilesInDirandSubDirs(animationsRoot)) {
               if (animationPath.EndsWith(".json")) {
                  var animationJson = Import.ReadAnimation(animationPath);
                  foreach (var animation in animationJson.animations) {
                     animations.TryAdd(animation.Key, animation.Value);
                  }
               }
            }

            Console.WriteLine("Implimenting Pokemon...");
            var pokemonTasks = new List<Task>();
            var pokemonStopWatch = Stopwatch.StartNew();
            foreach (var pokemon in targetPokemon) {
               pokemonTasks.Add(Tasks.ImportPokemon(pokemon));
            }
            printExceptionsToConsole(() => Task.WaitAll([.. pokemonTasks]));
            pokemonStopWatch.Stop();
            var pokemonCreateTime = pokemonStopWatch.ElapsedMilliseconds;

            Console.WriteLine($"Implimented {sucessCounter}/{targetPokemon.Count} Pokemon in {pokemonCreateTime / 1000}s.");
            Console.WriteLine("Implimenting Pokeballs...");

            //Generic Pokeball Model
            Import.ImportModel("pokeball", Path.Combine(config.resourcesPath, @"assets\cobblemon\bedrock\poke_balls\models\poke_ball.geo.json"), Path.Combine(config.resourcePath, @"models\entity\pokeballs\poke_ball.geo.json"));

            //Generic Pokeball Animation
            string pokeballAnimationData = File.ReadAllText(Path.Combine(config.resourcesPath, @"assets\cobblemon\bedrock\poke_balls\animations\poke_ball.animation.json"));
            AnimationJson pokeballAnimations = JsonConvert.DeserializeObject<AnimationJson>(pokeballAnimationData)!;
            await File.WriteAllTextAsync(Path.Combine(config.resourcePath, @"animations\pokeballs\pokeball.animation.json"), JsonConvert.SerializeObject(pokeballAnimations, config.SerializerSettings));

            //Read each pokeball variation
            var pokeballTasks = new List<Task>();
            string[] ballDirs = Directory.GetFiles(Path.Combine(config.resourcesPath, @"assets\cobblemon\bedrock\poke_balls\variations\"));
            foreach (string file in ballDirs) {
               pokeballTasks.Add(Tasks.ImportPokeball(file, pokeballAnimations));
            }
            printExceptionsToConsole(() => Task.WaitAll([.. pokemonTasks]));

            //Imports all Apricorns using textures (best method we have without parsing kotlin code) (Uses Black Apricorn block and sappling as template)
            Console.WriteLine("Importing Apricorns...");
            var apricornTasks = new List<Task>();
            foreach (string file in Directory.GetFiles(Path.Combine(config.resourcesPath, "assets/cobblemon/textures/item/"))) {
               if (!file.EndsWith("apricorn.png")) {
                  continue;
               }
               string apricornName = Path.GetFileNameWithoutExtension(file);
               apricornTasks.Add(Tasks.createApricorn(apricornName));
            }
            printExceptionsToConsole(() => Task.WaitAll([.. apricornTasks]));


            //Read each pokeball variation
            Console.WriteLine("Importing Berries...");
            var berryTasks = new List<Task>();
            string[] berryDirs = Directory.GetFiles(Path.Combine(config.resourcesPath, @"data/cobblemon/berries"));
            foreach (string file in berryDirs) {
               berryTasks.Add(Tasks.CreateBerry(file));
            }
            printExceptionsToConsole(() => Task.WaitAll([.. berryTasks]));

            Console.WriteLine("Importing Other Resources...");
            await Import.ImportAllTexturesFromFolder(Path.Combine(config.resourcesPath, "assets/cobblemon/textures/block"), Path.Combine(config.resourcePath, "textures/block"), Import.TextureType.Block);
            await Import.ImportAllTexturesFromFolder(Path.Combine(config.resourcesPath, "assets/cobblemon/textures/item"), Path.Combine(config.resourcePath, "textures/item"), Import.TextureType.Item);
            await Import.ImportAllRecipesInFolder(Path.Combine(config.resourcesPath, "data/cobblemon/recipes"), Path.Combine(config.behaviorPath, "recipes"));
            //Unfortunately, model conversion is just too rough to automate
            //Import.ImportAllRegisteredJavaModels();

            //Copy all particles
            foreach (string file in getAllFilesInDirandSubDirs(Path.Combine(config.resourcesPath, "assets/cobblemon/bedrock/particles"))) {
               File.Copy(file, Path.Combine(config.resourcePath, "particles", Path.GetFileName(file)), true);
            }

            //Takes textures and creates functionless items with them only if that item isn't already implimented
            //Creates list items that cannot be created because they already exist
            List<string> existingItems = new List<string>();
            foreach (string file in getAllFilesInDirandSubDirs(Path.Combine(config.behaviorPath, "items"), Path.Combine(config.behaviorPath, "items/generic"))) {
               existingItems.Add(Path.GetFileNameWithoutExtension(file));
            }
            List<Task> genericItemTasks = new List<Task>();
            foreach (string file in getAllFilesInDirandSubDirs(Path.Combine(config.resourcesPath, "assets/cobblemon/textures/item"), Path.Combine(config.resourcesPath, "assets/cobblemon/textures/item\\advancements"))) {
               if (!existingItems.Contains(Path.GetFileNameWithoutExtension(file))) {
                  //genericItemTasks.Add(Tasks.CreateGemericItemFromTexture(file));
                  genericItemTasks.Add(Tasks.CreateGemericItemFromTexture(file));
               }
            }
            printExceptionsToConsole(() => Task.WaitAll([.. genericItemTasks]));


            Console.WriteLine("Finishing up...");
            //Capping off Important Files
            File.WriteAllText(Path.Combine(config.resourcePath, @"textures\item_texture.json"), JsonConvert.SerializeObject(itemAtlasJson, config.SerializerSettings));
            File.WriteAllText(Path.Combine(config.resourcePath, @"textures\terrain_texture.json"), JsonConvert.SerializeObject(blockAtlasJson, config.SerializerSettings));

            //Add liscence information
            File.WriteAllText(Path.Combine(config.resourcePath, "models/entity/cobblemon/license"), Resources.CCPL);

            //Creates Entity Texture List
            string TextureListString = JsonConvert.SerializeObject(EntityTextures, config.SerializerSettings);
            File.WriteAllText(Path.Combine(config.resourcePath, "textures/textures_list.json"), TextureListString);

            //Deletes Temporary Folders
            foreach (string path in temporaryFolders) {
               Directory.Delete(path, true);
            }
         }

         if (config.buildTasks.Contains("deploy")) {
            if (!Directory.Exists(Path.Combine(config.projectPath, "node_modules"))) {
               //Project has not been set up with npm
               Console.WriteLine("Initializing Node Project...");
               RunCmd("npm", "install", config.projectPath);
            }
            Console.WriteLine("Deploying...");
            RunCmd("gulp", config.gulpArgs, config.projectPath);
         }

         if (config.buildTasks.Contains("launch")) {
            if (config.useMinecraftPreview) {
               OpenUri("minecraft-preview://");
            }
            else {
               OpenUri("minecraft://");
            }
         }
      }
   }
}