using Godot;
using NuLua;
using System;
using System.Collections.Generic;
using System.Linq;

public class World(string worldDataId)
{
    
    public readonly string WorldDataId = worldDataId;
    
    public readonly Dictionary<Coordinate, Chunk> Chunks = [];
    
    private readonly FastNoiseLite _generatorFastNoise = new()
    {
        NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin
    };
    
    
    public Chunk GetChunkAtPosition(Coordinate position)
    {
        if (Chunks.TryGetValue(position, out Chunk storedChunk))
            return storedChunk;
        
        Chunk newChunk = new(position);
        Chunks[position] = newChunk;
        
        Endpoint.Universe.ChunksNode.CallDeferred(Node.MethodName.AddChild, newChunk.MeshInstance);
        
        return newChunk;
    }
    
    
    public string GetGeneratedVoxelId(Coordinate position)
    {
        // float heightPercent = Mathf.Clamp(_generatorFastNoise.GetNoise2D(position.X, position.Z) * 10 + 1, -10f, 20f) / 2f;
        // int height = Mathf.FloorToInt(25 * heightPercent);
        
        // if (position.Y > height)
        //     return null;
        
        // if (position.Y == height)
        //     return "glitchifyed.base.voxels.grass";
        
        // if (position.Y >= height - 3)
        //     return "glitchifyed.base.voxels.dirt";
        
        // return "glitchifyed.base.voxels.stone";
        
        var worldData = LoadedData.GetDataById<WorldData>(WorldDataId);
        // var generatorData = LoadedData.GetDataById<LuaFunctionData>(worldData.GeneratorId);
        
        var result = LuauScriptManager.GetFunction(worldData.GeneratorId).Invoke([position.X, position.Y, position.Z]);
        
        if (!result[0].TryRead(out string voxelId))
            throw new InvalidOperationException("Invalid voxel id.");
        
        if (voxelId != "")
            return voxelId;
        
        return null;
    }
    
    // public string GetGeneratedVoxelId(Coordinate position)
    // {
    //     if (position.Y < 0)
    //         return "game.voxel.stone";
        
    //     return null;
    // }
    
    public bool TryGetGeneratedVoxelData(Coordinate voxelPos, out VoxelData voxelData)
    {
        string generatedId = GetGeneratedVoxelId(voxelPos);
        
        if (generatedId == null)
        {
            voxelData = null;
            
            return false;
        }
        
        voxelData = LoadedData.GetDataById<VoxelData>(generatedId);
        
        return voxelData != null;
    }
    
    public bool TryGetVoxelIdAtPosition(Coordinate position, out string id)
    {
        Coordinate chunkPos = position / GameSettings.CHUNK_VOXEL_SIZE;
        
        if (Chunks.TryGetValue(chunkPos, out Chunk chunk) && chunk != null)
        {
            Coordinate localPos = position - chunkPos * GameSettings.CHUNK_VOXEL_SIZE;
            int? voxelId = chunk.GetVoxelIdAtPosition(localPos);
            
            if (voxelId.HasValue)
            {
                id = GenericData.GetIdFromNumeric(voxelId.Value);
                
                return id != null;
            }
        }
        
        id = GetGeneratedVoxelId(position);
        
        return id != null;
    }
    
    public bool TryGetVoxelDataAtPosition(Coordinate position, out VoxelData voxelData)
    {
        bool exists = TryGetVoxelIdAtPosition(position, out string id);
        voxelData = exists ? LoadedData.GetDataById<VoxelData>(id) : null;
        
        return voxelData != null;
    }
    
}
