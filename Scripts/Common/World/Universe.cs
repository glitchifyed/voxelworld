using Godot;
using System;
using System.Threading;

public partial class Universe : Node
{
	public World CurrentWorld;
	
	public Node3D ChunksNode { get; private set; }
	
	
	public override void _Ready()
	{
		base._Ready();
		
		
		CurrentWorld = new();
		
		
		ChunksNode = new()
		{
			Name = "Chunks"
		};
		AddChild(ChunksNode);
	}

    public override void _Process(double delta)
    {
		base._Process(delta);
		
		
        
    }
}
