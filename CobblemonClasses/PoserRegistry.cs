using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CobbleBuild.Kotlin;
using static CobbleBuild.Kotlin.KotlinParser;

namespace CobbleBuild.CobblemonClasses {
   public static class PoserRegistry {
      /// <summary>
      /// Dictionary of Posers. The dictionary key is the poser name (ex: AlakazamModel)
      /// </summary>
      public static Dictionary<string, KotlinPoser> posers = [];
      /// <summary>
      /// Dictionary where the key is the poser name (ex: BulbasaurModel) and the value is the identifier (ex: bulbasaur)
      /// </summary>
      public static Dictionary<string, string>? mappings = null;

      /// <summary>
      /// Takes in the fileData of PokemonModelRepository.kt and returns the mappings between the poser names and indentifiers
      /// </summary>
      public static Dictionary<string, string> getMappings(AntlrInputStream modelRepostitoryStream) {
         var lexer = new KotlinLexer(modelRepostitoryStream);
         var tokens = new CommonTokenStream(lexer);
         var parser = new KotlinParser(tokens);
         KotlinFileContext tree = parser.kotlinFile();

         //ParseTreeWalker.Default.Walk(output, tree);
         PoserRegistryVisitor visitor = new PoserRegistryVisitor();

         return visitor.Visit(tree);
      }
      public static void InitMappings() {
         var filePath = Path.Combine(Config.config.kotlinBasePath, "client/render/models/blockbench/repository/PokemonModelRepository.kt");
         if (!File.Exists(filePath))
            Misc.error("File PokemonModelRepository.kt could not be found at " + filePath);
         //Inverted. I now want it the other way around.
         mappings = getMappings(new AntlrInputStream(File.OpenRead(filePath))).ToDictionary(x => x.Value, x => x.Key);
      }
   }
   internal class PoserRegistryVisitor : KotlinParserBaseVisitor<Dictionary<string, string>> {
      public override Dictionary<string, string> VisitKotlinFile([NotNull] KotlinFileContext context) {
         var output = new Dictionary<string, string>();
         // Visit all children and collect function names
         foreach (var child in context.children) {
            var childResult = Visit(child);
            if (childResult != null) {
               output = AggregateResult(output, childResult);
            }
         }
         return output;
      }
      protected override Dictionary<string, string> AggregateResult(Dictionary<string, string> aggregate, Dictionary<string, string> nextResult) {
         if (aggregate == null)
            return nextResult;
         if (nextResult == null)
            return aggregate;

         return aggregate.Union(nextResult).ToDictionary((x) => x.Key, (x) => x.Value);
      }
      public override Dictionary<string, string> VisitFunctionDeclaration([NotNull] FunctionDeclarationContext context) {
         if (context.simpleIdentifier().GetText() != "registerInBuiltPosers")
            return base.VisitFunctionDeclaration(context);
         var children = context.functionBody().block().statements().children
             .Where(x => x is StatementContext)
             .Select(x => (StatementContext)x);

         var output = new Dictionary<string, string>();
         foreach (var child in children) {
            var functionCall = child.FindFirstOf<PostfixUnaryExpressionContext>();
            if (functionCall?.primaryExpression().GetText() != "inbuilt")
               continue;

            var arguments = child.FindAll<ValueArgumentContext>();
            if (arguments.Length != 2)
               continue;

            output.TryAdd(arguments[0].FindFirstOf<LineStringContentContext>()?.GetText(), arguments[1].FindFirstOf<SimpleIdentifierContext>()?.GetText());
         }
         return output;
      }
   }
}
