using Godot;
using System;

public static class WorldSettings
{
	public const int CHUNK_VOXEL_COUNT = 32;
	public const int CHUNKS_TO_CONSTRUCT = 4;
	public const int CHUNK_CALCULATION_THREADS = 4;
	
	public const byte RENDER_DISTANCE_XZ = 4;
	public const byte RENDER_DISTANCE_Y = 6;
	
	// TODO: world scale
	
	public static Material ChunkMeshMaterial { get; private set; }
	
	
	public static void LoadWorldSettings()
	{
		ChunkMeshMaterial = GD.Load<Material>("res://materials/chunk_material.tres");
	}
}
