using Newtonsoft.Json;

namespace CobbleBuild {
   public class Config {
      private static Config _config;
      [JsonIgnore]
      public string targetMinecraftVersion = "1.20.1";
      /// <summary>
      /// Currently Loaded Configuration
      /// </summary>
      public static Config config {
         get {
            if (_config == null)
               throw new Exception("The config must be initialized before it can be read from.");
            return _config;
         }
         set { _config = value; }
      }
      //Currently Unused
      public int formatVersion = 2;
      /// <summary>
      /// Path to cobblemon source
      /// </summary>
      public string cobblemonPath = "%userprofile%/Desktop/CobblemonToBedrock/cobblemon-1.4.1-src";
      /// <summary>
      /// Path to cobblemon bedrock source
      /// </summary>
      public string projectPath = "%userprofile%/Documents/VSCode/Cobblemon TS";
      /// <summary>
      /// Path to target version of minecraft java
      /// </summary>
      public string minecraftJavaPath = "%appdata%/.minecraft/versions/1.20.1/1.20.1.jar";
      //These are only used by gulp
      public string minecraftPath = "%localappdata%/Packages/Microsoft.MinecraftUWP_8wekyb3d8bbwe/LocalState/games/com.mojang/";
      public string minecraftPreviewPath = "%localappdata%/Packages/Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe/LocalState/games/com.mojang/";
      public bool useMinecraftPreview { get; set; } = false;
      public string BPName { get; set; } = "CobblemonBedrock";
      public string RPName { get; set; } = "CobblemonBedrock";
      public bool usePowershell { get; set; } = false;
      public bool minify { get; set; } = false;
      public bool temporairlyExtract { get; set; } = true;
      public bool multithreaded { get; set; } = true;
      public string gulpArgs { get; set; } = "";
      public bool cachePosers { get; set; } = true;
      public List<string> tasks { get; set; } = [];
      public static List<string> defaultTasks = new List<string>() { "clean", "translatePosers", "build", "deploy" };

      //Not Serialized
      /// <summary>
      /// Path of the target behavior pack
      /// </summary>
      [JsonIgnore]
      public string behaviorPath { get; set; }
      /// <summary>
      /// Path of the target resource pack
      /// </summary>
      [JsonIgnore]
      public string resourcePath { get; set; }
      /// <summary>
      /// Configured Default serializer settings
      /// </summary>
      [JsonIgnore]
      public JsonSerializerSettings SerializerSettings { get; set; }
      /// <summary>
      /// Path to cobblemon resources (/common/src/main/resources)
      /// </summary>
      [JsonIgnore]
      public string resourcesPath { get; set; }
      /// <summary>
      /// Path to base kotlin code (/common/src/main/kotlin/com/cobblemon/mod/common)
      /// </summary>
      [JsonIgnore]
      public string kotlinBasePath { get; set; }

      //Tasks to complete
      [JsonIgnore]
      public List<string> buildTasks { get; set; } = new List<string>();

      [JsonConstructor]
      public Config() { }

      public static Config getDefault() {
         return new Config() { tasks = defaultTasks };
      }

      /// <summary>
      /// Overwrites the config file in the current working directory
      /// </summary>
      public void overwriteConfig() {
         File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "cbconfig.json"), JsonConvert.SerializeObject(this, Formatting.Indented));
      }

      private static void handleEnvVariables(ref string path) {
         path = Environment.ExpandEnvironmentVariables(path);
      }

      /// <summary>
      /// Initializes the config to static property Config.config
      /// </summary>
      /// <param name="args">Arguments that the program launched with.</param> 
      public static void init(string[] args) {
         config = loadConfig(args);
         handleArgs(args);

         //Resolve the env variables in the input paths
         handleEnvVariables(ref config.projectPath);
         handleEnvVariables(ref config.cobblemonPath);
         handleEnvVariables(ref config.minecraftJavaPath);

         config.behaviorPath = Path.Combine(config.projectPath, "behavior_packs", config.BPName);
         config.resourcePath = Path.Combine(config.projectPath, "resource_packs", config.RPName);
         config.resourcesPath = Path.Combine(config.cobblemonPath, "common", "src", "main", "resources");
         config.kotlinBasePath = Path.Combine(config.cobblemonPath, "common/src/main/kotlin/com/cobblemon/mod/common");
         if (config.minify) {
            config.SerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
         }
         else {
            config.SerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
         }
         if (config.buildTasks.Count < 1) {
            config.buildTasks = config.tasks;
         }
      }
      /// <summary>
      /// Loads config from current directory and returns it.
      /// </summary>
      public static Config loadConfig(string[] args) {
         if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "cbconfig.json"))) {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "cbconfig.json");
            Console.WriteLine($"Using Config \'{configPath}\'");
            return Misc.LoadFromJson<Config>(configPath);
         }
         else if (!args.Contains("-s") && Misc.yesOrNo("No config found in this directory. Create one?")) {
            var blankConfig = Config.getDefault();
            blankConfig.overwriteConfig();
            Environment.Exit(0);
            throw new Exception("Envoirnment Terminating"); //Make the compiler happy
         }
         else { //Continue with blank config if they do not choose to make one.
            return Config.getDefault();
         }
      }
      public static void handleArgs(string[] args) {
         //Basic Argument Handler
         //Testing of these paths are handled in the main void
         for (int i = 0; i < args.Length; i++) {
            string arg = args[i].ToLower();
            if (arg == "-i") {
               config.resourcesPath = args[i + 1];
               i++;
            }
            else if (arg == "-p") {
               config.projectPath = args[i + 1];
               i++;
            }
            else if (arg == "-r") {
               config.RPName = args[i + 1];
               i++;
            }
            else if (arg == "-b") {
               config.BPName = args[i + 1];
               i++;
            }
            else if (arg == "-g") {
               config.minecraftJavaPath = args[i + 1];
               i++;
            }
            else if (arg == "-m") {
               config.minify = true;
            }
            else if (arg == "-h") {
               Console.WriteLine(@"
    Options:
    -m : Minify the output json files
    -s : Skip asking to create config file
    -h : Display this help message
    -i [path] : Specify path to cobblemon source files
    -p [path] : Specify path to cobblemon bedrock source code
    -r [path] : Specify resource pack name
    -b [path] : Specify behavior pack name
    -g [path] : Specify path to the minecraft java jar file
");
               Environment.Exit(0);
            }
            else if (!arg.StartsWith("-")) {
               //Treat it as buildtask if not a valid flag
               config.buildTasks.Add(arg);
            }
            else if (arg != "-s") {
               Misc.error("Unknown flag: " + arg);
            }
         }
      }
   }
}
