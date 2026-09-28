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
    private static CancellationTokenSource _chunkGeneratorTokenSource;
    
    private static readonly VoxelPos _renderBounds = new(WorldSettings.RENDER_DISTANCE_XZ * 2 + 1, WorldSettings.RENDER_DISTANCE_Y * 2 + 1, WorldSettings.RENDER_DISTANCE_XZ * 2 + 1, VoxelPos.VoxelEnumerator.FromCentre);
    private static readonly VoxelPos _renderBoundsHalf = _renderBounds / 2;
    
    private static readonly ConcurrentQueue<Chunk>[] _calculationThreads = new ConcurrentQueue<Chunk>[WorldSettings.CHUNK_CALCULATION_THREADS];
    private static readonly ConcurrentQueue<Chunk> _chunksToConstruct = [];
    private static readonly ConcurrentQueue<Chunk> _chunksToUnload = [];
    
    private static readonly HashSet<Chunk> _constructedChunks = [];
    private static HashSet<Chunk> _loadedChunks;
    
    private static int _currentCalculationThread;
    
    // TODO: Frustum culling
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnReady()
    {
        
        for (int threadId = 0; threadId < WorldSettings.CHUNK_CALCULATION_THREADS; threadId++)
        {
            ConcurrentQueue<Chunk> threadChunks = [];
            _calculationThreads[threadId] = threadChunks;
            
            Task.Run(
                () => RunChunkMeshCalculator(threadChunks)
                );
        }
        
    }
    
    public static void OnProcess()
    {
        
        RunChunkGenerator();
        
        
        TryUnloadChunks();
        
        TryConstructChunks();
        
    }
    
    
    private static void RunChunkGenerator()
    {
        
        if (_viewpointNode == null)
            return;
        
        // Only run again when crossing chunk borders
        VoxelPos currentChunkPosition = _viewpointNode.GlobalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        
        // Get within range
        Queue<Chunk> currentlyWithinRendering = [];
        
        foreach (VoxelPos renderOffset in _renderBounds)
        {
            VoxelPos chunkPosition = _chunkPosition.Value + renderOffset - _renderBoundsHalf;
            Chunk chunk = World.Singleton.CurrentDimension.GetChunkAtPosition(chunkPosition);
            
            currentlyWithinRendering.Enqueue(chunk);
            
            chunk.IsWithinRange = true;
        }
        
        
        // Get outside of range
        if (_loadedChunks != null)
            foreach (Chunk chunk in _loadedChunks)
            {
                if (currentlyWithinRendering.Contains(chunk))
                    continue;
                
                _chunksToUnload.Enqueue(chunk);
                
                chunk.IsWithinRange = false;
            }
        
        _loadedChunks = currentlyWithinRendering.ToHashSet();
        
        
        // Cancel the current generator thread
        _chunkGeneratorTokenSource?.Cancel();
        
        // Run a new generator thread
        _chunkGeneratorTokenSource = new();
        
        CancellationToken token = _chunkGeneratorTokenSource.Token;
        
        Task.Run(
            () => GenerateChunksInVicinity(token, currentlyWithinRendering),
            token
            );
        
    }
    
    private static void TryUnloadChunks()
    {
        while (_chunksToUnload.TryDequeue(out Chunk chunk))
        {
            
            if (chunk.IsWithinRange)
                continue;
            
            _constructedChunks.Remove(chunk);
            
            World.Singleton.CurrentDimension.UnloadChunk(chunk);
            
        }
    }
    
    private static void TryConstructChunks()
    {
        if (_chunksToConstruct.IsEmpty)
            return;
        
        int i = 0;

        while (i < WorldSettings.CHUNKS_TO_CONSTRUCT)
        {
            if (!_chunksToConstruct.TryDequeue(out Chunk chunk))
                break;

            if (!chunk.IsCalculated() || !chunk.IsWithinRange || _chunksToUnload.Contains(chunk))
                continue;

            i++;

            chunk.ConstructMesh();
        }
    }
    
    private static void RunChunkMeshCalculator(ConcurrentQueue<Chunk> threadChunks)
    {
        
        while (true)
            CalculateChunkMeshes(threadChunks);
        
    }
    
    private static void GenerateChunksInVicinity(CancellationToken token, Queue<Chunk> chunksToGenerate)
    {
        
        while (!token.IsCancellationRequested)
        {
            if (!chunksToGenerate.TryDequeue(out Chunk chunk))
                break;
            
            chunk.Generate();
            
            if (!chunk.HasChangedSinceLastRender())
                continue;
            
            _calculationThreads[_currentCalculationThread].Enqueue(chunk);
            _currentCalculationThread = (_currentCalculationThread + 1) % WorldSettings.CHUNK_CALCULATION_THREADS;
        }
        
    }
    
    private static void CalculateChunkMeshes(ConcurrentQueue<Chunk> threadChunks)
    {
        
        while (!threadChunks.IsEmpty)
        {
            if (!threadChunks.TryDequeue(out Chunk chunk))
                break;
            
            if (!chunk.CalculateMesh() || !chunk.IsWithinRange)
                continue;
            
            _chunksToConstruct.Enqueue(chunk);
            
        }
        
    }
    
}
