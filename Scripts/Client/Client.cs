using Godot;
using System;

public partial class Client : Endpoint
{
    public static readonly PlayerInput PlayerInput = new();

    public static Client Instance { get; private set; }


    public static Node3D Viewpoint { get; private set; }
    
    
    private readonly ChunkRenderer _chunkRenderer = new();


    public override void _Ready()
    {
        base._Ready();
        
        
        Instance = this;
        
        
        Viewpoint = (Node3D) GD.Load<PackedScene>("res://Assets/Prefabs/SpectatorCamera.tscn").Instantiate();
        AddChild(Viewpoint);
        
        
        _chunkRenderer.OnReady();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        
        PlayerInput.CheckInputs();
        
        
        _chunkRenderer.OnProcess();
    }
}