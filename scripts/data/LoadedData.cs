using Godot;
using System;
using System.Collections.Generic;

public static class LoadedData
{
    
    private static readonly Dictionary<string, GenericData> _loadedData;
    
    
    public static void Load()
    {
        // TODO: Load data from JSON files
        Add(new("Unknown", new("none", "unknown")));
    }
    
    
    public static GenericData GetDataById(string id)
    {
        if (_loadedData.TryGetValue(id, out GenericData value))
            return value;
        
        // Unknown value
        GD.PushWarning($"Failed to get data: '{id}' - did you use the wrong identifier? Returning Unknown generic.");
        
        return _loadedData["none:unknown"];
    }
    
    public static GenericData GetDataById(Identifier id)
        => GetDataById(id.ToString());
    
    
    private static void Add(GenericData data)
        => _loadedData.Add(data.Id.ToString(), data);
    
}
