using Godot;
using System;

public partial class Server : Endpoint
{
    public static Server Instance { get; private set; }


    public override void _Ready()
    {
        base._Ready();
        
        
        Instance = this;
        
        
        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}