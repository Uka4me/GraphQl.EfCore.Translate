using GraphQl.EfCore.Translate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.Converters
{
	class JsonNullableStringEnumConverter : JsonConverterFactory
	{
		readonly JsonStringEnumConverter stringEnumConverter;

		public JsonNullableStringEnumConverter(JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true)
		{
			stringEnumConverter = new(namingPolicy, allowIntegerValues);
		}

		public override bool CanConvert(Type typeToConvert)
			=> Nullable.GetUnderlyingType(typeToConvert)?.IsEnum == true;

		public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var type = Nullable.GetUnderlyingType(typeToConvert)!;
			return (JsonConverter?)Activator.CreateInstance(typeof(ValueConverter<>).MakeGenericType(type),
				stringEnumConverter.CreateConverter(type, options));
		}

		class ValueConverter<T> : JsonConverter<T?>
			where T : struct, Enum
		{
			readonly JsonConverter<T> converter;

			public ValueConverter(JsonConverter<T> converter)
			{
				this.converter = converter;
			}

			public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var enumText = System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
				if (reader.TokenType == JsonTokenType.Null)
				{
					reader.Read();
					return null;
				}
				var isNullable = IsNullableType(typeToConvert);
				var enumType = isNullable ? Nullable.GetUnderlyingType(typeToConvert) : typeToConvert;
				var names = Enum.GetNames(enumType);
				var match = names.FirstOrDefault(e => string.Equals(StringUtils.ToConstantCase(e), enumText, StringComparison.OrdinalIgnoreCase));
				return (T?)(match != null ? Enum.Parse(enumType, match) : null);
				// return converter.Read(ref reader, typeof(T), options);
			}

			public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
			{
				if (value == null)
					writer.WriteNullValue();
				else
					converter.Write(writer, value.Value, options);
			}

			private static bool IsNullableType(Type t)
			{
				return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
			}
		}
	}
}
