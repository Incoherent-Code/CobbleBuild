using static CobbleBuild.Kotlin.KotlinParser;

namespace CobbleBuild.Kotlin {
   /// <summary>
   /// Parameter from Kotlin Function Call
   /// Value is always string. It is up to the user to parse it correctly based on what they expect.
   /// </summary>
   public class KotlinArgument(string value, string? name = null) {
      /// <summary>
      /// Contains the stringified value of whatever was passed in.
      /// Note: strings have the quotes still attached to them. Be aware when parsing.
      /// </summary>
      public string Value = value;
      /// <summary>
      /// If the name of the argument was specified, it will be here.
      /// </summary>
      public string? Name = name;
      /// <summary>
      /// Parses Kotlin Arguments and returns them as a StringOrPropertyAndString.
      /// </summary>
      public static List<KotlinArgument> parseKotlinArgs(ValueArgumentsContext context) {
         return context.valueArgument()
             .Select(getFromKotlin)
             .ToList();
      }
      public static KotlinArgument getFromKotlin(ValueArgumentContext context) {
         return new KotlinArgument(context.expression().GetText(), context.simpleIdentifier()?.GetText());
      }
   }

}
