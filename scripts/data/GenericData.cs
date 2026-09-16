using Godot;
using System;
using System.Collections.Generic;

public class GenericData
{
    
    private readonly static List<string> _numericIds = [];
    
    
    public readonly string Name;
    public readonly string Id;
    
    public readonly int NumericId;
    
    
    public GenericData(string name, string id)
    {
        NumericId = _numericIds.Count;
        _numericIds.Add(id);
        
        Name = name;
        Id = id;
    }
    
    
    public static string GetIdFromNumeric(int numericId)
        => _numericIds[numericId];
    
    public static void Reset()
    {
        _numericIds.Clear();
    }
    
}
