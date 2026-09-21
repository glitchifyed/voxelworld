using Godot;
using System;
using System.Collections.Generic;

public static class LoadedData
{
    
    private static readonly Dictionary<string, GenericData> _loadedData = [];
    
    
    public static void Load()
    {
        _loadedData.Clear();
        GenericData.Reset();
        
        // TODO: Load data from JSON files
        Add(new VoxelData("Stone", "game.voxel.stone") { ColorTemp = Color.FromHsv(0, 0, 0.7f) });
        Add(new VoxelData("Grass", "game.voxel.grass") { ColorTemp = Color.FromHsv(0.28f, 0.8f, 0.8f) });
        
        var pos = new Vector3(-5, -5, -5);
        
        var mesher = new ArrayMesher();
            mesher.AddCube(Vector3.Zero);
            mesher.AddSurfaces();
            
            World.Singleton.AddChild(new MeshInstance3D()
            {
                Name = "Middle",
                Position = pos,
                Mesh = mesher.ArrayMesh
            });
        
        
        foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace))){
            var normal = MeshStructures.CubeFaceNormals[(int)cubeFace];            
            
            
            var mesher2 = new ArrayMesher();
            mesher2.AddFaceQuad(cubeFace, Vector3.Zero, Colors.Azure);
            mesher2.AddSurfaces();
            
            World.Singleton.AddChild(new MeshInstance3D()
            {
                Name = cubeFace.ToString(),
                Position = pos + normal,
                Mesh = mesher2.ArrayMesh
            });
        }
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
    
    public static GenericData GetDataById(string id)
        => GetDataById<GenericData>(id);
    
    
    private static void Add(GenericData data)
        => _loadedData.Add(data.Id, data);
    
}
