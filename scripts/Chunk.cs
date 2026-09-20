using Godot;
using System;

public class Chunk
{
	public readonly VoxelPos Position;
	public readonly MeshInstance3D MeshInstance;
	
	private readonly int?[,,] _voxels = new int?[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	
	private readonly ArrayMesher _arrayMesher = new();
	private readonly Dimension _dimension;
	
	
	
	public Chunk(VoxelPos position)
	{
		Position = position;

        MeshInstance = new()
        {
            Position = Position.ToVector3() * WorldSettings.CHUNK_VOXEL_COUNT,
			Mesh = _arrayMesher.ArrayMesh,
			MaterialOverride = WorldSettings.ChunkMeshMaterial
        };
		
		World.Singleton.ChunksNode.AddChild(MeshInstance);
		
		ConstructMesh();
    }
	
	
	public int? GetVoxelIdAtPosition(VoxelPos position)
		=> _voxels[position.X, position.Y, position.Z];
	
	
	public void Generate()
	{
		for (int x = 0; x < WorldSettings.CHUNK_VOXEL_COUNT; x++)
		for (int y = 0; y < WorldSettings.CHUNK_VOXEL_COUNT; y++)
		for (int z = 0; z < WorldSettings.CHUNK_VOXEL_COUNT; z++)
		{
			
			var voxelPos = new VoxelPos(x, y, z) + Position;
			
			if (World.Singleton.CurrentDimension.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
				_voxels[x, y, z] = voxelData.NumericId;
			
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
			
			if (
				!World.Singleton.CurrentDimension.TryGetVoxelDataAtPosition(voxelPos, out VoxelData voxelData)
				|| !voxelData.Renderable
				)
				continue;
			
			foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace)))
			{
				VoxelPos nextPos = voxelPos + MeshStructures.CubeFaceNormals[(int)cubeFace].ToVoxelPos();
				
				if (
					World.Singleton.CurrentDimension.TryGetVoxelDataAtPosition(nextPos, out VoxelData adjacentVoxelData)
					&& !adjacentVoxelData.Translucent
					)
					continue;
				
				_arrayMesher.AddFaceQuad(cubeFace, localPos.ToVector3(), voxelData.ColorTemp);
			}
			
		}
		
		_arrayMesher.AddSurfaces();
	}
	
}
