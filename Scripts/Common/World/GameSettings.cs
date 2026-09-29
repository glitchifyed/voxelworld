using Godot;
using System;

public static class GameSettings
{
	public const int CHUNK_VOXEL_COUNT = 16;
	public const int MAX_CHUNK_MESHES_PER_FRAME = 8;
	public const int CHUNK_MESH_THREADS = 8;
	public const int CHUNK_FILL_THREADS = 2;
	
	public const byte RENDER_DISTANCE_XZ = 16;
	public const byte RENDER_DISTANCE_Y = 4;
	
	// TODO: world scale
	
	public static Material ChunkMeshMaterial { get; private set; }
	
	
	public static void Load()
	{
		ChunkMeshMaterial = GD.Load<Material>("res://Assets/Materials/ChunkMaterial.tres");
	}
}
