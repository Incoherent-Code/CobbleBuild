using CobbleBuild.BedrockClasses;

namespace CobbleBuild.CobblemonClasses {
   public class JavaRecipe {
      public string type;
      public List<string>? pattern;
      public Dictionary<string, Item>? key;
      public Item result;
      public List<Item>? ingredients;

      /// <summary>
      /// Used with smelting recipe
      /// </summary>
      public Item? ingredient;

      /// <summary>
      /// Used in stonecutter recipes
      /// </summary>
      public int? count;

      public JavaRecipe Clone() {
         return new JavaRecipe() {
            type = type,
            pattern = pattern?.ToArray().ToList(),
            result = result?.Clone(),
            ingredients = ingredients?.Select(x => x.Clone()).ToList(),
            ingredient = ingredient?.Clone(),
            count = count,
            key = key?.ToDictionary((x) => x.Key, x => x.Value.Clone())
         };
      }
   }
}
