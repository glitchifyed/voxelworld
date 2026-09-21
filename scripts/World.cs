using Godot;
using System;

public partial class World : Node
{
	
	public static World Singleton;
	
	public Dimension CurrentDimension;
	
	public Node3D ChunksNode { get; private set; }
	
	
	public override void _Ready()
	{
		if (Singleton != null)
			return;
		
		Singleton = this;
		
		CurrentDimension = new();
		
		ChunksNode = GetNode<Node3D>("Chunks");
		
		WorldSettings.LoadWorldSettings();
		
		LoadedData.Load();
		
		foreach (VoxelPos chunkPos in VoxelPos.OnAllAxes(WorldSettings.WORLD_CHUNK_COUNT))
			CurrentDimension.GetOrGenerateChunk(chunkPos);
		
		foreach (Chunk chunk in CurrentDimension.Chunks)
			chunk.ConstructMesh();
	}
	
}
