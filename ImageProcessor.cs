using CobbleBuild.BedrockClasses;
using SkiaSharp;

namespace CobbleBuild {
   public class ImageProcessor {
      public static SKBitmap CreateVerticalUV(params string[] files)//Assumes all are same size
      {
         List<SKBitmap> SKBitmaps = new List<SKBitmap>();
         foreach (string file in files) {
            try {
               SKBitmaps.Add(SKBitmap.Decode(file));
            }
            catch { }
         }
         if (SKBitmaps.Count > 0) {
            return CreateVerticalUV([.. SKBitmaps]);
         }
         else {
            throw new Exception("Unable to create UV, no SKBitmaps sucessfully loaded from the provided filepaths.");
         }
      }
      public static SKBitmap CreateVerticalUV(params SKBitmap[] SKBitmaps)//Assumes all are same size
      {
         if (SKBitmaps.Length > 0) {
            var combined = new SKBitmap(SKBitmaps[0].Width, SKBitmaps[0].Height * SKBitmaps.Length, false);
            var g = new SKCanvas(combined);
            for (int i = 0; i < SKBitmaps.Length; i++) {
               g.DrawBitmap(SKBitmaps[i], new SKPoint(0, SKBitmaps[0].Height * i));
            }
            return combined;
         }
         else {
            throw new Exception("Unable to create UV, no SKBitmaps provided.");
         }
      }
      public static Vector2 getImageSize(string file) {
         SKBitmap img = SKBitmap.Decode(file);
         return new Vector2(img.Width, img.Height);
      }
      /// <summary>
      /// Returns a new Bitmap with the alpha values set to that specific number.
      /// </summary>
      /// <param name="image">Base Image</param>
      /// <param name="alpha">Number between 0 and 255</param>
      /// <returns></returns>
      public static SKBitmap setAlphaValue(SKBitmap image, byte alpha) {
         var output = new SKBitmap(image.Info);
         var paint = CreateAlphaPaint(alpha);
         var canvas = new SKCanvas(output);
         //canvas.DrawBitmap(image, 0, 0);
         canvas.DrawBitmap(image, 0, 0, paint);
         return output;
      }
      /// <summary>
      /// Creates a paint that modifies the alpha value to a specific number.
      /// </summary>
      /// <param name="newAlpha">Number between 0 and 255 to set alpha value to</param>
      private static SKPaint CreateAlphaPaint(byte newAlpha) {
         // Define the color filter to apply the new alpha value
         SKColor alphaColor = new SKColor(255, 255, 255, newAlpha); // Adjust this as needed
         SKColorFilter alphaFilter = SKColorFilter.CreateBlendMode(alphaColor, SKBlendMode.DstIn);

         // Create a paint object with the color filter
         SKPaint paint = new SKPaint {
            ColorFilter = alphaFilter
         };

         return paint;
      }
   }
}
