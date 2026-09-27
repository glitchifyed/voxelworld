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
    private static CancellationTokenSource _chunkGeneratorTokenSource;
    
    private static readonly VoxelPos _renderBounds = new(WorldSettings.RENDER_DISTANCE_XZ * 2 + 1, WorldSettings.RENDER_DISTANCE_Y * 2 + 1, WorldSettings.RENDER_DISTANCE_XZ * 2 + 1, VoxelPos.VoxelEnumerator.FromCentre);
    private static readonly VoxelPos _renderBoundsHalf = _renderBounds / 2;
    
    // private static readonly ConcurrentBag<Chunk> _chunkMeshesToCalculate = [];
    private static readonly ConcurrentQueue<Chunk> _chunkMeshesToCalculate = [];
    private static readonly ConcurrentBag<Chunk> _chunkMeshesToConstruct = [];
    
    private static readonly List<Chunk> _chunkConstructMain = [];
    
    private static Vector3 _globalPosition;
    
    // TODO: Frustum culling
    
    
    public static void SetViewpointNode(Node3D node)
        => _viewpointNode = node;
    
    public static void OnReady()
    {
        //Task.Run(RunChunkGenerator);
        Task.Run(RunChunkMeshCalculator);
    }
    
    public static void OnProcess()
    {
        
        _globalPosition = _viewpointNode.GlobalPosition;
        
        
        RunChunkGenerator();
        
        
        if (!_chunkMeshesToConstruct.IsEmpty || _chunkConstructMain.Any())
        {
            if (_chunkMeshesToConstruct.Count <= WorldSettings.CHUNKS_TO_CONSTRUCT)
            {
                foreach (Chunk chunk in _chunkMeshesToConstruct)
                    chunk.ConstructMesh();
                
                _chunkMeshesToConstruct.Clear();
            }
            else
            {
                _chunkConstructMain.AddRange(_chunkMeshesToConstruct.ToArray());
                _chunkMeshesToConstruct.Clear();
                
                if (_chunkConstructMain.Count > 1)
                    _chunkConstructMain.Sort(CompareChunksByDistance);
                
                foreach (Chunk chunk in _chunkConstructMain.Take(WorldSettings.CHUNKS_TO_CONSTRUCT))
                    chunk.ConstructMesh();
            }
        }
        
    }
    
    
    private static void RunChunkGenerator()
    {
        
        if (_viewpointNode == null)
            return;
        
        VoxelPos currentChunkPosition = _globalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
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
    
    private static void RunChunkMeshCalculator()
    {
        while (true)
            CalculateChunkMeshes();
    }
    
    private static void GenerateChunksInVicinity(CancellationToken token)
    {
        
        foreach (VoxelPos renderOffset in _renderBounds)
        {
            if (token.IsCancellationRequested)
                break;
            
            VoxelPos chunkPosition = _chunkPosition + renderOffset - _renderBoundsHalf;
            
            Chunk chunk = World.Singleton.CurrentDimension.GetOrGenerateChunk(chunkPosition);
            
            if (!chunk.HasChangedSinceLastRender())
                continue;
            
            _chunkMeshesToCalculate.Enqueue(chunk);
        }
        
    }
    
    private static void CalculateChunkMeshes()
    {
        
        if (_chunkMeshesToCalculate.IsEmpty)
            return;
        
        while (true)
        {
            if (!_chunkMeshesToCalculate.TryDequeue(out Chunk chunk))
                break;
            
            if (!chunk.CalculateMesh())
                continue;
            
            _chunkMeshesToConstruct.Add(chunk);
        }
        
    }
    
    private static int CompareChunksByDistance(Chunk left, Chunk right)
    {
        int leftAdded = Math.Abs(left.Position.X) + Math.Abs(left.Position.Y) + Math.Abs(left.Position.Z);
        int rightAdded = Math.Abs(right.Position.X) + Math.Abs(right.Position.Y) + Math.Abs(right.Position.Z);
        
        return leftAdded.CompareTo(rightAdded);
    }
    
}
