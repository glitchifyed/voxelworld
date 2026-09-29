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
		get => Position * GameSettings.CHUNK_VOXEL_COUNT;
	}
	
	
	public bool IsWithinRenderBounds;
	
	
	private readonly int?[] _voxels = new int?[
		GameSettings.CHUNK_VOXEL_COUNT *
		GameSettings.CHUNK_VOXEL_COUNT *
		GameSettings.CHUNK_VOXEL_COUNT
		];
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	private bool _isVoxelFilling;
	private bool _isVoxelFilled;
	
	private bool _isAddingMeshData;
	private bool _isCreatingMesh;
	
	private bool _requiresAddMeshData;
	private bool _requiresCreateMesh;
	
	
	public static int GetArrayIndex(Coordinate position)
		=> position.X + GameSettings.CHUNK_VOXEL_COUNT * (position.Y + GameSettings.CHUNK_VOXEL_COUNT * position.Z);
	
	
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
		=> _voxels[GetArrayIndex(position)];
		
	
	
	public void FillVoxels()
	{
		if (!RequiresVoxelFill())
			return;
		
		_isVoxelFilling = true;
		
		foreach (Coordinate localPos in Coordinate.OnAllAxes(GameSettings.CHUNK_VOXEL_COUNT))
		{
			var voxelPos = localPos + GlobalPosition;
			
			if (!Endpoint.CurrentWorld.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
				continue;

			_voxels[GetArrayIndex(localPos)] = voxelData.NumericId;
			
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
		
		
		foreach (Coordinate localPos in Coordinate.OnAllAxes(GameSettings.CHUNK_VOXEL_COUNT))
		{
			Coordinate voxelPos = localPos + GlobalPosition;
			
			if (
				!Endpoint.CurrentWorld.TryGetVoxelDataAtPosition(voxelPos, out VoxelData voxelData)
				|| !voxelData.Renderable
				)
				continue;
			
			foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace)))
			{
				Coordinate nextPos = voxelPos + MeshStructures.CubeFaceNormals[(int)cubeFace].ToCoordinate();
				
				if (
					Endpoint.CurrentWorld.TryGetVoxelDataAtPosition(nextPos, out VoxelData adjacentVoxelData)
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
