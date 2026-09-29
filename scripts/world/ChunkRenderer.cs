using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

public static class ChunkRenderer
{
    
    private static Coordinate? _chunkPosition;
    private static Node3D _viewpointNode;
    private static Universe _universe;
    
    private static readonly Coordinate _renderBounds = new(
        GameSettings.RENDER_DISTANCE_XZ,
        GameSettings.RENDER_DISTANCE_Y,
        GameSettings.RENDER_DISTANCE_XZ,
        
        Coordinate.FillBounds.FromCentre
        );
    
    private static readonly Queue<Chunk> _currentChunks = [];
    
    
    private static World CurrentWorld {
        get => _universe.CurrentWorld;
    }
    
    
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void SetWorld(Universe world)
        => _universe = world;
    
    public static void OnProcess()
    {
        
        RunChunkScanner();
        
        
        
        
    }
    
    
    private static void RunChunkScanner()
    {
        
        if (_viewpointNode == null || _universe == null)
            return;
        
        // Only run again when crossing chunk borders
        Coordinate currentChunkPosition = _viewpointNode.GlobalPosition.ToCoordinate() / GameSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        
        while (_currentChunks.TryDequeue(out Chunk chunk))
            if (!chunk.Position.IsWithinBounds(_renderBounds))
                CurrentWorld.UnloadChunk(chunk);
        
        foreach (Coordinate position in _renderBounds)
        {
            Chunk chunk = CurrentWorld.GetChunkAtPosition(position);
            
            
        }
        
    }
    
}
