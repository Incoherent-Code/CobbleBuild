using CobbleBuild.CobblemonClasses;
using System.Diagnostics.CodeAnalysis;
using static CobbleBuild.CobblemonClasses.ResolverVariation;
using static CobbleBuild.Config;

namespace CobbleBuild {
   /// <summary>
   /// Intermediary between ResolverVariation and Data needed for creating entity.
   /// Basically ResolverVariation but after data has already been parsed.
   /// </summary>
   public class Variation {
      public string? texturePartialPath;
      public string? geometryName;
      public List<Layer>? layerData;
      public string variantName;
      public List<string> aspects;
      public string? poserIdentifier;

      public Variation(List<string> aspects) {
         this.aspects = aspects;
         variantName = CreateName(aspects);
      }
      public Variation mergeWithParent(Variation parent) {
         this.texturePartialPath ??= parent.texturePartialPath;
         this.geometryName ??= parent.geometryName;
         this.layerData = MergeLayerData(this.layerData, parent.layerData);
         this.poserIdentifier ??= parent.poserIdentifier;
         return this;
      }
      public Variation mergeWithoutLayerMerge(Variation parent) {
         this.texturePartialPath ??= parent.texturePartialPath;
         this.geometryName ??= parent.geometryName;
         //this.layerData = MergeLayerData(this.layerData, parent.layerData);
         this.poserIdentifier ??= parent.poserIdentifier;
         return this;
      }

      public static Variation getFromResolverVariation(ResolverVariation resolver, Pokemon pokemon) {
         var output = new Variation(resolver.aspects!);
         if (resolver.texture != null) {
            string texturePartialPath = resolver.texture.Substring(10);
            Import.ImportTexture(texturePartialPath.Remove(texturePartialPath.Length - 4), Path.Combine(config.resourcesPath, "assets/cobblemon/", texturePartialPath), Path.Combine(config.resourcePath, texturePartialPath), Import.TextureType.Entity);
            output.texturePartialPath = texturePartialPath.Remove(texturePartialPath.Length - 4);
         }
         if (resolver.model != null) {
            string modelName = resolver.model.Remove(0, 10);
            Import.ImportModel(modelName, Path.Combine(config.resourcesPath, "assets/cobblemon/bedrock/pokemon/models/", pokemon.folder_name, $"{modelName}.json"), Path.Combine(config.resourcePath, "models/entity/cobblemon/", $"{modelName}.json"));
            output.geometryName = modelName;
         }
         if (resolver.layers != null) {
            Import.ImportTexturesInLayersAsUV(resolver.layers, output.variantName, pokemon);
            output.layerData = resolver.layers;
         }
         output.poserIdentifier = resolver.poser;
         return output;
      }

      public static string CreateName(List<string> strings) {
         string output = string.Empty;
         if (strings.Count > 0) {
            output = strings[0].Replace("-", "_"); //Certain Fields in Bedrock do not like - (molang sees it as a minus)
         }
         for (int i = 1; i < strings.Count; i++) {
            output = output + "_" + strings[i].Replace("-", "_");
         }
         if (output != string.Empty) {
            return output;
         }
         else {
            return "base";
         }
      }
      public static List<Layer> MergeLayerData(List<Layer>? old, List<Layer>? @new) {
         if (old == null)
            return @new ?? new List<Layer>();
         if (@new == null)
            return old ?? new List<Layer>();

         //Prevents collection modification errors when the same array is passed in twice
         var @new2 = @new.ToArray();

         foreach (var layer in @new2) {
            var oldLayerIndex = old.FindIndex(x => x.name == layer.name);
            if (oldLayerIndex != -1) {
               old[oldLayerIndex] = layer;
            }
            else {
               old.Add(layer);
            }
         }
         return old;
      }
      public static float calcWeight(List<string> aspects, Pokemon pokemon, float baseWeight = 4096) {
         float output = baseWeight;
         foreach (string aspect in aspects) {
            if (aspect == "shiny") {
               output = output / baseWeight; //Odds are 1 and baseweight
            }
            if (aspect == "female") {
               output = output * (1 - pokemon.data.maleRatio); //This isn't correct but ill fix later
            }
            if (aspect == "male") {
               output = output * pokemon.data.maleRatio;
            }
         }
         return output;
      }

      public static bool hasTextureAnimation(Pokemon pokemon) {
         foreach (Variation v in pokemon.Variations) {
            foreach (Layer l in v.layerData) {
               if (l.texture.texture == null) {
                  return true;
               }
            }
         }
         return false;
      }

      public class NameComparer : IEqualityComparer<Variation> {
         public bool Equals(Variation? x, Variation? y) {
            return x.variantName == y.variantName;
         }

         public int GetHashCode([DisallowNull] Variation obj) {
            return obj.variantName.GetHashCode();
         }
      }
   }
}
