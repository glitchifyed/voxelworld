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

    public override bool Equals(object equalTo)
    {
        if (equalTo is VoxelPos voxelPos)
            return voxelPos.X == X && voxelPos.Y == Y && voxelPos.Z == Z;
        
        if (equalTo is Vector3 vectorPos)
            return vectorPos.ToVoxelPos() == this;
        
        return false;
    }


    
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
    
    public static bool operator ==(VoxelPos left, VoxelPos right)
        => left.Equals(right);
    
    public static bool operator !=(VoxelPos left, VoxelPos right)
        => !left.Equals(right);
    
    
    public bool TryGetValueInArraySafely<T>(T[,,] array, out T value)
    {
        if (AreIndicesWithinArrayBounds(array, X, Y, Z))
        {
            value = array[X, Y, Z];
            
            return true;
        }
        
        value = default;
        
        return false;
    }
    
    
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
