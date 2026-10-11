using Godot;
using System;

public partial class Client : Endpoint
{
    public static readonly PlayerInput PlayerInput = new();

    public static Client Instance { get; private set; }


    public static Camera3D Viewpoint { get; private set; }
    
    public static readonly ChunkRenderer ChunkRenderer = new();


    public override void _Ready()
    {
        base._Ready();
        
        
        Instance = this;
        
        
        Viewpoint = (Camera3D) GD.Load<PackedScene>("res://Assets/Prefabs/SpectatorCamera.tscn").Instantiate();
        AddChild(Viewpoint);
        
        
        ChunkRenderer.OnReady();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        
        PlayerInput.CheckInputs();
        
        
        ChunkRenderer.OnProcess();
    }
}