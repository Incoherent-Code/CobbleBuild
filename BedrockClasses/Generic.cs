using ConcreteMC.MolangSharp.Parser;
using ConcreteMC.MolangSharp.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobbleBuild.BedrockClasses {

   /// <summary>
   /// Indicates that a component or property is just meant to be an empty json object to enable it. <br />
   /// To enable a property using this, give it a new one, and to disable a property, set it to null.
   /// </summary>
   public class BlankJSONObject : ServerEntity.Component {
      public BlankJSONObject() { }
   }
   [JsonConverter(typeof(ItemConverter))]
   public class Item : IEquatable<Item> //Seem to be close enough around java and bedrock recipes (Not to be confused with ItemJson; this one is used in item values in recipies)
   {
      public string? item;
      public int? count;
      public string? tag;
      public int? data;
      public Item() { }
      public Item Clone() {
         return (Item)this.MemberwiseClone();
      }

      public static Item CreateUsingTag(string tag) {
         return new Item() { tag = tag };
      }
      public static Item CreateUsingItem(string item, int? count = null, int? data = null) {
         return new Item() { item = item, data = data };
      }
      public Item getBedrock() {
         Item output = (Item)this.MemberwiseClone();
         output.prepareForBedrock();
         return output;
      }
      public void prepareForBedrock() {
         //Maybe we'll have a converter for tag aswell later
         if (item != null && BedrockConversion.JavaToBedrockItemNames.ContainsKey(item)) {
            item = BedrockConversion.JavaToBedrockItemNames[item];
         }
      }
      //how tf does the default equator work and why doesnt it work?
      public bool Equals(Item? other) {
         return this.count == other.count
             && this.data == other.data
             && this.tag == other.tag
             && this.item == other.item;
      }
   }
   //Bedrock filter format mostly used with biome_filter
   public class Filter {
      public string test;
      [JsonProperty("operator")]
      public string? @operator;
      public string? subject;
      public object? value;
      public string? domain;
      public List<Filter>? all_of;
      public List<Filter>? any_of;
      public List<Filter>? none_of;
      public Filter(string Test) {
         test = Test;
      }
      public Filter() { }
   }
   public class BedrockJson {
      public string format_version = "1.20.40"; // Default (Recommended to use :base("Version Class was based off of") to make sure specific classes don't have too high format version)
      public BedrockJson() { }
      public BedrockJson(string format_version) {
         this.format_version = format_version;
      }
   }


   public class minMaxDistance {
      public int max_distance;
      public int min_distance;
      public minMaxDistance(int minDistance, int maxDistance) {
         max_distance = maxDistance;
         min_distance = minDistance;
      }
   }
   public class MinMaxSize {
      public int min_size;
      public int max_size;
      public MinMaxSize(int min, int max) {
         min_size = min;
         max_size = max;
      }
   }
   public class MinMiaxValue {
      public int min;
      public int max;
      public MinMiaxValue(int min, int max) {
         this.min = min;
         this.max = max;
      }
   }

   public class widthAndHeight : ServerEntity.Component {
      public float width;
      public float height;
      public widthAndHeight(float width, float height) {
         this.width = width;
         this.height = height;
      }
   }
   [JsonConverter(typeof(Vector3Converter))]
   public class Vector3 //Serialized as array
   {
      public float x;
      public float y;
      public float z;
      public Vector3(float x, float y, float z) {
         this.x = x;
         this.y = y;
         this.z = z;
      }
      public void AddTogether(Vector3 add) {
         this.x += add.x;
         this.y += add.y;
         this.z += add.z;
      }
      public void SubtractWith(Vector3 subtrator) {
         this.x -= subtrator.x;
         this.y -= subtrator.y;
         this.z -= subtrator.z;
      }
      public Vector3 Clone() {
         return new Vector3(x, y, z);
      }
   }
   [JsonConverter(typeof(MolangVector3Converter))]
   public class MolangVector3 {
      public Molang x;
      public Molang y;
      public Molang z;
      public MolangVector3(Molang x, Molang y, Molang z) {
         this.x = x;
         this.y = y;
         this.z = z;
      }
      public JArray toJArray() {
         JArray output = [JToken.FromObject(x), JToken.FromObject(y), JToken.FromObject(z)];
         return output;
      }
   }
   [JsonConverter(typeof(Vector2Converter))]
   public class Vector2 //Serialized as array
   {
      public float x;
      public float y;
      public Vector2(float x, float y) {
         this.x = x;
         this.y = y;
      }
      public void AddTogether(Vector2 add) {
         this.x += add.x;
         this.y += add.y;
      }
      public Vector2 Clone() {
         return new Vector2(this.x, this.y);
      }
   }
   [JsonConverter(typeof(ObjectOrObjectArrayConverter))]
   public class objectOrObjectArray //Supposed to be either a value or array; DO NOT POPULATE BOTH OBJECTS
   {
      public object? value;
      public object[]? arrayValue;

      public objectOrObjectArray() { }
      public objectOrObjectArray(object value) {
         this.value = value;
      }
      public objectOrObjectArray(object[] value) {
         this.arrayValue = value;
      }
   }
   ///// <summary>
   ///// When using this in a json, specify above it [JsonConverter(typeof(TypedObjectOrObjectArrayConverter<[Type]>))]
   ///// This is important for casting to work properly.
   ///// </summary>
   ///// <typeparam name="T"></typeparam>
   //[JsonConverter(typeof(TypedObjectOrObjectArrayConverter<object>))]
   //public class ObjectOrObjectArray<T> //Supposed to be either a value or array
   //{
   //   public T? value;
   //   public T[]? arrayValue;

   //   public ObjectOrObjectArray() { }
   //   public ObjectOrObjectArray(T value) {
   //      this.value = value;
   //   }
   //   public ObjectOrObjectArray(T[] value) {
   //      this.arrayValue = value;
   //   }
   //}
   [JsonConverter(typeof(StringOrPropertyAndStringConverter))]
   public class StringOrPropertyAndString //String always exists, but if Value exists it will be serialize as property {string:value}; Exists for AnimationControllers and ClientEntities
   {
      public string String;
      public string? value;
      public StringOrPropertyAndString(string String, string? value = null) {
         this.String = String;
         this.value = value;
      }
      /// <summary>
      /// DO NOT USE THIS CONSTRUCTOR! IT IS ONLY FOR JSONCONVERTER
      /// </summary>
      [JsonConstructor]
      public StringOrPropertyAndString() { }
   }
   /// <summary>
   /// Object that only contains a single property. (Usually a molang statement)
   /// </summary>
   public class SinglePropertyObject : StringOrPropertyAndString {
      public SinglePropertyObject(string propName, string value) : base(propName, value) { }
      /// <summary>
      /// FOR JSON USE ONLY
      /// </summary>
      [JsonConstructor]
      public SinglePropertyObject() { }
   }

   public class EventTrigger {
      [JsonProperty("event")]
      public string Event;
      public string? target;
      public Filter? filters;

      public EventTrigger(string EVent, string? Target = "self", Filter? Filter = null) {
         Event = EVent;
         target = Target;
         filters = Filter;
      }
   }
   [JsonConverter(typeof(MolangConverter))]
   //Either stringValue or floatValue is not null, but also contains a Value property that will always return in string form.
   public class Molang {
      public string Value {
         get {
            if (floatValue != null && stringValue == null)
               return floatValue.ToString();
            else if (stringValue != null && floatValue == null)
               return stringValue;
            else
               throw new Exception("Invalid Molang Statement");
         }
         set {
            floatValue = null;
            stringValue = value;
         }
      }
      public string? stringValue;
      public float? floatValue;
      public Molang(string value) {
         stringValue = value;
      }
      public Molang(float value) {
         floatValue = value;
      }
      public bool AttemptToCompute(out float? result) {
         result = null;
         try {
            MoLangRuntime runtime = new MoLangRuntime();
            var parsed = MoLangParser.Parse(Value);
            result = runtime.Execute(parsed).AsFloat();
            return true;
         }
         catch {
            return false;
         }
      }
   }
   //Necessary because just a string is valid in some formats
   public class ItemConverter : JsonConverter<Item> {
      public override void WriteJson(JsonWriter writer, Item? value, JsonSerializer serializer) {
         if (value == null)
            return;
         JObject obj = new JObject();
         if (value.item != null) obj["item"] = value.item;
         if (value.count != null) obj["count"] = value.count;
         if (value.tag != null) obj["tag"] = value.tag;
         if (value.data != null) obj["data"] = value.data;
         obj.WriteTo(writer);
      }
      public override Item? ReadJson(JsonReader reader, Type objectType, Item? existingValue, bool hasExistingValue, JsonSerializer serializer) {
         JToken token = JToken.Load(reader);
         if (token.Type == JTokenType.String) {
            var value = token.ToObject<string>();
            return new Item() { item = value };
         }
         else if (token.Type == JTokenType.Object) {
            var output = new Item();
            foreach (var child in token.Children()) {
               if (child.Type == JTokenType.Property) {
                  var prop = (JProperty)child;
                  switch (prop.Name) {
                     case "item":
                        output.item = prop.Value.ToObject<string>();
                        break;
                     case "data":
                        output.data = prop.Value.ToObject<int>();
                        break;
                     case "count":
                        output.count = prop.Value.ToObject<int>();
                        break;
                     case "tag":
                        output.tag = prop.Value.ToObject<string>();
                        break;
                  }
               }
            }
            return output;
         }
         else {
            throw new JsonSerializationException($"Could not cast {token.Type} to Item.");
         }
      }
   }

   //Converts for the wierd types that need to be deserialized in a specific way (For old non typesafe objectorobjectarray
   public class ObjectOrObjectArrayConverter : JsonConverter<objectOrObjectArray> {
      public override void WriteJson(JsonWriter writer, objectOrObjectArray value, JsonSerializer serializer) {
         if (value.value != null) {
            serializer.Serialize(writer, value.value);
         }
         else if (value.arrayValue != null) {
            serializer.Serialize(writer, value.arrayValue);
         }
      }

      public override objectOrObjectArray ReadJson(JsonReader reader, Type objectType, objectOrObjectArray existingValue, bool hasExistingValue, JsonSerializer serializer) {
         objectOrObjectArray output = new objectOrObjectArray();
         JToken token = JToken.Load(reader);
         if (token.Type == JTokenType.Array) {
            output.arrayValue = token.ToObject<object[]>();
         }
         else if (token.Type == JTokenType.Object) {
            output.value = token.ToObject<object>();
         }
         return output;
      }
   }
   public class MolangConverter : JsonConverter<Molang> {
      public override void WriteJson(JsonWriter writer, Molang value, JsonSerializer serializer) {
         if (value.stringValue != null) {
            serializer.Serialize(writer, value.stringValue);
         }
         else if (value.floatValue != null) {
            serializer.Serialize(writer, value.floatValue);
         }
      }

      public override Molang ReadJson(JsonReader reader, Type objectType, Molang existingValue, bool hasExistingValue, JsonSerializer serializer) {
         JToken token = JToken.Load(reader);
         if (token.Type == JTokenType.String) {
            return new Molang(token.ToObject<string>());
         }
         else if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer) {
            return new Molang(token.ToObject<float>());
         }
         else {
            throw new Exception("Could not read Molang Value: Value is not a number or string.");
         }
      }
   }
   public class Vector3Converter : JsonConverter<Vector3> {
      public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) {
         serializer.Serialize(writer, new float[] { value.x, value.y, value.z });
      }

      public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
         JToken token = JToken.Load(reader);
         if (token.Type == JTokenType.Array) {
            float[] obj = token.ToObject<float[]>();
            Vector3 output = new Vector3(obj[0], obj[1], obj[2]);
            return output;
         }
         else {
            throw new Exception("Attempted to deserialize non array into vector3");
         }
      }
   }
   public class MolangVector3Converter : JsonConverter<MolangVector3> {
      public override void WriteJson(JsonWriter writer, MolangVector3 value, JsonSerializer serializer) {
         serializer.Serialize(writer, new Molang[] { value.x, value.y, value.z });
      }

      public override MolangVector3 ReadJson(JsonReader reader, Type objectType, MolangVector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
         JToken token = JToken.Load(reader);
         if (token.Type == JTokenType.Array) {
            Molang[] obj = token.ToObject<Molang[]>();
            MolangVector3 output = new MolangVector3(obj[0], obj[1], obj[2]);
            return output;
         }
         else {
            throw new Exception("Attempted to deserialize non array into vector3");
         }
      }
   }
   public class Vector2Converter : JsonConverter<Vector2> {
      public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
         serializer.Serialize(writer, new float[] { value.x, value.y });
      }

      public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
         JToken token = JToken.Load(reader);
         if (token.Type == JTokenType.Array) {
            float[] obj = token.ToObject<float[]>();
            Vector2 output = new Vector2(obj[0], obj[1]);
            return output;
         }
         else {
            throw new Exception("Attempted to deserialize non array into vector2");
         }
      }
   }
   public class StringOrPropertyAndStringConverter : JsonConverter<StringOrPropertyAndString> {
      public override void WriteJson(JsonWriter writer, StringOrPropertyAndString value, JsonSerializer serializer) {
         if (value.value != null) {
            serializer.Serialize(writer, new JObject(new JProperty(value.String, value.value)));
         }
         else {
            serializer.Serialize(writer, value.String);
         }
      }

      public override StringOrPropertyAndString ReadJson(JsonReader reader, Type objectType, StringOrPropertyAndString existingValue, bool hasExistingValue, JsonSerializer serializer) {
         //Needs fixing
         StringOrPropertyAndString output = new StringOrPropertyAndString();
         JToken token = JToken.Load(reader);
         return ParseJToken(token);
      }
      public StringOrPropertyAndString ParseJToken(JToken token) {
         var output = new StringOrPropertyAndString();
         if (token.Type == JTokenType.Property) {
            JProperty property = token.ToObject<JProperty>();
            output.String = property.Name;
            output.value = property.Value.ToObject<string>();
         }
         else if (token.Type == JTokenType.String) {
            output.String = token.ToObject<string>();
         }
         else if (token.Type == JTokenType.Object) {
            return ParseJToken(token.Children().First());
         }
         else {
            throw new Exception("Invalid Token Type for StringOrPropertyAndString");
         }
         return output;
      }
   }

   public class EnumToStringConverter<T> : JsonConverter<T> where T : Enum {
      public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer) {
         var token = JToken.Load(reader);
         if (token.Type != JTokenType.String)
            throw new JsonSerializationException($"Enum {typeof(T).Name} was not a string.");
         var name = token.ToObject<string>();
         if (Enum.TryParse(typeof(T), name, true, out var result)) {
            return (T)result;
         }
         else {
            throw new JsonSerializationException($"Enum {typeof(T).Name} has no member named {name}.");
         }
      }

      public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer) {
         if (value == null)
            return;
         var enumName = Enum.GetName(typeof(T), value);
         if (enumName == null)
            throw new JsonSerializationException($"Unable to find name of value {value} in enum {typeof(T).Name}.");
         writer.WriteValue(enumName);
      }
   }
   //public class TypedObjectOrObjectArrayConverter<T> : JsonConverter<ObjectOrObjectArray<T>> {
   //   public override ObjectOrObjectArray<T>? ReadJson(JsonReader reader, Type objectType, ObjectOrObjectArray<T>? existingValue, bool hasExistingValue, JsonSerializer serializer) {
   //      var token = JToken.Load(reader);
   //      if (token.Type == JTokenType.Array) {
   //         var output = new List<T>();
   //         foreach (var child in token.Children()) {
   //            var value = child.ToObject<T>();
   //            if (value != null)
   //               output.Add(value);
   //         }
   //         return new ObjectOrObjectArray<T>(output.ToArray());
   //      }
   //      else {
   //         return new ObjectOrObjectArray<T>(token.ToObject<T>()!);
   //      }
   //   }

   //   public override void WriteJson(JsonWriter writer, ObjectOrObjectArray<T>? value, JsonSerializer serializer) {
   //      if (value == null) {
   //         if (serializer.NullValueHandling != NullValueHandling.Ignore)
   //            writer.WriteNull();
   //         return;
   //      }
   //      serializer.Serialize(writer, value.arrayValue ?? (object)value.value!);
   //   }
   //}
}
