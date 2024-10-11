using Newtonsoft.Json;

namespace CobbleBuild.BedrockClasses {
   public class ClientEntityJson : BedrockJson {
      [JsonProperty("minecraft:client_entity")]
      public ClientEntity client_entity;
      public ClientEntityJson(ClientEntity clientEntity) : base("1.20.40") {
         client_entity = clientEntity;
      }
   }
   public class ClientEntity {
      public Description description;
      public ClientEntity(string Identifier) {
         description = new Description();
         description.identifier = Identifier;
      }
      public class Description {
         public string identifier;
         public Dictionary<string, string>? materials;
         public Dictionary<string, string>? textures;
         public Dictionary<string, string>? geometry;
         public Dictionary<string, string>? animations;
         public List<string>? render_controllers;
         public ScriptField? scripts;

         public string getTextureFromPath(string partialPath) {
            List<string> Textures = textures.Values.ToList(); //There might be multipe of the same but as far as I'm concerned it should work just the same
            if (Textures.Contains(partialPath)) {
               int index = Textures.IndexOf(partialPath);
               return textures.Keys.ToList()[index];
            }
            else {
               throw new Exception("Texture not found in Client Entity!");
            }
         }
      }

      public class ScriptField {
         //Doesn't encapsulate all possible scripting fields
         public List<StringOrPropertyAndString>? animate;
         public List<string>? pre_animation;
         public string? scale;
         public List<string>? initialize;
         public ScriptField() { }
      }
   }
}
