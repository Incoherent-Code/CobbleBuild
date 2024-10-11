namespace CobbleBuild.JavaClasses {
   public class TagData {
      public bool? replace;
      /// <summary>
      /// Typically, the object will just be a string either containing a minecraft block, or a block tag denoted with the #. It will be a json object typically if it is
      /// refering to a modded minecraft block, hence why we ignore it. Just check if it is typeof string.
      /// </summary>
      public List<object> values { get; set; }
   }
}
