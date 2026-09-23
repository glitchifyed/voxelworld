using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class ChunkRenderManager
{
    
    private static VoxelPos _chunkPosition;
    private static Node3D _viewpointNode;
    private static Dictionary<VoxelPos, Chunk> _currentLoadedChunks;
    
    private static readonly VoxelPos _renderBounds = new(WorldSettings.RENDER_DISTANCE_XZ * 2 + 1, WorldSettings.RENDER_DISTANCE_Y * 2 + 1, WorldSettings.RENDER_DISTANCE_XZ * 2 + 1);
    private static readonly VoxelPos _renderBoundsHalf = _renderBounds * 0.5f;
    
    private static readonly ConcurrentBag<Chunk> _chunksToLoad = [];
    
    private static Vector3 _globalPosition;
    
    // TODO: Frustum culling
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnReady()
    {
        Task loadChunks = Task.Run(RunLoadChunksLoop);
    }
    
    public static void OnProcess()
    {
        
        if (!_chunksToLoad.IsEmpty)
        {
            // TODO: render the closest chunks first. Use vertical render distance for amount.
            foreach (Chunk nextChunk in _chunksToLoad.Take(Math.Min(24, _chunksToLoad.Count)))
                nextChunk.ConstructMesh();
            
            //_chunksToLoad.Clear();
        }
        
        _globalPosition = _viewpointNode.GlobalPosition;
    }
    
    
    private static void RunLoadChunksLoop()
    {
        while (true)
            LoadChunks();
    }
    
    private static void LoadChunks()
    {
        
        if (_viewpointNode == null)
            return;
        
        VoxelPos currentChunkPosition = _globalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        Dictionary<VoxelPos, Chunk> loadedChunks = [];
        
        foreach (VoxelPos renderOffset in _renderBounds)
        {
            VoxelPos chunkPosition = _chunkPosition + renderOffset - _renderBoundsHalf;
            
            Chunk chunk = World.Singleton.CurrentDimension.GetOrGenerateChunk(chunkPosition);
            
            if (chunk != null)
                loadedChunks[chunkPosition] = chunk;
        }
        
        if (_currentLoadedChunks != null)
            foreach (var pair in _currentLoadedChunks)
            {
                if (loadedChunks.ContainsKey(pair.Key))
                    continue;
                // TODO: chunk data to disk and unload from memory
                
                //pair.Value.Unload();
            }
        
        _currentLoadedChunks = loadedChunks;
        
        foreach (var pair in loadedChunks)
        {
            if (!pair.Value.CalculateMesh())
                continue;
            
            if (!_chunksToLoad.Contains(pair.Value))
                _chunksToLoad.Add(pair.Value);
        }
        
    }
    
}
