using Godot;
using System;

public partial class World : Node
{
	
	public static World Singleton;
	
	public Dimension CurrentDimension;
	
	
	public override void _Ready()
	{
		if (Singleton != null)
			return;
		
		Singleton = this;
		
		CurrentDimension = new();
		
		var chunk = new Chunk(VoxelPos.Zero);
	}
	
}
