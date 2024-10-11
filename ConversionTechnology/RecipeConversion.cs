using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;

namespace CobbleBuild.ConversionTechnology {
   public class RecipeConversion {
      public static RecipeJson[]? convertToBedrock(JavaRecipe original, string identifier) {
         var output = new List<JavaRecipe>() { original };
         if (original.ingredients != null && original.ingredients.Any(x => x.tag != null)) {
            foreach (var ingredient in original.ingredients) {
               if (ingredient.tag != null) {
                  var resolvedTag = BedrockConversion.resolveItemTag(ingredient.tag);
                  if (resolvedTag.tag != null) {
                     ingredient.tag = resolvedTag.tag;
                  }
                  else if (resolvedTag.blocks != null) {
                     //Multiply the output with all possiblities
                     var newOutput = new List<JavaRecipe>();
                     foreach (var recipe in output) {
                        newOutput.AddRange(resolvedTag.blocks.Select(x => {
                           var newRecipe = recipe.Clone();
                           var newIngredient = newRecipe.ingredients!.Find(y => y.Equals(ingredient))!;
                           newIngredient.tag = null;
                           newIngredient.item = x;
                           return newRecipe;
                        }));
                     }
                     output = newOutput;
                  }
                  else {
                     Misc.warn($"Unknown Tag \"{ingredient.tag}\". Bedrock will likely throw an error for this recipe.");
                  }
               }
            }
         }
         if (original.key != null && original.key.Values.Any(x => x.tag != null)) {
            foreach (var ingredient in original.key.Values) {
               if (ingredient.tag != null) {
                  var resolvedTag = BedrockConversion.resolveItemTag(ingredient.tag);
                  if (resolvedTag.tag != null) {
                     ingredient.tag = resolvedTag.tag;
                  }
                  else if (resolvedTag.blocks != null) {
                     //Multiply the output with all possiblities
                     var newOutput = new List<JavaRecipe>();
                     foreach (var recipe in output) {
                        newOutput.AddRange(resolvedTag.blocks.Select(x => {
                           var newRecipe = recipe.Clone();
                           var newIngredient = newRecipe.key!.Values.ToList().Find(y => y.Equals(ingredient))!;
                           newIngredient.tag = null;
                           newIngredient.item = x;
                           return newRecipe;
                        }));
                     }
                     output = newOutput;
                  }
                  else {
                     Misc.warn($"Unknown Tag \"{ingredient.tag}\". Bedrock will likely throw an error for this recipe.");
                  }
               }
            }
         }
         if (original.ingredient != null && original.ingredient.tag != null) {
            var ingredient = original.ingredient;
            var resolvedTag = BedrockConversion.resolveItemTag(ingredient.tag);
            if (resolvedTag.tag != null) {
               ingredient.tag = resolvedTag.tag;
            }
            else if (resolvedTag.blocks != null) {
               //Multiply the output with all possiblities
               var newOutput = new List<JavaRecipe>();
               foreach (var recipe in output) {
                  newOutput.AddRange(resolvedTag.blocks.Select(x => {
                     var newRecipe = recipe.Clone();
                     var newIngredient = ingredient;
                     newIngredient.tag = null;
                     newIngredient.item = x;
                     return newRecipe;
                  }));
               }
               output = newOutput;
            }
            else {
               Misc.warn($"Unknown Tag \"{ingredient.tag}\". Bedrock will likely throw an error for this recipe.");
            }
         }
         return output.Select((x, i) => _convertToBedrock(x, $"{identifier}_{i}")).Where(x => x != null).ToArray();
      }
      /// <summary>
      /// Dumb Recipe conversion that does not resolve any tags
      /// </summary>
      public static RecipeJson? _convertToBedrock(JavaRecipe original, string identifier) {
         if (original.type == "minecraft:crafting_shaped") {
            foreach (KeyValuePair<string, Item> v in original.key) {
               v.Value.prepareForBedrock();
               original.key[v.Key] = v.Value;
            }
            ShapedRecipe output = new ShapedRecipe(identifier, original.result.getBedrock(), original.pattern, original.key);
            return new RecipeJson(output);
         }
         else if (original.type == "minecraft:crafting_shapeless") {
            foreach (Item i in original.ingredients) {
               original.ingredients[original.ingredients.IndexOf(i)].prepareForBedrock();
            }
            ShapelessRecipe output = new ShapelessRecipe(identifier, original.result.getBedrock(), null, original.ingredients);
            return new RecipeJson(output);
         }
         else if (original.type == "minecraft:smelting") {
            var output = new FurnaceRecipe(identifier, original.ingredient!.getBedrock(), original.result!.getBedrock()) {
               tags = ["furnace"]
            };
            return new RecipeJson(output);
         }
         else if (original.type == "minecraft:campfire_cooking") {
            var output = new FurnaceRecipe(identifier, original.ingredient!.getBedrock(), original.result!.getBedrock()) {
               tags = ["campfire", "soul_campfire"]
            };
            return new RecipeJson(output);
         }
         else if (original.type == "minecraft:smoking") {
            var output = new FurnaceRecipe(identifier, original.ingredient!.getBedrock(), original.result!.getBedrock()) {
               tags = ["smoker"]
            };
            return new RecipeJson(output);
         }
         else if (original.type == "minecraft:blasting") {
            var output = new FurnaceRecipe(identifier, original.ingredient!.getBedrock(), original.result!.getBedrock()) {
               tags = ["blast_furnace"]
            };
            return new RecipeJson(output);
         }
         else if (original.type == "minecraft:stonecutting") {
            var output = new ShapelessRecipe(identifier, original.result!.getBedrock(), ["stonecutter"], original.ingredient!.getBedrock());
            return new RecipeJson(output);
         }
         else {
            Misc.warn($"Could not convert recipe to bedrock: Unsupported type\'{original.type}\'");
            return null;
         }
      }
   }
}
