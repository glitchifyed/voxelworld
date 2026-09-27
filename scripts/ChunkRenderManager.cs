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
    
    private static ConcurrentQueue<Chunk>[] _chunkCalculationsByThread = new ConcurrentQueue<Chunk>[WorldSettings.CHUNK_CALCULATION_THREADS];
    private static readonly ConcurrentQueue<Chunk> _chunkMeshesToConstruct = [];
    
    private static int _currentCalculationThread;
    
    // TODO: Frustum culling
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnReady()
    {
        
        for (int threadId = 0; threadId < WorldSettings.CHUNK_CALCULATION_THREADS; threadId++)
        {
            ConcurrentQueue<Chunk> threadChunks = [];
            _chunkCalculationsByThread[threadId] = threadChunks;
            
            Task.Run(() => RunChunkMeshCalculator(threadChunks, threadId));
        }
        
    }
    
    public static void OnProcess()
    {
        
        RunChunkGenerator();
        
        
        if (!_chunkMeshesToConstruct.IsEmpty)
        {
            for (int i = 0; i < WorldSettings.CHUNKS_TO_CONSTRUCT; i++)
            {
                if (!_chunkMeshesToConstruct.TryDequeue(out Chunk chunk))
                    break;
                
                chunk.ConstructMesh();
            }
        }
        
    }
    
    
    private static void RunChunkGenerator()
    {
        
        if (_viewpointNode == null)
            return;
        
        VoxelPos currentChunkPosition = _viewpointNode.GlobalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        _chunkGeneratorTokenSource?.Cancel();
        _chunkGeneratorTokenSource = new();
        
        CancellationToken token = _chunkGeneratorTokenSource.Token;
        
        Task.Run(
            () => GenerateChunksInVicinity(token),
            token);
        
    }
    
    private static void RunChunkMeshCalculator(ConcurrentQueue<Chunk> threadChunks, int threadId)
    {
        
        while (true)
            CalculateChunkMeshes(threadChunks);
        
    }
    
    private static void GenerateChunksInVicinity(CancellationToken token)
    {
        
        foreach (VoxelPos renderOffset in _renderBounds)
        {
            if (token.IsCancellationRequested)
                break;
            
            VoxelPos chunkPosition = _chunkPosition.Value + renderOffset - _renderBoundsHalf;
            
            Chunk chunk = World.Singleton.CurrentDimension.GetOrGenerateChunk(chunkPosition);
            
            if (!chunk.HasChangedSinceLastRender())
                continue;
            
            _chunkCalculationsByThread[_currentCalculationThread].Enqueue(chunk);
            _currentCalculationThread = (_currentCalculationThread + 1) % WorldSettings.CHUNK_CALCULATION_THREADS;
        }
        
    }
    
    private static void CalculateChunkMeshes(ConcurrentQueue<Chunk> threadChunks)
    {
        
        while (!threadChunks.IsEmpty)
        {
            if (!threadChunks.TryDequeue(out Chunk chunk))
                break;
            
            if (!chunk.CalculateMesh())
                continue;
            
            _chunkMeshesToConstruct.Enqueue(chunk);
            
        }
        
    }
    
}
