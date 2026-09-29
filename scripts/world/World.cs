using Godot;
using System;
using System.Threading;

public partial class World : Node
{
	
	public static World Singleton;
	
	public Dimension CurrentDimension;
	
	public Node3D ChunksNode { get; private set; }
	
	public Thread CurrentThread { get; private set; }
	
	[Export] private Node3D viewportNode;
	
	
	public override void _Ready()
	{
		if (Singleton != null)
			return;
		
		Singleton = this;
		
		CurrentDimension = new();
		
		CurrentThread = Thread.CurrentThread;
		
		ChunksNode = GetNode<Node3D>("Chunks");
		
		WorldSettings.LoadWorldSettings();
		
		LoadedData.Load();
		
		ChunkRenderManager.SetViewpointNode(viewportNode);
		ChunkRenderManager.SetWorld(this);
	}

    public override void _Process(double delta)
    {
        ChunkRenderManager.OnProcess();
    }
	
	
	public bool GetIsMainThread()
		=> Thread.CurrentThread == CurrentThread;

	
}
