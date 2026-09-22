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
    
    
    private static readonly ConcurrentBag<List<Chunk>> _chunksToLoad = [];
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnProcess()
    {
        
        if (!_chunksToLoad.IsEmpty)
        {
            // foreach (Chunk chunk in _chunksToLoad)
            // {
            //     //chunk.CalculateMesh();
            //     chunk.ConstructMesh();
            // }
            
            List<Chunk> chunksLoadedThisFrame = [];
            
            foreach (var taskChunks in _chunksToLoad)
            {
                foreach (Chunk chunk in taskChunks)
                {
                    if (chunksLoadedThisFrame.Contains(chunk))
                        continue;
                    
                    chunksLoadedThisFrame.Add(chunk);
                    
                    chunk.ConstructMesh();
                }
            }
            
            _chunksToLoad.Clear();
        }
        
        if (_viewpointNode == null)
            return;
        
        VoxelPos currentChunkPosition = _viewpointNode.GlobalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        Task loadChunks = Task.Run(LoadChunks);
        
    }
    
    
    private static void LoadChunks()
    {
        
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
                
                //pair.Value.ClearMesh();
            }
        
        _currentLoadedChunks = loadedChunks;
        
        foreach (var pair in loadedChunks)
        {
            pair.Value.CalculateMesh();
            
            // if (!_chunksToLoad.Contains(pair.Value))
            //     _chunksToLoad.Add(pair.Value);
        }
        
        _chunksToLoad.Add(loadedChunks.Values.ToList());
        
    }
    
}
