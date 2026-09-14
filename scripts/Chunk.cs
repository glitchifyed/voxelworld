using Godot;
using System;

public class Chunk
{
	public readonly VoxelPos Position;
	public readonly MeshInstance3D MeshInstance;
	
	public readonly int[,,] Voxels = new int[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	
	public Chunk(VoxelPos position)
	{
		Position = position;

        MeshInstance = new()
        {
            Position = Position.ToVector3() * WorldSettings.CHUNK_VOXEL_COUNT,
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
					VoxelPos localPos = new(x, y, z);
					VoxelPos voxelPos = localPos + Position;
					
					if (World.Singleton.CurrentDimension.GetBlockIdAtPosition(voxelPos) == 0)
						continue;
					
					foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace)))
					{
						VoxelPos nextPos = voxelPos + MeshStructures.CubeFaceNormals[(int)cubeFace].ToVoxelPos();
						
						if (World.Singleton.CurrentDimension.GetBlockIdAtPosition(nextPos) != 0)
							continue;
						
						_arrayMesher.AddFaceQuad(cubeFace, localPos.ToVector3());
					}
				}
		
		_arrayMesher.AddSurfaces();
	}
	
}
