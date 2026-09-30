using Godot;
using System;

public partial class Endpoint : Node
{
    
    public static Universe Universe { get; private set; }
    
    public static DataFileManager BuiltInData { get; private set; }
    public static DataFileManager DynamicData { get; private set; }
    
    public static World CurrentWorld
    {
        get => Universe.CurrentWorld;
    }


    public override void _Ready()
    {
        base._Ready();
        
        
        BuiltInData = new(ProjectSettings.GlobalizePath("res://Data"));
        // TODO: if standalone server, get path inside executable
        DynamicData = new(ProjectSettings.GlobalizePath("user://Data"));
        
        
        
        
        
        GameSettings.Load();
		LoadedData.Load();
        
        
        Universe = new Universe();
        AddChild(Universe);
    }
}