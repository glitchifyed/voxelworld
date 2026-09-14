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
		// Godot uses clockwise ordering
		
		// Front
		new Vector3(0, 0, -1),

		// Back
		new Vector3(0, 0, 1),

		// Right
        new Vector3(1, 0, 0),
		
		// Left
        new Vector3(-1, 0, 0),
		
		// Top
        new Vector3(0, 1, 0),

		// Bottom
        new Vector3(0, -1, 0),
	];
}
