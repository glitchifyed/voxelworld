using Godot;
using System;

public class GenericData
{
    
    public static uint DataCount { get; private set; }
    
    
    public readonly string Name;
    public readonly string Id;
    
    public readonly uint NumericId;
    
    
    public GenericData(string name, string id)
    {
        NumericId = DataCount++;
        
        Name = name;
        Id = id;
    }
    
    
    public static void ResetCount()
        => DataCount = 0;
    
}
