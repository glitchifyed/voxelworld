using System;
using Godot;

public partial class Entity : Node3D
{
	public World World { get; private set; }
	
	
	public void SetWorld(World world, Vector3 position)
	{
		World = world;
		Position = position;
	}
}