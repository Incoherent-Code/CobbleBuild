using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;

namespace CobbleBuild.DataCreator {
   public class ClientEntityCreator {
      public static ClientEntityJson Create(ref Pokemon pokemon, out RenderControllerJson? renderControllerJson, out AnimationControllerJson? animationControllerJson) {
         ClientEntityJson output = new ClientEntityJson(new ClientEntity(pokemon.identifier));
         renderControllerJson = null;
         animationControllerJson = null;
         //Sets up the data
         output.client_entity.description.render_controllers = new List<string>();
         output.client_entity.description.textures = new Dictionary<string, string>() { { "blank", "textures/blank" } };
         output.client_entity.description.geometry = new Dictionary<string, string>();
         output.client_entity.description.materials = new Dictionary<string, string>() {
            { "default", "entity_alphatest" },
            { "animated", "cobblemon_animated" },
            { "emissive", "entity_emissive_alpha" },
            { "animated_emissive", "cobblemon_animated_emissive" }
         };
         output.client_entity.description.animations = new Dictionary<string, string>();
         output.client_entity.description.scripts = new ClientEntity.ScriptField();
         output.client_entity.description.scripts.animate = new List<StringOrPropertyAndString>();

         //Adds Textures and Geometry of each variation to the client Entity
         foreach (Variation v in pokemon.Variations) {
            output.client_entity.description.textures.Add(v.variantName, v.texturePartialPath);
            output.client_entity.description.geometry.Add(v.variantName, "geometry." + v.geometryName);
            //Layer textures are handled by RenderControllerCreator
         }
         //We can no longer use the standard controllers because transformedParts.withVisible() needs to modify the render controller.
         ////Selects Render Controller
         //if (pokemon.Variations.Any(x => x.variantName == "female") && pokemon.Variations.Count == 4 && pokemon.Variations.First().layerData.Count == 0) {
         //    output.client_entity.description.render_controllers.Add("controller.render.cobblemon.gendered");
         //}
         //else if (pokemon.Variations.Any(x => x.variantName == "base") && pokemon.Variations.Count == 2 && pokemon.Variations.First().layerData.Count == 0) {
         //    output.client_entity.description.render_controllers.Add("controller.render.cobblemon.standard");
         //}
         //else //Requires custom render controller
         //{
         renderControllerJson = RenderControllerCreator.Create(pokemon, ref output);
         output.client_entity.description.render_controllers.Add($"controller.render.cobblemon.{pokemon.shortName}");
         //}

         animationControllerJson = AnimationControllerCreator.Create(ref pokemon, ref output);

         //Bedrock Complains if they have no children.
         if (output.client_entity.description.animations.Count < 1) {
            output.client_entity.description.animations = null;
            //output.client_entity.description.scripts = null;
         }
         if (output.client_entity.description.scripts.animate.Count < 1) {
            output.client_entity.description.scripts.animate = null;
         }
         return output;
      }
      public static ClientEntityJson Create(PokeballResourceData pokeball, AnimationJson animations) {
         ClientEntityJson output = new ClientEntityJson(new ClientEntity(pokeball.pokeball));
         string name = pokeball.pokeball.Split(":")[1];
         //Sets up the data
         output.client_entity.description.render_controllers = new List<string>();
         output.client_entity.description.textures = new Dictionary<string, string>();
         output.client_entity.description.geometry = new Dictionary<string, string>();
         output.client_entity.description.materials = new Dictionary<string, string>() { { "default", "entity_alphatest" } };
         output.client_entity.description.animations = new Dictionary<string, string>();
         //output.client_entity.description.scripts = new ClientEntity.ScriptField();
         //output.client_entity.description.scripts.animate = new List<StringOrPropertyAndString>()
         //{
         //    new StringOrPropertyAndString("throw", "q.is_moving && !q.is_on_ground")
         //};

         //Selects Render Controller
         output.client_entity.description.render_controllers.Add("controller.render.cow");


         //Adds Textures and Geometry
         output.client_entity.description.textures.Add("default", "textures/pokeballs/" + name);
         output.client_entity.description.geometry.Add("default", "geometry.pokeball");


         //Adds Animations to the client entity
         foreach (string animationName in animations.animations.Keys.ToList()) {
            output.client_entity.description.animations.Add(animationName.Split(".")[2], animationName);
         }

         //Bedrock complains if there are no clildren in these fields.
         //if (output.client_entity.description.animations.Count < 1) {
         //    output.client_entity.description.animations = null;
         //}
         //if (output.client_entity.description.scripts.animate.Count < 1) {
         //    output.client_entity.description.scripts = null;
         //}
         //Adds Arrow Movement
         //output.client_entity.description.scripts.pre_animation = new List<string>() {
         //"variable.shake = query.shake_time - query.frame_alpha;"
         //};
         //output.client_entity.description.animations.Add("move", "animation.arrow.move");
         //output.client_entity.description.scripts.animate.Add(new StringOrPropertyAndString("move", "q.is_moving"));

         return output;
      }
      /// <summary>
      /// Dummy used for the healing machine
      /// </summary>
      public static ClientEntityJson CreatePokeballDummy(PokeballResourceData pokeball, AnimationJson animations) {
         ClientEntityJson output = new ClientEntityJson(new ClientEntity(pokeball.pokeball + "_dummy"));
         string name = pokeball.pokeball.Split(":")[1];
         //Sets up the data
         output.client_entity.description.render_controllers = new List<string>();
         output.client_entity.description.textures = new Dictionary<string, string>();
         output.client_entity.description.geometry = new Dictionary<string, string>();
         output.client_entity.description.materials = new Dictionary<string, string>() { { "default", "entity_alphatest" } };
         output.client_entity.description.animations = new Dictionary<string, string>();
         output.client_entity.description.scripts = new ClientEntity.ScriptField();
         output.client_entity.description.scripts.animate = new List<StringOrPropertyAndString>();

         //Selects Render Controller
         output.client_entity.description.render_controllers.Add("controller.render.cow");


         //Adds Textures and Geometry
         output.client_entity.description.textures.Add("default", "textures/pokeballs/" + name);
         output.client_entity.description.geometry.Add("default", "geometry.pokeball");


         //Adds Animations to the client entity
         foreach (string animationName in animations.animations.Keys.ToList()) {
            output.client_entity.description.animations.Add(animationName.Split(".")[2], animationName);
         }


         //Bedrock Complains if they have no children. Me personally, I don't have a problem with not having children.
         if (output.client_entity.description.animations.Count < 1) {
            output.client_entity.description.animations = null;
         }
         if (output.client_entity.description.scripts.animate.Count < 1) {
            output.client_entity.description.scripts = null;
         }
         //Adds Arrow Movement
         //output.client_entity.description.scripts.pre_animation = new List<string>() {
         //"variable.shake = query.shake_time - query.frame_alpha;",
         //"variable.shake_power = variable.shake > 0.0 ? -Math.sin(variable.shake * 200.0) * variable.shake : 0.0;"
         //};
         //output.client_entity.description.animations.Add("move", "animation.arrow.move");
         //output.client_entity.description.scripts.animate.Add(new Dictionary<string, string>() { { "move", "query.is_moving" } });

         return output;
      }
   }
}
