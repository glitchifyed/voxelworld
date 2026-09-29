using Godot;
using System;

public static class Vector3Extension
{
    public static Coordinate ToVoxelPos(this Vector3 position)
        => new((int)position.X, (int)position.Y, (int)position.Z);
}
