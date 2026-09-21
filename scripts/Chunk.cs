using Godot;
using System;

public class Chunk
{
	public readonly VoxelPos Position;
	public VoxelPos GlobalPosition
	{
		get => Position * WorldSettings.CHUNK_VOXEL_COUNT;
	}
	public readonly MeshInstance3D MeshInstance;
	
	private readonly int?[,,] _voxels = new int?[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	
	private readonly ArrayMesher _arrayMesher = new();
	private readonly Dimension _dimension;
	
	
	
	public Chunk(VoxelPos position)
	{
		Position = position;

        MeshInstance = new()
        {
			Name = $"Chunk @ {position}",
            Position = Position.ToVector3() * WorldSettings.CHUNK_VOXEL_COUNT,
			Mesh = _arrayMesher.ArrayMesh,
			MaterialOverride = WorldSettings.ChunkMeshMaterial
        };
		
		World.Singleton.ChunksNode.AddChild(MeshInstance);
    }
	
	
	public int? GetVoxelIdAtPosition(VoxelPos position)
		=> _voxels[position.X, position.Y, position.Z];
	
	
	public void Generate()
	{
		foreach (VoxelPos localPos in VoxelPos.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		{
			
			var voxelPos = localPos + GlobalPosition;
			
			if (World.Singleton.CurrentDimension.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
				_voxels[localPos.X, localPos.Y, localPos.Z] = voxelData.NumericId;
			
		}
	}
	
	
	public void ConstructMesh()
	{
		_arrayMesher.ClearArrays();
		
		foreach (VoxelPos localPos in VoxelPos.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		{
			
			VoxelPos voxelPos = localPos + GlobalPosition;
			
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
	
	public void ClearMesh()
	{
		_arrayMesher.ClearSurfaces();
	}
	
}
