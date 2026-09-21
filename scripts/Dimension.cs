using Godot;
using System;

public class Dimension
{
    
    public readonly Chunk[,,] Chunks = new Chunk[WorldSettings.WORLD_CHUNK_COUNT, WorldSettings.WORLD_CHUNK_COUNT, WorldSettings.WORLD_CHUNK_COUNT];
    
    
    public Chunk GetOrGenerateChunk(VoxelPos position)
    {
        Chunk storedChunk = Chunks[position.X, position.Y, position.Z];
        if (storedChunk != null)
            return storedChunk;
        
        Chunk newChunk = new(position);
        newChunk.Generate();
        
        Chunks[position.X, position.Y, position.Z] = newChunk;
        
        return newChunk;
    }
    
    
    public string GetGeneratedVoxelId(VoxelPos position)
    {
        Random random = new();
        
        if (position.Y < 8 + random.NextInt64(-1, 2))
            return random.NextInt64(0, 2) == 1 ? "game.voxel.stone" : "game.voxel.grass";
        
        return null;
        
        // if (random.NextInt64(0, 3) <= 1)
        //     return null;
        
        // return random.NextInt64(0, 2) == 1 ? "game.voxel.stone" : "game.voxel.grass";
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
        Chunk chunk = VoxelPos.TryGetValueInArraySafely(Chunks, chunkPos);
        
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
        
        //id = GetGeneratedVoxelId(position);
        id = null;
        
        return id != null;
    }
    
    public bool TryGetVoxelDataAtPosition(VoxelPos position, out VoxelData voxelData)
    {
        voxelData = TryGetVoxelIdAtPosition(position, out string id)
            ? LoadedData.GetDataById<VoxelData>(id) : null;
        
        return voxelData != null;
    }
    
}
