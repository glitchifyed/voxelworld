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
	
	
	public readonly int?[] Voxels = new int?[GameSettings.CHUNK_VOXEL_COUNT];
	public readonly Chunk[] AdjacentChunks = new Chunk[6];
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	private bool _isVoxelFilling;
	private bool _isVoxelFilled;
	
	private bool _isAddingMeshData;
	private bool _isMeshDataAdded;
	
	private bool _isCreatingMesh;
	
	private bool _requiresAddMeshData;
	private bool _requiresCreateMesh;
	
	
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
		=> Voxels[position.GetIndexInChunk()];
	
	public bool TryGetVoxelDataAtPosition(int index, out VoxelData voxelData)
	{
		int? id = Voxels[index];
		
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
		
		_requiresAddMeshData = Endpoint.CurrentWorld.FillVoxels(this);
		
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
		
		_isMeshDataAdded = true;
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
		_isMeshDataAdded = false;
		
		_isCreatingMesh = false;
		
		_requiresAddMeshData = false;
		_requiresCreateMesh = false;
		
		IsWithinRenderBounds = false;
	}
	
	
	public void RequestChunkMeshUpdate()
	{
		if (!IsWithinRenderBounds || !_isMeshDataAdded)
			return;
		
		_requiresCreateMesh = true;
		_isMeshDataAdded = false;
		
		// TODO: client vs server logic
		Client.ChunkRenderer.RequestChunkMeshUpdate(this);
	}
	
	
	public bool RequiresVoxelFill()
		=> IsWithinRenderBounds && !_isVoxelFilled && !_isVoxelFilling;
	
	public bool RequiresMeshData()
		=> IsWithinRenderBounds && _requiresAddMeshData;
	
	public bool RequiresCreateMesh()
		=> IsWithinRenderBounds && _requiresCreateMesh;
	
}
