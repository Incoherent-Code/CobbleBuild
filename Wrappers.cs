namespace CobbleBuild {
   internal class Wrappers {
      /// <summary>
      /// Wraps action in exception printed to console
      /// </summary>
      /// <param name="action">Action to be wrapped</param>
      /// <param name="actionOrigin">Will be printed to console as $"Unable to {actionOrigin}: {error}".</param>
      public static void printExceptionsToConsole(Action action, string? actionOrigin = null) {
         try {
            action();
         }
         catch (AggregateException ex) {
            foreach (var exception in ex.InnerExceptions) {
               var message = string.Empty;
               if (actionOrigin != null) {
                  message += $"Unable to {actionOrigin}: ";
               }
               message += exception.Message;
               Misc.softError(message);
            }
         }
         catch (Exception ex) {
            string message = string.Empty;
            if (actionOrigin != null) {
               message += $"Unable to {actionOrigin}: ";
            }
            message += ex.Message;
            Misc.softError(message);
         }
      }
      /// <summary>
      /// Measures how long an action takes and returns the time in ms.
      /// </summary>
      /// <param name="action">Action to be taken.</param>
      /// <param name="result">Out of the result of the function call</param>
      /// <returns>time in ms</returns>
      public static long timeExecution<TResult>(Func<TResult> action, out TResult result) {
         var watch = System.Diagnostics.Stopwatch.StartNew();
         result = action();
         watch.Stop();
         return watch.ElapsedMilliseconds;
      }
      /// <summary>
      /// Measures how long an action takes and returns the time in ms.
      /// </summary>
      /// <param name="action">Action to be taken.</param>
      /// <returns>time in ms</returns>
      public static long timeExecution(Action action) {
         var watch = System.Diagnostics.Stopwatch.StartNew();
         action();
         watch.Stop();
         return watch.ElapsedMilliseconds;
      }
   }
}
