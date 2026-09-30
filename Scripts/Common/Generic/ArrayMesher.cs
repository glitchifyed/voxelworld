using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArrayMesher
{

	public readonly ArrayMesh ArrayMesh = new();

	// private readonly List<Vector3> _vertices = [];
	// private readonly List<Vector3> _normals = [];
	// private readonly List<Vector2> _uvs = [];
	// private readonly List<Color> _colors = [];
	
	private readonly Vector3[] _vertices;
	private readonly Vector3[] _normals;
	private readonly Color[] _colors;
	
	private int _currentIndex;

	private readonly Godot.Collections.Array _surfaceArray = [];


	public ArrayMesher(int maxIndex)
	{
		_surfaceArray.Resize((int)Mesh.ArrayType.Max);
		
		_vertices = new Vector3[maxIndex];
		_normals = new Vector3[maxIndex];
		_colors = new Color[maxIndex];
	}


	public void AddQuad(int cubeFaceId, Vector3 offset, Color color)
	{
		ref readonly Vector3 normal = ref MeshStructures.CubeFaceNormals[cubeFaceId];
		
		foreach (int index in MeshStructures.CubeFaceIndices[cubeFaceId])
		{
			ref readonly Vector3 vertex = ref MeshStructures.CubeVertices[index];
			
			_vertices[_currentIndex] = vertex + offset;
			_normals[_currentIndex] = normal;
			_colors[_currentIndex] = color;
			
			_currentIndex++;
		}
	}

	public void AddQuad(CubeFace cubeFace, Vector3 offset, Color color)
		=> AddQuad((int)cubeFace, offset, color);
		

	public void ClearArrays()
	{
		_currentIndex = 0;
	}

	public void AddSurfaces()
	{
		if (_currentIndex == 0)
		{
			ArrayMesh.ClearSurfaces();
			
			return;
		}
		
		_surfaceArray[(int)Mesh.ArrayType.Vertex] = _vertices[.._currentIndex];
		_surfaceArray[(int)Mesh.ArrayType.Normal] = _normals[.._currentIndex];
		_surfaceArray[(int)Mesh.ArrayType.Color] = _colors[.._currentIndex];
		
		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _surfaceArray);
	}
	
	public void ClearSurfaces()
	{
		ArrayMesh.ClearSurfaces();
	}

}
