using Godot;
using System;

public class Chunk
{
	public readonly int X, Y, Z;
	public readonly MeshInstance3D MeshInstance;
	
	public readonly int[,,] Voxels = new int[WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT, WorldSettings.CHUNK_VOXEL_COUNT];
	
	
	public Chunk(int x, int y, int z)
	{
		
		X = x;
		Y = y;
		Z = z;

        MeshInstance = new()
        {
            Position = new Vector3(X, Y, Z) * WorldSettings.CHUNK_VOXEL_COUNT
        };
		
		Node chunksNode = World.Singleton.GetNode("Chunks");
		chunksNode.AddChild(MeshInstance);
		
    }
	
	
	public void Render()
	{
		
		Vector3[] vertices =
		{
			Vector3.Up,
			Vector3.Right,
			Vector3.Forward
		};
		
	}
	
}
