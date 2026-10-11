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
		get => Position * GameSettings.CHUNK_VOXEL_SIZE;
	}
	
	
	public bool IsWithinRenderBounds;
	
	
	private readonly int?[] _voxels = new int?[
		GameSettings.CHUNK_VOXEL_COUNT
		];
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	private bool _isVoxelFilling;
	private bool _isVoxelFilled;
	
	private bool _isAddingMeshData;
	private bool _isCreatingMesh;
	
	private bool _requiresAddMeshData;
	private bool _requiresCreateMesh;
	
	
	// public static int GetArrayIndex(Coordinate position)
	// 	=> position.X + GameSettings.CHUNK_VOXEL_SIZE * (position.Y + GameSettings.CHUNK_VOXEL_SIZE * position.Z);
	
	// public static Coordinate GetArrayPosition(int index)
	// {
	// 	int reduced = index / GameSettings.CHUNK_VOXEL_SIZE;
		
	// 	int z = reduced / GameSettings.CHUNK_VOXEL_SIZE;
	// 	int y = reduced - GameSettings.CHUNK_VOXEL_SIZE * z;
	// 	int x = index - GameSettings.CHUNK_VOXEL_SIZE * (y + GameSettings.CHUNK_VOXEL_SIZE * z);
		
	// 	return new(x, y, z);
	// }
	
	
	public Chunk(Coordinate position)
	{
		Position = position;

        MeshInstance = new()
        {
			Name = Position.ToString(),
            Position = GlobalPosition.ToVector3(),
			Mesh = _arrayMesher.ArrayMesh,
			MaterialOverride = GameSettings.ChunkMeshMaterial
        };
		
		Reset();
    }
	
	
	public int? GetVoxelIdAtPosition(Coordinate position)
		=> _voxels[position.GetIndexInChunk()];
	
	public bool TryGetVoxelDataAtPosition(int index, out VoxelData voxelData)
	{
		int? id = _voxels[index];
		
		voxelData = id.HasValue ?
			LoadedData.GetDataById<VoxelData>(GenericData.GetIdFromNumeric(id.Value)) :
			null;
		
		return voxelData != null;
	}
	
	// public bool TryGetVoxelDataAtPosition(Coordinate position, out VoxelData voxelData)
	// 	=> TryGetVoxelDataAtPosition(GetArrayIndex(position), out voxelData);
		
	
	
	public void FillVoxels()
	{
		if (!RequiresVoxelFill())
			return;
		
		_isVoxelFilling = true;
		
		for (int index = 0; index < GameSettings.CHUNK_VOXEL_COUNT; index++)
		{
			var voxelPos = Coordinate.GetArrayPositionInChunk(index) + GlobalPosition;
			
			if (!Endpoint.CurrentWorld.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
				continue;

			_voxels[index] = voxelData.NumericId;
			
			_requiresAddMeshData = true;
		}
		
		_isVoxelFilled = true;
		_isVoxelFilling = false;
	}
	
	
	public void AddMeshData()
	{
		if (!RequiresMeshData())
			return;
		
		_requiresAddMeshData = false;
		_isAddingMeshData = true;
		
		
		_arrayMesher.ClearArrays();
		
		
		for (int index = 0; index < GameSettings.CHUNK_VOXEL_COUNT; index++)
		{
			if (
				!TryGetVoxelDataAtPosition(index, out VoxelData voxelData)
				|| !voxelData.Renderable
				)
				continue;
			
			Coordinate localPos = Coordinate.GetArrayPositionInChunk(index);
			Coordinate voxelPos = localPos + GlobalPosition;
			
			for (int cubeFaceId = 0; cubeFaceId < MeshStructures.CubeFaceOffsets.Length; cubeFaceId++)
			{
				Coordinate nextPos = voxelPos + MeshStructures.CubeFaceOffsets[cubeFaceId];
				
				if (
					Endpoint.CurrentWorld.TryGetVoxelDataAtPosition(nextPos, out VoxelData adjacentVoxelData)
					&& !adjacentVoxelData.Translucent
					)
					continue;
				
				_arrayMesher.AddQuad(cubeFaceId, localPos.ToVector3(), voxelData.ColorTemp);
				
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
	
	public void Clear()
	{
		_arrayMesher.ClearArrays();
		_arrayMesher.ClearSurfaces();
	}
	
	public void Reset()
	{
		_isVoxelFilling = false;
		_isVoxelFilled = false;
		
		_isAddingMeshData = false;
		_isCreatingMesh = false;
		
		_requiresAddMeshData = false;
		_requiresCreateMesh = false;
		
		IsWithinRenderBounds = false;
	}
	
	
	public bool RequiresVoxelFill()
		=> IsWithinRenderBounds && !_isVoxelFilled && !_isVoxelFilling;
	
	public bool RequiresMeshData()
		=> IsWithinRenderBounds && _requiresAddMeshData;
	
	public bool RequiresCreateMesh()
		=> IsWithinRenderBounds && _requiresCreateMesh;
	
}
