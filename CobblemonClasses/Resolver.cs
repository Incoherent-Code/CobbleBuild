using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobbleBuild.CobblemonClasses {
   public class ResolverJson {
      public string species;
      public int? order;
      public List<ResolverVariation> variations;
   }
   public class ResolverVariation {
      public List<string>? aspects;
      public string? poser;
      public string? model;
      public string? texture;
      public List<Layer>? layers;

      public class Layer {
         public string name;
         public LayerTexture texture;
         public bool? emissive;
         public bool? translucent;

         /// <summary>
         /// Makes sure that layers can be treated the same.
         /// </summary>
         public bool isIntercompatibleWith(Layer other) {
            bool normalStatement = (this.name == other.name && this.emissive == other.emissive && this.translucent == other.translucent);
            if (this.texture.texture != null) {
               //Makes sure that other is also not animated.
               return normalStatement && this.texture.texture != null && other.texture.texture != null;
            }
            else {
               //Makes sure that animations are intercompatible
               return normalStatement && this.texture.loop == other.texture.loop && this.texture.fps == other.texture.fps && this.texture.frames?.Count == other.texture.frames?.Count;
            }
         }
         /// <summary>
         /// Returns "default", "emissive", "animated" or "animated_emissive" depending on the configuration of the entity.
         /// </summary>
         public string getProperMaterial() {
            //Not animated
            if (texture.texture != null) {
               if (emissive == true)
                  return "emissive";
               else
                  return "default";
            }
            //Animated
            else {
               if (emissive == true)
                  return "animated_emissive";
               else
                  return "animated";
            }
         }
      }
      /// <summary>
      /// Texture for layer
      /// If .texture is null, then it is assumed to be an animation, and the other properties should not be null.
      /// </summary>
      [JsonConverter(typeof(LayerTextureConverter))]
      public class LayerTexture {
         /// <summary>
         /// If null, then other props should create an animation.
         /// </summary>
         public string? texture; //Texture; If it is null, it will be assumed that it is keyframed using the other properties
         public List<string>? frames;
         public int? fps;
         public bool? loop;
      }

      public class LayerTextureConverter : JsonConverter<LayerTexture> {
         public override void WriteJson(JsonWriter writer, LayerTexture value, JsonSerializer serializer) {
            if (value.texture != null) {
               serializer.Serialize(writer, value.texture);
            }
            else {
               serializer.Serialize(writer, value);
            }
         }

         public override LayerTexture ReadJson(JsonReader reader, Type objectType, LayerTexture existingValue, bool hasExistingValue, JsonSerializer serializer) {
            LayerTexture output = new LayerTexture();
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Object) {
               JObject obj = token.ToObject<JObject>();
               foreach (JToken tkn in obj.Children()) {
                  if (tkn.Type == JTokenType.Property) {
                     JProperty property = tkn.ToObject<JProperty>();
                     if (property.Name == "frames") {
                        output.frames = property.Value.ToObject<List<string>>();
                     }
                     if (property.Name == "fps") {
                        output.fps = property.Value.ToObject<int>();
                     }
                     if (property.Name == "loop") {
                        output.loop = property.Value.ToObject<bool>();
                     }
                  }
                  else {
                     Misc.warn("Layer Texture Deserializer: Unreadable Property found");
                  }
               }
            }
            else if (token.Type == JTokenType.String) {
               output.texture = token.ToObject<string>();
            }
            return output;
         }
      }
   }
}
