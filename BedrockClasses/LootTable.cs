namespace CobbleBuild.BedrockClasses {
   public class LootTableJson {
      public List<LootTable> pools;
      public LootTableJson(List<LootTable> pools) {
         this.pools = pools;
      }
      public LootTableJson() { }
      public LootTableJson(params LootTable[] tables) //Very Unlikely to have more than one pool per Table
      {
         pools = new List<LootTable>();
         foreach (LootTable table in tables) {
            pools.Add(table);
         }
      }
   }
   public class LootTable {
      public object rolls; //Can either be an int or a MinMaxValue
      public List<LootTableEntry> entries;
      public class LootTableEntry {
         public string? type;
         public string? name;
         public int? weight;

         public LootTableEntry(int weight) //Blank entry
         {
            this.weight = weight;
            this.type = "empty";
         }
         public LootTableEntry(string item, int weight) {
            this.weight = weight;
            this.type = "item";
            this.name = item;
         }
         public LootTableEntry() { }
      }
   }
}
