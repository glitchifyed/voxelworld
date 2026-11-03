using Godot;
using System;

public static class GameSettings
{
	public const int CHUNK_VOXEL_SIZE = 8;
	public const int CHUNK_VOXEL_COUNT = CHUNK_VOXEL_SIZE * CHUNK_VOXEL_SIZE * CHUNK_VOXEL_SIZE;
	
	public const int MAX_CHUNK_MESHES_PER_FRAME = 8;
	public const int CHUNK_MESH_THREADS = 3;
	public const int CHUNK_FILL_THREADS = 1;
	
	public const byte RENDER_DISTANCE_XZ = 8;
	public const byte RENDER_DISTANCE_Y = 4;
	
	// TODO: world scale
	
	public static Material ChunkMeshMaterial { get; private set; }
	public static Material InvalidChunkMeshMaterial { get; private set; }
	
	
	public static void Load()
	{
		ChunkMeshMaterial = GD.Load<Material>("res://Assets/Materials/ChunkMaterial.tres");
		InvalidChunkMeshMaterial = GD.Load<Material>("res://Assets/Materials/InvalidChunkMaterial.tres");
	}
}
