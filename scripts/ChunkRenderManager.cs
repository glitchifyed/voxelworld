using Godot;
using System;

public static class ChunkRenderManager
{
    
    private static VoxelPos _chunkPosition;
    private static Chunk _lastChunk;
    private static Node3D _viewpointNode;
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnProcess()
    {
        if (_viewpointNode == null)
            return;
        
        VoxelPos currentChunkPosition = _viewpointNode.GlobalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        _lastChunk?.ClearMesh();
        
        Chunk chunk = World.Singleton.CurrentDimension.GetOrGenerateChunk(currentChunkPosition);
        chunk?.ConstructMesh();
        
        _lastChunk = chunk;
    }
    
}
