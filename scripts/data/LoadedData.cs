using Godot;
using System;
using System.Collections.Generic;

public static class LoadedData
{
    
    private static readonly Dictionary<string, GenericData> _loadedData = [];
    
    
    public static void Load()
    {
        _loadedData.Clear();
        
        // TODO: Load data from JSON files
        Add(new("Unknown", "unknown"));
        
        Add(new VoxelData("Air", "game.voxel.air") { Renderable = false, Translucent = true });
        
        Add(new VoxelData("Stone", "game.voxel.stone") { ColorTemp = Color.FromHsv(0, 0, 0.7f) });
        Add(new VoxelData("Grass", "game.voxel.grass") { ColorTemp = Color.FromHsv(0.28f, 0.8f, 0.8f) });
    }
    
    
    public static T GetDataById<T>(string id) where T : GenericData
    {
        if (_loadedData.TryGetValue(id, out GenericData value))
            return (T) value;
        
        // Unknown value
        GD.PushWarning($"Failed to get data: '{id}' - did you use the wrong identifier? Returning Unknown generic.");
        
        return (T) _loadedData["unknown"];
    }
    
    public static GenericData GetDataById(string id)
        => GetDataById<GenericData>(id);
    
    
    private static void Add(GenericData data)
        => _loadedData.Add(data.Id, data);
    
}
