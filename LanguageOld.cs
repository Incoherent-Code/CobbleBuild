namespace CobbleBuild {
   /// <summary>
   /// The most basic language file interpretation ever.
   /// Literally just a self contained string.
   /// </summary>
   [Obsolete]
   public class LanguageFileOld {
      public string data;
      public void LoadFile(string filename) {
         data = File.ReadAllText(filename);
      }
      public void Load(string data) {
         this.data = data;
      }
      public void Add(string key, string translation) {
         data += $"\n{key}={translation}";
      }
      public LanguageFileOld() { }
   }
}
