using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using CobbleBuild.JavaClasses;
using static CobbleBuild.BedrockClasses.BedrockConversion;
using static CobbleBuild.BedrockClasses.ItemData;

namespace CobbleBuild.DataCreator {
   public class ItemCreator {
      public static ItemJson Create(PokeballResourceData pokeball) {
         ItemData output = new ItemData(pokeball.pokeball, new Dictionary<string, Component>()
         {
                {"minecraft:icon", new Component.Texuture(pokeball.pokeball.Split(":")[1]) },
                {"minecraft:projectile", new Component.Projectile(pokeball.pokeball) },
                {"minecraft:shooter", new Component.Shooter(pokeball.pokeball) },
                {"minecraft:use_modifiers", new Component.UseModifiers(0.5f) },
                {"minecraft:display_name", new Component.DisplayName($"item.cobblemon.{pokeball.pokeball.Substring(10)}") }
            });
         AddRelevantTags(output);
         return new ItemJson(output);
      }
      /// <summary>
      /// Creates apricorn as well as the sapling item
      /// </summary>
      public static ItemJson Create(string apricornName, out ItemJson saplingPlacer) {
         ItemData output = new ItemData($"cobblemon:{apricornName}", new Dictionary<string, Component>()
         {
                {"minecraft:icon", new Component.Texuture(apricornName) },
                {"minecraft:block_placer", new Component.BlockPlacer($"cobblemon:{apricornName}_block", ["cobblemon:apricorn_leaves"]) },
                {"minecraft:display_name", new Component.DisplayName($"item.cobblemon.{apricornName}") },
                {"minecraft:tags", new Component.Tags(["cobblemon:apricorns"]) }
            });
         output.description.category = "Nature";
         AddRelevantTags(output);
         ItemData sapling = new ItemData($"cobblemon:{apricornName}_sapling", new Dictionary<string, Component>()
         {
                {"minecraft:icon", new Component.Texuture(apricornName + "_seed") },
                {"minecraft:block_placer", new Component.BlockPlacer($"cobblemon:{apricornName}_sapling_block", ["dirt","grass","podzol"]) },
                {"minecraft:display_name", new Component.DisplayName($"item.cobblemon.{apricornName}_seed") }
            });
         sapling.description.category = "Nature";
         AddRelevantTags(output);
         saplingPlacer = new ItemJson(sapling);
         return new ItemJson(output);
      }
      public static ItemJson CreateGeneric(string itemName, string? textureName = null) {
         if (textureName == null) {
            textureName = itemName;
         }
         var identifier = $"cobblemon:{itemName}";
         ItemData output = new ItemData(identifier, new Dictionary<string, Component>()
         {
                {"minecraft:icon", new Component.Texuture(textureName) },
                {"minecraft:display_name", new Component.DisplayName($"item.cobblemon.{itemName}") }
            });
         AddRelevantTags(output);
         return new ItemJson(output);
      }
      public static ItemJson CreateBerry(string berryName, Berry berryData) {
         ItemData output = new ItemData($"cobblemon:{berryName}", new Dictionary<string, Component>()
         {
                {"minecraft:icon", new Component.Texuture(berryName) },
                {"minecraft:display_name", new Component.DisplayName($"item.cobblemon.{berryName}") },
                {"minecraft:tags", new Component.Tags("cobblemon:berries") }
            });
         AddRelevantTags(output);
         return new ItemJson(output);
      }
      /// <summary>
      /// Mutates the item parameter to add all tags that contain the item identifier
      /// </summary>
      private static void AddRelevantTags(ItemData item) {
         var cobblemonItemTags = JavaData.cobblemonData!.itemTags
             .Where(x => x.Value.values.Contains(item.description.identifier))
             .Select(x => "cobblemon:" + x.Key);
         //Only add minecraft item tags that have bedrock equivalents
         var minecraftItemTags = JavaData.minecraftData!.itemTags
             .Where(x => x.Value.values.Contains(item.description.identifier))
             .Select(x => resolveItemTag(x.Key).tag!)
             .Where(x => x != null)
             .Select(x => "minecraft:" + x);
         var itemTagsToAdd = cobblemonItemTags.Union(minecraftItemTags).ToArray();
         if (itemTagsToAdd.Length < 0)
            return;
         if (!item.components!.ContainsKey("minecraft:tags")) {
            item.components["minecraft:tags"] = new Component.Tags(itemTagsToAdd);
            return;
         }
         var currentItems = (item.components["minecraft:tags"] as Component.Tags)?.tags ?? [];
         item.components["minecraft:tags"] = new Component.Tags(currentItems.Union(itemTagsToAdd).ToArray());
      }
   }
}
