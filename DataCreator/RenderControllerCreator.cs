using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;

namespace CobbleBuild.DataCreator {
   public static class RenderControllerCreator {
      public static RenderControllerJson Create(Pokemon pokemon, ref ClientEntityJson entity) {
         RenderControllerJson outputJSON = new RenderControllerJson();

         //Creates Base render controller
         RenderController output = new RenderController() {
            geometry = "array.cobblemon_geometry[query.variant]",
            textures = ["array.cobblemon_texture[query.variant]"],
            arrays = new RenderController.ArraysField() {
               textures = new Dictionary<string, List<string>>() { { "array.cobblemon_texture", [] } },
               geometries = new Dictionary<string, List<string>>() { { "array.cobblemon_geometry", [] } }
            },
            materials = [new StringOrPropertyAndString("*", "material.default")]
         };
         //Adds base textures
         foreach (var variation in pokemon.Variations) {
            output.arrays.textures["array.cobblemon_texture"].Add($"texture.{variation.variantName}");
            output.arrays.geometries["array.cobblemon_geometry"].Add($"geometry.{variation.variantName}");
         }

         var variationArray = pokemon.Variations;

         //Creates part visibility for Poses.
         //Finds all unique posers in variations
         var uniquePosers = variationArray
             .Select(x => x.poserIdentifier)
             .Select(x => x.StartsWith("cobblemon:") ? x.Substring(10) : x)
             .Distinct()
             .ToList();
         foreach (var poserIdentifier in uniquePosers) {
            if (!PoserRegistry.posers.TryGetValue(poserIdentifier, out var poser))
               continue;
            var posesWithInvisibleParts = poser.poses
                .Where(x => x.Value?.transformedParts.Any(x => !x.Value.visible) == true);
            if (posesWithInvisibleParts.Count() > 0 && output.part_visibility == null)
               output.part_visibility = [];

            var poseKeyArray = poser.poses.Keys.ToList();
            var poserVar = $"v.state_of_{poserIdentifier.ToLower()}";
            foreach (var pose in posesWithInvisibleParts) {
               var parts = pose.Value.transformedParts.Where(x => !x.Value.visible);
               foreach (var part in parts) {
                  var partName = poser.registeredBodyParts[part.Key] ?? part.Key;
                  var partIndex = output.part_visibility.FindIndex(x => x.String == partName);
                  if (partIndex == -1) {
                     output.part_visibility.Add(new StringOrPropertyAndString(partName, $"!({poserVar} == {poseKeyArray.FindIndex(x => x == pose.Key)})"));
                  }
                  else {
                     output.part_visibility[partIndex].value += $" && !({poserVar} == {poseKeyArray.FindIndex(x => x == pose.Key)})";
                  }
               }
            }
         }

         outputJSON.render_controllers[$"controller.render.cobblemon.{pokemon.shortName}"] = output;

         //Creates individual layer render controllers.
         for (int i = 0; i < variationArray.Count; i++) {
            var variation = variationArray[i];

            if (variation.layerData != null) {
               for (int j = 0; j < variation.layerData.Count; j++) {
                  var layer = variation.layerData[j];
                  string layerControllerName = $"controller.render.cobblemon.{pokemon.shortName}_{layer.name}";
                  RenderController renderController;
                  if (!outputJSON.render_controllers.ContainsKey(layerControllerName)) {
                     renderController = getNewLayerRenderController(output);
                     entity.client_entity.description.render_controllers.Add(layerControllerName);
                  }
                  else {
                     renderController = outputJSON.render_controllers[layerControllerName];
                  }
                  //The previous variation not having any layer data is a valid edge case that I need to address (TODO later)

                  //If the render controller for previous layer in the previous variation are not compatible with this layer variation
                  if (i > 0 && variationArray[i - 1].layerData.Where(x => layer.name == x.name).Any(x => !layer.isIntercompatibleWith(x))) {
                     string newControllerName = layerControllerName + "_" + i;
                     //Prepare the old render controller by adding blank entries for the next variant
                     renderController.arrays.geometries["array.cobblemon_geometry"].FillUpTo(variationArray.Count, renderController.arrays.geometries["array.cobblemon_geometry"].First());
                     foreach (var array in renderController.arrays.textures) {
                        renderController.arrays.textures[array.Key].FillUpTo(variationArray.Count, "texture.blank");
                     }
                     outputJSON.render_controllers[newControllerName] = renderController.Clone();
                     entity.client_entity.description.render_controllers.Add(newControllerName);

                     var newRenderController = getNewLayerRenderController(output);
                     renderController = newRenderController;
                  }

                  renderController.part_visibility = output.part_visibility;
                  renderController.materials = [new StringOrPropertyAndString("*", "material." + layer.getProperMaterial())];

                  string arrayName = $"array.variation_{layer.name}";
                  if (layer.texture.texture == null) //If layer is animated
                  {
                     renderController.uv_anim = new RenderController.UVAniationField((int)layer.texture.fps, layer.texture.frames.Count);
                     string uvPartial = "textures/pokemon/" + pokemon.folder_name + $"/{variation.variantName}_{layer.name}_uv";
                     entity.client_entity.description.textures.TryAdd($"{variation.variantName}_{layer.name}_uv", uvPartial);
                     if (!renderController.arrays.textures.ContainsKey(arrayName)) {
                        renderController.arrays.textures.Add(arrayName, (new List<string>()).FillUpTo(i, "texture.blank"));
                        //renderController.textures.Add($"{arrayName}[query.variant - {pokemon.Variations.Values.ToList().IndexOf(v)}]");
                        renderController.textures.Add($"{arrayName}[query.variant]");
                     }
                     renderController.arrays.textures[arrayName].Add("texture." + $"{variation.variantName}_{layer.name}_uv");
                     //renderController.arrays.geometries["array.cobblemon_geometry"].Add("geometry." + variation.variantName);
                  }
                  else {
                     string textruePartialPath = layer.texture.texture.Remove(0, 10);
                     entity.client_entity.description.textures.TryAdd($"{variation.variantName}_{layer.name}", textruePartialPath.Remove(textruePartialPath.Length - 4));
                     string uvPartial = layer.texture.texture.Remove(0, 10);
                     entity.client_entity.description.textures.TryAdd($"{variation.variantName}_{layer.name}", uvPartial);
                     if (!renderController.arrays.textures.ContainsKey(arrayName)) {
                        renderController.arrays.textures.Add(arrayName, (new List<string>()).FillUpTo(i, "texture.blank"));
                        //renderController.textures.Add($"{arrayName}[query.variant - {pokemon.Variations.Values.ToList().IndexOf(v)}]");
                        renderController.textures.Add($"{arrayName}[query.variant]");
                     }
                     renderController.arrays.textures[arrayName].Add("texture." + $"{variation.variantName}_{layer.name}");
                     //renderController.arrays.geometries["array.cobblemon_geometry"].Add("geometry." + variation.variantName);
                  }
                  outputJSON.render_controllers[layerControllerName] = renderController;
               }
            }
         }
         return outputJSON;
      }
      /// <summary>
      /// Creates a new Render controller for layers
      /// Makes sure that the geometry is same as base controller.
      /// </summary>
      /// <param name="base">Base render controller for pokemon</param>
      /// <returns></returns>
      public static RenderController getNewLayerRenderController(RenderController @base) {
         return new RenderController() {
            materials = new List<StringOrPropertyAndString>() { new StringOrPropertyAndString("*", "material.animated") },
            arrays = new RenderController.ArraysField() {
               geometries = @base.arrays.geometries,
               textures = new Dictionary<string, List<string>>()
            },
            textures = new List<string>(),
            geometry = @base.geometry
         };
      }
   }
}
