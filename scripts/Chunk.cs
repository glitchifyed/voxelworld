using Godot;
using System;

public class Chunk
{
	public readonly int X, Y, Z;
	public readonly MeshInstance3D MeshInstance;
	
	public readonly int[,,] Voxels = new int[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	
	public Chunk(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;

        MeshInstance = new()
        {
            Position = new Vector3(X, Y, Z) * WorldSettings.CHUNK_VOXEL_COUNT,
			Mesh = _arrayMesher.ArrayMesh
        };
		
		Node chunksNode = World.Singleton.GetNode("Chunks");
		chunksNode.AddChild(MeshInstance);
		
		ConstructMesh();
		
    }
	
	
	public void ConstructMesh()
	{
		
		_arrayMesher.ClearArrays();
		
		for (int x = 0; x < WorldSettings.CHUNK_VOXEL_COUNT; x++)
			for (int y = 0; y < WorldSettings.CHUNK_VOXEL_COUNT; y++)
				for (int z = 0; z < WorldSettings.CHUNK_VOXEL_COUNT; z++)
				{
					var globalX = X + x;
					var globalY = Y + y;
					var globalZ = Z + z;
					
					if (World.Singleton.CurrentDimension.GetBlockIdAtPosition(globalX, globalY, globalZ) == 0)
						continue;
					
					_arrayMesher.AddCube(new Vector3(x, y, z));
				}
		
		_arrayMesher.AddSurfaces();
	}
	
}
