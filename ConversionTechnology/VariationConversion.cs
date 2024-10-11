using CobbleBuild.CobblemonClasses;

namespace CobbleBuild.ConversionTechnology {
   /// <summary>
   /// Converting from ResolverVariation to Variation.
   /// The big thing is making sure that the ResolverVariation stuff is already parsed.
   /// </summary>
   public static class VariationConversion {
      public static List<Variation> convertVariations(ResolverJson[] resolverFiles, Pokemon pokemon) {
         if (resolverFiles.Length == 0 || resolverFiles[0].variations.Count == 0)
            throw new ArgumentException("No Valid Base Resolver was found.");
         var output = new List<Variation>();
         var grandparent = Variation.getFromResolverVariation(resolverFiles[0].variations[0], pokemon);
         for (int i = 0; i < resolverFiles.Length; i++) {
            if (resolverFiles[i].variations.Count == 0)
               throw new Exception("Resolver does not have any variations inside.");
            var parent = Variation.getFromResolverVariation(resolverFiles[i].variations[0], pokemon);
            for (int j = 0; j < resolverFiles[i].variations.Count; j++) {
               //Merging with itself should theoretically cause no problems except importing the same thing multiple times
               //That should be fine tho
               try {
                  output.Add(
                      Variation.getFromResolverVariation(resolverFiles[i].variations[j], pokemon)
                      .mergeWithParent(parent)
                      .mergeWithoutLayerMerge(grandparent)
                  );
               }
               catch (FileNotFoundException ex) {
                  Misc.warn($"Could not find file {ex.FileName}:" + ex.Message);
                  Misc.warn($"Variation {i} of {pokemon.shortName} will be skipped...");
               }
               catch (Exception ex) {
                  throw;
               }
            }
         }
         //Removes Duplicates
         return output.Distinct(new Variation.NameComparer()).ToList();
      }
   }
}
