using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using CobbleBuild.ConversionTechnology;
using CobbleBuild.Kotlin;

namespace CobbleBuild.DataCreator {
   public static class AnimationControllerCreator {
      /// <summary>
      /// Can be used to apply blend_transitions globally to a minimum number
      /// </summary>
      public static float minimumTransitionTime = 0.5f;
      //Just for poseTransitions; Adds to the condition if already exists.
      private static void AddOptionalCondition(this Dictionary<string, string> poseTransitions, string name, string molangExp) {
         if (!poseTransitions.ContainsKey(name))
            poseTransitions.Add(name, molangExp);
         else
            poseTransitions[name] += $"|| {molangExp}";
      }
      private static void AddCondition(this Dictionary<string, string> poseTransitions, string name, string molangExp) {
         if (!poseTransitions.ContainsKey(name))
            poseTransitions.Add(name, molangExp);
         else
            poseTransitions[name] += $"&& {molangExp}";
      }
      public static AnimationControllerJson? Create(ref Pokemon pokemon, ref ClientEntityJson entity) {
         var output = new AnimationControllerJson();

         for (int i = 0; i < pokemon.Variations.Count; i++) {
            var variation = pokemon.Variations[i];
            string poserIdentifier = variation.poserIdentifier.StartsWith("cobblemon:") ? variation.poserIdentifier.Substring(10) : variation.poserIdentifier;
            if (!PoserRegistry.posers.TryGetValue(poserIdentifier!, out var poser)) {
               if (!(PoserRegistry.posers.Count == 0))
                  Misc.warn($"Couldn't find poser {poserIdentifier}");
               continue;
            }
            if (poser.poses.Count == 0) {
               Misc.warn($"Poser {poserIdentifier} has no poses!");
               return null;
            }
            string poserID = $"controller.animation.cobblemon.{poserIdentifier.ToLower()}";
            //If poser has not been translated into an animationController yet
            if (!output.animation_controllers.ContainsKey(poserID)) {
               var animController = new AnimationController();
               //Determine initial state
               string initialState = poser.poses.Any(x => x.Key == "standing") ? "standing" : poser.poses.First().Key;
               animController.initial_state = initialState;
               //Transitions that should be added to all poses
               //Key: poseName, value: molang for transition
               Dictionary<string, string> poseTransitions = [];
               foreach (var poseValuePair in poser.poses) {
                  var name = poseValuePair.Key;
                  var pose = poseValuePair.Value;
                  var state = new AnimationController.State() { animations = [] };
                  if (pose.transformTicks != null && ((float)pose.transformTicks / 20f) > minimumTransitionTime)
                     state.blend_transition = (float)pose.transformTicks / 20f; //convert to seconds
                  else
                     state.blend_transition = minimumTransitionTime;
                  foreach (var animation in pose.idleAnimations) {
                     var resolved = AnimationIngest.Resolve(animation, poser, ref entity.client_entity, ref pokemon);
                     if (resolved != null)
                        state.animations.Add(new StringOrPropertyAndString(resolved));
                  }
                  if (pose.transformedParts.Count > 0) {
                     var outAnimation = new Animation() { bones = [] };
                     string animationID = $"{poserIdentifier.ToLower()}.{pose.poseName.ToLower()}.transform";
                     string fullAnimationID = $"animation.{animationID}";
                     //Handle TransformParts
                     foreach (var transformedPart in pose.transformedParts) {
                        poser.registeredBodyParts.TryGetValue(transformedPart.Key, out string? part);
                        var transformedBoneName = part ?? transformedPart.Key;
                        var bone = transformedPart.Value.toBone(poser);
                        outAnimation.bones.Add(transformedBoneName, bone);
                     }
                     entity.client_entity.description.animations.Add(animationID, fullAnimationID);
                     pokemon.animationData.Add(fullAnimationID, outAnimation);
                     state.animations.Add(new StringOrPropertyAndString(animationID));
                  }

                  //Handle transitions
                  if (pose.poseTypes.HasFlag(PoseType.MOVING_POSES)) {
                     poseTransitions.AddOptionalCondition(name, "q.is_moving");
                  }
                  else {
                     if (pose.poseTypes.HasFlag(PoseType.WALK)) {
                        poseTransitions.AddOptionalCondition(name, "(q.is_moving && !q.is_swimming)");
                     }
                     if (pose.poseTypes.HasFlag(PoseType.SWIM)) {
                        poseTransitions.AddOptionalCondition(name, "q.is_swimming");
                     }
                     //if (pose.poseTypes.HasFlag(PoseType.FLY))
                  }

                  if (pose.poseTypes.HasFlag(PoseType.STAND)) {
                     poseTransitions.AddOptionalCondition(name, "!q.is_moving");
                  }

                  if (pose.poseTypes.HasFlag(PoseType.SLEEP)) {
                     poseTransitions.AddCondition(name, "q.is_sleeping");
                  }
                  if (pose.condition != null) {
                     if (poseTransitions.ContainsKey(name)) {
                        poseTransitions[name] = $"({poseTransitions[name]}) && ({pose.condition})";
                     }
                     else {
                        poseTransitions[name] = pose.condition;
                     }
                  }
                  if (state.animations?.Count == 0)
                     state.animations = null;

                  animController.states.Add(name, state);
               }

               if (entity.client_entity.description.scripts.initialize == null)
                  entity.client_entity.description.scripts.initialize = [];
               var poserVar = $"v.state_of_{poserIdentifier.ToLower()}";
               entity.client_entity.description.scripts.initialize.Add($"{poserVar} = -1.0;");

               //Add transitions to other states
               for (int j = 0; j < animController.states.Count; j++) {
                  var stateKey = animController.states.Keys.ToArray()[j];
                  if (animController.states[stateKey].transitions == null)
                     animController.states[stateKey].transitions = [];

                  foreach (var transition in poseTransitions) {
                     if (transition.Key != stateKey)
                        animController.states[stateKey].transitions.Add(new SinglePropertyObject(transition.Key, transition.Value));
                  }

                  //Adds a declaration to update a variable on what pose the entity is currently in
                  if (animController.states[stateKey].transitions!.Count > 0) {
                     animController.states[stateKey].transitions[0].value = $"{poserVar} = {j}; return {animController.states[stateKey].transitions[0].value};";
                  }
               }

               foreach (var quirkKey in poser.quirks.Keys) {
                  var quirk = poser.quirks[quirkKey];
                  string controllerID = $"controller.animation.{poserIdentifier}.{quirkKey}";

                  //Create quirk condition
                  string? quirkCondition = null;
                  var poseArray = poser.poses.Values.ToArray();
                  for (int j = 0; j < poseArray.Length; j++) {
                     var pose = poseArray[j];
                     if (pose.quirks != null && pose.quirks.Contains(quirkKey)) {
                        if (quirkCondition == null)
                           quirkCondition = $"{poserVar} == {j}";
                        else
                           quirkCondition += $" || {poserVar} == {j}";
                     }
                  }

                  if (!output.animation_controllers.ContainsKey(controllerID)) {
                     var blinkController = Animations.createQuirkController(quirk, quirkKey, quirkCondition, poser, ref entity.client_entity, ref pokemon);
                     if (blinkController == null)
                        continue;
                     //Appearently animation controllers don't get their own scope for variable declarations...
                     foreach (var key in blinkController.states.Keys) {
                        blinkController.states[key].transitions[0].value =
                        blinkController.states[key].transitions[0].value
                            .Replace("v.next_quirk_time", $"v.next_quirk_{poserIdentifier}_{quirkKey}")
                            .Replace("v.quirk_end_time", $"v.quirk_end_{poserIdentifier}_{quirkKey}");
                     }

                     output.animation_controllers.Add(controllerID, blinkController);
                     entity.client_entity.description.animations.Add(controllerID, controllerID);
                     entity.client_entity.description.scripts.animate.Add(new StringOrPropertyAndString(controllerID));
                  }
               }

               output.animation_controllers.Add(poserID, animController);
               entity.client_entity.description.animations.Add(poserID, poserID);
               entity.client_entity.description.scripts.animate.Add(new StringOrPropertyAndString(poserID, $"q.variant == {i}"));
            }
            else {
               var index = entity.client_entity.description.scripts.animate.FindIndex(x => x.String == poserID);
               if (index == -1)
                  throw new Exception($"Somehow, {poserID} is not in entity description.");
               entity.client_entity.description.scripts.animate[index].value += $" || q.variant == {i}";
            }

         }
         return output;
      }
   }
}
