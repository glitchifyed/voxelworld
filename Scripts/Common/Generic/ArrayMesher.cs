using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArrayMesher
{

	public readonly ArrayMesh ArrayMesh = new();

	private readonly List<Vector3> _vertices = [];
	private readonly List<Vector3> _normals = [];
	private readonly List<Vector2> _uvs = [];
	private readonly List<Color> _colors = [];

	private readonly Godot.Collections.Array _surfaceArray = [];

	


	public ArrayMesher()
	{
		_surfaceArray.Resize((int)Mesh.ArrayType.Max);
	}


	public void AddQuad(int cubeFaceId, Vector3 offset, Color color)
	{
		ref readonly Vector3 normal = ref MeshStructures.CubeFaceNormals[cubeFaceId];
		
		foreach (int index in MeshStructures.CubeFaceIndices[cubeFaceId])
		{
			_vertices.Add(MeshStructures.CubeVertices[index] + offset);
			_normals.Add(normal);
			_colors.Add(color);
		}
	}

	public void AddQuad(CubeFace cubeFace, Vector3 offset, Color color)
		=> AddQuad((int)cubeFace, offset, color);
	

	public void ClearArrays()
	{
		_vertices.Clear();
		_normals.Clear();
		_uvs.Clear();
		_colors.Clear();
		
		_surfaceArray.Clear();
	}

	public void AddSurfaces()
	{
		if (!_vertices.Any())
		{
			ArrayMesh.ClearSurfaces();
			
			return;
		}
		
		_surfaceArray[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
		_surfaceArray[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
		_surfaceArray[(int)Mesh.ArrayType.Color] = _colors.ToArray();
		
		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _surfaceArray);
	}
	
	public void ClearSurfaces()
	{
		ArrayMesh.ClearSurfaces();
	}

}
