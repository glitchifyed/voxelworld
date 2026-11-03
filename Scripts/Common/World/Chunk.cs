using Godot;
using NuLua.Interop.Luau;
using System;
using System.Collections.Generic;
using System.Threading;

public class Chunk
{
	public readonly Coordinate Position;
	public readonly MeshInstance3D MeshInstance;
	
	public readonly World World;
	
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
	
	
	public Chunk(World world, Coordinate position)
	{
		World = world;
		
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
	
	
	public void SetValidity(bool validity)
	{
		MeshInstance.MaterialOverride = validity ? GameSettings.ChunkMeshMaterial : GameSettings.InvalidChunkMeshMaterial;
	}
	
	
	
	public void FillVoxels()
	{
		if (!RequiresVoxelFill())
			return;
		
		_isVoxelFilling = true;
		
		_requiresAddMeshData = World.FillVoxels(this);
		
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
			if (!_isAddingMeshData)
			{
				AddMeshData();
				
				return;
			}
			
			if (!IsWithinRenderBounds)
			{
				_isAddingMeshData = false;
				
				return;
			}
			
			if (
				!TryGetVoxelDataAtPosition(index, out VoxelData voxelData)
				|| !voxelData.Renderable
				)
				continue;
			
			Coordinate localPos = Coordinate.GetArrayPositionInChunk(index);
			Coordinate voxelPos = localPos + GlobalPosition;
			
			for (int cubeFaceId = 0; cubeFaceId < MeshStructures.CubeFaceOffsets.Length; cubeFaceId++)
			{
				if (!_isAddingMeshData)
				{
					AddMeshData();
					
					return;
				}
				
				if (!IsWithinRenderBounds)
				{
					_isAddingMeshData = false;
					
					return;
				}
				
				Coordinate nextPos = voxelPos + MeshStructures.CubeFaceOffsets[cubeFaceId];
				
				if (
					World.TryGetVoxelDataAtPosition(nextPos, out VoxelData adjacentVoxelData)
					&& !adjacentVoxelData.Translucent
					)
					continue;
				
				_arrayMesher.AddQuad(cubeFaceId, localPos.ToVector3(), voxelData.ColorTemp);
				
				_requiresCreateMesh = true;
			}
		}
		
		if (RequiresMeshData())
		{
			AddMeshData();
			
			return;
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
		_requiresAddMeshData = true;
		_isMeshDataAdded = false;
		
		if (_isAddingMeshData)
		{
			_isAddingMeshData = false;
			
			return;
		}
		
		// TODO: client vs server logic
		Client.ChunkRenderer.RequestChunkMeshUpdate(this);
		// Client.ChunkRenderer.SendChunkForReAddMesh(this);
	}
	
	
	public bool RequiresVoxelFill()
		=> IsWithinRenderBounds && !_isVoxelFilled && !_isVoxelFilling;
	
	public bool RequiresMeshData()
		=> IsWithinRenderBounds && _requiresAddMeshData;
	
	public bool RequiresCreateMesh()
		=> IsWithinRenderBounds && _requiresCreateMesh;
	
}
