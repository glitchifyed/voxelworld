using Godot;
using System;

public partial class World : Node
{
	
	public static World Singleton;
	
	
	public override void _Ready()
	{
		if (Singleton != null)
			return;
		
		Singleton = this;
		
		new Chunk(0, 0, 0);
	}
	
}
