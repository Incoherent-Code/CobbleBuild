using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobbleBuild {
   public class Either<T1, T2> {
      public T1? Left;
      public T2? Right;

      public Either(T1? Left = default, T2? Right = default) {
         // Using EqualityComparer to handle both value types and reference types properly
         if (EqualityComparer<T1?>.Default.Equals(Left, default) && EqualityComparer<T2?>.Default.Equals(Right, default))
            throw new ArgumentNullException("Left & Right: Both are null");
         if (!EqualityComparer<T1?>.Default.Equals(Left, default) && !EqualityComparer<T2?>.Default.Equals(Right, default))
            throw new ArgumentException("Left & Right: Both are not null");

         this.Left = Left;
         this.Right = Right;
      }

      public object Value { get { return this.Left ?? (object)this.Right!; } }
   }

   public abstract class EitherSerializer<T1, T2> : JsonConverter<Either<T1, T2>> {
      public abstract bool IsLeft(JToken token);
      public abstract bool IsRight(JToken token);
      public override Either<T1, T2>? ReadJson(JsonReader reader, Type objectType, Either<T1, T2>? existingValue, bool hasExistingValue, JsonSerializer serializer) {
         var token = JToken.Load(reader);
         if (this.IsLeft(token))
            return new Either<T1, T2>(token.ToObject<T1>(), default);
         else if (this.IsRight(token))
            return new Either<T1, T2>(default, token.ToObject<T2>());
         return null;
      }

      public override void WriteJson(JsonWriter writer, Either<T1, T2>? value, JsonSerializer serializer) {
         serializer.Serialize(writer, value?.Value);
      }
   }

}
