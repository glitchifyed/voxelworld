using Godot;
using System;

public class Dimension
{
	
    public int GetBlockIdAtPosition(int x, int y, int z)
    {
        if (y < 8)
            return 1;
        
        return 0;
    }
    
}
