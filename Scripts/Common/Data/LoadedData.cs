using Godot;
using System;
using System.Collections.Generic;
using System.Net;

public static class LoadedData
{
    
    private static readonly Dictionary<string, GenericData> _loadedData = [];
    
    
    public static void Load(DataFileManager data)
    {
        _loadedData.Clear();
        GenericData.Reset();
        
        // TODO: Load data from JSON files
        Add(new VoxelData("game.voxel.stone") { ColorTemp = Color.FromHsv(0, 0, 0.7f) });
        Add(new VoxelData("game.voxel.grass") { ColorTemp = Color.FromHsv(0.28f, 0.8f, 0.8f) });
        Add(new VoxelData("game.voxel.dirt") { ColorTemp = Colors.Brown });
        
        // TODO: add an ingame option to reset the pack instead
        data.ClearPack("glitchifyed.base");
        data.GenerateFolderFromResources("data");
        
        data.CompileDataPacks("data");
    }
    
    
    public static T GetDataById<T>(string id) where T : GenericData
    {
        if (_loadedData.TryGetValue(id, out GenericData value))
        {
            if (value is not T castValue)
                return null;
            
            return castValue;
        }
        
        // Unknown value
        GD.PushWarning($"Failed to get data: '{id}' - did you use the wrong identifier?");
        
        return null;
    }
    
    public static T GetDataByNumericId<T>(int id) where T : GenericData
        => GetDataById<T>(GenericData.GetIdFromNumeric(id));
    
    public static GenericData GetDataById(string id)
        => GetDataById<GenericData>(id);
    
    
    private static void Add(GenericData data)
        => _loadedData.Add(data.Id, data);
    
}
