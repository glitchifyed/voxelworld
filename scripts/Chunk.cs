using Godot;
using System;
using System.Threading;

public class Chunk
{
	public readonly VoxelPos Position;
	public VoxelPos GlobalPosition
	{
		get => Position * WorldSettings.CHUNK_VOXEL_COUNT;
	}
	public readonly MeshInstance3D MeshInstance;
	
	public bool CalculationLock { get; private set; }
	
	private readonly int?[,,] _voxels = new int?[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	private bool _haveVoxelsChanged;
	
	
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
		
		if (World.Singleton.GetIsMainThread())
			World.Singleton.ChunksNode.AddChild(MeshInstance);
		else
			World.Singleton.ChunksNode.CallDeferred(Node.MethodName.AddChild, MeshInstance);
    }
	
	
	public int? GetVoxelIdAtPosition(VoxelPos position)
		=> _voxels[position.X, position.Y, position.Z];
	
	
	public void Generate()
	{
		foreach (VoxelPos localPos in VoxelPos.OnAllAxes(WorldSettings.CHUNK_VOXEL_COUNT))
		{
			
			var voxelPos = localPos + GlobalPosition;
			
			if (World.Singleton.CurrentDimension.TryGetGeneratedVoxelData(voxelPos, out VoxelData voxelData))
			{
				_voxels[localPos.X, localPos.Y, localPos.Z] = voxelData.NumericId;
				
				_haveVoxelsChanged = true;
			}
			
		}
	}
	
	
	public bool CalculateMesh()
	{
		if (CalculationLock || !_haveVoxelsChanged)
			return false;
		
		_haveVoxelsChanged = false;
		
		CalculationLock = true;
		
		//GD.Print("Calculating mesh...");
		
		_arrayMesher.ClearArrays();
		
		bool haveIRenderedSomething = false;
		
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
				
				// TODO: cull faces bordering non-generated chunks
				if (
					World.Singleton.CurrentDimension.TryGetVoxelDataAtPosition(nextPos, out VoxelData adjacentVoxelData)
					&& !adjacentVoxelData.Translucent
					)
					continue;
				
				_arrayMesher.AddFaceQuad(cubeFace, localPos.ToVector3(), voxelData.ColorTemp);
				
				haveIRenderedSomething = true;
			}
			
		}
		
		CalculationLock = false;
		
		return haveIRenderedSomething;
	}
	
	public void ConstructMesh()
	{
		//GD.Print("Constructng mesh..");
		
		_arrayMesher.ClearSurfaces();
		_arrayMesher.AddSurfaces();
	}
	
	public void Unload()
	{
		_arrayMesher.ClearSurfaces();
		
		_haveVoxelsChanged = true;
	}
	
	
	public bool HasChangedSinceLastRender()
		=> _haveVoxelsChanged;
	
}
