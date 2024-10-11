namespace CobbleBuild.CobblemonClasses {
   public class PokeballResourceData {
      public string pokeball;
      public int order;
      public List<PokeballVariation> variations;

   }
   public class PokeballVariation {
      public string[] aspects;
      public string poser;
      public string model;
      public string texture;
      public string[] layers;
   }
}
