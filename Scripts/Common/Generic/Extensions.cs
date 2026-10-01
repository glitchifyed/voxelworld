using Godot;
using System;

public static class Extensions
{
    public static Coordinate ToCoordinate(this Vector3 position)
        => new(
            Mathf.FloorToInt(position.X),
            Mathf.FloorToInt(position.Y),
            Mathf.FloorToInt(position.Z)
            );
    
    public static void CheckError(this Error error, string message = null)
    {
        if (error == Error.Ok)
            return;

        GD.PushWarning(error);

        throw new Exception(message ?? $"An error occurred.");
    }
}