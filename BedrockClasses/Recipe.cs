using Newtonsoft.Json;

namespace CobbleBuild.BedrockClasses {
   public class RecipeJson {
      public string format_version = "1.20.30";
      [JsonProperty("minecraft:recipe_shaped")] //Only populate one of these two
      public ShapedRecipe? ShapedRecipe { get; set; }
      [JsonProperty("minecraft:recipe_shapeless")]
      public ShapelessRecipe? ShapelessRecipe { get; set; }
      [JsonProperty("minecraft:recipe_furnace")]
      public FurnaceRecipe? FurnaceRecipe { get; set; }
      public RecipeJson(ShapedRecipe recipe) {
         ShapedRecipe = recipe;

      }
      public RecipeJson(ShapelessRecipe recipe) {
         ShapelessRecipe = recipe;
      }
      public RecipeJson(FurnaceRecipe recipe) {
         FurnaceRecipe = recipe;
      }
      public class Description {
         public string identifier;
         public Description(string identifier) {
            this.identifier = identifier;
         }
      }
      public string getIdentifier() {
         return ShapedRecipe?.description.identifier
             ?? ShapelessRecipe?.description.identifier
             ?? FurnaceRecipe?.description.identifier!;
      }
   }
   public class Recipe {
      public RecipeJson.Description description;
      public List<string>? tags;
      public Recipe(string identifier) {
         description = new RecipeJson.Description(identifier);
      }
      public Recipe() { }
   }
   public class ShapelessRecipe : Recipe {
      public List<Item> ingredients;
      public string? group;
      public List<Item> unlock;
      public Item result;
      public int? priority;

      public ShapelessRecipe(string identifier) : base(identifier) {
      }
      public ShapelessRecipe(string identifier, Item Result, List<string>? tags = null, params Item[] ingredients) : base(identifier) {
         this.result = Result;
         this.ingredients = [.. ingredients];
         this.unlock = [.. ingredients]; //All ingredients lead to unlock by default
         if (tags == null) {
            this.tags = new List<string>() { "crafting_table" };
         }
         else {
            this.tags = tags;
         }
      }
      public ShapelessRecipe(string identifier, Item Result, List<string>? tags, List<Item> ingredients) : base(identifier) {
         this.result = Result;
         this.ingredients = ingredients;
         this.unlock = ingredients; //All ingredients lead to unlock by default
         if (tags == null) {
            this.tags = new List<string>() { "crafting_table" };
         }
         else {
            this.tags = tags;
         }
      }
      public ShapelessRecipe() : base() { }
   }
   public class ShapedRecipe : Recipe {
      public List<string> pattern;
      public Dictionary<string, Item> key;
      public string? group;
      public List<Item> unlock;
      public Item result;

      public ShapedRecipe(string identifier) : base(identifier) {
      }
      public ShapedRecipe(string identifier, Item result, List<string> pattern, Dictionary<string, Item> key, List<string>? tags = null) : base(identifier) {
         if (tags == null) {
            this.tags = new List<string>() { "crafting_table" };
         }
         else {
            this.tags = tags;
         }
         this.unlock = new List<Item>();
         foreach (KeyValuePair<string, Item> p in key) {
            unlock.Add(p.Value);
         }
         this.result = result;
         this.pattern = pattern;
         this.key = key;
      }
      public ShapedRecipe() : base() { }
   }

   public class FurnaceRecipe : Recipe {
      public Item input;
      public Item output;
      public FurnaceRecipe(string identifier) : base(identifier) { }
      public FurnaceRecipe(string identifier, Item input, Item output) : base(identifier) {
         this.input = input;
         this.output = output;
      }
   }
}
