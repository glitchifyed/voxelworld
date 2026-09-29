using Godot;
using System;
using System.Collections.Generic;

public readonly record struct Coordinate
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    public readonly FillBounds Enumerator;
    
    
    private static readonly Coordinate[] NeighbourOffset =
    [
        new(x: 1),
        new(x: -1),
        
        new(y: 1),
        new(y: -1),
        
        new(z: 1),
        new(z: -1)
    ];
    
    
    public Coordinate(int x = 0, int y = 0, int z = 0, FillBounds enumerator = FillBounds.Default)
    {
        X = x;
        Y = y;
        Z = z;
        
        Enumerator = enumerator;
    }
    
    public Coordinate(Coordinate coordinate, FillBounds enumerator = FillBounds.Default)
    {
        X = coordinate.X;
        Y = coordinate.Y;
        Z = coordinate.Z;
        
        Enumerator = enumerator;
    }
    
    public static Coordinate OnAllAxes(int i)
        => new(i, i, i);
    
    
    public static readonly Coordinate Zero = new();
    public static readonly Coordinate One = new(1, 1, 1);
    
    
    public readonly Vector3 ToVector3()
        => new(X, Y, Z);
    

    public override readonly string ToString()
        => $"({X}, {Y}, {Z})";
    
    public override readonly int GetHashCode()
        => base.GetHashCode();


    
    public readonly IEnumerator<Coordinate> GetEnumerator()
    {
        return Enumerator switch
        {
            FillBounds.FromCentre => EnumerateFromCentre(this),
            
            _ => EnumerateDefault(this)
        };
    }
    
    public static IEnumerator<Coordinate> EnumerateDefault(Coordinate bounds)
    {
        for (int x = 0; x < bounds.X; x++)
		    for (int y = 0; y < bounds.Y; y++)
		        for (int z = 0; z < bounds.Z; z++)
                    yield return new(x, y, z);
    }
    
    public static IEnumerator<Coordinate> EnumerateFromCentre(Coordinate bounds)
    {
        Queue<Coordinate> queue = [];
        HashSet<Coordinate> hasChecked = [];
        
        queue.Enqueue(Zero);
        hasChecked.Add(Zero);
        
        while (queue.TryDequeue(out Coordinate currentPosition))
        {
            yield return currentPosition;
            
            foreach (Coordinate offset in NeighbourOffset)
            {
                Coordinate position = currentPosition + offset;
                
                if (position.IsWithinBounds(bounds) && hasChecked.Add(position))
                    queue.Enqueue(position);
            }
        }
    }
    
    
    public static Coordinate operator +(Coordinate left, Coordinate right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    
    public static Coordinate operator -(Coordinate left, Coordinate right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    
    public static Coordinate operator *(Coordinate left, float right)
        => new(Mathf.FloorToInt(left.X * right), Mathf.FloorToInt(left.Y * right), Mathf.FloorToInt(left.Z * right));
    
    public static Coordinate operator /(Coordinate left, float right)
        => new(Mathf.FloorToInt(left.X / right), Mathf.FloorToInt(left.Y / right), Mathf.FloorToInt(left.Z / right));
    
    
    public bool IsWithinBounds(Coordinate bounds)
        =>
        Math.Abs(X) <= bounds.X &&
        Math.Abs(Y) <= bounds.Y &&
        Math.Abs(Z) <= bounds.Z;
    
    
    public enum FillBounds
    {
        Default,
        FromCentre
    }
}
