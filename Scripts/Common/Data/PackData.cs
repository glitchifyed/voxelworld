using System;
using System.Collections.Generic;
using Godot;

public class PackData
{
    public readonly string Id;
    
    public readonly HashSet<GenericData> Data = [];
    
    
    public PackData(string id)
    {
        Id = id;
    }
}