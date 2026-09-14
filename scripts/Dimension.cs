using Godot;
using System;

public class Dimension
{
    
    public readonly Chunk[,,] Chunks = new Chunk[WorldSettings.WORLD_CHUNK_COUNT, WorldSettings.WORLD_CHUNK_COUNT, WorldSettings.WORLD_CHUNK_COUNT];
	
    
    public int GetBlockIdAtPosition(VoxelPos position)
    {
        if (position.Y < 8)
            return 1;
        
        return 0;
    }
    
}
