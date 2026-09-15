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
    
    
    public string GetVoxelIdAtPosition(VoxelPos position)
    {
        if (position.Y < 8)
            return new Random().NextInt64(0, 2) == 1 ? "game.voxel.stone" : "game.voxel.grass";
        
        return "game.voxel.air";
    }
    
    public VoxelData GetVoxelDataAtPosition(VoxelPos position)
        => LoadedData.GetDataById<VoxelData>(GetVoxelIdAtPosition(position));
    
}
