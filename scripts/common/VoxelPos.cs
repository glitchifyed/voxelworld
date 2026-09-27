using Godot;
using System;
using System.Collections.Generic;

public readonly record struct VoxelPos
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    public readonly VoxelEnumerator Enumerator;
    
    
    public VoxelPos(int x = 0, int y = 0, int z = 0, VoxelEnumerator enumerator = VoxelEnumerator.Default)
    {
        X = x;
        Y = y;
        Z = z;
        
        Enumerator = enumerator;
    }
    
    public static VoxelPos OnAllAxes(int i)
        => new(i, i, i);
    
    
    public static readonly VoxelPos Zero = new();
    public static readonly VoxelPos One = new(1, 1, 1);
    
    
    public readonly Vector3 ToVector3()
        => new(X, Y, Z);

    public override readonly string ToString()
        => $"{GetType().Name}({X}, {Y}, {Z})";

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    // public override readonly bool Equals(object equalTo)
    // {
    //     if (equalTo is VoxelPos voxelPos)
    //         return voxelPos.X == X && voxelPos.Y == Y && voxelPos.Z == Z;
        
    //     if (equalTo is Vector3 vectorPos)
    //         return vectorPos.ToVoxelPos() == this;
        
    //     return false;
    // }


    
    public readonly IEnumerator<VoxelPos> GetEnumerator()
    {
        return Enumerator switch
        {
            VoxelEnumerator.FromCentre => EnumerateFromCentre(this),
            
            _ => EnumerateDefault(this)
        };
    }
    
    public static IEnumerator<VoxelPos> EnumerateDefault(VoxelPos bounds)
    {
        for (int x = 0; x < bounds.X; x++)
		    for (int y = 0; y < bounds.Y; y++)
		        for (int z = 0; z < bounds.Z; z++)
                    yield return new(x, y, z);
    }
    
    public static IEnumerator<VoxelPos> EnumerateFromCentre(VoxelPos bounds)
    {
        VoxelPos halfOfThis = bounds / 2;
        
        Queue<VoxelPos> queue = [];
        
        FloodFillPortion(bounds, queue, halfOfThis.X, halfOfThis.Y, halfOfThis.Z);
        
        while (queue.Count != 0)
            yield return queue.Dequeue();
    }
    
    
    private static void FloodFillPortion(VoxelPos bounds, Queue<VoxelPos> queue, int x, int y, int z)
    {
        if (x < 0 || x > bounds.X || y < 0 || y > bounds.Y || z < 0 || z > bounds.Z)
            return;
        
        VoxelPos position = new(x, y, z);
        
        if (queue.Contains(position))
            return;
        
        queue.Enqueue(position);
        
        FloodFillPortion(bounds, queue, x + 1, y, z);
        FloodFillPortion(bounds, queue, x - 1, y, z);
        FloodFillPortion(bounds, queue, x, y + 1, z);
        FloodFillPortion(bounds, queue, x, y - 1, z);
        FloodFillPortion(bounds, queue, x, y, z + 1);
        FloodFillPortion(bounds, queue, x, y, z - 1);
    }
    
    
    public static VoxelPos operator +(VoxelPos left, VoxelPos right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    
    public static VoxelPos operator -(VoxelPos left, VoxelPos right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    
    public static VoxelPos operator *(VoxelPos left, float right)
        => new(Mathf.FloorToInt(left.X * right), Mathf.FloorToInt(left.Y * right), Mathf.FloorToInt(left.Z * right));
    
    public static VoxelPos operator /(VoxelPos left, float right)
        => new(Mathf.FloorToInt(left.X / right), Mathf.FloorToInt(left.Y / right), Mathf.FloorToInt(left.Z / right));
    
    // public static bool operator ==(VoxelPos left, VoxelPos right)
    //     => left.Equals(right);
    
    // public static bool operator !=(VoxelPos left, VoxelPos right)
    //     => !left.Equals(right);
    
    
    public readonly bool TryGetValueInArraySafely<T>(T[,,] array, out T value)
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
    
    
    public enum VoxelEnumerator
    {
        Default,
        FromCentre
    }
}
