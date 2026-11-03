using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArrayMesher
{

	public readonly ArrayMesh ArrayMesh = new();

	private readonly List<Vector3> _vertices = [];
	private readonly List<Vector3> _normals = [];
	private readonly List<Color> _colors = [];
	
	private readonly List<int> _indices = [];

	private int _vertexIndex = -1;


	public void AddQuad(int cubeFaceId, Vector3 offset, Color color)
	{
		ref readonly Vector3 normal = ref MeshStructures.CubeFaceNormals[cubeFaceId];
		ref readonly int[] vertices = ref MeshStructures.CubeFaceVertexIndices[cubeFaceId];
		
		if (_vertexIndex == -1)
			_vertexIndex = 0;
		
		var currentVertexIndex = _vertexIndex;
		
		foreach (int index in MeshStructures.QuadTriangleIndices)
		{
			if (_vertexIndex == -1)
				return;
			
			_indices.Add(_vertexIndex + index);
		}
		
		foreach (int index in vertices)
		{
			if (_vertexIndex == -1)
				return;
			
			ref readonly Vector3 vertex = ref MeshStructures.CubeVertices[index];
			
			_vertices.Add(vertex + offset);
			_normals.Add(normal);
			_colors.Add(color);
			
			currentVertexIndex++;
		}
		
		_vertexIndex = currentVertexIndex;
	}

	public void AddQuad(CubeFace cubeFace, Vector3 offset, Color color)
		=> AddQuad((int)cubeFace, offset, color);
	

	public void ClearArrays()
	{
		_vertexIndex = -1;
		
		_vertices.Clear();
		_normals.Clear();
		_colors.Clear();
		
		_indices.Clear();
	}

	public void AddSurfaces()
	{
		if (_vertexIndex < 1 || _vertices.Count < 1 || _normals.Count < 1 || _colors.Count < 1 || _indices.Count < 1)
		{
			ClearSurfaces();
			ClearArrays();
			
			return;
		}
		
		var vertices = _vertices.ToArray();
		var normals = _normals.ToArray();
		var colors = _colors.ToArray();
		var indices = _indices.ToArray();
		
		if (_vertexIndex < 1 || vertices.Length < 1 || normals.Length < 1 || colors.Length < 1 || indices.Length < 1)
		{
			ClearSurfaces();
			ClearArrays();
			
			return;
		}
		
		using Godot.Collections.Array surfaceArray = [];
		surfaceArray.Resize((int)Mesh.ArrayType.Max);
		
		surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices;
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals;
		surfaceArray[(int)Mesh.ArrayType.Color] = colors;
		
		surfaceArray[(int)Mesh.ArrayType.Index] = indices;
		
		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
	}
	
	public void ClearSurfaces()
		=> ArrayMesh.ClearSurfaces();

}
