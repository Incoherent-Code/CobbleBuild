namespace CobbleBuild.BedrockClasses {
   public class AnimationControllerJson {
      public string format_version = "1.10.0";
      public Dictionary<string, AnimationController> animation_controllers;
      public AnimationControllerJson(string animationControllerName, AnimationController animationController) {
         animation_controllers = new Dictionary<string, AnimationController>() { { animationControllerName, animationController } };
      }
      public AnimationControllerJson() {
         animation_controllers = [];
      }
   }
   public class AnimationController {
      public string? initial_state;
      public Dictionary<string, State> states = new Dictionary<string, State>();

      public class State {
         public List<StringOrPropertyAndString>? animations;
         //Shoud be singlePropertyObject but I was getting deserialization errors.
         public List<StringOrPropertyAndString>? transitions;
         public float? blend_transition;
         public Dictionary<string, AnimationVariable>? variables;
         public State(List<StringOrPropertyAndString> animations) {
            this.animations = animations;
         }
         public State() { }
      }

      public class AnimationVariable {
         public string? input; //Molang Expression
         public Dictionary<string, float>? remap_curve;
         public AnimationVariable(string input) {
            this.input = input;
         }
      }
   }
}
