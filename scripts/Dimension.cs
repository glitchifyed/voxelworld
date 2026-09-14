using Godot;
using System;

public class Dimension
{
	
    public int GetBlockIdAtPosition(VoxelPos position)
    {
        if (position.Y < 8)
            return 1;
        
        return 0;
    }
    
}
