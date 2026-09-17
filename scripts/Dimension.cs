using Godot;
using System;

public class Dimension
{
    
    private readonly Chunk[,,] _chunks = new Chunk[WorldSettings.WORLD_CHUNK_COUNT, WorldSettings.WORLD_CHUNK_COUNT, WorldSettings.WORLD_CHUNK_COUNT];
	
    
    public Chunk GetOrGenerateChunk(VoxelPos position)
    {
        Chunk storedChunk = _chunks[position.X, position.Y, position.Z];
        if (storedChunk != null)
            return storedChunk;
        
        storedChunk = new(position);
        
        _chunks[position.X, position.Y, position.Z] = storedChunk;
        
        return storedChunk;
    }
    
    
    public string GetGeneratedVoxelId(VoxelPos position)
    {
        if (position.Y < 8)
            return new Random().NextInt64(0, 2) == 1 ? "game.voxel.stone" : "game.voxel.grass";
        
        return null;
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
        Chunk chunk = _chunks[chunkPos.X, chunkPos.Y, chunkPos.Z];
        
        if (chunk != null)
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
        
        return id != null;
    }
    
    public bool TryGetVoxelDataAtPosition(VoxelPos position, out VoxelData voxelData)
    {
        voxelData = TryGetVoxelIdAtPosition(position, out string id)
            ? LoadedData.GetDataById<VoxelData>(id) : null;
        
        return voxelData != null;
    }
    
}
