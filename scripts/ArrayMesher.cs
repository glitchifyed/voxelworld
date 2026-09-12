using Godot;
using System;
using System.Collections.Generic;

public class ArrayMesher
{

	public readonly ArrayMesh ArrayMesh = new();

	private readonly List<Vector3> _vertices = [];
	private readonly List<Vector3> _normals = [];
	private readonly List<Vector2> _uvs = [];
	private readonly List<int> _indices = [];

	private readonly Godot.Collections.Array _surfaceArray = [];

	private readonly Vector3[] _cubeVertices = [
		new(0, 0, 1),
		new(1, 0, 1),
		new(1, 0, 0),
		new(0, 0, 0),

		new(0, 1, 1),
		new(1, 1, 1),
		new(1, 1, 0),
		new(0, 1, 0),
	];

	private readonly int[][] _cubeFaceIndices = [
		// Godot uses clockwise ordering
		
		[ // Front
			0, 4, 5,
			0, 5, 1
		],

		[ // Back
			3, 2, 6,
			3, 6, 7,
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


	public ArrayMesher()
	{
		_surfaceArray.Resize((int)Mesh.ArrayType.Max);
	}


	public void AddFaceQuad(CubeFace cubeFace, Vector3 offset)
	{
		foreach (var index in _cubeFaceIndices[(int)cubeFace])
			_vertices.Add(_cubeVertices[index] + offset);
	}
	
	public void AddCube(Vector3 offset)
	{
		foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace)))
			AddFaceQuad(cubeFace, offset);
	}
	

	public void Clear()
	{
		_vertices.Clear();
		_normals.Clear();
		_uvs.Clear();
		_indices.Clear();
	}

	public void AddSurfaces()
	{
		_surfaceArray[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
		//_surfaceArray[(int)Mesh.ArrayType.Index] = _indices.ToArray();

		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _surfaceArray);
	}

}
