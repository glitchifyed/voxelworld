using Godot;
using System;

public partial class Endpoint : Node
{
    
    public Universe Universe { get; private set; }
    
    public DataFileManager Data;


    public override void _Ready()
    {
        base._Ready();
        
        
        // TODO: if standalone server, get path inside executable
        Data = new();
        
        
        GameSettings.Load();
		LoadedData.Load(Data);
        
        
        Universe = new Universe();
        AddChild(Universe);
    }
}