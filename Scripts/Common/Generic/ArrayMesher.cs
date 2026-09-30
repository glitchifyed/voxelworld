using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArrayMesher
{

	public readonly ArrayMesh ArrayMesh = new();

	private readonly List<Vector3> _vertices = [];
	private readonly List<Vector3> _normals = [];
	// private readonly List<Vector2> _uvs = [];
	private readonly List<Color> _colors = [];
	
	private readonly List<int> _indices = [];

	private readonly Godot.Collections.Array _surfaceArray = [];

	private int _vertexIndex;	


	public ArrayMesher()
	{
		_surfaceArray.Resize((int)Mesh.ArrayType.Max);
	}


	public void AddQuad(int cubeFaceId, Vector3 offset, Color color)
	{
		ref readonly Vector3 normal = ref MeshStructures.CubeFaceNormals[cubeFaceId];
		ref readonly int[] vertices = ref MeshStructures.CubeFaceVertexIndices[cubeFaceId];
		
		foreach (int index in MeshStructures.QuadTriangleIndices)
			_indices.Add(_vertexIndex + index);
		
		foreach (int index in vertices)
		{
			ref readonly Vector3 vertex = ref MeshStructures.CubeVertices[index];
			
			_vertices.Add(vertex + offset);
			_normals.Add(normal);
			_colors.Add(color);
			
			_vertexIndex++;
		}
		
		// foreach (int index in MeshStructures.CubeFaceIndices[cubeFaceId])
		// {
		// 	_vertices.Add(MeshStructures.CubeVertices[index] + offset);
		// 	_normals.Add(normal);
		// 	_colors.Add(color);
		// }
	}

	public void AddQuad(CubeFace cubeFace, Vector3 offset, Color color)
		=> AddQuad((int)cubeFace, offset, color);
	

	public void ClearArrays()
	{
		_vertexIndex = 0;
		
		_vertices.Clear();
		_normals.Clear();
		// _uvs.Clear();
		_colors.Clear();
		
		_indices.Clear();
	}

	public void AddSurfaces()
	{
		if (_vertexIndex == 0)
		{
			ArrayMesh.ClearSurfaces();
			
			return;
		}
		
		_surfaceArray[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
		_surfaceArray[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
		_surfaceArray[(int)Mesh.ArrayType.Color] = _colors.ToArray();
		
		_surfaceArray[(int)Mesh.ArrayType.Index] = _indices.ToArray();
		
		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _surfaceArray);
	}
	
	public void ClearSurfaces()
		=> ArrayMesh.ClearSurfaces();

}
