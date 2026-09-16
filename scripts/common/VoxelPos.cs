using Godot;
using System;

public class VoxelPos
{
    public int X;
    public int Y;
    public int Z;
    
    
    public VoxelPos(int x = 0, int y = 0, int z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    
    public static readonly VoxelPos Zero = new();
    public static readonly VoxelPos One = new(1, 1, 1);
    
    
    public Vector3 ToVector3()
        => new(X, Y, Z);
    
    
    public static VoxelPos operator +(VoxelPos left, VoxelPos right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    
    public static VoxelPos operator -(VoxelPos left, VoxelPos right)
        => new(left.X - right.X, left.Y + right.Y, left.Z - right.Z);
    
    public static VoxelPos operator *(VoxelPos left, int right)
        => new(left.X * right, left.Y * right, left.Z * right);
    
    public static VoxelPos operator /(VoxelPos left, int right)
        => new(left.X / right, left.Y / right, left.Z / right);
}
