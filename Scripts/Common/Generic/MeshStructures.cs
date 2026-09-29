using Godot;
using System;

public class MeshStructures
{
    public static readonly Vector3[] CubeVertices = [
		new(0, 0, 1),
		new(1, 0, 1),
		new(1, 0, 0),
		new(0, 0, 0),

		new(0, 1, 1),
		new(1, 1, 1),
		new(1, 1, 0),
		new(0, 1, 0),
	];

	public static readonly int[][] CubeFaceIndices = [
		// Godot uses clockwise ordering
		
		[ // Front
			3, 2, 6,
			3, 6, 7,
		],
        
        [ // Back
			0, 4, 5,
			0, 5, 1
		],

		[ // Right
			1, 5, 6,
			1, 6, 2,
		],
		
		[ // Left
			0, 3, 7,
			0, 7, 4,
		],
		
		[ // Top
			4, 7, 6,
			4, 6, 5,
		],

		[ // Bottom
			0, 1, 2,
			0, 2, 3
		],
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
        new(1),
		
		// Left
        new(-1),
		
		// Top
        new(y: 1),

		// Bottom
        new(y: -1),
	];
	
}
