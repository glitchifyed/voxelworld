using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

public static class ChunkRenderManager
{
    
    private static Coordinate? _chunkPosition;
    private static Node3D _viewpointNode;
    private static World _world;
    
    private static readonly Coordinate _renderBounds = new(
        WorldSettings.RENDER_DISTANCE_XZ,
        WorldSettings.RENDER_DISTANCE_Y,
        WorldSettings.RENDER_DISTANCE_XZ,
        
        Coordinate.FillBounds.FromCentre
        );
    
    private static readonly Queue<Chunk> _currentChunks = [];
    
    
    private static Dimension CurrentDimension {
        get => _world.CurrentDimension;
    }
    
    
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void SetWorld(World world)
        => _world = world;
    
    public static void OnProcess()
    {
        
        RunChunkScanner();
        
        
        
        
    }
    
    
    private static void RunChunkScanner()
    {
        
        if (_viewpointNode == null || _world == null)
            return;
        
        // Only run again when crossing chunk borders
        Coordinate currentChunkPosition = _viewpointNode.GlobalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        
        while (_currentChunks.TryDequeue(out Chunk chunk))
            if (!chunk.Position.IsWithinBounds(_renderBounds))
                CurrentDimension.UnloadChunk(chunk);
        
        foreach (Coordinate position in _renderBounds)
        {
            Chunk chunk = CurrentDimension.GetChunkAtPosition(position);
            
            
        }
        
    }
    
}
