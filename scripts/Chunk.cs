using Godot;
using System;

public class Chunk
{
	public readonly int X, Y, Z;
	public readonly MeshInstance3D MeshInstance;
	
	public readonly int[,,] Voxels = new int[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	
	private readonly ArrayMesher _arrayMesher = new();
	
	
	
	public Chunk(int x, int y, int z)
	{
		
		X = x;
		Y = y;
		Z = z;

        MeshInstance = new()
        {
            Position = new Vector3(X, Y, Z) * WorldSettings.CHUNK_VOXEL_COUNT,
			Mesh = _arrayMesher.ArrayMesh
        };
		
		Node chunksNode = World.Singleton.GetNode("Chunks");
		chunksNode.AddChild(MeshInstance);
		
		ConstructMesh();
		
    }
	
	
	public void ConstructMesh()
	{
		
		//_arrayMesher.AddFaceQuad(CubeFace.Bottom);
		_arrayMesher.AddCube(Vector3.One);
		
		_arrayMesher.AddSurfaces();
	}
	
}
