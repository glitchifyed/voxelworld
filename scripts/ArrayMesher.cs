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


	public void AddFaceQuad(CubeFace cubeFace, Vector3 offset, Color color)
	{
		var faceId = (int)cubeFace;
		Vector3 normal = MeshStructures.CubeFaceNormals[faceId];
		
		foreach (var index in MeshStructures.CubeFaceIndices[faceId])
		{
			_vertices.Add(MeshStructures.CubeVertices[index] + offset);
			_normals.Add(normal);
			_colors.Add(color);
		}
	}
	
	public void AddCube(Vector3 offset)
	{
		foreach (CubeFace cubeFace in Enum.GetValues(typeof(CubeFace)))
			AddFaceQuad(cubeFace, offset, Colors.White);
	}
	

	public void ClearArrays()
	{
		_vertices.Clear();
		_normals.Clear();
		_uvs.Clear();
		_colors.Clear();
	}

	public void AddSurfaces()
	{
		_surfaceArray[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
		_surfaceArray[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
		_surfaceArray[(int)Mesh.ArrayType.Color] = _colors.ToArray();

		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _surfaceArray);
	}

}
