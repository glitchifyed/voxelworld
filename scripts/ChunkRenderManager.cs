using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class ChunkRenderManager
{
    
    private static VoxelPos? _chunkPosition;
    private static Node3D _viewpointNode;
    
    private static readonly VoxelPos _renderBounds = new(WorldSettings.RENDER_DISTANCE_XZ * 2 + 1, WorldSettings.RENDER_DISTANCE_Y * 2 + 1, WorldSettings.RENDER_DISTANCE_XZ * 2 + 1, VoxelPos.VoxelEnumerator.FromCentre);
    private static readonly VoxelPos _renderBoundsHalf = _renderBounds / 2;
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnProcess()
    {
        
        RunChunkScanner();
        
        
        
        
    }
    
    
    private static void RunChunkScanner()
    {
        
        if (_viewpointNode == null)
            return;
        
        // Only run again when crossing chunk borders
        VoxelPos currentChunkPosition = _viewpointNode.GlobalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        
        
        
    }
    
}
