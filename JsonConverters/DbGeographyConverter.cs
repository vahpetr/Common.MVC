using System;
using System.Data.Entity.Spatial;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.MVC.JsonConverters
{
    public class DbGeographyConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type == typeof(DbGeography);
        }

        public override object ReadJson(JsonReader reader, Type type, object obj, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return default(DbGeography);

            var position = JArray.Load(reader);

            if (position.Count != 2) return default(DbGeography);

            //4326 format puts LONGITUDE first then LATITUDE
            var point = string.Format(CultureInfo.InvariantCulture, "POINT({1} {0})", position[0], position[1]);
            var result = DbGeography.FromText(point, 4326);//, DbGeography.DefaultCoordinateSystemId
            return result;
        }

        public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
        {
            var dbGeography = obj as DbGeography;

            var value = (dbGeography == null || dbGeography.IsEmpty)
                ? null
                : new[]
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    dbGeography.Longitude.Value,
                    // ReSharper disable once PossibleInvalidOperationException
                    dbGeography.Latitude.Value
                };

            serializer.Serialize(writer, value);
        }
    }
}