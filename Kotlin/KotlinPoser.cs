using Antlr4.Runtime;
using CobbleBuild.BedrockClasses;
using Newtonsoft.Json;
using static CobbleBuild.Kotlin.KotlinArgument;
using static CobbleBuild.Kotlin.KotlinParser;

namespace CobbleBuild.Kotlin {
   /// <summary>
   /// Poser from kotlin source that will be converted into animation controller.
   /// </summary>
   public class KotlinPoser {
      public string? poserName;
      public Dictionary<string, Quirk> quirks = [];
      public Dictionary<string, Pose?> poses = [];
      /// <summary>
      /// Key is standard body part name, and value is body part on the model.
      /// </summary>
      public Dictionary<string, string> registeredBodyParts = [];
      /// <summary>
      /// All interfaces that the poser inherits from (ex: BipedFrame, HeadedFrame)
      /// </summary>
      public List<string> implimentations = [];
      /// <summary>
      /// Variable declaration and assignment as text.
      /// Necessary because transformedParts uses them sometimes.
      /// </summary>
      public Dictionary<string, string> stringifiedVariables = [];

      public void MergeWith(KotlinPoser? merger) {
         if (merger == null)
            return;

         implimentations = implimentations.Union(merger.implimentations).ToList();
         registeredBodyParts = MergeDictionaries(registeredBodyParts, merger.registeredBodyParts);
         poses = MergeDictionaries(poses, merger.poses);
         poserName ??= merger.poserName;
         quirks = MergeDictionaries(quirks, merger.quirks);
      }
      /// <summary>
      /// Merges Dictionaries; Only overwrites entries if Value is null
      /// </summary>
      private static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(Dictionary<TKey, TValue> DictA, Dictionary<TKey, TValue> DictB) {
         foreach (var item in DictB.Keys) {
            if (!DictA.ContainsKey(item) || DictA[item] == null) {
               DictA[item] = DictB[item];
            }
         }
         return DictA;
      }
      /// <summary>
      /// Parses a kotlin poser from a .kt file
      /// </summary>
      public static KotlinPoser import(AntlrInputStream fileData) {
         var lexer = new KotlinLexer(fileData);
         var tokens = new CommonTokenStream(lexer);
         var parser = new KotlinParser(tokens);
         KotlinFileContext tree = parser.kotlinFile();

         //ParseTreeWalker.Default.Walk(output, tree);
         KotlinPoserVisitor visitor = new KotlinPoserVisitor();

         return visitor.Visit(tree);
      }
      public static KotlinPoser import(string fileData) {
         return import(new AntlrInputStream(fileData));
      }
      public static KotlinPoser import(FileStream file) {
         return import(new AntlrInputStream(file));
      }
      public string? tryResolveVariable(string variable) {
         if (stringifiedVariables.TryGetValue(variable, out var value))
            return value;
         return null;
      }
   }


   public class Pose(string poseName) {
      public string poseName = poseName;
      public PoseType poseTypes;
      public int? transformTicks;
      public List<AnimationRefrence> idleAnimations = [];
      /// <summary>
      /// Should fit into a key in Poser.Quirks
      /// </summary>
      public string[]? quirks;
      public string? condition;
      /// <summary>
      /// Parts that are a certain way during a pose.
      /// </summary>
      public Dictionary<string, TransformedPart> transformedParts = [];
   }
   public class Quirk(AnimationRefrence animation) {
      public bool preventsIdle = true;
      public AnimationRefrence statefulAnimation = animation;
      public string? quirkName;
      public (float, float)? secondsBetweenOccurences;
      /// <summary>
      /// Range of time it can take to loop.
      /// Currently unused.
      /// </summary>
      public (int, int)? loopTimes;
   }
   public enum AnimationType {
      BUILT_IN,
      BEDROCK
   }
   /// <summary>
   /// Refrences either A built in animation or a bedrock animation
   /// </summary>
   public class AnimationRefrence(AnimationType type, string refrenceName) {
      public AnimationType type = type;
      /// <summary>
      /// Animation Name like "ground_idle"
      /// </summary>
      public string refrenceName = refrenceName;
      public List<KotlinArgument>? kotlin_arguments;
      /// <summary>
      /// Usually just "charizard" but refers to the name of the animation file.
      /// </summary>
      public string? bedrock_animation_file;
      public static AnimationRefrence bedrock(string animation_id, string bedrock_animation_file) {
         return new AnimationRefrence(AnimationType.BEDROCK, animation_id) { bedrock_animation_file = bedrock_animation_file };
      }
      public static AnimationRefrence builtin(string builtin_name, ValueArgumentsContext? builtin_args = null) {
         return new AnimationRefrence(AnimationType.BUILT_IN, builtin_name) {
            kotlin_arguments = builtin_args == null ? null : parseKotlinArgs(builtin_args)
         };
      }
   }
   /// <summary>
   /// Equivalent to the PoseType enum in Kotlin.
   /// Uses the Enum Flag System.
   /// </summary>
   [Flags]
   public enum PoseType : ushort {
      STAND = 0b_0000_0000_0000_0001,
      WALK = 0b_0000_0000_0000_0010,
      SLEEP = 0b_0000_0000_0000_0100,
      HOVER = 0b_0000_0000_0000_1000,
      FLY = 0b_0000_0000_0001_0000,
      FLOAT = 0b_0000_0000_0010_0000,
      SWIM = 0b_0000_0000_0100_0000,
      SHOULDER_LEFT = 0b_0000_0000_1000_0000,
      SHOULDER_RIGHT = 0b_0000_0001_0000_0000,
      PROFILE = 0b_0000_0010_0000_0000,
      PORTRAIT = 0b_0000_0100_0000_0000,
      //This intuitively sounds like it would be all zeros, but I don't think it is implimented like that in kotlin
      NONE = 0b_0000_1000_0000_0000,
      ALL_POSES = 0b_0000_1111_1111_1111,
      FLYING_POSES = FLY | HOVER,
      SWIMMING_POSES = SWIM | FLOAT,
      STANDING_POSES = STAND | WALK,
      SHOULDER_POSES = SHOULDER_LEFT | SHOULDER_RIGHT,
      UI_POSES = PROFILE | PORTRAIT,
      MOVING_POSES = WALK | SWIM | FLY,
      STATIONARY_POSES = STAND | FLOAT | HOVER,
      //Value for default(PoseType)
      @default = 0b_0000_0000_0000_0000
   }
   public class TransformedPart {
      //I've never used a tuple before.
      public (ValueOrRefrence<float>? x, ValueOrRefrence<float>? y, ValueOrRefrence<float>? z) rotation = (null, null, null);
      public (ValueOrRefrence<float>? x, ValueOrRefrence<float>? y, ValueOrRefrence<float>? z) position = (null, null, null);
      public bool visible = true;

      /// <summary>
      /// Converts the transformedPart into a bone animation.
      /// Do not use in the KotlinPoserVisitor; the entire poser is required.
      /// </summary>
      /// <param name="poser">Required so that variable refrences can be read.</param>
      public Animation.Bone toBone(KotlinPoser poser) {
         var output = new Animation.Bone();
         if (rotation.x != null || rotation.y != null || rotation.z != null) {
            output.Rotation = new MolangVector3(
                resolveValueOrRefrence(rotation.x, poser),
                resolveValueOrRefrence(rotation.y, poser),
                resolveValueOrRefrence(rotation.z, poser)
            );
         }
         if (position.x != null || position.y != null || position.z != null) {
            output.Position = new MolangVector3(
                resolveValueOrRefrence(position.x, poser),
                resolveValueOrRefrence(position.y, poser),
                resolveValueOrRefrence(position.z, poser)
            );
         }
         //THIS IS NOT THE PROPER WAY TO DO THIS
         //TODO: integrate with rendercontroller to make this better (annoying)
         if (!visible) {
            output.Scale = 0.001f;
         }
         return output;
      }
      private static Molang resolveValueOrRefrence(ValueOrRefrence<float>? value, KotlinPoser poser) {
         if (value == null)
            return new Molang(0);
         if (value.refrenceName != null) {
            if (float.TryParse(poser.tryResolveVariable(value.refrenceName), out var @return))
               return new Molang(@return);
         }
         else if (value.value != default) {
            return new Molang(value.value);
         }
         return new Molang(0);
      }
   }
   /// <summary>
   /// Either a value or a variable refrence.
   /// </summary>
   public class ValueOrRefrence<T> {
      public T? value;
      public string? refrenceName;
      public ValueOrRefrence(T value) {
         this.value = value;
      }
      /// <summary>
      /// This contructor sets the refrence only if value is default
      /// </summary>
      public ValueOrRefrence(string refrence, T? value = default) {
         if (value?.Equals(default) != false)
            refrenceName = refrence;
         else
            this.value = value;
      }
      /// <summary>
      /// FOR JSON USE ONLY
      /// </summary>
      [JsonConstructor]
      public ValueOrRefrence() { }
   }
   public static class PoseTypeExtensions {
      /// <summary>
      /// Combines all Poses in a list together.
      /// </summary>
      /// <param name="list"></param>
      /// <returns></returns>
      public static PoseType CombinePoses(this IEnumerable<PoseType> list) {
         PoseType output = default;
         foreach (var poseType in list) {
            output = output | poseType;
         }
         return output;
      }
      /// <summary>
      /// Subtracts all Poses in a list together.
      /// </summary>
      /// <param name="list"></param>
      /// <returns></returns>
      public static PoseType SubtractPoses(this IEnumerable<PoseType> list) {
         if (list.Count() < 1)
            return default;
         var enumerator = list.GetEnumerator();
         enumerator.MoveNext();
         var output = enumerator.Current;
         while (enumerator.MoveNext()) {
            if (output.HasFlag(enumerator.Current))
               output = output - (ushort)enumerator.Current;
         }
         return output;
      }
      public static PoseType Add(this PoseType poseType, PoseType operand) {
         return poseType | operand;
      }
      public static PoseType Subtract(this PoseType poseType, PoseType operand) {
         if (poseType.HasFlag(operand))
            return poseType - (ushort)operand;
         return poseType;
      }
   }
}
