using CobbleBuild.JavaClasses;

namespace CobbleBuild.BedrockClasses {
   public static class BedrockConversion {
      /// <summary>
      /// I didn't find a comprehensive list of differences, so these are just the ones I noticed
      /// Alot of these are that blocks in java are just the same block but with a different block data id
      /// ex: cut_sandstone is just sandstone with a different block data id
      /// </summary>
      public static Dictionary<string, string> JavaToBedrockItemNames = new Dictionary<string, string>()
      {
            {"minecraft:lily_pad", "minecraft:waterlily" },
            {"minecraft:nether_quartz", "minecraft:quartz" },
            //Different Saplings use data vales (except cherry sapling and mangrove)
            //Minecraft parity update fixed this
            //{"minecraft:dark_oak_sapling", "minecraft:sapling" }, 
            //{"minecraft:oak_sapling", "minecraft:sapling" },
            //{"minecraft:birch_sapling", "minecraft:sapling" },
            //{"minecraft:spruce_sapling", "minecraft:sapling" },
            //{"minecraft:jungle_sapling", "minecraft:sapling" },
            //{"minecraft:acacia_sapling", "minecraft:sapling" },
            {"minecrft:vines", "minecraft:vine" },
            {"minecraft:snow", "minecraft:snow_layer" },
            {"minecraft:snow_block", "minecraft:snow" },
            {"minecraft:double_smooth_stone_slab", "minecraft:double_stone_block_slab" }, //Represented by data
            {"minecraft:chiseled_sandstone", "minecraft:sandstone" },
            {"minecraft:cut_sandstone", "minecraft:sandstone" },
            {"minecraft:light_gray_glazed_terracotta", "minecraft:silver_glazed_terracotta" }, //Why is it called this?
            {"minecraft:terracotta", "minecraft:hardened_clay" },
            {"minecraft:bricks", "minecraft:brick_block" },
            {"minecraft:cobblestone_stairs", "minecraft:stone_stairs" },
            {"minecraft:stone_stairs", "minecraft:normal_stone_stairs" },
            {"minecraft:stone_bricks", "minecraft:stonebrick" },
            {"minecraft:mossy_cobblestone_slab", "minecraft:stone_block_slab4" }, //Represented by data
            {"minecraft:mossy_cobblestone_wall", "minecraft:cobblestone_wall" },
            {"minecraft:mossy_stone_bricks", "minecraft:stonebrick" },
            {"minecraft:chiseled_stone_bricks", "minecraft:stonebrick" },
            {"minecraft:cracked_stone_bricks", "minecraft:stonebrick" },
            {"minecraft:netherack", "minecraft:netherrack" },
            {"minecraft:cobweb", "minecraft:web" },
            {"minecraft:powered_rail", "minecraft:golden_rail" },
            {"minecraft:light_grey_carpet", "minecraft:light_gray_carpet" },
            {"minecraft:infested_cracked_stone_bricks", "minecraft:monster_egg" },
            {"minecraft:red_sand", "minecraft:sand" }, //Represented by data
            {"minecraft:coarse_dirt", "minecraft:dirt" }, //Represented by data
            {"minecraft:rooted_dirt", "minecraft:dirt_with_roots" },
            {"minecraft:dandelion", "minecraft:yellow_flower" },
            {"minecraft:flowering_azalea_leaves", "minecraft:azalea_leaves_flowered" },
            {"minecraft:magma_block", "minecraft:magma" }
            //{"minecraft:sugar_cane", "minecraft:reeds" }//BLOCK ONLY NOT ITEM
        };
      /// <summary>
      /// Replaces any java specific item id with its bedrock equivalent
      /// Note: any invalid block will be returned as is, unless it uses an identifier that is not cobblemon: or minecraft. This means block tags will not be handled.
      /// </summary>
      /// <param name="item">item id</param>
      /// <returns>bedrock item id or null if invalid namespace</returns>
      public static string? getBedrockItem(string item) {
         string @namespace = item.Split(":")[0];
         if (!(@namespace == "cobblemon" || @namespace == "minecraft"))
            return null;
         if (JavaToBedrockItemNames.ContainsKey(item)) {
            return JavaToBedrockItemNames[item];
         }
         return item;
      }
      /// <summary>
      /// Java to bedrock item tags
      /// Even ones that are the same should be included here to be sure that the tag is in minecraft bedrock
      /// </summary>
      public static Dictionary<string, string> JavaToBedrockItemTags = new Dictionary<string, string>() {
            {"minecraft:armors", "minecraft:is_armor" },
            {"minecraft:arrows", "minecraft:arrow" },
            {"minecraft:axes", "minecraft:is_axe" },
            {"minecraft:banners", "minecraft:banner" },
            {"minecraft:boats", "minecraft:boats" }, //This actually works both singular and plural
            {"minecraft:coals", "minecraft:coals" },
            {"minecraft:crimson_stems", "minecraft:crimson_stems" },
            {"minecraft:fishes", "minecraft:is_fish" },
            {"minecraft:hoes", "minecraft:is_hoe" },
            {"minecraft:lectern_books", "minecraft:lecturn_books" },
            {"minecraft:logs", "minecraft:logs" },
            {"minecraft:logs_that_burn", "minecraft:logs_that_burn" },
            {"minecraft:mangrove_logs", "minecraft:mangrove_logs" },
            {"minecraft:music_discs", "minecraft:music_disc" },
            {"minecraft:planks", "minecraft:planks" },
            {"minecraft:sand", "minecraft:sand" },
            {"minecraft:signs", "minecraft:sign" },
            {"minecraft:soul_fire_base_blocks", "minecraft:soul_fire_base_blocks" },
            {"minecraft:stone_bricks", "minecraft:stone_bricks" },
            {"minecraft:stone_crafting_materials", "minecraft:stone_crafting_materials" },
            {"minecraft:stone_tool_materials", "minecraft:stone_tool_materials" },
            {"minecraft:pickaxes", "minecraft:is_pickaxe" },
            {"minecraft:shovels", "minecraft:is_shovel" },
            {"minecraft:swords", "minecraft:is_sword" },
            {"minecraft:tools", "minecraft:is_tool" },
            {"minecraft:tools/axes", "minecraft:is_axe" },
            {"minecraft:tools/hoes", "minecraft:is_hoe" },
            {"minecraft:tools/pickaxes", "minecraft:is_pickaxe" },
            {"minecraft:tools/shovels", "minecraft:is_shovel" },
            {"minecraft:tools/swords", "minecraft:is_sword" },
            {"minecraft:tools/tridents", "minecraft:is_trident" },
            {"minecraft:dampens_vibrations", "minecraft:vibration_dampener" },
            {"minecraft:warped_stems", "minecraft:warped_stems" },
            {"minecraft:wooden_slabs", "minecraft:wooden_slabs" },
            {"minecraft:wool", "minecraft:wool" },

            //Translated cobblemon tags
            {"cobblemon:apricorns", "cobblemon:apricorns" },
            {"cobblemon:berries", "cobblemon:berries" }
        };
      /// <summary>
      /// Attempts to resolve an item tag.
      /// </summary>
      /// <returns>A tuple where tag will be populated if there is a bedrock equivalent tag, or blocks will be populated if there is not, or neither if unknown tag.</returns>
      public static (string? tag, string[]? blocks) resolveItemTag(string itemTag) {
         if (JavaToBedrockItemTags.TryGetValue(itemTag, out var resolvedTag)) {
            return (resolvedTag, null);
         }
         else {
            var @namespace = itemTag.Split(":").First();
            if (@namespace == "minecraft" && JavaData.minecraftData!.itemTags.TryGetValue(itemTag.Split(":").Last(), out var minecraftTagData)) {
               return (null, minecraftTagData.values!
                   .Where(x => x is string)
                   .Select(x => getBedrockItem((string)x))
                   .Where(x => x != null)
                   .ToArray());
            }
            else if (@namespace == "cobblemon" && JavaData.cobblemonData!.itemTags.TryGetValue(itemTag.Split(":").Last(), out var cobblemonTagData)) {
               return (null, cobblemonTagData.values!
                   .Where(x => x is string)
                   .Select(x => getBedrockItem((string)x))
                   .Where(x => x != null)
                   .ToArray());
            }
            else {
               return (null, null);
            }
         }
      }
   }
}
