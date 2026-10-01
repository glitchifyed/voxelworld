using Godot;
using System;

public partial class Endpoint : Node
{
    
    public static Universe Universe { get; private set; }
    
    public static DataFileManager Data;
    
    public static World CurrentWorld
    {
        get => Universe.CurrentWorld;
    }


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