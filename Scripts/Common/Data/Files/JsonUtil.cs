using System;
using System.Collections.Generic;
using Godot;

public static class JsonUtil
{
    private static Json _jsonParser = new();

    public static JsonObject Parse(string toParse)
    {
        _jsonParser.Parse(toParse).CheckError("Failed to parse JSON.");

        using Variant dataVariant = _jsonParser.Data;
        
        return ProcessVariant(dataVariant);
    }


    private static JsonObject ProcessVariant(Variant variant)
        => variant.VariantType switch
        {
            Variant.Type.String
                => new(variant.AsString()),

            Variant.Type.Array
                => new(FromGodotArray(variant.AsGodotArray())),

            Variant.Type.Dictionary
                => new(FromGodotDictionary(variant.AsGodotDictionary())),

            _ => throw new Exception($"{variant.VariantType} JSON is not implemented in ProcessVariant.")
        };

    private static JsonObject[] FromGodotArray(Godot.Collections.Array godotArray)
    {
        var array = new JsonObject[godotArray.Count];

        for (int i = 0; i < godotArray.Count; i++)
        {
            using var variant = godotArray[i];

            array[i] = ProcessVariant(variant);
        }

        return array;
    }

    private static Dictionary<string, JsonObject> FromGodotDictionary(Godot.Collections.Dictionary godotDictionary)
    {
        Dictionary<string, JsonObject> dictionary = [];

        foreach (var idk in godotDictionary)
        {
            dictionary[idk.Key.AsString()] = ProcessVariant(idk.Value);
            
            idk.Key.Dispose();
            idk.Value.Dispose();
        }

        return dictionary;
    }
}