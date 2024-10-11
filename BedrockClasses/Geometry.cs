using CobbleBuild.JavaClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobbleBuild.BedrockClasses {
   public class GeometryJson {
      public string format_version = "1.18.0";
      [JsonProperty("minecraft:geometry")]
      public List<Geometry> geometry;
      public GeometryJson() { }
   }
   public class Geometry {
      public Description description;
      public List<Bone> bones;
      public Geometry Clone() {
         return new Geometry() {
            description = description.Clone(),
            bones = this.bones.Select(x => x.Clone()).ToList()
         };
      }
      public class Description {
         public string identifier;
         public int? texture_width;
         public int? texture_height;
         public float? visible_bounds_width;
         public float? visible_bounds_height;
         public Vector3? visible_bounds_offset;
         public Description() { }
         public Description(string identifier) {
            this.identifier = identifier;
         }
         public Description Clone() {
            return new Description(this.identifier) {
               texture_width = this.texture_width,
               texture_height = this.texture_height,
               visible_bounds_width = this.visible_bounds_width,
               visible_bounds_height = this.visible_bounds_height,
               visible_bounds_offset = this.visible_bounds_offset?.Clone()
            };
         }
      }
      public class Bone {
         public string name;
         public string? parent;
         public Vector3? pivot;
         public Vector3? rotation;
         public List<Cube>? cubes;
         public Bone() { }
         public Bone(string name) {
            this.name = name;
         }
         public Bone Clone() {
            return new Bone(this.name) {
               parent = this.parent,
               pivot = this.pivot?.Clone(),
               rotation = this.rotation?.Clone(),
               cubes = this.cubes?.Select(c => c.Clone()).ToList()
            };
         }
      }
      public class Cube {
         public Vector3? origin;
         public Vector3? size;
         public UVProperty? uv;
         public Vector3? rotation;
         public Vector3? pivot;
         public float? inflate;
         public bool? mirror;
         public Cube Clone() {
            return new Cube() {
               origin = this.origin?.Clone(),
               size = this.size?.Clone(),
               uv = this.uv?.Clone(),
               rotation = this.rotation?.Clone(),
               pivot = this.pivot?.Clone(),
               inflate = this.inflate,
               mirror = this.mirror
            };
         }
         public void applyTranslation(Vector3 translation) {
            if (origin != null) {
               origin = new Vector3(origin.x + translation.x, origin.y - translation.y, origin.z - translation.z);
            }
            else {
               origin = translation;
            }
         }
         /// <summary>
         /// Really Bad Apply Rotation Function.
         /// </summary>
         public void applyRotationFromCenter(Vector3 Rotation) //!! Doesn't account for preexisting pivot
         {
            if (rotation != null && rotation != new Vector3(0, 0, 0)) {
               rotation = new Vector3(rotation.x + Rotation.x, rotation.y + Rotation.y, rotation.z + Rotation.z);
            }
            else {
               rotation = Rotation;
            }
         }
         /// <summary>
         /// Really Bad Apply Rotation Function.
         /// </summary>
         public void applyRotation(JavaModel.Rotation Rotation) //!!Also doesn't account for preexisting pivot
         {
            if (pivot == null) {
               pivot = Rotation.origin;
            }
            else {
               pivot.AddTogether(Rotation.origin);
            }

            if (rotation == null) {
               rotation = new Vector3(0, 0, 0);
            }

            if (Rotation.axis.Contains("x")) {
               rotation.x = rotation.x + (float)Rotation.angle;
            }
            if (Rotation.axis.Contains("y")) {
               rotation.y = rotation.y + (float)Rotation.angle;
            }
            if (Rotation.axis.Contains("z")) {
               rotation.z = rotation.z + (float)Rotation.angle;
            }
         }
         public void applyScale(Vector3 scale, bool applyToOrigin = true) {
            size = new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z);
            if (applyToOrigin) {
               origin = new Vector3(origin.x * scale.x + (8 * scale.x), origin.y * scale.y, origin.z * scale.z + (-8 * scale.z));
            }
         }
      }
      /// <summary>
      /// Either standard_uv or complex_uv should be not null based on what type of UV it is
      /// Basically an optional type
      /// </summary>
      [JsonConverter(typeof(UVPropertyConverter))]
      public class UVProperty {
         //Either one of these would be not null based on uv type.
         public Vector2? standard_uv;
         public UVField? complex_uv;
         public UVProperty(int x, int y) {
            standard_uv = new Vector2(x, y);
         }
         public UVProperty(UV north, UV south, UV west, UV east, UV up, UV down) {
            complex_uv = new UVField(north, south, west, east, up, down);
         }
         public UVProperty(UVField field) {
            complex_uv = field;
         }
         public UVProperty() { }
         public UVProperty Clone() {
            return new UVProperty() {
               standard_uv = this.standard_uv?.Clone(),
               complex_uv = this.complex_uv?.Clone()
            };
         }
      }
      //I'm unsure if leaving these null are valid but the java models do.
      public class UVField {
         public UV? north;
         public UV? south;
         public UV? west;
         public UV? east;
         public UV? up;
         public UV? down;
         public UVField(UV north, UV south, UV west, UV east, UV up, UV down) {
            this.north = north;
            this.south = south;
            this.west = west;
            this.east = east;
            this.up = up;
            this.down = down;
         }
         public UVField() { }
         public UVField Clone() {
            return new UVField() {
               north = this.north?.Clone(),
               south = this.south?.Clone(),
               west = this.west?.Clone(),
               east = this.east?.Clone(),
               up = this.up?.Clone(),
               down = this.down?.Clone()
            };
         }
      }
      public class UV {
         public Vector2 uv;
         public Vector2 uv_size;
         public UV(float x, float y, float xsize, float ysize) {
            uv = new Vector2(x, y);
            uv_size = new Vector2(xsize, ysize);
         }
         public UV Clone() {
            return new UV(uv.x, uv.y, uv_size.x, uv_size.y);
         }
      }
      public class UVPropertyConverter : JsonConverter<UVProperty> {
         public override void WriteJson(JsonWriter writer, UVProperty value, JsonSerializer serializer) {
            if (value.standard_uv != null) {
               serializer.Serialize(writer, value.standard_uv);
            }
            else if (value.complex_uv != null) {
               serializer.Serialize(writer, value.complex_uv);
            }
            else {
               throw new Exception("Could not write UVProperty to json; UV is neither standard nor complex.");
            }
         }

         public override UVProperty ReadJson(JsonReader reader, Type objectType, UVProperty existingValue, bool hasExistingValue, JsonSerializer serializer) {
            //Needs fixing
            UVProperty output = new UVProperty();
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array) {
               output.standard_uv = token.ToObject<Vector2>();
            }
            else if (token.Type == JTokenType.Object) {
               output.complex_uv = token.ToObject<UVField>();
            }
            return output;
         }
      }
   }
}
