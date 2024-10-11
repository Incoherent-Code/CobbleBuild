using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static CobbleBuild.Kotlin.KotlinArgument;
using static CobbleBuild.Kotlin.KotlinParser;
using static CobbleBuild.Kotlin.PoseConditionParser;

namespace CobbleBuild.Kotlin {
   /// <summary>
   /// Pretty Janky, but mostly works to extract necessary poser data from pokemon
   /// </summary>
   public class KotlinPoserVisitor : KotlinParserBaseVisitor<KotlinPoser> {
      public override KotlinPoser VisitKotlinFile([NotNull] KotlinFileContext context) {
         var outputPoser = new KotlinPoser();
         // Visit all children and collect function names
         foreach (var child in context.children) {
            var childResult = Visit(child);
            if (childResult != null) {
               outputPoser = AggregateResult(outputPoser, childResult);
            }
         }
         return outputPoser;
      }
      public override KotlinPoser VisitPropertyDeclaration([NotNull] PropertyDeclarationContext context) {
         string propertyDeclarationName = context.variableDeclaration().simpleIdentifier().GetText();
         //Body part declaration
         if (context.expression() != null &&
             (context.expression().GetText().StartsWith("getPart")
             || context.expression().GetText().StartsWith("root.registerChildWithAllChildren")
             || context.expression().GetText().StartsWith("root.registerChildWithSpecificChildren"))) { //Cannot be bothered to manually parse this for now
            var bodyPartName = context.expression().FindFirstOf<LineStringContentContext>();
            if (bodyPartName != null) {
               return new KotlinPoser() {
                  registeredBodyParts = {
                        { context.variableDeclaration().simpleIdentifier().GetText(), bodyPartName.GetText() }
                    }
               };
            }
         }
         return base.VisitPropertyDeclaration(context);
      }
      //Class Implimentations
      public override KotlinPoser VisitDelegationSpecifiers([NotNull] DelegationSpecifiersContext context) {
         if (context.Parent is ClassDeclarationContext) {
            var output = new KotlinPoser();
            foreach (var child in context.children) {
               if (child is AnnotatedDelegationSpecifierContext) {
                  output.implimentations.Add(child.GetText());
               }
            }
            return output;
         }
         return base.VisitDelegationSpecifiers(context);
      }
      public override KotlinPoser VisitClassDeclaration([NotNull] ClassDeclarationContext context) {
         var output = base.VisitClassDeclaration(context);
         output.poserName = context.simpleIdentifier().GetText();
         return output;
      }
      //Registered Poses
      public override KotlinPoser VisitFunctionDeclaration([NotNull] FunctionDeclarationContext context) {
         if (context.simpleIdentifier() != null && context.simpleIdentifier().GetText() == "registerPoses") {
            var functionChildren = context.functionBody()?.block()?.statements();
            if (functionChildren != null) {
               var output = new KotlinPoser();
               foreach (var child2 in functionChildren.children) {
                  if (child2 is StatementContext) {
                     var child = (child2 as StatementContext)!;
                     if (child == null) { //Shouldn't be possible
                        throw new Exception("StatementContext is not StatementContext");
                     }
                     //If Statement is regular assignment
                     if (child?.assignment() != null) {
                        var poseName = child.assignment()?.directlyAssignableExpression()?.GetText();
                        if (poseName == null)
                           continue;

                        var expression = child.FindFirstOf<PostfixUnaryExpressionContext>();
                        if (expression == null || expression.primaryExpression()?.GetText() != "registerPose")
                           continue;
                        var pose = new Pose(poseName);
                        var arguments = expression.FindAll<ValueArgumentContext>(); //Should be fine b/c findall doesn't search inside targets
                        foreach (var argument in arguments) {
                           var property = argument.simpleIdentifier()?.GetText();
                           var expressionText = argument.expression()?.GetText();
                           if (expressionText == null)
                              throw new Exception($"RegisterPose arg {argument.GetText()} has no value.");
                           switch (property) {
                              case null:
                                 throw new Exception("This parser does not yet support the property identifier being left out in registerPose()");
                              case "poseName":
                                 //PoseName is inferred from name of variable being assigned.
                                 break;
                              case "poseType":
                              case "poseTypes":
                                 pose.poseTypes = parsePoseTypeExpression(argument.expression());
                                 break;
                              case "transformTicks":
                                 int ticks;
                                 if (int.TryParse(expressionText, out ticks)) {
                                    pose.transformTicks = ticks;
                                 }
                                 break;
                              case "idleAnimations":
                                 var arrayOfStatement = argument.expression().FindFirstOf<PostfixUnaryExpressionContext>();
                                 var arrayOfCallText = arrayOfStatement.primaryExpression()?.GetText();
                                 if (arrayOfCallText == "emptyArray")
                                    break;
                                 if (arrayOfStatement == null || arrayOfCallText != "arrayOf")
                                    throw new Exception("idleAnimations property of poser does not use arrayOf().");
                                 foreach (var animation in arrayOfStatement.FindAll<ValueArgumentContext>()) {
                                    var animationCall = animation.FindFirstOf<PostfixUnaryExpressionContext>();
                                    if (animationCall == null)
                                       throw new Exception("idleAnimation is not a function call.");
                                    var callName = animationCall.primaryExpression().GetText();
                                    //As far as I know, the only difference between bedrock and bedrockStateful is that bedrockStateful can
                                    //contain a condition that prevents idle.
                                    if (callName == "bedrock" || callName == "bedrockStateful") {
                                       //Calls a bedrock animation
                                       var stringArguments = animationCall
                                           .FindAll<ValueArgumentContext>()
                                           .Select(x => x.FindFirstOf<LineStringContentContext>()?.GetText())
                                           .Where(x => x != null)
                                           .ToArray();
                                       if (stringArguments.Length < 2)
                                          throw new Exception("Bedrock Animation Refrence does not have enough string arguments.");
                                       pose.idleAnimations.Add(
                                           AnimationRefrence.bedrock(stringArguments[1], stringArguments[0])
                                       );
                                    }
                                    else {
                                       //Calls an internal animation (Needs to be translated in later stages.)
                                       pose.idleAnimations.Add(
                                           AnimationRefrence.builtin(callName, animationCall.FindFirstOf<ValueArgumentsContext>())
                                       );
                                    }
                                 }
                                 break;
                              case "quirks": {
                                    var poseUnaryExp = argument.expression().FindFirstOf<PostfixUnaryExpressionContext>();
                                    var ExpressionCall = poseUnaryExp?.primaryExpression()?.GetText();
                                    if (ExpressionCall != "arrayOf")
                                       throw new Exception($"Expected arrayOf in quirks, got {ExpressionCall}");
                                    pose.quirks = poseUnaryExp
                                        .postfixUnarySuffix()
                                        .FirstOrDefault()
                                        .FindAll<ValueArgumentContext>()
                                        .Select(x => x.GetText())
                                        .ToArray();
                                 }
                                 break;
                              case "condition":
                                 pose.condition = ParseConditionStatement(argument.expression());
                                 break;
                              case "transformedParts":
                                 var arrayOfStatement2 = argument.expression().FindFirstOf<PostfixUnaryExpressionContext>();
                                 var arrayOfCallText2 = arrayOfStatement2.primaryExpression()?.GetText();
                                 if (arrayOfCallText2 == "emptyArray")
                                    break;
                                 if (arrayOfStatement2 == null || arrayOfCallText2 != "arrayOf")
                                    throw new Exception("transformedParts property of poser does not use arrayOf().");
                                 foreach (var transformation in arrayOfStatement2.FindAll<ValueArgumentContext>()) {
                                    var transformationCall = transformation.FindFirstOf<PostfixUnaryExpressionContext>();
                                    if (transformationCall == null) {
                                       Misc.warn($"Invalid transformedParts Expression");
                                       break;
                                    }
                                    var transformedPart = parseTransformedPart(transformationCall);
                                    if (transformedPart.HasValue) //Why tf is value.key and value.value required here
                                       pose.transformedParts.Add(transformedPart.Value.Key, transformedPart.Value.Value);
                                 }
                                 break;
                              default:
                                 Misc.warn($"Unknown Pose property {property}");
                                 break;
                           }
                        }
                        output.poses[poseName] = pose;
                     }
                     //Variable Declaration (Quirk declaration handling.)
                     else if (child.declaration()?.propertyDeclaration()?.variableDeclaration() != null) {
                        var quirkCall = child.declaration().propertyDeclaration().FindFirstOf<PostfixUnaryExpressionContext>();
                        var valueName = child.declaration().propertyDeclaration().variableDeclaration().simpleIdentifier().GetText();
                        //If Not a quirk declaration
                        //May need to change later when we look for cryProviders aswell
                        if (quirkCall.primaryExpression()?.GetText() != "quirk")
                           continue;
                        var quirkCallArguemtns = parseKotlinArgs(quirkCall.postfixUnarySuffix().First().callSuffix().valueArguments());
                        var quirkName = quirkCallArguemtns.First().Value.Replace("\"", "");
                        (float, float)? secondsBetweenOccurences = null;
                        if (quirkCallArguemtns.Count > 1 && quirkCallArguemtns[1].Name == "secondsBetweenOccurrences") {
                           var rangeArray = quirkCallArguemtns[1].Value
                               .Split(" to ")
                               .Select(x => x.Replace("F", ""))
                               .Select(x => { float.TryParse(x, out float result); return result; })
                               .Where(x => x != default)
                               .ToArray();
                           if (rangeArray.Length > 1)
                              secondsBetweenOccurences = (rangeArray[0], rangeArray[1]);
                        }

                        var animationCall = quirkCall.postfixUnarySuffix().First().callSuffix().annotatedLambda()?.FindFirstOf<PostfixUnaryExpressionContext>();
                        if (animationCall == null || animationCall.primaryExpression().GetText() != "bedrockStateful") {
                           Misc.warn($"Could not resolve animation call for quirk {valueName}");
                           continue;
                        }

                        var bedrockStatefulArgs = parseKotlinArgs(animationCall.postfixUnarySuffix().First().callSuffix().valueArguments());
                        if (bedrockStatefulArgs.Count < 2) {
                           Misc.warn($"Not enough arguments for bedrockStateful in quirk " + valueName);
                           continue;
                        }

                        bool preventsIdle = true;
                        if (animationCall.postfixUnarySuffix().Length >= 3 && animationCall.postfixUnarySuffix()[1].navigationSuffix()?.simpleIdentifier()?.GetText() == "setPreventsIdle") {
                           var setPreventsIdleArgs = parseKotlinArgs(animationCall.postfixUnarySuffix()[2].callSuffix().valueArguments());
                           preventsIdle = bool.Parse(setPreventsIdleArgs.First().Value.ToLower());
                        }
                        output.quirks.Add(valueName, new Quirk(AnimationRefrence.bedrock(bedrockStatefulArgs[1].Value.Replace("\"", ""), bedrockStatefulArgs[0].Value.Replace("\"", ""))) {
                           preventsIdle = preventsIdle,
                           quirkName = quirkName,
                           secondsBetweenOccurences = secondsBetweenOccurences
                        });
                     }
                  }
               }
               return output;
            }
         }
         return base.VisitFunctionDeclaration(context);
      }
      protected override KotlinPoser AggregateResult(KotlinPoser aggregate, KotlinPoser nextResult) {
         if (aggregate == null)
            return nextResult;

         aggregate.MergeWith(nextResult);
         return aggregate;
      }
      /// <summary>
      /// Retrieves the corresponding Enum from the string provided.
      /// (NOT CASE SENSITIVE)
      /// </summary>
      /// <typeparam name="T">Enum Searching From</typeparam>
      /// <param name="enum_name">Enum name to find. (NOT CASE SENSITIVE)</param>
      /// <returns>The Enum or default</returns>
      private static T? getEnumFromReflection<T>(string enum_name) where T : Enum {
         return typeof(T)
             .GetFields()
             .Where(x => x.Name.ToUpper() == enum_name.ToUpper())
             .Select(x => (T?)x.GetRawConstantValue())
             .FirstOrDefault();
      }
      public static PoseType parsePoseTypeExpression(ExpressionContext context) {
         return _parsePoseTypeExpression(context);
      }
      public static PoseType parsePoseTypeExpression(PostfixUnaryExpressionContext context) {
         return _parsePoseTypeExpression(context);
      }
      /// <summary>
      /// Supports parsing either poseType or PoseTypes property of Poser
      /// </summary>
      /// <param name="context">PoseType Expression from Antlr Tree</param>
      /// <returns>Valid PoseType</returns>
      private static PoseType _parsePoseTypeExpression(IParseTree context) {
         if (context is ExpressionContext) {
            var additiveExpression = context.FindFirstOf<AdditiveExpressionContext>();
            //Because the Kotlin grammar is the way that it is, Additive Expression should always be there.
            if (additiveExpression == null)
               throw new Exception("Malformed PoseType or PoseTypes Expression.");
            if (additiveExpression.ChildCount > 1) {
               //As far as I know, theres no parenthesis in the posers, so it should be fine.
               PoseType leftHandOperator = default;
               for (int i = 0; i < additiveExpression.ChildCount; i++) {
                  var child = additiveExpression.GetChild(i);
                  if (child is MultiplicativeExpressionContext && leftHandOperator == default)
                     leftHandOperator = _parsePoseTypeExpression(child);
                  else if (child is AdditiveOperatorContext) {
                     string @operator = child.GetText();
                     if (@operator == "+")
                        leftHandOperator = leftHandOperator.Add(parsePoseTypeFromString(additiveExpression.GetChild(i + 1).GetText()));
                     else if (@operator == "-")
                        leftHandOperator = leftHandOperator.Subtract(parsePoseTypeFromString(additiveExpression.GetChild(i + 1).GetText()));
                     i++;
                  }
               }
               return leftHandOperator;
            }
         }
         PostfixUnaryExpressionContext UnaryExpression;
         if (!(context is PostfixUnaryExpressionContext))
            UnaryExpression = context.FindFirstOf<PostfixUnaryExpressionContext>();
         else
            UnaryExpression = context as PostfixUnaryExpressionContext;
         if (UnaryExpression == null)
            throw new Exception("Could not find PostfixUnaryExpression.");

         //Parse setOf Statement
         if (UnaryExpression.primaryExpression().GetText() == "setOf") {
            return UnaryExpression
                .FindAll<ValueArgumentContext>()
                .Select(x => _parsePoseTypeExpression(x))
                .CombinePoses();
         }

         //If not setOf or AdditiveExpression
         var ResolveAttempt = parsePoseTypeFromString(context.GetText());
         return ResolveAttempt;
      }
      /// <summary>
      /// Takes in string like PoseType.Fly or UI_POSES and returns PoseType
      /// </summary>
      /// <param name="pose"></param>
      /// <returns></returns>
      private static PoseType parsePoseTypeFromString(string pose) {
         if (pose.StartsWith("PoseType."))
            pose = pose.Substring(9);
         return getEnumFromReflection<PoseType>(pose);
      }
      private static KeyValuePair<string, TransformedPart>? parseTransformedPart(PostfixUnaryExpressionContext context) {
         var boneName = context.primaryExpression()?.simpleIdentifier()?.GetText();
         if (boneName == null) {
            Misc.warn($"TransformPart Statement does not contain simpleIdentifier?");
            return null;
         }
         if (context.postfixUnarySuffix().First().GetText() != ".asTransformed") {
            Misc.warn($"TransformPart expects an .asTransformed() statment, but it was not found.");
            return null;
         }

         var output = new KeyValuePair<string, TransformedPart>(boneName, new TransformedPart());
         for (int i = 0; i < context.postfixUnarySuffix().Length; i++) {
            var statement = context.postfixUnarySuffix()[i];
            if (statement.navigationSuffix() == null || statement.navigationSuffix().simpleIdentifier() == null)
               continue;
            //Analyze function call.
            var operationType = statement.navigationSuffix().simpleIdentifier().GetText();
            var arguments = context.postfixUnarySuffix()[i + 1].callSuffix()?.valueArguments();
            if (arguments == null)
               continue;
            var kotlinArgs = parseKotlinArgs(arguments);
            //This isn't half the implimented functions but these are the only ones that are used as far as I know.
            switch (operationType) {
               //AsTransformed does nothing
               case "asTransformed":
                  break;
               case "withVisibility":
                  if (bool.TryParse(kotlinArgs.First().Value.ToLower(), out var visible))
                     output.Value.visible = visible;
                  break;
               case "addPosition":
                  //The overload with 3 args is x, y z coords
                  if (kotlinArgs.Count == 3) {
                     float.TryParse(kotlinArgs[0].Value.Replace("F", ""), out var xnum);
                     output.Value.position.x = new ValueOrRefrence<float>(kotlinArgs[0].Value, xnum);
                     float.TryParse(kotlinArgs[1].Value.Replace("F", ""), out var ynum);
                     output.Value.position.y = new ValueOrRefrence<float>(kotlinArgs[1].Value, ynum);
                     float.TryParse(kotlinArgs[2].Value.Replace("F", ""), out var znum);
                     output.Value.position.z = new ValueOrRefrence<float>(kotlinArgs[2].Value, znum);
                  }
                  else if (kotlinArgs.Count == 2) {
                     //This overload takes in an axis and value
                     var axis = kotlinArgs[0].Value;
                     if (axis.EndsWith("X_AXIS")) {
                        float.TryParse(kotlinArgs[1].Value.Replace("F", ""), out var num);
                        output.Value.position.x = new ValueOrRefrence<float>(kotlinArgs[1].Value, num);
                     }
                     else if (axis.EndsWith("Y_AXIS")) {
                        float.TryParse(kotlinArgs[1].Value.Replace("F", ""), out var num);
                        output.Value.position.y = new ValueOrRefrence<float>(kotlinArgs[1].Value, num);
                     }
                     else if (axis.EndsWith("Z_AXIS")) {
                        float.TryParse(kotlinArgs[1].Value.Replace("F", ""), out var num);
                        output.Value.position.z = new ValueOrRefrence<float>(kotlinArgs[1].Value, num);
                     }
                     else {
                        Misc.warn($"Unknown Position Axis {kotlinArgs[0].Value}");
                     }
                  }
                  else {
                     Misc.warn("Invalid addPosition Overload.");
                  }
                  break;
               default:
                  Misc.warn($"Unknown Transform operation type {operationType}");
                  break;
            }
         }
         return output;
      }
   }
   public static class ParseTreeNavigatorExtensions {
      /// <summary>
      /// Recursively navigates down the tree until the child node does not have 1 child and returns it.
      /// Usually better to use .FindFirstOf() but this has its valid use cases
      /// </summary>
      public static IParseTree FindFirstSplit(this IParseTree context) {
         if (context.ChildCount == 1) {
            return context.GetChild(0).FindFirstSplit();
         }
         else {
            return context;
         }
      }
      /// <summary>
      /// Recursively searches all children for a specific type of child. If no child is found, default is returned.
      /// </summary>
      /// <typeparam name="T">Type to find (Ex: FunctionBodyContext)</typeparam>
      /// <param name="tree"></param>
      /// <returns></returns>
      public static T? FindFirstOf<T>(this IParseTree? tree) where T : IParseTree {
         if (tree == null)
            return default;
         for (var i = 0; i < tree.ChildCount; i++) {
            var child = tree.GetChild(i);
            if (child is T)
               return (T)child;
            else {
               var grandchild = child.FindFirstOf<T>();
               if (grandchild != null)
                  return grandchild;
            }
         }
         return default;
      }
      /// <summary>
      /// Searches for all instances of T in parse tree. Does not search inside found instances of T.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="tree"></param>
      /// <returns>Array of T. Will not be null, but will be empty on no match.</returns>
      public static T[] FindAll<T>(this IParseTree? tree) where T : IParseTree {
         if (tree == null)
            return Array.Empty<T>();
         var output = new List<T>();
         for (var i = 0; i < tree.ChildCount; i++) {
            var child = tree.GetChild(i);
            if (child is T)
               output.Add((T)child);
            else {
               var grandchild = child.FindAll<T>();
               if (grandchild != null)
                  output = output.Concat(grandchild).ToList();
            }
         }
         return output.ToArray();
      }
   }
}
