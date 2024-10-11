using CobbleBuild.BedrockClasses;

namespace CobbleBuild.JavaClasses {
   public class JavaModel {
      public Vector2? texture_size;
      public Dictionary<string, string>? textures;
      public List<Element> elements;
      public List<Group>? groups;
      public string? gui_light;
      public Dictionary<string, Display>? display;
      public string? parent;
      public class Element {
         public string? name;
         public Vector3 to;
         public Vector3 from;
         public Rotation? rotation;
         public FaceField? faces;
      }
      public class Rotation {
         public float? angle;
         public string? axis;
         public Vector3? origin;
      }
      public class Face {
         public float[] uv;
         public string texture;
         public Geometry.UV toBedrockUV(Vector2 imageSize) {
            Geometry.UV output = new Geometry.UV(uv[0] * (imageSize.x / 16), uv[1] * (imageSize.y / 16), (uv[2] - uv[0]) * (imageSize.x / 16), (uv[3] - uv[1]) * (imageSize.y / 16));
            return output;
         }
      }
      public class FaceField //Better to have this static ig
      {
         public Face? up;
         public Face? down;
         public Face? north;
         public Face? south;
         public Face? west;
         public Face? east;
      }
      public class Group {
         public string name;
         public Vector3? origin;
         public int[] children;
         public int color;
      }
      public class Display {
         public Vector3? rotation;
         public Vector3? translation;
         public Vector3? scale;
      }
      public Group? getParent(int i) {
         if (groups != null) {
            foreach (var group in groups) {
               if (group.children.Contains(i)) {
                  return group;
               }
            }
            return null;
         }
         else {
            return null;
         }
      }
      //NOTE: Curently this only merges metadata because It seems like that's the intended behavior
      public void mergeWith(JavaModel otherModel) {
         //elements = elements.Concat(otherModel.elements).ToList();
         //if (otherModel.groups != null)
         //{
         //    groups = otherModel.groups.Concat(groups ?? []).ToList();
         //}
         gui_light = gui_light ?? otherModel.gui_light;
         if (otherModel.textures != null) {
            textures = otherModel.textures.Union(textures ?? []).ToDictionary(x => x.Key, x => x.Value);
         }
         if (otherModel.display != null) {
            display = otherModel.display.Union(display ?? []).ToDictionary(x => x.Key, x => x.Value);
         }
         texture_size = otherModel.texture_size ?? texture_size;

      }
   }
}
