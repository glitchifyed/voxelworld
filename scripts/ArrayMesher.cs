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
    
    
    public ArrayMesher()
    {
        _surfaceArray.Resize((int)Mesh.ArrayType.Max);
    }
    
    
    public void AddQuad()
    {
        _vertices.AddRange([
			new(0, 0, 0),
			new(0, 0, 1),
			new(1, 0, 0),
			new(1, 0, 1)
		]);
		
		_normals.AddRange([
			Vector3.Up,
			Vector3.Up,
			Vector3.Up,
			Vector3.Up,
		]);
		
		_uvs.AddRange([
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(0, 1),
			new Vector2(1, 1),
		]);
		
		_indices.AddRange([
			0, 2, 1,
			2, 3, 1,
		]);
    }
    
    public void AddSurfaces()
    {
        _surfaceArray[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
        _surfaceArray[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
        _surfaceArray[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();
        _surfaceArray[(int)Mesh.ArrayType.Index] = _indices.ToArray();
		
		ArrayMesh.ClearSurfaces();
		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _surfaceArray);
    }
    
}
