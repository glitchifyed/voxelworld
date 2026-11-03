using Godot;
using System;

public static class MeshStructures
{
    public static readonly Vector3[] CubeVertices = [
		new(0, 0, 1), // 0
		new(1, 0, 1), // 1
		new(1, 0, 0), // 2
		new(0, 0, 0), // 3

		new(0, 1, 1), // 4
		new(1, 1, 1), // 5
		new(1, 1, 0), // 6
		new(0, 1, 0), // 7
	];
	
	public static readonly int[][] CubeFaceVertexIndices = [
		// Godot uses clockwise ordering
		
		// Front
		[3, 2, 6, 7],
        
		// Back
        [0, 4, 5, 1],

		// Right
		[1, 5, 6, 2],
		
		// Left
		[0, 3, 7, 4],
		
		// Top
		[4, 7, 6, 5],

		// Bottom
		[0, 1, 2, 3],
	];
	
	public static readonly int[] QuadTriangleIndices = [
        0, 1, 2,
        0, 2, 3
	];
    
    public static readonly Vector3[] CubeFaceNormals = [
		// Front
		new(0, 0, -1),

		// Back
		new(0, 0, 1),

		// Right
        new(1, 0, 0),
		
		// Left
        new(-1, 0, 0),
		
		// Top
        new(0, 1, 0),

		// Bottom
        new(0, -1, 0),
	];
	
	public static readonly Coordinate[] CubeFaceOffsets = [
		// Front
		new(z: -1),

		// Back
		new(z: 1),

		// Right
        new(x: 1),
		
		// Left
        new(x: -1),
		
		// Top
        new(y: 1),

		// Bottom
        new(y: -1),
	];
	
	public static readonly Coordinate[] ChunkFaceOffsets = [
		// Back
		new(z: 1),

		// Front
		new(z: -1),

		// Right
        new(x: 1),
		
		// Left
        new(x: -1),
		
		// Top
        new(y: 1),

		// Bottom
        new(y: -1),
	];
	
	
	public static int GetFaceOpposingIndex(int index)
	{
		if (index % 2 == 0)
			return index + 1;
		
		return index - 1;
	}
	
}
