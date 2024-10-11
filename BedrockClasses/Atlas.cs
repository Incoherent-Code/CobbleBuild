using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobbleBuild.BedrockClasses {
   public class AtlasJson {
      public int? num_mip_levels;
      public int? padding;
      public string resource_pack_name;
      public string texture_name;
      public Dictionary<string, Atlas> texture_data;
      /// <summary>
      /// This constructor is for JSON.Net, Do not use
      /// </summary>
      public AtlasJson() { }
      public AtlasJson(string resourcePackName, string atlasName) {
         texture_data = new Dictionary<string, Atlas>();
         texture_name = atlasName;
         resource_pack_name = resourcePackName;
      }
   }
   public class Atlas {
      [JsonConverter(typeof(AtlasSerializer))]

      public Either<string, string[]> textures;
      public Atlas(string? textures, string[]? textureArray = null) {
         this.textures = new Either<string, string[]>(textures, textureArray);
      }
   }

   public class AtlasSerializer : EitherSerializer<string, string[]> {
      public override bool IsLeft(JToken token) {
         return token.Type == JTokenType.String;
      }

      public override bool IsRight(JToken token) {
         return token.Type == JTokenType.Array;
      }
   }

}
