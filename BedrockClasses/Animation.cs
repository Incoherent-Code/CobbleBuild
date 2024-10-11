using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobbleBuild.BedrockClasses {

   public class AnimationJson {
      public string format_version;
      public Dictionary<string, Animation> animations;
      public AnimationJson(string formatVersion, Dictionary<string, Animation> Animations) {
         format_version = formatVersion;
         animations = Animations;
      }
   }

   public class Animation {
      public object? loop;
      public float? animation_length;
      public Molang? start_delay;
      public Molang? loop_delay;
      public Molang? anim_time_update;
      public Molang? blend_weight;
      public bool? override_previous_animation;
      public Dictionary<string, Bone>? bones;
      //I spent awhile trying to make a more elegant solution for this, but I think this is the best.
      public Dictionary<string, objectOrObjectArray>? sound_effects;
      public Dictionary<string, objectOrObjectArray>? particle_effects;
      /// <summary>
      /// Class representing Bones in animation.
      /// Keyframe data should only not be null if the standard molang vector is null.
      /// Basically that property can be one or the other.
      /// Scale can also be just a float.
      /// </summary>
      [JsonConverter(typeof(BoneConverter))]
      public class Bone {
         //Only Rotation or KeyframeRotation should be not null in any case (same with position)
         public MolangVector3? Rotation;
         public Dictionary<string, KeyframeData>? KeyframeRotation;

         public Dictionary<string, KeyframeData>? KeyframePosition;
         public MolangVector3? Position;

         //Appearently there's also a scale property that can be just a single float
         public float? Scale;
         public MolangVector3? ScaleArray;
         public Dictionary<string, KeyframeData>? KeyframeScale;
         public Bone() { }
      }
      //A Keyframe can either be a static molang vector or the extraData class
      public class KeyframeData {
         //Either data type should be not null but not both.
         public MolangVector3? standardArray;
         public KeyframeExtraData? extraData;

         public KeyframeData() { }
      }
      public class KeyframeExtraData {
         //Technically Bedrock does not support Molang in KeyframeExtraData but ig the java engine does because cobblemon uses it
         public MolangVector3? pre;
         public MolangVector3? post;
         public string? lerp_mode;

         public KeyframeExtraData() { }
      }
      [JsonConverter(typeof(SoundEffectConverter))]
      public class SoundEffectArray : Either<SoundEffect, SoundEffect[]> { }
      public class SoundEffect {
         public string? effect;
         public SoundEffect() { }
      }
      private class SoundEffectConverter : EitherSerializer<SoundEffect, SoundEffect[]> {
         public override bool IsLeft(JToken token) {
            return !(token.Type == JTokenType.Array);
         }

         public override bool IsRight(JToken token) {
            return token.Type == JTokenType.Array;
         }
      }
      [JsonConverter(typeof(ParticleEffectArray))]
      public class ParticleEffectArray : Either<ParticleEffect, ParticleEffect[]> { }
      public class ParticleEffect(string effect) {
         public string effect = effect;
         public string? locator;
         public Molang? pre_effect_script;
         public bool? bind_to_actor;
      }
      private class ParticleEffectConverter : EitherSerializer<ParticleEffect, ParticleEffect[]> {
         public override bool IsLeft(JToken token) {
            return !(token.Type == JTokenType.Array);
         }

         public override bool IsRight(JToken token) {
            return token.Type == JTokenType.Array;
         }
      }
      //Bone Custom JsonConverter
      public class BoneConverter : JsonConverter<Bone> {
         public override void WriteJson(JsonWriter writer, Bone value, JsonSerializer serializer) {
            //Serialize Rotation Data
            if (value.Rotation != null) {
               //If standard molang vector, serialize it as such
               JProperty rotation = new JProperty("rotation", JToken.FromObject(value.Rotation));
               writer.WriteStartObject();
               rotation.WriteTo(writer);
            }
            else if (value.KeyframeRotation != null) {
               //Sort through the keyframes to either make it an array keyframe or an object keyframe
               List<JProperty> properties = new List<JProperty>();
               foreach (KeyValuePair<string, KeyframeData> Pair in value.KeyframeRotation) {
                  if (Pair.Value.standardArray != null) {
                     properties.Add(new JProperty(Pair.Key, JToken.FromObject(Pair.Value.standardArray)));
                  }
                  else if (Pair.Value.extraData != null) {
                     JObject obj = new JObject();
                     if (Pair.Value.extraData.pre != null) {
                        obj.Add("pre", Pair.Value.extraData.pre.toJArray());
                     }
                     if (Pair.Value.extraData.post != null) {
                        obj.Add("post", Pair.Value.extraData.post.toJArray());
                     }
                     if (Pair.Value.extraData.lerp_mode != null) {
                        obj.Add("lerp_mode", new JValue(Pair.Value.extraData.lerp_mode));
                     }
                     properties.Add(new JProperty(Pair.Key, obj));
                  }
               }
               JObject keyframes = new JObject(properties);
               JProperty rotation = new JProperty("rotation", JToken.FromObject(keyframes));
               writer.WriteStartObject();
               rotation.WriteTo(writer);
            }
            else {
               writer.WriteStartObject();
            }

            //Serialize Scale Data
            //I put it in the middle so I didn't interupt the WriteStartObject and WriteEndObject Structure I had going
            if (value.Scale != null) {
               //If just an float, write it as a float
               JProperty scale = new JProperty("scale", value.Scale);
               scale.WriteTo(writer);
            }
            if (value.ScaleArray != null) {
               //If just an array, write it as just an array
               JProperty scale = new JProperty("scale", value.ScaleArray.toJArray());
               scale.WriteTo(writer);
            }
            else if (value.KeyframeScale != null) {
               //Sort through the keyframes to either make it an array keyframe or an object keyframe
               List<JProperty> properties = new List<JProperty>();
               foreach (KeyValuePair<string, KeyframeData> Pair in value.KeyframeScale) {
                  if (Pair.Value.standardArray != null) {
                     properties.Add(new JProperty(Pair.Key, Pair.Value.standardArray.toJArray()));
                  }
                  else if (Pair.Value.extraData != null) {
                     JObject obj = new JObject();
                     if (Pair.Value.extraData.pre != null) {
                        obj.Add("pre", Pair.Value.extraData.pre.toJArray());
                     }
                     if (Pair.Value.extraData.post != null) {
                        obj.Add("post", Pair.Value.extraData.post.toJArray());
                     }
                     if (Pair.Value.extraData.lerp_mode != null) {
                        obj.Add("lerp_mode", new JValue(Pair.Value.extraData.lerp_mode));
                     }
                     properties.Add(new JProperty(Pair.Key, obj));
                  }
               }
               JObject keyframes = new JObject(properties);
               JProperty scale = new JProperty("scale", keyframes);
               scale.WriteTo(writer);
            }

            //Serialize Position Data
            if (value.Position != null) {
               //If just a molang vector, write it as such
               JProperty position = new JProperty("position", JToken.FromObject(value.Position));
               position.WriteTo(writer);
               writer.WriteEndObject();
            }
            else if (value.KeyframePosition != null) {
               //Sort through the keyframes to either make it an array keyframe or an object keyframe
               List<JProperty> properties = new List<JProperty>();
               foreach (KeyValuePair<string, KeyframeData> Pair in value.KeyframePosition) {
                  if (Pair.Value.standardArray != null) {
                     properties.Add(new JProperty(Pair.Key, JToken.FromObject(Pair.Value.standardArray)));
                  }
                  else if (Pair.Value.extraData != null) {
                     JObject obj = new JObject();
                     if (Pair.Value.extraData.pre != null) {
                        obj.Add("pre", Pair.Value.extraData.pre.toJArray());
                     }
                     if (Pair.Value.extraData.post != null) {
                        obj.Add("post", Pair.Value.extraData.post.toJArray());
                     }
                     if (Pair.Value.extraData.lerp_mode != null) {
                        obj.Add("lerp_mode", new JValue(Pair.Value.extraData.lerp_mode));
                     }
                     properties.Add(new JProperty(Pair.Key, obj));
                  }
               }
               JObject keyframes = new JObject(properties);
               JProperty position = new JProperty("position", keyframes);
               position.WriteTo(writer);
               writer.WriteEndObject();
            }
            else {
               writer.WriteEndObject();
            }
         }

         public override Bone ReadJson(JsonReader reader, Type objectType, Bone existingValue, bool hasExistingValue, JsonSerializer serializer) {
            Bone output = new Bone();
            JObject jo = JObject.Load(reader);

            //Deserialize rotation data
            JToken rotation = jo["rotation"];
            if (rotation != null) {
               if (rotation.Type == JTokenType.Array) {
                  //If its a normal array, make it just rotation property
                  output.Rotation = rotation.ToObject<MolangVector3>();
               }
               else if (rotation.Type == JTokenType.Object) {
                  //If its object, break into keyframes, either setting the StandardArray property if array or extradata if its more
                  Dictionary<string, KeyframeData> subOutput = new Dictionary<string, KeyframeData>();
                  JEnumerable<JToken> children = rotation.Children();
                  foreach (JProperty child in children) {
                     if (child.Value.Type == JTokenType.Array) {
                        KeyframeData data = new KeyframeData();
                        data.standardArray = child.Value.ToObject<MolangVector3>();
                        subOutput.Add(child.Name, data);
                     }
                     else if (child.Value.Type == JTokenType.Object) {

                        KeyframeData data = new KeyframeData();
                        KeyframeExtraData extraData = child.Value.ToObject<KeyframeExtraData>();
                        data.extraData = extraData;
                        subOutput.Add(child.Name, data);
                     }
                  }
                  output.KeyframeRotation = subOutput;
               }
            }

            //Deserialize position data
            JToken position = jo["position"];
            if (position != null) {
               if (position.Type == JTokenType.Array) {
                  //If its a normal array, make it just rotation property
                  output.Position = position.ToObject<MolangVector3>();
               }
               else if (position.Type == JTokenType.Object) {
                  //If its object, break into keyframes, either setting the StandardArray property if array or extradata if its more
                  Dictionary<string, KeyframeData> subOutput = new Dictionary<string, KeyframeData>();
                  JEnumerable<JToken> children = position.Children();
                  foreach (JProperty child in children) {
                     if (child.Value.Type == JTokenType.Array) {
                        KeyframeData data = new KeyframeData();
                        data.standardArray = child.Value.ToObject<MolangVector3>();
                        subOutput.Add(child.Name, data);
                     }
                     else if (child.Value.Type == JTokenType.Object) {

                        KeyframeData data = new KeyframeData();
                        KeyframeExtraData extraData = child.Value.ToObject<KeyframeExtraData>();
                        data.extraData = extraData;
                        subOutput.Add(child.Name, data);
                     }
                  }
                  output.KeyframePosition = subOutput;
               }
            }

            //Deserialize scale data
            JToken scale = jo["scale"];
            if (scale != null) {
               if (scale.Type == JTokenType.Float) {
                  //If its a float, set the scale property
                  output.Scale = scale.ToObject<float>();
               }
               else if (scale.Type == JTokenType.Array) {
                  //If its a normal array, make it scaleArray property
                  output.ScaleArray = scale.ToObject<MolangVector3>();
               }
               else if (scale.Type == JTokenType.Object) {
                  //If its object, break into keyframes, either setting the StandardArray property if array or extradata if its more
                  Dictionary<string, KeyframeData> subOutput = new Dictionary<string, KeyframeData>();
                  JEnumerable<JToken> children = scale.Children();
                  foreach (JProperty child in children) {
                     if (child.Value.Type == JTokenType.Array) {
                        KeyframeData data = new KeyframeData();
                        data.standardArray = child.Value.ToObject<MolangVector3>();
                        subOutput.Add(child.Name, data);
                     }
                     else if (child.Value.Type == JTokenType.Object) {

                        KeyframeData data = new KeyframeData();
                        KeyframeExtraData extraData = child.Value.ToObject<KeyframeExtraData>();
                        data.extraData = extraData;
                        subOutput.Add(child.Name, data);
                     }
                  }
                  output.KeyframeScale = subOutput;
               }
            }
            return output;
         }
      }


      //Old Outdated Custom Deserializer I made and then replaced but still use as reference sometimes
      public class RotationPositionConverter : JsonConverter {
         public override bool CanConvert(Type objectType) {
            return true;
         }

         public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array) {
               Molang[] item = token.ToObject<Molang[]>();
               return new Dictionary<string, object[]>()
               {
                        {"0.0",item}
                    };

            }
            else if (token.Type == JTokenType.Object) {
               Dictionary<string, object[]> output = new Dictionary<string, object[]>();
               JEnumerable<JToken> children = token.Children();
               foreach (JProperty child in children) {
                  if (child.Value.Type == JTokenType.Array) {
                     object[] data = child.Value.ToObject<object[]>();
                     output.Add(child.Name, data);
                  }
                  else if (child.Value.Type == JTokenType.Object) {
                     KeyframeExtraData data = child.Value.ToObject<KeyframeExtraData>();
                     object[] dataArray = new object[1];
                     dataArray[0] = data;
                  }
               }
               return output;
            }
            else {
               throw new JsonSerializationException("Unexpected token type");
            }
         }

         public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
         }
      }
   }
}
