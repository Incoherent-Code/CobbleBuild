using Antlr4.Runtime.Tree;
using static CobbleBuild.Kotlin.KotlinParser;

namespace CobbleBuild.Kotlin {
   public static class PoseConditionParser {
      /// <summary>
      /// Takes in the condition expression for Kotlin Poses and returns a molang statement in the form of a string.
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      public static string ParseConditionStatement(ExpressionContext context) {
         return _parseConditionStatement(context);
      }
      private static string _parseConditionStatement(IParseTree context) {
         var split = context.FindFirstSplit();
         if (split.ChildCount == 0) {
            Misc.warn("ParseConditionStatement hit an empty statement!");
            return "false"; //Disable condition by default
         }
         //Skip Lamda Literal (The children are just the curly brackets.)
         if (split is LambdaLiteralContext) {
            return _parseConditionStatement(((LambdaLiteralContext)split).statements());
         }
         else if (split is PostfixUnaryExpressionContext) {
            var expression = (PostfixUnaryExpressionContext)split;
            var expressionCall = expression.primaryExpression().GetText();
            var suffixes = expression.postfixUnarySuffix();
            if (expressionCall == "it") {
               //Combines the suffixes into one call.
               var conditionName = suffixes
                   .Select(x => x.GetText())
                   .Aggregate((x, y) => $"{x}{y}");
               if (!itLocalMolangConversion.TryGetValue(conditionName, out var molang)) {
                  Misc.warn($"Unrecognized it condition conversion {conditionName}");
                  return "false";
               }
               return molang;
            }
            else {
               Misc.warn($@"Unknown expression '{expression.GetText()}'");
            }
         }
         else if (split is PrefixUnaryExpressionContext) {
            var expression = (PrefixUnaryExpressionContext)split;
            var prefix = expression.unaryPrefix();
            var postfix = expression.postfixUnaryExpression();
            if (prefix.Length != 1) {
               Misc.warn($"Unusual amount of prefixes for PrefixUnaryExpression.");
               return "false";
            }
            //Most prefixes also exist in kotlin so this is fine (This is almost always an !)
            //And if it causes any issues bedrock's logs should highlight it.
            return $"{prefix.First().GetText()}{_parseConditionStatement(postfix)}";
         }
         else if (split is ConjunctionContext) {
            var expression = (ConjunctionContext)split;
            var @operator = expression.CONJ();
            if (@operator.Length + 1 != expression.equality().Length) {
               Misc.warn($"Invalid amount of operators for conjunction.");
               return "false";
            }
            var statements = expression.equality();
            var output = _parseConditionStatement(statements[0]);
            for (int i = 1; i < statements.Length; i++) {
               output += $" {@operator[i - 1]} {_parseConditionStatement(statements[i])}";
            }

            return output;
         }
         else if (split is InfixOperationContext) {
            var expression = (InfixOperationContext)split;
            var expressions = expression.elvisExpression();
            //The second expression is it.aspects.get() in any dataKeys statement
            if (expressions.Length != 2) {
               Misc.warn($"Unusual amount of expressions in infixOperation.");
               return "false";
            }
            var dataKeyExpression = expressions.First().FindFirstOf<PostfixUnaryExpressionContext>();
            if (dataKeyExpression == null) {
               Misc.warn($"Could not find postFixUnary in InfixExpression!");
               return "false";
            }

            if (dataKeyExpression.primaryExpression().GetText() != "DataKeys") {
               Misc.warn($"Infix operations are only supported for dataKeys");
               return "false";
            }

            var dataKeyName = dataKeyExpression.postfixUnarySuffix()
                .Select(x => x.GetText())
                .Aggregate((x, y) => $"{x}{y}");

            if (!dataKeysMolang.TryGetValue(dataKeyName, out var molang)) {
               Misc.warn($"Unknown data key {dataKeyName}");
               return "false";
            }

            var @operator = expression.inOperator().First().GetText().Trim();
            if (@operator == "in") {
               return molang;
            }
            else if (@operator == "!in") {
               return $"!{molang}";
            }
            else {
               Misc.warn("Unsupported Infix operation!");
               return "false";
            }
         }
         Misc.warn("Unable to parse condition statement! The split type had no valid case!");
         return "false"; //Disable statement
      }
      /// <summary>
      /// What to translate all it.condition calls to.
      /// These can be tweaked for better results.
      /// </summary>
      private static Dictionary<string, string> itLocalMolangConversion = new Dictionary<string, string>() {
            {".isSubmergedInWater", "q.is_swimming" },
            {".isTouchingWater", "q.is_in_water" },
            {".isBattling",  @"q.property('cobblemon:in_battle')"},
            {".isMoving.get()", "q.is_moving" },
            {".isTouchingWaterOrRain", "q.is_in_water_or_rain" }
        };
      /// <summary>
      /// Molang to convert data keys to
      /// </summary>
      private static Dictionary<string, string> dataKeysMolang = new Dictionary<string, string>() {
            {".HAS_BEEN_SHEARED", @"q.property('cobblemon:has_been_sheared')" }
        };
   }
}
