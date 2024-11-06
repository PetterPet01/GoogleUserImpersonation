using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet.RUIGoogle
{
    public class DeviceIndexJsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                        object existingValue,
                                        JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        
        public override void WriteJson(JsonWriter writer,
                                       object value,
                                       JsonSerializer serializer)
        {
            if (value == null) return;

            writer.WriteStartArray();

            if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
            {
                foreach (object obj in value as IEnumerable)
                    writer.WriteValue(obj);
                writer.WriteEndArray();
                return;
            }

            var properties = value.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                //Checks if property is IEnumerable OR property is a non-null class
                bool isIEnumerable = !typeof(String).Equals(property.PropertyType) &&
        typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
                bool isNotNull = !typeof(String).Equals(property.PropertyType) && property.PropertyType.IsClass && property.GetValue(value) != null;
                if (isIEnumerable | isNotNull)
                    WriteJson(writer, property.GetValue(value), serializer);
                else
                    writer.WriteValue(value.GetType().GetProperty(property.Name).GetValue(value));
            }

            writer.WriteEndArray();
        }
    }
}
