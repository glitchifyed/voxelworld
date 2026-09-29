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
	
	
	public Chunk(Coordinate position)
	{
		Position = position;

        MeshInstance = new()
        {
			Name = $"Chunk @ {position}",
            Position = Position.ToVector3() * WorldSettings.CHUNK_VOXEL_COUNT,
			Mesh = _arrayMesher.ArrayMesh,
			MaterialOverride = WorldSettings.ChunkMeshMaterial
        };
    }
	
	
	public int? GetVoxelIdAtPosition(Coordinate position)
		=> _voxels[position.X, position.Y, position.Z];
	
	
	public void Generate()
	{
		foreach (Coordinate localPos in Coordinate.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		{
			var voxelPos = localPos + GlobalPosition;
			
			if (World.Singleton.CurrentDimension.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
			{
				_voxels[localPos.X, localPos.Y, localPos.Z] = voxelData.NumericId;
			}
		}
	}
	
	
	public void AddMeshData()
	{
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
			}
			
		}
	}
	
	public void CreateMesh()
	{
		_arrayMesher.ClearSurfaces();
		_arrayMesher.AddSurfaces();
	}
	
	public void Unload()
	{
		_arrayMesher.ClearArrays();
		_arrayMesher.ClearSurfaces();
	}
	
}
