namespace CobbleBuild.BedrockClasses {
   public class RenderControllerJson {
      public string format_version = "1.20.0";
      public Dictionary<string, RenderController> render_controllers;
      public RenderControllerJson(string renderControllerName, RenderController renderer) //Unlikely to have more than one render controller per file
      {
         render_controllers = new Dictionary<string, RenderController>() { { renderControllerName, renderer } };
      }
      public RenderControllerJson() {
         render_controllers = [];
      }
   }
   public class RenderController {
      public ArraysField? arrays;
      public string? geometry;
      public List<StringOrPropertyAndString>? materials;
      public List<StringOrPropertyAndString>? part_visibility;
      public List<string>? textures;
      public Color? color;
      public Color? is_hurt_color;
      public Color? on_fire_color;
      public Color? overlay_color;
      public UVAniationField? uv_anim;
      public RenderController() { }
      public class ArraysField {
         public Dictionary<string, List<string>>? materials;
         public Dictionary<string, List<string>>? textures;
         public Dictionary<string, List<string>>? geometries;

         public bool isEmpty() {
            bool output = false;
            if (materials != null && materials.Count > 0) {
               output = true;
            }
            else if (textures != null && textures.Count > 0) {
               output = true;
            }
            else if (geometries != null && geometries.Count > 0) {
               output = true;
            }
            return output;
         }
         public ArraysField() { }
      }
      public class Color //Strings can be molang expressions
      {
         public string r;
         public string g;
         public string b;
         public string a;
         public Color(string r, string g, string b, string a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
         }
      }
      public class UVAniationField {
         public object[] offset;
         public object[] scale;
         public UVAniationField() {
            offset = [0, 0];
            scale = [1, 1];
         }
         public UVAniationField(int fps, int frameCount) {
            offset = [0.0, $"math.mod(math.floor(q.life_time * {fps}),{frameCount}) / {frameCount}"];
            scale = [1.0, $"1 / {frameCount}"];
         }
      }
   }
}
