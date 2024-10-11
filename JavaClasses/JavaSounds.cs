using CobbleBuild.BedrockClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static CobbleBuild.Misc;

namespace CobbleBuild.JavaClasses {
   public class JavaSoundJson : Dictionary<string, JavaSoundEvent> {
      public SoundDefinitionJson toBedrock() {
         var output = new SoundDefinitionJson([]);
         foreach (var soundPair in this) {
            var SoundDef = new SoundDefinition();
            SoundDef.category = "neutral";
            SoundDef.sounds = soundPair.Value.sounds.Select(x => x.toBedrock()).ToList();
            SoundDef.max_distance = (int?)soundPair.Value.sounds.Find(x => x.attenuation_distance != null)?.attenuation_distance;

            output.sound_definitions[soundPair.Key] = SoundDef;
         }
         return output;
      }
   }

   public class JavaSoundEvent {
      public List<JavaSound> sounds = [];
      public bool? replace;
      public string? subtitle;

      public JavaSoundEvent() { }
   }
   [JsonConverter(typeof(JavaSoundConverter))]
   public class JavaSound {
      /// <summary>
      /// Refers to a sound location
      /// namespace:sound/location/after/sounds/sound1
      /// Do not include the filename.
      /// </summary>
      public string name;
      /// <summary>
      /// Range 0.0 - 1.0
      /// </summary>
      public float? volume;
      /// <summary>
      /// Range 0.0 - 1.0
      /// </summary>
      public float? pitch;
      public int? weight;
      public bool? stream;
      public float? attenuation_distance;
      public float? preload;
      /// <summary>
      /// Values accepted: "sound" | "event"
      /// "sound" - Indicates name refers to a sound location.
      /// "event" - Indicates name refers to a already defined event
      /// </summary>
      public string? type;
      /// <summary>
      /// Returns true if only the field name is populated
      /// </summary>
      public bool onlySoundName() {
         return !this.GetType().GetFields().Any(x => x.Name != "name" && x.GetValue(this) != null);
      }

      public SoundDefinition.Sound toBedrock() {
         var output = new SoundDefinition.Sound(
              "sounds/" + (tryRemoveNamespace(this.name, out var name) ? name : this.name)
          );
         output.pitch = this.pitch;
         output.weight = this.weight;
         output.stream = this.stream;
         output.weight = this.weight;
         return output;
      }

      public JavaSound(string name) {
         this.name = name;
      }
   }
   class JavaSoundConverter : JsonConverter<JavaSound> {
      public override JavaSound? ReadJson(JsonReader reader, Type objectType, JavaSound? existingValue, bool hasExistingValue, JsonSerializer serializer) {
         var token = JToken.Load(reader);
         if (token.Type == JTokenType.String) {
            return new JavaSound(token.ToObject<string>()!);
         }
         else if (token.Type == JTokenType.Object) {
            var children = token.Children()
               .Where(x => x.Type == JTokenType.Property)
               .Select(x => (JProperty)x)
               .ToDictionary(x => x.Name, x => x.Value);
            if (!children.ContainsKey("name"))
               throw new JsonSerializationException("Java sound does not contain a name.");
            var output = new JavaSound(children["name"].ToObject<string>()!);
            foreach (var item in typeof(JavaSound).GetFields()) {
               if (children.ContainsKey(item.Name)) {
                  item.SetValue(output, children[item.Name].ToObject(item.FieldType));
               }
            }
            return output;
         }
         else {
            throw new JsonSerializationException("JavaSound wrong type of object.");
         }
      }

      public override void WriteJson(JsonWriter writer, JavaSound? value, JsonSerializer serializer) {
         if (value == null) {
            if (serializer.NullValueHandling == NullValueHandling.Ignore)
               return;
            writer.WriteNull();
            return;
         }
         if (value.onlySoundName() == true) {
            writer.WriteValue(value.name);
         }
         else {
            var obj = new JObject();
            foreach (var item in typeof(JavaSound).GetFields()) {
               var val = item.GetValue(value);
               if (val != null)
                  obj[item.Name] = JToken.FromObject(val);
            }
            writer.WriteValue(obj);
            return;
         }
      }
   }
}

