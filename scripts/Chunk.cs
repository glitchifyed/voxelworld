using Godot;
using System;

public class Chunk
{
	public readonly VoxelPos Position;
	public readonly MeshInstance3D MeshInstance;
	
	public readonly uint[,,] Voxels = new uint[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	
	public Chunk(VoxelPos position)
	{
		Position = position;

        MeshInstance = new()
        {
            Position = Position.ToVector3() * WorldSettings.CHUNK_VOXEL_COUNT,
			Mesh = _arrayMesher.ArrayMesh,
			MaterialOverride = WorldSettings.ChunkMeshMaterial
        };
		
		Node chunksNode = World.Singleton.GetNode("Chunks");
		chunksNode.AddChild(MeshInstance);
		
		ConstructMesh();
    }
	
	
	public void Generate()
	{
		for (int x = 0; x < WorldSettings.CHUNK_VOXEL_COUNT; x++)
		for (int y = 0; y < WorldSettings.CHUNK_VOXEL_COUNT; y++)
		for (int z = 0; z < WorldSettings.CHUNK_VOXEL_COUNT; z++)
		{
			
			var voxelPos = new VoxelPos(x, y, z) + Position;
			var voxelData = World.Singleton.CurrentDimension.GetVoxelDataAtPosition(voxelPos);
			
			Voxels[x, y, z] = voxelData.NumericId;
			
		}
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
			
			VoxelData voxelData = World.Singleton.CurrentDimension.GetVoxelDataAtPosition(voxelPos);
			
			
			
			if (!voxelData.Renderable)
				continue;
			
			foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace)))
			{
				VoxelPos nextPos = voxelPos + MeshStructures.CubeFaceNormals[(int)cubeFace].ToVoxelPos();
				
				if (!World.Singleton.CurrentDimension.GetVoxelDataAtPosition(nextPos).Translucent)
					continue;
				
				_arrayMesher.AddFaceQuad(cubeFace, localPos.ToVector3(), voxelData.ColorTemp);
			}
			
		}
		
		_arrayMesher.AddSurfaces();
	}
	
}
