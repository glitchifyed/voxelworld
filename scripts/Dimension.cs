using Godot;
using System;
using System.Collections.Generic;

public class Dimension
{
    
    public readonly Dictionary<VoxelPos, Chunk> Chunks = [];
    
    private readonly FastNoiseLite _generatorFastNoise = new()
    {
        NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin
    };
    
    
    public Chunk GetOrGenerateChunk(VoxelPos position)
    {
        if (Chunks.TryGetValue(position, out Chunk storedChunk))
            return storedChunk;
        
        Chunk newChunk = new(position);
        
        // newChunk.Generate();
        
        Chunks[position] = newChunk;
        
        return newChunk;
    }
    
    
    public string GetGeneratedVoxelId(VoxelPos position)
    {
        float heightPercent = Mathf.Clamp(_generatorFastNoise.GetNoise2D(position.X, position.Z) * 10 + 1, -10f, 20f) / 2f;
        int height = Mathf.FloorToInt(25 * heightPercent);
        
        if (position.Y > height)
            return null;
        
        if (position.Y == height)
            return "game.voxel.grass";
        
        if (position.Y >= height - 3)
            return "game.voxel.dirt";
        
        return "game.voxel.stone";
    }
    
    public bool TryGetGeneratedVoxelData(VoxelPos voxelPos, out VoxelData voxelData)
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
    
    public bool TryGetVoxelIdAtPosition(VoxelPos position, out string id)
    {
        VoxelPos chunkPos = position / WorldSettings.CHUNK_VOXEL_COUNT;
        
        if (Chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            VoxelPos localPos = position - chunkPos * WorldSettings.CHUNK_VOXEL_COUNT;
            int? voxelId = chunk.GetVoxelIdAtPosition(localPos);
            
            if (voxelId.HasValue)
            {
                id = GenericData.GetIdFromNumeric(voxelId.Value);
                
                return id != null;
            }
        }
        
        id = GetGeneratedVoxelId(position);
        //id = null;
        
        return id != null;
    }
    
    public bool TryGetVoxelDataAtPosition(VoxelPos position, out VoxelData voxelData)
    {
        voxelData = TryGetVoxelIdAtPosition(position, out string id)
            ? LoadedData.GetDataById<VoxelData>(id) : null;
        
        return voxelData != null;
    }
    
}
