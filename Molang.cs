namespace CobbleBuild {
   internal static class MolangUtil {
      /// <summary>
      /// Takes in a specific aspect and pokemon and returns a molang string that represents that aspect.
      /// Passing in "base" as the aspect will get all variants excluding regional variants like galarian.
      /// </summary>
      /// <param name="aspect"></param>
      /// <param name="pokemon"></param>
      /// <returns>Molang String like "(q.variant == 1)"</returns>
      /// <exception cref="Exception">The provided pokemon has no variatons.</exception>
      public static string getVariationConditionalFromAspect(string aspect, Pokemon pokemon) {
         if (pokemon.Variations == null || pokemon.Variations.Count == 0)
            throw new Exception("Couldn't get VariationConditional: Variations are not initialized.");
         var indexs = pokemon.Variations
             .ToList()
             .FindAll(x => {
                if (aspect == "base")
                   //I think this'll work
                   return !Misc.validRegionalVariants.Any(y => x.aspects.Contains(y));
                else
                   return x.aspects.Contains(aspect);
             })
             .Select(x => pokemon.Variations.FindIndex(y => y == x))
             .ToArray();
         if (indexs.Count() == 0)
            return "";
         string output = $"(q.variant == {indexs[0]})";
         for (int i = 1; i < indexs.Length; i++) {
            output += $"|| (q.variant == {indexs[i]})";
         }
         return output;
      }
   }
}
