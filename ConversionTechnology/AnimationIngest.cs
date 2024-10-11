using CobbleBuild.BedrockClasses;
using CobbleBuild.Kotlin;
using Newtonsoft.Json;
using System.Text;

namespace CobbleBuild.ConversionTechnology {
   internal static class AnimationIngest {
      /// <summary>
      /// Finds an argument in the animation provided using the provided string and returns it using the parseFunction to convert it from String
      /// </summary>
      /// <param name="argumentName">Name of argument to find (directly compared against arguments in animation.kotlin_args)</param>
      /// <param name="defaultValue">Default if value not found in kotliln_args</param>
      /// <param name="animation">Animation Refrence to search</param>
      /// <param name="parseFunction">Function to convert from StringOrPropertyAndString to T</param>
      /// <returns>Parsed item or defaultValue</returns>
      private static T FindArgumentFor<T>(string argumentName, T defaultValue, AnimationRefrence animation, Func<KotlinArgument, T> parseFunction) {
         if (animation.kotlin_arguments == null)
            return defaultValue;
         if (animation.kotlin_arguments.Any(x => x.Name == argumentName)) {
            return animation.kotlin_arguments
                .Where(x => x.Name == argumentName)
                .Select(parseFunction)
                .FirstOrDefault(defaultValue);
         }
         return defaultValue;
      }
      public static string? Resolve(AnimationRefrence refrence, KotlinPoser poser, ref ClientEntity entity, ref Pokemon pokemon) {
         if (refrence.type == AnimationType.BEDROCK) {
            string animationId = $"{refrence.bedrock_animation_file}.{refrence.refrenceName}";
            if (!Program.animations.TryGetValue($"animation.{animationId}", out var animation)) {
               Misc.warn("Unknown Animation " + animationId);
               return null;
            }
            entity.description.animations.TryAdd(animationId, "animation." + animationId);
            pokemon.animationData.TryAdd($"animation.{animationId}", animation);
            return animationId;
         }
         else {
            //Technically this doesnt allow for different parameters in the same poser, but its alright for now.
            string animationId = $"{poser.poserName}.{refrence.refrenceName}".ToLower();
            //Animation already implimented
            if (entity.description.animations.Keys.Contains(animationId))
               return animationId;

            Animation? animation = null;
            switch (refrence.refrenceName) {
               case "QuadrupedWalkAnimation":
               case "BipedWalkAnimation":
                  float periodMultiplier = FindArgumentFor("periodMultiplier", 0.662f, refrence, x => float.Parse(x.Value.Substring(0, x.Value.Length - 1)));
                  float amplitudeMultiplier = FindArgumentFor("amplitudeMultiplier", 1.4f, refrence, x => float.Parse(x.Value.Substring(0, x.Value.Length - 1)));
                  animation = (refrence.refrenceName == "QuadrupedWalkAnimation")
                      ? Animations.getQuadrupedWalk(periodMultiplier, amplitudeMultiplier)
                      : Animations.getBipedWalk(periodMultiplier, amplitudeMultiplier);
                  break;
               case "BimanualSwingAnimation":
                  float swingPeriodMultiplier = FindArgumentFor("swingPeriodMultiplier", 0.662f, refrence, x => float.Parse(x.Value.Substring(0, x.Value.Length - 1)));
                  float amplitudeMultiplier2 = FindArgumentFor("amplitudeMultiplier", 1f, refrence, x => float.Parse(x.Value.Substring(0, x.Value.Length - 1)));
                  animation = Animations.getBimanualSwing(swingPeriodMultiplier, amplitudeMultiplier2);
                  break;
               case "singleBoneLook":
                  bool invertX = FindArgumentFor("invertX", false, refrence, x => bool.Parse(x.Value));
                  bool invertY = FindArgumentFor("invertY", false, refrence, x => bool.Parse(x.Value));
                  bool disableX = FindArgumentFor("disableX", false, refrence, x => bool.Parse(x.Value));
                  bool disableY = FindArgumentFor("disableY", false, refrence, x => bool.Parse(x.Value));
                  animation = Animations.getSingleBoneLook(invertX, invertY, disableX, disableY);
                  break;
            }
            if (animation == null)
               return null;
            //Maps the bones of the animation to the correct bone specified by the poser.
            foreach (var bone in animation.bones.ToList()) {
               if (poser.registeredBodyParts.ContainsKey(bone.Key) && poser.registeredBodyParts[bone.Key] != bone.Key) {
                  animation.bones.Remove(bone.Key);
                  animation.bones[poser.registeredBodyParts[bone.Key]] = bone.Value;
               }
            }
            if (pokemon.animationData == null)
               pokemon.animationData = [];
            pokemon.animationData.Add("animation." + animationId, animation);
            entity.description.animations.Add(animationId, "animation." + animationId);
            return animationId;
         }
      }
   }
   public static class Animations {
      /// <summary>
      /// Get the Serialized JSON of an internal animation (Animation.resx)
      /// </summary>
      /// <param name="resourceID">Resource ID from Animations.resx</param>
      /// <exception cref="ArgumentException">Invalid Resource ID</exception>
      private static string GetInternalAnimationString(string resourceID) {
         var animation = CobbleBuild.InternalAnimations.ResourceManager.GetObject(resourceID);
         if (animation == null || !(animation is byte[])) {
            throw new ArgumentException("Invalid Resource ID");
         }
         return Encoding.Default.GetString(animation as byte[]);
      }
      /// <summary>
      /// Get an internal animation.
      /// </summary>
      /// <param name="resourceID">Resource ID from Animations.resx</param>
      /// <exception cref="ArgumentException">Invalid Resource ID</exception>
      private static Animation GetInernalAnimation(string resourceID) {
         return JsonConvert.DeserializeObject<AnimationJson>(GetInternalAnimationString(resourceID))
             .animations
             .First()
             .Value;
      }
      public static Animation getQuadrupedWalk(float periodMultiplier = 0.662f, float amplitudeMultiplier = 1.4f) {
         var animation = GetInternalAnimationString("QuadrupedWalk")
             .Replace("v.periodMultiplier", periodMultiplier.ToString())
             .Replace("v.amplitudeMultiplier", amplitudeMultiplier.ToString());

         return JsonConvert.DeserializeObject<AnimationJson>(animation)
             .animations
             .First()
             .Value;
      }
      public static Animation getBipedWalk(float periodMultiplier = 0.662f, float amplitudeMultiplier = 1.4f) {
         var animation = GetInternalAnimationString("BipedWalk")
             .Replace("v.periodMultiplier", periodMultiplier.ToString())
             .Replace("v.amplitudeMultiplier", amplitudeMultiplier.ToString());

         return JsonConvert.DeserializeObject<AnimationJson>(animation)
             .animations
             .First()
             .Value;
      }
      public static Animation getBimanualSwing(float swingPeriodMultiplier = 0.662f, float amplitudeMultiplier = 1f) {
         var animation = GetInternalAnimationString("BimanualSwing")
             .Replace("v.periodMultiplier", swingPeriodMultiplier.ToString())
             .Replace("v.amplitudeMultiplier", amplitudeMultiplier.ToString());

         return JsonConvert.DeserializeObject<AnimationJson>(animation)
             .animations
             .First()
             .Value;
      }
      /// <summary>
      /// TODO: Impliment these bools (rn they're unused in 1.4.1 codebase but maybe they'll be used later)
      /// </summary>
      public static Animation getSingleBoneLook(bool invertX = false, bool invertY = false, bool disableX = false, bool disableY = false) {
         return GetInernalAnimation("SingleBoneLook");
      }
      /// <summary>
      /// Creates animation controller that handles quirks.
      /// Most of these argument are here so that this can call AnimationIngest.Resolve() on its own.
      /// </summary>
      /// <param name="quirkKey">Name of quirk as it is refrenced. Important because it will be distinct per quirk definition.</param>
      public static AnimationController? createQuirkController(Quirk quirk, string quirkKey, string? condition, KotlinPoser poser, ref ClientEntity entity, ref Pokemon pokemon) {
         var blinkController = JsonConvert.DeserializeObject<AnimationControllerJson>(GetInternalAnimationString("QuirkController"))?.animation_controllers.Values.FirstOrDefault();
         if (blinkController == null) //Shouldn't happen unless Animations.resx is messed up.
            throw new Exception("Unable to retrieve QuirkController from internal resources!");

         var resolvedAnimation = AnimationIngest.Resolve(quirk.statefulAnimation, poser, ref entity, ref pokemon);
         if (resolvedAnimation == null) {
            Misc.warn($"Could not resolve animation of quirk {quirkKey}");
            return null;
         }
         var secondsBetweenOccurences = quirk.secondsBetweenOccurences ?? (2f, 7f);

         foreach (var key in blinkController.states.Keys) {
            blinkController.states[key].transitions[0].value =
            blinkController.states[key].transitions[0].value
                .Replace("v.min_quirk_time", secondsBetweenOccurences.Item1.ToString())
                .Replace("v.max_quirk_time", secondsBetweenOccurences.Item2.ToString())
                .Replace("v.condition", condition ?? "true");
         }

         blinkController.states["quirk_active"].animations = [new StringOrPropertyAndString(resolvedAnimation)];

         return blinkController;
      }
   }
}
