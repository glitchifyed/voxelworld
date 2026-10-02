using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public readonly record struct Coordinate
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    
    
    private static readonly Coordinate[] NeighbourOffset =
    [
        new(x: 1),
        new(x: -1),
        
        new(y: 1),
        new(y: -1),
        
        new(z: 1),
        new(z: -1)
    ];
    
    
    public Coordinate(int x = 0, int y = 0, int z = 0)
    {
        X = x;
        Y = y;
        Z = z;
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
        for (int x = 0; x < X; x++)
		    for (int y = 0; y < Y; y++)
		        for (int z = 0; z < Z; z++)
                    yield return new(x, y, z);
    }
    
    public IEnumerable<Coordinate> FloodFillFromCenter()
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
                
                if (position.IsWithinBounds(this) && hasChecked.Add(position))
                    queue.Enqueue(position);
            }
        }
    }
    
    public IEnumerable<Coordinate> SpiralFromCenter()
    {
        int y = 0;
        
        for (int w = 0; w < Y * 2 + 1; w++)
        {
            int x = 0, z = 0;
            int dx = 1, dz = 0;
            
            int currentSteps = 1;
            int steps = 0;
            int currentTurns = 0;
            
            for (int i = 0; i < X * Z; i++)
            {
                yield return new(x, y, z);
                
                x += dx;
                z += dz;
                
                steps++;
                
                if (steps != currentSteps)
                    continue;
                
                steps = 0;
                
                (dx, dz) = (-dz, dx);
                
                if (currentTurns++ == 2)
                {
                    currentTurns = 0;
                    currentSteps++;
                }
            }
            
            if (y > 0)
                y = -y;
            else
                y = -y + 1;
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
    
    
    public static bool IsWithinBounds(int x, int y, int z, Coordinate bounds)
        =>
        Math.Abs(x) <= bounds.X &&
        Math.Abs(y) <= bounds.Y &&
        Math.Abs(z) <= bounds.Z;
    
    public bool IsWithinBounds(Coordinate bounds)
        => IsWithinBounds(X, Y, Z, bounds);
}
