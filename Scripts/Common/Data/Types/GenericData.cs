using Godot;
using System;
using System.Collections.Generic;

public class GenericData
{
    
    private readonly static List<string> _numericIds = [];
    
    
    public readonly string Id;
    
    public readonly int NumericId;
    
    
    public GenericData(string id)
    {
        NumericId = _numericIds.Count;
        _numericIds.Add(id);
        
        Id = id;
    }
    
    
    public static string GetIdFromNumeric(int numericId)
        => _numericIds[numericId];
    
    public static void Reset()
        => _numericIds.Clear();
    
}
