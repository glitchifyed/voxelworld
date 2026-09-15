using Godot;
using System;

public static class WorldSettings
{
	public const int CHUNK_VOXEL_COUNT = 16;
	public const int WORLD_CHUNK_COUNT = 4;
	
	public static Material ChunkMeshMaterial { get; private set; }
	
	
	public static void LoadWorldSettings()
	{
		ChunkMeshMaterial = GD.Load<Material>("res://materials/chunk_material.tres");
	}
}
