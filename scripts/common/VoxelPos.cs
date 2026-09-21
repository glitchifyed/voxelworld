using Godot;
using System;
using System.Collections.Generic;

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
    
    public static VoxelPos OnAllAxes(int i)
        => new(i, i, i);
    
    
    public static readonly VoxelPos Zero = new();
    public static readonly VoxelPos One = new(1, 1, 1);
    
    
    public Vector3 ToVector3()
        => new(X, Y, Z);

    public override string ToString()
        => $"{GetType().Name}({X}, {Y}, {Z})";

    
    public IEnumerator<VoxelPos> GetEnumerator()
    {
        for (int x = 0; x < X; x++)
		    for (int y = 0; y < Y; y++)
		        for (int z = 0; z < Z; z++)
                    yield return new(x, y, z);
    }
    
    
    public static VoxelPos operator +(VoxelPos left, VoxelPos right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    
    public static VoxelPos operator -(VoxelPos left, VoxelPos right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    
    public static VoxelPos operator *(VoxelPos left, float right)
        => new(Mathf.FloorToInt(left.X * right), Mathf.FloorToInt(left.Y * right), Mathf.FloorToInt(left.Z * right));
    
    public static VoxelPos operator /(VoxelPos left, float right)
        => new(Mathf.FloorToInt(left.X / right), Mathf.FloorToInt(left.Y / right), Mathf.FloorToInt(left.Z / right));
    
    
    public T TryGetValueInArraySafely<T>(T[,,] array)
        => AreIndicesWithinArrayBounds(array, X, Y, Z) ? array[X, Y, Z] : default;
    
    public static T TryGetValueInArraySafely<T>(T[,,] array, VoxelPos indices)
        => indices.TryGetValueInArraySafely(array);
    
    
    private static bool AreIndicesWithinArrayBounds<T>(T[,,] array, params int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            int index = indices[i];
            
            if (index < 0 || index > array.GetUpperBound(i))
                return false;
        }
        
        return true;
    }
}
