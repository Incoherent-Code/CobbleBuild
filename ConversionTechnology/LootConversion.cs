using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;

namespace CobbleBuild.ConversionTechnology {
   public class LootConversion //Not very accurate but good enough for now
    {
      public static LootTableJson? convertToBedrock(DropTable? dropData) {
         if (dropData != null && dropData.entries != null && dropData.amount != null) {
            LootTable output = new LootTable();
            output.rolls = dropData.amount;
            output.entries = new List<LootTable.LootTableEntry>();
            int? leftoverPercent = null;
            foreach (var entry in dropData.entries) {
               LootTable.LootTableEntry entry1 = new LootTable.LootTableEntry();
               entry1.type = "item";

               //Makes sure item isnt on the list of differently identified items
               if (BedrockConversion.JavaToBedrockItemNames.ContainsKey(entry.item)) {
                  entry1.name = BedrockConversion.JavaToBedrockItemNames[entry.item];
               }
               else if (BedrockConversion.JavaToBedrockItemNames.ContainsKey("minecraft:" + entry.item)) {
                  entry1.name = BedrockConversion.JavaToBedrockItemNames["minecraft:" + entry.item];
               }
               else {
                  entry1.name = entry.item;
               }


               if (entry.percentage != null) {
                  entry1.weight = (int?)entry.percentage;
                  if (leftoverPercent != null) {
                     leftoverPercent = leftoverPercent - entry1.weight;
                  }
                  else {
                     leftoverPercent = 100 - entry1.weight;
                  }
               }
               else if (entry.quantityRange != null) {
                  string[] minMaxArray = entry.quantityRange.Split("-");
                  int Out;
                  if (int.TryParse(minMaxArray[1], out Out)) {
                     entry1.weight = (dropData.amount / Out);
                     if (leftoverPercent != null) {
                        leftoverPercent = leftoverPercent - entry1.weight;
                     }
                     else {
                        leftoverPercent = 100 - entry1.weight;
                     }
                  }
               }
               output.entries.Add(entry1);

            }
            if (output.entries.Count > 0) {
               if (leftoverPercent != null && leftoverPercent > 0) {
                  output.entries.Add(new LootTable.LootTableEntry((int)leftoverPercent));
               }
               return new LootTableJson(output);
            }
            else { return null; }
         }
         else {
            return null;
         }
      }
   }
}
