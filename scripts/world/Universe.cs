using Godot;
using System;
using System.Threading;

public partial class Universe : Node
{
	
	public static Universe Singleton;
	
	public World CurrentWorld;
	
	public Node3D ChunksNode { get; private set; }
	
	public Thread CurrentThread { get; private set; }
	
	[Export] private Node3D viewportNode;
	
	
	public override void _Ready()
	{
		if (Singleton != null)
			return;
		
		Singleton = this;
		
		CurrentWorld = new();
		
		CurrentThread = Thread.CurrentThread;
		
		ChunksNode = GetNode<Node3D>("Chunks");
		
		GameSettings.Load();
		
		LoadedData.Load();
		
		ChunkRenderer.SetViewpointNode(viewportNode);
		ChunkRenderer.SetWorld(this);
	}

    public override void _Process(double delta)
    {
        ChunkRenderer.OnProcess();
    }
	
	
	public bool GetIsMainThread()
		=> Thread.CurrentThread == CurrentThread;

	
}
