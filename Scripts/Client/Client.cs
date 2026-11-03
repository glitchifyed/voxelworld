using Godot;
using System;

public partial class Client : Endpoint
{
    public static readonly PlayerInput PlayerInput = new();
    public static readonly ChunkRenderer ChunkRenderer = new();

    public static Client Instance { get; private set; }

    public static Camera3D Viewpoint { get; private set; }
    
    public static Player Player { get; private set; }
    public static World World
    {
        get => Player.World;
    }
    


    public override void _Ready()
    {
        base._Ready();
        
        
        Instance = this;
        
        
        Player = new();
        
        Viewpoint = (Camera3D) GD.Load<PackedScene>("res://Assets/Prefabs/SpectatorCamera.tscn").Instantiate();
        Player.AddChild(Viewpoint);
        
        AddChild(Player);
        
        
        ChunkRenderer.OnReady();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        
        PlayerInput.CheckInputs();
        
        
        ChunkRenderer.OnProcess();
    }
}