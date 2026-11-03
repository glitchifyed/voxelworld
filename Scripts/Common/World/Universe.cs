using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class Universe : Node
{
	public readonly Dictionary<string, World> Worlds = [];
	public readonly List<Player> Players = [];
	
	public Node3D ChunksNode { get; private set; }
	
	
	public override void _Ready()
	{
		base._Ready();
		
		
		// TODO: world creation
		AddWorld("glitchifyed.base.worlds.earth", 12345);
		
		
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
	
	
	public World GetWorld(string id)
		=> Worlds[id];
	
	
	private World AddWorld(string id, int seed)
	{
		World world = new(this, id, seed);
		
		Worlds[id] = world;
		
		return world;
	}
}
