namespace CobbleBuild {
   internal static class ListExtensions {
      /// <summary>
      /// Fills up a list with fillValue until it reaches targetCount
      /// </summary>
      public static List<T> FillUpTo<T>(this List<T> list, int targetCount, T fillValue) {
         if (list.Count < targetCount) {
            list.AddRange(Enumerable.Repeat(fillValue, targetCount - list.Count));
         }
         return list;
      }
      /// <summary>
      /// Equivalent to the javascript .shift() function.
      /// </summary>
      /// <returns>The element shifted out.</returns>
      /// <exception cref="InvalidOperationException">List is empty.</exception>
      public static T Shift<T>(this List<T> list) {
         if (list == null || list.Count == 0) {
            throw new InvalidOperationException("Cannot shift an element from an empty list.");
         }

         T firstElement = list[0];
         list.RemoveAt(0);
         return firstElement;
      }
      /// <summary>
      /// Removes objects in params if they exist in the list.
      /// Does not mutate the original list.
      /// </summary>
      public static List<T> RemoveIfInList<T>(this List<T> list, params T[] items) {
         var listCopy = list.ToArray().ToList();
         foreach (var item in items) {
            var index = listCopy.FindIndex(x => x?.Equals(item) == true);
            if (index != -1) {
               listCopy.RemoveAt(index);
            }
         }
         return listCopy;
      }
   }
}
