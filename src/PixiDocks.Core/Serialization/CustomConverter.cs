using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PixiDocks.Core.Serialization;

public abstract class CustomConverter<T> : JsonConverter<T>
{
    public void StartReadingScope(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartObject");
        }

        reader.Read();
    }


    public void StartReadingProperty(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException("Expected PropertyName");
        }

        reader.Read();
    }

    public void EndReadingScope(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected EndObject");
        }

        reader.Read();
    }

    public bool TryReadToNextProperty(ref Utf8JsonReader reader, out string propName)
    {
        if (reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray)
        {
            propName = string.Empty;
            return false;
        }

        if (reader.TokenType == JsonTokenType.PropertyName)
        {
            propName = reader.GetString();
            reader.Read();
            return true;
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                propName = reader.GetString();
                reader.Read();
                return true;
            }

            if (reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray)
            {
                propName = string.Empty;
                return false;
            }
        }

        propName = string.Empty;
        return false;
    }

    public string ReadStringProperty(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected String");
        }

        string prop = reader.GetString();
        reader.Read();
        return prop;
    }

    public double ReadDoubleProperty(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expected Number");
        }

        double prop = reader.GetDouble();
        reader.Read();
        return prop;
    }
}