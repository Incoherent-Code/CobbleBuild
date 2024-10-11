namespace CobbleBuild.BedrockClasses {
   public class SoundDefinitionJson {
      public string format_version;
      public Dictionary<string, SoundDefinition> sound_definitions;
      public SoundDefinitionJson(Dictionary<string, SoundDefinition> soundDefinitions) {
         format_version = "1.20.20";
         sound_definitions = soundDefinitions;
      }
   }
   public class SoundDefinition {
      public string? __use_legacy_max_distance;
      public string? category;
      public int? min_distance;
      public int? max_distance;
      public List<Sound> sounds;
      public SoundDefinition() { }
      public SoundDefinition(string Category, List<Sound> Sounds) {
         category = Category;
         sounds = Sounds;
      }

      public class Sound {
         public bool? is3D;
         public string name;
         public float? volume;
         public int? weight;
         public float? pitch;
         public bool? stream;
         public bool? load_on_low_memory;
         public Sound(string soundPath) {
            name = soundPath;
         }
      }
   }
}
