using System.Text.RegularExpressions;

namespace CobbleBuild {
   public class TranslationKey : Dictionary<string, string> {
      public string getBedrockKey() {
         string output = "";
         foreach (var key in this) {
            output += $"{key.Key}={key.Value}\n";
         }
         return output;
      }
      /// <summary>
      /// Does necessary operations on the translation key
      /// </summary>
      [Obsolete]
      public void Prepare() {
         var exp = new Regex(@"cobblemon\.species\..*\.name");
         //Creates valid translations for pokemon
         foreach (var item in this.Where(x => exp.IsMatch(x.Key)).ToList()) {
            this.Add($"entity.cobblemon:{item.Key.Substring(18)}", item.Value);
         }
      }
      /// <summary>
      /// Overwrites this key with the provided translation key.
      /// MUTATES THE ORIGINAL TRANSLATIONKEY
      /// </summary>
      public TranslationKey Merge(TranslationKey other) {
         foreach (var item in other) {
            this[item.Key] = item.Value;
         }
         return this;
      }
   }
}
