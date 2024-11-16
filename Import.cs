using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using CobbleBuild.JavaClasses;
using Newtonsoft.Json;

namespace CobbleBuild {
   public static class Import {
      public enum TextureType {
         Entity,
         Block,
         Item
      }
      [Obsolete]
      public static List<string> ImportJavaModels = new List<string>()
      {
            "assets/cobblemon/models/block/black_apricorn.json",
            "assets/cobblemon/models/block/apricorn_stage_0.json",
            "assets/cobblemon/models/block/apricorn_stage_1.json",
            "assets/cobblemon/models/block/apricorn_stage_2.json",
            "assets/cobblemon/models/block/pc.json",
            "assets/cobblemon/models/block/pc_top.json",
            "assets/cobblemon/models/block/pc_top_on.json",
            //"assets/cobblemon/models/block/pasture_bottom.json",
            //"assets/cobblemon/models/block/pasture_top_off.json",
            //"assets/cobblemon/models/block/pasture_top_on.json"
        };
      /// <summary>
      /// Imports a model. The model is processed before being copied to the destination.
      /// </summary>
      /// <param name="geoName">Name the geometry should take (not including geometry. )</param>
      /// <param name="ImportPath">Path to the file INCLUDING FILENAME</param>
      /// <param name="OutputPath">Path to where the model should output INCLUDING FILENAME</param>
      /// <returns></returns>
      public static async Task ImportModel(string geoName, string ImportPath, string OutputPath) //Geo name does not include geometry.
      {
         GeometryJson deserializedModel = await Misc.LoadFromJsonAsync<GeometryJson>(ImportPath);
         deserializedModel.geometry[0].description.identifier = "geometry." + geoName; //You have no idea how many models are incorrectly identified
         PostProcessor.PostProcess(ref deserializedModel);
         await Misc.SaveToJsonAsync(deserializedModel, OutputPath, false);
      }

      //Convert Models from Java to Bedrock before Saving them
      public static async Task ImportJavaModel(string ImportPath, string OutputPath) {
         JavaModel deserializedModel = await Misc.LoadFromJsonAsync<JavaModel>(ImportPath);
         var geometry = ConversionTechnology.BlockModelConversion.convertToBedrock(deserializedModel, Path.GetFileNameWithoutExtension(ImportPath));
         await Misc.SaveToJsonAsync(geometry, OutputPath);
      }
      /// <summary>
      /// Returns the animationJson from the specified filepath.
      /// </summary>
      public static async Task<AnimationJson> ReadAnimation(string ImportFile) {
         AnimationJson animationJSON = await Misc.LoadFromJsonAsync<AnimationJson>(ImportFile); //Parsing just gets rid of junk data like gecko_lib_version that bedrock complains about
         PostProcessor.PostProcess(ref animationJSON);
         return animationJSON;
      }
      public static async Task ImportTexture(string InternalLocation, string ImportPath, string OutputPath, TextureType type) // Internal location is location inside resource pack without file extension
      {
         Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!); //Ensures Directory Exists
         await Misc.CopyAsync(ImportPath, OutputPath);
         if (type == TextureType.Entity) {
            Program.EntityTextures.Add(InternalLocation);
         }
         else if (type == TextureType.Item) {
            Program.itemAtlasJson.texture_data.TryAdd(Path.GetFileNameWithoutExtension(ImportPath), new Atlas(InternalLocation));
         }
         else if (type == TextureType.Block) {
            Program.blockAtlasJson.texture_data.TryAdd(Path.GetFileNameWithoutExtension(ImportPath), new Atlas(InternalLocation));
         }
      }
      public static async Task ImportTexture(string ImportPath, string OutputPath, TextureType type) {
         await ImportTexture(Misc.getInternalLocation(OutputPath), ImportPath, OutputPath, type);
      }
      /// <summary>
      /// Imports a recipe (May save multiple recipes per recipe because making equivalent tags is hard.)
      /// </summary>
      /// <param name="ImportPath">Path to the original recipe</param>
      /// <param name="identifier">Identifier for the recipe</param>
      /// <param name="OutputPath">Folder path to output the file to</param>
      public static async Task ImportRecipe(string ImportPath, string identifier, string OutputPath) {

         string modelData = await File.ReadAllTextAsync(ImportPath);
         JavaRecipe deserializedModel = JsonConvert.DeserializeObject<JavaRecipe>(modelData)!;
         RecipeJson[]? recipes = ConversionTechnology.RecipeConversion.convertToBedrock(deserializedModel, identifier);
         if (recipes == null)
            return;
         await Task.WhenAll(recipes.Select(recipe =>
            Misc.SaveToJsonAsync(recipe, Path.Combine(OutputPath, $"{recipe.getIdentifier().Split(":").Last()}.recipe.json"))
         ));

      }
      public static async Task ImportAllRecipesInFolder(string folderPath, string outputPath) {
         if (!Directory.Exists(folderPath)) {
            throw new FileNotFoundException($"Folder {folderPath} was not found.");
         }
         var tasks = new List<Task>();
         foreach (var file in Misc.getAllFilesInDirandSubDirs(folderPath)) {
            tasks.Add(ImportRecipe(file, "cobblemon:" + Path.GetFileNameWithoutExtension(file), outputPath));
         }
         await Task.WhenAll(tasks);
      }
      /// <summary>
      /// Takes in a text file and replaces all occurences of param original and replaces it with param replace.
      /// </summary>
      /// <param name="TemplatePath">Path to template file.</param>
      /// <param name="original">String to replace</param>
      /// <param name="replace">String to replace with</param>
      /// <param name="outputPath">Output path of the final text file.</param>
      public static async Task ImportUsingTemplate(string TemplatePath, string original, string replace, string outputPath) //Takes a text template file and relaces all occurences of "original" with "replace" and saves it
      {
         string templateData = await File.ReadAllTextAsync(TemplatePath);
         templateData = templateData.Replace(original, replace);
         await File.WriteAllTextAsync(outputPath, templateData);
      }

      private static readonly byte defaultAlphaValue = 110;
      //This system of having 1 static alpha value needs to be improved
      public static async Task ImportTexturesInLayersAsUV(List<ResolverVariation.Layer> layers, string variationName, Pokemon pokemon) {
         foreach (ResolverVariation.Layer layer in layers) {
            if (layer.texture.texture != null) {
               string texturePartialPath = layer.texture.texture.Remove(0, 10);
               string importPath = Path.Combine(Config.config.resourcesPath, "assets/cobblemon/", texturePartialPath);
               string outputPath = Path.Combine(Config.config.resourcePath, texturePartialPath);
               await ImportTexture(texturePartialPath.Remove(texturePartialPath.Length - 4), importPath, outputPath, TextureType.Entity);
               //Overwrite the file with the new data
               if (layer.emissive == true) {
                  var newTexture = ImageProcessor.setAlphaValue(SkiaSharp.SKBitmap.Decode(importPath), defaultAlphaValue);
                  var encoding = newTexture.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                  using (var fileStream = File.OpenWrite(outputPath))
                     encoding.SaveTo(fileStream);
               }
            }
            else //If Frame Animation
            {
               List<string> files = new List<string>();
               foreach (string s in layer.texture.frames!) {
                  string texturePartialPath = s.Remove(0, 10);
                  files.Add(Path.Combine(Config.config.resourcesPath, "assets/cobblemon/", texturePartialPath));
               }
               var uv = ImageProcessor.CreateVerticalUV([.. files]);
               if (layer.emissive == true)
                  uv = ImageProcessor.setAlphaValue(uv, defaultAlphaValue);
               string uvPartialPath = Path.Combine("textures/pokemon", pokemon.folder_name, $"{variationName}_{layer.name}_uv").Replace("\\", "/");
               Program.EntityTextures.Add(uvPartialPath);
               var finalFilePath = Path.Combine(Config.config.resourcePath, uvPartialPath + ".png");
               using (var fileSteam = File.Create(finalFilePath))
                  uv.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).SaveTo(fileSteam);
            }
         }
      }
      public static async Task ImportAllTexturesFromFolder(string ImportPath, string OutputPath, TextureType type) {
         var tasks = new List<Task>();
         foreach (string file in Misc.getAllFilesInDirandSubDirs(ImportPath)) {
            if (!file.EndsWith(".png"))
               continue;
            string internalLocation = Misc.getInternalLocation(file);
            tasks.Add(ImportTexture(internalLocation, file, Path.Combine(OutputPath, Misc.getPathFrom(file, ImportPath.Replace("\\", "/").Split("/").Last(), false)), type));
         }
         await Task.WhenAll(tasks);
      }
      /// <summary>
      /// Uses the Sound Definitions already converted to bedrock to copy the sounds.
      /// NOTE: Mutates the original json to remove sounds less than 125 ms (way of removing empty sounds)
      /// </summary>
      public static async Task ImportAllSoundsFromSoundDef(SoundDefinitionJson json) {
         var soundTasks = new List<Task>();
         foreach (var soundDef in json.sound_definitions.Values) {
            foreach (var sound in (List<SoundDefinition.Sound>)[.. soundDef.sounds]) {
               soundTasks.Add(Task.Run(async () => {
                  var ogPath = Path.Combine(Config.config.resourcesPath, "assets/cobblemon", sound.name + ".ogg");
                  var newPath = Path.Combine(Config.config.resourcePath, sound.name + ".ogg");

                  if (!File.Exists(ogPath)) {
                     soundDef.sounds.Remove(sound);
                     return;
                  }

                  //Filters out unnecessary dummy sounds from cobblemon.
                  var soundReader = new NVorbis.VorbisReader(ogPath);
                  if (soundReader.TotalTime.Milliseconds < 125) {
                     soundDef.sounds.Remove(sound);
                     return;
                  }

                  Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
                  await Misc.CopyAsync(ogPath, newPath);
               }));
            }
         }
         await Task.WhenAll(soundTasks);
      }
   }
}
