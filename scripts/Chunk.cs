using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public class Chunk
{
	public readonly Coordinate Position;
	public readonly MeshInstance3D MeshInstance;
	
	public Coordinate GlobalPosition
	{
		get => Position * WorldSettings.CHUNK_VOXEL_COUNT;
	}
	
	
	private readonly int?[,,] _voxels = new int?[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	private bool _isVoxelFilling;
	private bool _isVoxelFilled;
	
	private bool _isAddingMeshData;
	private bool _isCreatingMesh;
	
	private bool _requiresAddMeshData;
	private bool _requiresCreateMesh;
	
	
	public Chunk(Coordinate position)
	{
		Position = position;

        MeshInstance = new()
        {
			Name = position.ToString(),
            Position = Position.ToVector3() * WorldSettings.CHUNK_VOXEL_COUNT,
			Mesh = _arrayMesher.ArrayMesh,
			MaterialOverride = WorldSettings.ChunkMeshMaterial
        };
    }
	
	
	public int? GetVoxelIdAtPosition(Coordinate position)
		=> _voxels[position.X, position.Y, position.Z];
	
	
	public void FillVoxels()
	{
		if (!RequiresVoxelFill())
			return;
		
		_isVoxelFilling = true;
		
		foreach (Coordinate localPos in Coordinate.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		{
			var voxelPos = localPos + GlobalPosition;
			
			if (!World.Singleton.CurrentDimension.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
				continue;

			_voxels[localPos.X, localPos.Y, localPos.Z] = voxelData.NumericId;
			
			_requiresAddMeshData = true;
		}
		
		_isVoxelFilled = true;
	}
	
	
	public void AddMeshData()
	{
		if (!RequiresMeshData())
			return;
		
		_requiresAddMeshData = false;
		_isAddingMeshData = true;
		
		
		_arrayMesher.ClearArrays();
		
		
		foreach (Coordinate localPos in Coordinate.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		{
			Coordinate voxelPos = localPos + GlobalPosition;
			
			if (
				!World.Singleton.CurrentDimension.TryGetVoxelDataAtPosition(voxelPos, out VoxelData voxelData)
				|| !voxelData.Renderable
				)
				continue;
			
			foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace)))
			{
				Coordinate nextPos = voxelPos + MeshStructures.CubeFaceNormals[(int)cubeFace].ToVoxelPos();
				
				if (
					World.Singleton.CurrentDimension.TryGetVoxelDataAtPosition(nextPos, out VoxelData adjacentVoxelData)
					&& !adjacentVoxelData.Translucent
					)
					continue;
				
				_arrayMesher.AddFaceQuad(cubeFace, localPos.ToVector3(), voxelData.ColorTemp);
				
				_requiresCreateMesh = true;
			}
		}
		
		_isAddingMeshData = false;
	}
	
	public void CreateMesh()
	{
		if (!RequiresCreateMesh())
			return;
		
		_requiresCreateMesh = false;
		_isCreatingMesh = true;
		
		_arrayMesher.ClearSurfaces();
		_arrayMesher.AddSurfaces();
		
		_isCreatingMesh = false;
	}
	
	public void Unload()
	{
		_arrayMesher.ClearArrays();
		_arrayMesher.ClearSurfaces();
	}
	
	
	public bool RequiresVoxelFill()
		=> !_isVoxelFilled && !_isVoxelFilling;
	
	public bool RequiresMeshData()
		=> _requiresAddMeshData;
	
	public bool RequiresCreateMesh()
		=> _requiresCreateMesh;
	
}
