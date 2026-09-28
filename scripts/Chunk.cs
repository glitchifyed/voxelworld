using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public class Chunk
{
	public readonly VoxelPos Position;
	public readonly MeshInstance3D MeshInstance;
	
	public VoxelPos GlobalPosition
	{
		get => Position * WorldSettings.CHUNK_VOXEL_COUNT;
	}
	
	public bool IsWithinRange;
	
	private readonly int?[,,] _voxels = new int?[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	// private readonly HashSet<VoxelPos> _voxelsToCalculate = [];
	
	private readonly ArrayMesher _arrayMesher = new();
	// private readonly Dimension _dimension;
	
	private bool _generated;
	private bool _recalculate;
	private bool _calculated;
	private bool _empty = true;
	private bool _constructed;
	
	
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
		
		// if (World.Singleton.GetIsMainThread())
		// 	World.Singleton.ChunksNode.AddChild(MeshInstance);
		// else
		// 	World.Singleton.ChunksNode.CallDeferred(Node.MethodName.AddChild, MeshInstance);
    }
	
	
	public int? GetVoxelIdAtPosition(VoxelPos position)
		=> _voxels[position.X, position.Y, position.Z];
	
	
	public bool Generate()
	{
		
		if (_generated)
			return false;
		
		_generated = true;
		
		foreach (VoxelPos localPos in VoxelPos.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		{
			var voxelPos = localPos + GlobalPosition;
			
			if (World.Singleton.CurrentDimension.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
			{
				_voxels[localPos.X, localPos.Y, localPos.Z] = voxelData.NumericId;
				
				// if (voxelData.Renderable)
				// 	_voxelsToCalculate.Add(localPos);
				
				_recalculate = true;
			}
		}
		
		return true;
		
	}
	
	
	public bool CalculateMesh()
	{
		if (!_recalculate)
			return false;
		
		_recalculate = false;
		
		_arrayMesher.ClearArrays();
		
		bool haveIRenderedSomething = false;
		
		// TODO: optimise to scan only the voxels that exist
		// Mission failed. The optimisation sucked.
		foreach (VoxelPos localPos in VoxelPos.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		// foreach (VoxelPos localPos in _voxelsToCalculate)
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
				
				// TODO: cull faces bordering non-generated chunks (OR MAYBE NOT?)
				if (
					World.Singleton.CurrentDimension.TryGetVoxelDataAtPosition(nextPos, out VoxelData adjacentVoxelData)
					&& !adjacentVoxelData.Translucent
					)
					continue;
				
				_arrayMesher.AddFaceQuad(cubeFace, localPos.ToVector3(), voxelData.ColorTemp);
				
				haveIRenderedSomething = true;
			}
			
		}
		
		_calculated = true;
		_empty = !haveIRenderedSomething;
		
		return haveIRenderedSomething;
	}
	
	public bool ConstructMesh()
	{
		if (_constructed || _empty)
			return false;
		
		_constructed = true;
		
		_arrayMesher.ClearSurfaces();
		_arrayMesher.AddSurfaces();
		
		return true;
	}
	
	public void Unload()
	{
		_arrayMesher.ClearArrays();
		_arrayMesher.ClearSurfaces();
	
		_constructed = false;
		_calculated = false;
		_empty = true;
		_recalculate = true;
	}
	
	
	public bool HasChangedSinceLastRender()
		=> _recalculate;
	
	public bool IsConstructed()
		=> _constructed;
	
	public bool IsCalculated()
		=> _calculated;
	
}
