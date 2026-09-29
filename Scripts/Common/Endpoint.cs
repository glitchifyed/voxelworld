using Godot;
using System;

public partial class Endpoint : Node
{
    
    public static Universe Universe { get; private set; }
    public static World CurrentWorld
    {
        get => Universe.CurrentWorld;
    }


    public override void _Ready()
    {
        base._Ready();
        
        
        GameSettings.Load();
		LoadedData.Load();
        
        
        Universe = new Universe();
        AddChild(Universe);
    }
}