using CobbleBuild.BedrockClasses;
using CobbleBuild.JavaClasses;
using Newtonsoft.Json;

namespace CobbleBuild.ConversionTechnology {
   /// <summary>
   /// While I am proud of making this feature, Honestly It's not easier than converting with blockbench.
   /// Each Block has to be manually coded anyways. Plus sometimes manual intervention is just required.
   /// </summary>
   public class BlockModelConversion {
      //Currently lacks support for both rotation and textures
      public static GeometryJson convertToBedrock(JavaModel original, string newIdentifier) //don't include geometry in identifier
      {
         //Merging with parent when aplicable
         if (original.parent != null && original.parent.StartsWith("cobblemon:")) {
            string path = Path.Combine(Config.config.resourcesPath, "assets/cobblemon/models", original.parent.Substring(10) + ".json");
            if (File.Exists(path)) {
               JavaModel parent = JsonConvert.DeserializeObject<JavaModel>(File.ReadAllText(path))!;
               original.mergeWith(parent);
            }
         }

         GeometryJson output = new GeometryJson();
         output.geometry = new List<Geometry>();
         Geometry Base = new Geometry();
         string firstImage = original.textures.Values.ToList()[0];
         Vector2 imageSize = ImageProcessor.getImageSize(Path.Combine(Config.config.resourcesPath, "assets/cobblemon/textures/", original.textures.Values.ToList()[0].Remove(0, 10) + ".png"));
         Base.description = new Geometry.Description($"geometry.cobblemon.{newIdentifier}.base") { texture_width = (int)imageSize.x, texture_height = (int)imageSize.y };
         Base.bones = new List<Geometry.Bone>();
         //if (original.textures != null)
         //{
         //    Misc.warn("Textures are unimplimented in cobblemon block models and will not be considered when converting to bedrock!");
         //}
         if (original.groups != null) {
            foreach (JavaModel.Group g in original.groups) {
               Geometry.Bone bone = new Geometry.Bone(g.name + "_group");
               bone.pivot = new Vector3(0, 0, 0); //Idk why but they usually include pivot even in blank one
               Base.bones.Add(bone);
            }
         }
         foreach (JavaModel.Element e in original.elements) {
            Geometry.Bone bone = new Geometry.Bone();
            if (e.name != null) {
               bone.name = e.name;
            }
            else {
               bone.name = "Bone_" + original.elements.IndexOf(e).ToString();
            }
            bone.cubes = new List<Geometry.Cube> { new Geometry.Cube() };
            bone.cubes[0].pivot = new Vector3(8, 0, -8); //Different origin
            bone.cubes[0].origin = e.from;
            bone.cubes[0].applyTranslation(new Vector3(-8, 0, 8)); //Different origin between java and bedrock
            bone.cubes[0].size = new Vector3(e.to.x - e.from.x, e.to.y - e.from.y, e.to.z - e.from.z);
            bone.cubes[0].uv = new Geometry.UVProperty(new Geometry.UVField() {
               north = (e.faces.north != null) ? e.faces.north.toBedrockUV(imageSize) : null,
               south = (e.faces.south != null) ? e.faces.south.toBedrockUV(imageSize) : null,
               east = (e.faces.east != null) ? e.faces.east.toBedrockUV(imageSize) : null,
               west = (e.faces.west != null) ? e.faces.west.toBedrockUV(imageSize) : null,
               up = (e.faces.up != null) ? e.faces.up.toBedrockUV(imageSize) : null,
               down = (e.faces.down != null) ? e.faces.down.toBedrockUV(imageSize) : null
            });
            if (e.rotation != null) {
               bone.cubes[0].applyRotation(e.rotation);
            }
            //Applys group parameters if necessary
            JavaModel.Group? parent = original.getParent(original.elements.IndexOf(e));
            if (parent != null) {
               bone.parent = parent.name + "_group";
               if (parent.origin != null) {
                  bone.cubes[0].applyTranslation(parent.origin);
               }
            }
            Base.bones.Add(bone);
         }
         output.geometry.Add(Base);
         //Creating different geometries for different displays
         if (original.display != null && original.display.Count > 0) {
            foreach (var d in original.display) {
               Geometry displayVariant = Base.Clone();
               displayVariant.description.identifier = $"geometry.cobblemon.{newIdentifier}.{d.Key}";
               //Applying display changes
               for (int i = 0; i < displayVariant.bones.Count; i++) {
                  if (d.Value.translation != null && !displayVariant.bones[i].name.EndsWith("_group")) {
                     displayVariant.bones[i].cubes[0].applyTranslation(d.Value.translation);
                  }
                  if (d.Value.rotation != null && !displayVariant.bones[i].name.EndsWith("_group")) {
                     displayVariant.bones[i].cubes[0].applyRotationFromCenter(d.Value.rotation);
                  }
                  if (d.Value.scale != null && !displayVariant.bones[i].name.EndsWith("_group")) {
                     displayVariant.bones[i].cubes[0].applyScale(d.Value.scale);
                  }
               }
               output.geometry.Add(displayVariant);
            }
         }
         return output;
      }
   }
}
