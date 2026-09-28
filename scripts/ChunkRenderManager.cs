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
    
    private static readonly ConcurrentQueue<Chunk>[] _calculationThreads = new ConcurrentQueue<Chunk>[WorldSettings.CHUNK_CALCULATION_THREADS];
    private static readonly ConcurrentQueue<Chunk>[] _generationThreads = new ConcurrentQueue<Chunk>[WorldSettings.CHUNK_CALCULATION_THREADS];
    private static readonly ConcurrentQueue<Chunk> _chunksToConstruct = [];
    private static readonly ConcurrentQueue<Chunk> _chunksToUnload = [];
    
    private static readonly HashSet<Chunk> _constructedChunks = [];
    private static HashSet<Chunk> _loadedChunks;
    
    private static int _currentCalculationThread;
    private static int _currentGenerationThread;
    
    
    private static int _generatedChunks;
    private static int _calculatedChunks;
    private static int _renderedChunks;
    
    // TODO: Frustum culling on rendering AND generation
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnReady()
    {
        
        for (int threadId = 0; threadId < WorldSettings.CHUNK_CALCULATION_THREADS; threadId++)
        {
            ConcurrentQueue<Chunk> threadChunks = [];
            _calculationThreads[threadId] = threadChunks;
            
            Task.Run(
                () => RunChunkCalculator(threadChunks)
                );
        }
        
        for (int threadId = 0; threadId < WorldSettings.CHUNK_GENERATION_THREADS; threadId++)
        {
            ConcurrentQueue<Chunk> threadChunks = [];
            _generationThreads[threadId] = threadChunks;
            
            Task.Run(
                () => RunChunkGenerator(threadChunks)
                );
        }
        
    }
    
    public static void OnProcess()
    {
        
        RunChunkScanner();
        
        
        TryUnloadChunks();
        
        TryConstructChunks();
        
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
        
        
        // Get within range
        HashSet<Chunk> currentlyWithinRendering = [];
        
        foreach (VoxelPos renderOffset in _renderBounds)
        {
            VoxelPos chunkPosition = _chunkPosition.Value + renderOffset - _renderBoundsHalf;
            Chunk chunk = World.Singleton.CurrentDimension.GetChunkAtPosition(chunkPosition);
            
            currentlyWithinRendering.Add(chunk);
            
            if (chunk.IsWithinRange && (chunk.IsSentForCalculation || chunk.IsCalculated()))
                continue;
            
            chunk.IsSentForCalculation = true;
            
            _generationThreads[_currentGenerationThread].Enqueue(chunk);
            _currentGenerationThread = (_currentGenerationThread + 1) % WorldSettings.CHUNK_GENERATION_THREADS;
            
            chunk.IsWithinRange = true;
        }
        
        
        // Get outside of range
        if (_loadedChunks != null)
            foreach (Chunk chunk in _loadedChunks)
            {
                if (currentlyWithinRendering.Contains(chunk))
                    continue;
                
                if (!chunk.IsWithinRange)
                    continue;
                
                _chunksToUnload.Enqueue(chunk);
                
                chunk.IsWithinRange = false;
            }
        
        _loadedChunks = currentlyWithinRendering;
        
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
            
            _renderedChunks++;

            chunk.ConstructMesh();
        }
        
        // GD.Print($"Gen: {_generatedChunks} Cal: {_calculatedChunks} Ren: {_renderedChunks}");
    }
    
    private static void RunChunkCalculator(ConcurrentQueue<Chunk> threadChunks)
    {
        
        while (true)
            CalculateChunkMeshes(threadChunks);
        
    }
    
    private static void RunChunkGenerator(ConcurrentQueue<Chunk> threadChunks)
    {
        
        while (true)
            GenerateChunks(threadChunks);
        
    }
    
    private static void GenerateChunks(ConcurrentQueue<Chunk> chunksToGenerate)
    {
        
        while (chunksToGenerate.TryDequeue(out Chunk chunk))
        {
            chunk.Generate();
            
            _generatedChunks++;
            
            if (!chunk.HasChangedSinceLastRender())
                continue;
            
            _calculationThreads[_currentCalculationThread].Enqueue(chunk);
            _currentCalculationThread = (_currentCalculationThread + 1) % WorldSettings.CHUNK_CALCULATION_THREADS;
        }
        
    }
    
    private static void CalculateChunkMeshes(ConcurrentQueue<Chunk> threadChunks)
    {
        
        while (threadChunks.TryDequeue(out Chunk chunk))
        {
            
            if (!chunk.IsWithinRange || !chunk.CalculateMesh())
                continue;
            
            _calculatedChunks++;
            
            _chunksToConstruct.Enqueue(chunk);
            
        }
        
    }
    
}
