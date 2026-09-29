using Godot;
using System;
using System.Collections.Generic;

public class GenericData
{
    
    private readonly static List<string> _numericIds = [];
    
    
    public readonly string Id;
    public readonly string LangId;
    
    public readonly int NumericId;
    
    
    public GenericData(string id, string langId = null)
    {
        NumericId = _numericIds.Count;
        _numericIds.Add(id);
        
        Id = id;
        LangId = langId;
    }
    
    
    public string GetLangId()
        => LangId ?? Id;
    
    
    public static string GetIdFromNumeric(int numericId)
        => _numericIds[numericId];
    
    public static void Reset()
        => _numericIds.Clear();
    
}
