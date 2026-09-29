using Godot;
using System;

public static class GameSettings
{
	public const int CHUNK_VOXEL_COUNT = 16;
	public const int CHUNKS_TO_CONSTRUCT = 8;
	public const int CHUNK_CALCULATION_THREADS = 4;
	public const int CHUNK_GENERATION_THREADS = 4;
	
	public const byte RENDER_DISTANCE_XZ = 8;
	public const byte RENDER_DISTANCE_Y = 4;
	
	// TODO: world scale
	
	public static Material ChunkMeshMaterial { get; private set; }
	
	
	public static void Load()
	{
		ChunkMeshMaterial = GD.Load<Material>("res://Assets/Materials/ChunkMaterial.tres");
	}
}
