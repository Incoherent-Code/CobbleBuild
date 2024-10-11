using static CobbleBuild.BedrockClasses.BedrockConversion;
using static CobbleBuild.ConversionTechnology.SpawnConversion;

namespace CobbleBuild.JavaClasses {
   /// <summary>
   /// Class for storing tag data and other extracted resources from minecraft.
   /// </summary>
   public class JavaData {
      /// <summary>
      /// Data that would be under the minecraft: namespace
      /// </summary>
      public static JavaData? minecraftData;
      /// <summary>
      /// Data that would be under the cobblemon: namespace
      /// </summary>
      public static JavaData? cobblemonData;
      public bool initialized = false;

      public Dictionary<string, TagData> biomeTags = new Dictionary<string, TagData>();
      public Dictionary<string, TagData> blockTags = new Dictionary<string, TagData>();
      public Dictionary<string, TagData> itemTags = new Dictionary<string, TagData>();

      public JavaData() { }
      /// <summary>
      /// Loads data from minecraft extracted jar to minecraftData
      /// </summary>
      /// <param name="FolderPath">extracted jar folder /data/minecraft/ or the same folder in the mod jar file</param>
      public static void loadMinecraftData(string FolderPath) {
         if (minecraftData == null)
            minecraftData = new JavaData();
         minecraftData.loadData(FolderPath);
      }
      /// <summary>
      /// Same as loadMinecraftData(); Only loads data types in the base game, not any cobblemon specific data like presets
      /// </summary>
      public static void loadCobblemonData(string FolderPath) {
         if (cobblemonData == null)
            cobblemonData = new JavaData();
         cobblemonData.loadData(FolderPath);
      }
      /// <summary>
      /// Loads data from minecraft extracted jar. Can also be used to load more data, like from the cobblemon mod
      /// </summary>
      /// <param name="FolderPath">extracted jar folder /data/minecraft/ or the same folder in the mod jar file</param>
      /// <exception cref="DirectoryNotFoundException"></exception>
      public void loadData(string FolderPath) {
         if (!Directory.Exists(FolderPath))
            throw new DirectoryNotFoundException($"The directory {FolderPath} could not be found.");

         //Loading biome tags
         string biomeTagsPath = Path.Combine(FolderPath, "tags", "worldgen", "biome");
         if (Directory.Exists(biomeTagsPath)) {
            saveTagData(biomeTagsPath, ref biomeTags);
         }

         //Block tag resolution
         string blockTagsPath = Path.Combine(FolderPath, "tags", "blocks");
         if (Directory.Exists(blockTagsPath)) {
            saveTagData(blockTagsPath, ref blockTags);
         }

         //Item tag resolution
         string itemTagsPath = Path.Combine(FolderPath, "tags", "items");
         if (Directory.Exists(itemTagsPath)) {
            saveTagData(itemTagsPath, ref itemTags);
         }

         this.initialized = true;
      }
      /// <summary>
      /// Loads all of the Data from Java Minecraft and pertinent info from cobblemon using the config paths
      /// </summary>
      public static void populateData() {
         loadMinecraftData(Path.Combine(Config.config.minecraftJavaPath, "data", "minecraft"));
         loadMinecraftData(Path.Combine(Config.config.resourcesPath, "data", "minecraft"));
         loadCobblemonData(Path.Combine(Config.config.resourcesPath, "data", "cobblemon"));
         deconstructItemTags(ref JavaData.minecraftData!.itemTags);
         deconstructItemTags(ref JavaData.cobblemonData!.itemTags);
      }

      private static void throwIfUninitalized() {
         if (minecraftData == null || minecraftData.initialized == false)
            throw new Exception("Couldn't Resolve Biome: Minecraft's Data has not been initialized.");
         if (cobblemonData == null || cobblemonData.initialized == false)
            throw new Exception("Couldn't Resolve Biome: Cobblemon's Data has not been initialized.");
      }

      private void saveTagData(string rootFolder, ref Dictionary<string, TagData> saveTo) {
         var tagJsons = Misc.getAllFilesInDirandSubDirs(rootFolder);
         foreach (var tagJson in tagJsons) {
            if (Path.GetExtension(tagJson) != ".json")
               continue;

            try {
               string name = Path.GetFileNameWithoutExtension(tagJson);
               var data = Misc.LoadFromJson<TagData>(tagJson);
               if (saveTo.ContainsKey(name)) {
                  if (data.replace == true)
                     saveTo[name] = data;
                  else {
                     //Concatinate without duplicates
                     saveTo[name].values = saveTo[name].values.Union(data.values).ToList();
                  }
               }
               else {
                  saveTo.Add(name, data);
               }
            }
            catch (Exception ex) {
               Misc.softError($"The Biome Tag file {Path.GetFileName(tagJson)} could not be read from: {ex.Message}");
            }
         }
      }
      /// <summary>
      /// Goes through all the item tags and resolves any tag refrences inside it.
      /// </summary>
      private static void deconstructItemTags(ref Dictionary<string, TagData> tags) {
         foreach (var item in tags) {
            foreach (var tag in item.Value.values.Select(x => x as string).Where(x => x != null).ToList()) {
               if (tag!.StartsWith('#')) {
                  var newItems = ResolveItemTag(tag);
                  if (newItems != null) {
                     item.Value.values.Remove(tag);
                     item.Value.values = item.Value.values.Union(newItems).ToList();
                  }
               }
            }
         }
      }
      /// <summary>
      /// Resolves a biome to a list of biomes
      /// </summary>
      /// <param name="biome">biome / biome tag with namespace, like minecraft:plains </param>
      /// <returns>List of List of biome tags. Each list in the list are the required biome tags for each biome.</returns>
      /// <remarks>May return an empty list</remarks>
      /// <exception cref="Exception">Data Uninitialized</exception>
      public static List<string[]> resolveBiome(string biome) {
         throwIfUninitalized();
         string biomeName;
         if (!Misc.tryRemoveNamespace(biome, out biomeName)) {
            Misc.warn($"Couldn't resolve biome {biome}.");
            return [];
         }

         //If just a biome id
         if (!biome.StartsWith("#")) {
            if (!biome.StartsWith("minecraft:"))
               return []; //Skip non vanilla biomes

            if (biomeDictionary.ContainsKey(biomeName))
               return [biomeDictionary[biomeName]];
            else {
               Misc.warn($"Biome {biomeName} is not contained in the dictionary.");
               return [];
            }
         }
         JavaData data;
         if (biome.StartsWith("#minecraft:")) {
            data = minecraftData;
         }
         else if (biome.StartsWith("#cobblemon:")) {
            data = cobblemonData;
         }
         else {
            //Filter out namespaces that aren't minecraft or cobblemon
            return [];
         }

         if (!data.biomeTags.ContainsKey(biomeName)) {
            Misc.warn($"The biome tag {biome} could not be identified.");
            return [];
         }

         List<string[]> output = [];
         foreach (var item in data.biomeTags[biomeName].values) {
            if (item.GetType() == typeof(string)) {
               var resolved = resolveBiome((string)item);
               if (resolved.Count > 0)
                  output = output.Union(resolved).ToList();
            }
         }

         return output;
      }
      /// <summary>
      /// Resolve Block / Block tags into a list of valid blocks from the tag
      /// </summary>
      /// <param name="block">Block identifier or block tag starting with a #</param>
      /// <returns>List of valid blocks infered from the passed in id</returns>
      /// <exception cref="Exception">Uninitalized data</exception>
      public static List<string> resolveBlock(string block) {
         throwIfUninitalized();

         //Wierd tags I could not find a definition for
         switch (block) {
            case "#minecraft:dirt_like":
               block = "#minecraft:dirt";
               break;
            case "#minecraft:gravel":
            case "#minecraft:grass":
            case "#minecraft:water":
            case "#minecraft:lava":
               block = block.Substring(1);
               break;

         }

         string blockName;
         if (!Misc.tryRemoveNamespace(block, out blockName)) {
            Misc.warn($"Couldn't resolve block {block}.");
            return [];
         }

         //Normal block id
         if (!block.StartsWith("#")) {
            if (!block.StartsWith("minecraft:") || !block.StartsWith("cobblemon:"))
               return [];

            if (JavaToBedrockItemNames.ContainsKey(block)) {
               return [JavaToBedrockItemNames[block]];
            }
            else {
               return [block];
            }
         }

         //Parsing Tags
         JavaData data;
         if (block.StartsWith("#minecraft:")) {
            data = minecraftData;
         }
         else if (block.StartsWith("#cobblemon:")) {
            data = cobblemonData;
         }
         else {
            //Filter out namespaces that aren't minecraft or cobblemon
            return [];
         }
         List<string> output = [];

         if (!data.blockTags.ContainsKey(blockName)) {
            Misc.warn($"The block tag {block} could not be identified.");
            return [];
         }

         foreach (var item in data.blockTags[blockName].values) {
            if (item.GetType() == typeof(string)) {
               var resolved = resolveBlock((string)item);
               if (resolved.Count > 0)
                  output = output.Union(resolved).ToList();
            }
         }
         return output;
      }
      /// <summary>
      /// Returns the items that should be part of the item tag, or null if the tag doesn't exist.
      /// </summary>
      /// <param name="itemTag"> May or may not include the hashtag </param>
      private static string[]? ResolveItemTag(string itemTag) {
         if (itemTag.StartsWith("#"))
            itemTag = itemTag.Substring(1);
         var mcData = (itemTag.StartsWith("minecraft:")) ? JavaData.minecraftData
            : (itemTag.StartsWith("cobblemon:")) ? JavaData.cobblemonData : null;
         if (mcData == null)
            return null;
         if (!Misc.tryRemoveNamespace(itemTag, out var itemTagNoNamespace))
            return null;
         if (mcData.itemTags.TryGetValue(itemTagNoNamespace, out var tagEntries)) {
            var items = tagEntries.values.Select(x => x as string).Where(x => x != null).ToList();
            string[] immutableItems = [.. items];
            foreach (var item in immutableItems) {
               if (item.StartsWith('#')) {
                  var newItems = ResolveItemTag(item);
                  if (newItems != null) {
                     items.Remove(item);
                     items.Union(newItems);
                  }
               }
            }
            return items.ToArray();
         }
         return null;
      }
   }
}
