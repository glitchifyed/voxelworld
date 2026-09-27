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
    private static readonly VoxelPos _renderBoundsHalf = _renderBounds / 2;
    
    private static readonly ConcurrentBag<Chunk> _chunkMeshesToConstruct = [];
    private static readonly ConcurrentBag<Chunk> _chunkMeshesToCalculate = [];
    
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
        
        
        if (!_chunkMeshesToConstruct.IsEmpty)
        {
            // TODO: render the closest chunks first. Use vertical render distance for amount.
            
            // List<Chunk> takenChunksToLoad = _chunkMeshesToConstruct.Take(Math.Min(8, _chunkMeshesToConstruct.Count)).ToList();
            
            // if (takenChunksToLoad.Count > 1)
            //     takenChunksToLoad.Sort(CompareChunksByDistance);
            
            // foreach (Chunk chunk in takenChunksToLoad)
            foreach (Chunk chunk in _chunkMeshesToConstruct.Take(Math.Min(8, _chunkMeshesToConstruct.Count)))
                chunk.ConstructMesh();
        }
    }
    
    
    private static void RunChunkGenerator()
    {
        // while (true)
        //     GenerateChunksInVicinity();
        
        if (_viewpointNode == null)
            return;
        
        VoxelPos currentChunkPosition = _globalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        Task.Run(GenerateChunksInVicinity);
    }
    
    private static void RunChunkMeshCalculator()
    {
        while (true)
            CalculateChunkMeshes();
    }
    
    private static void GenerateChunksInVicinity()
    {
        
        // if (_viewpointNode == null)
        //     return;
        
        // VoxelPos currentChunkPosition = _globalPosition.ToVoxelPos() / WorldSettings.CHUNK_VOXEL_COUNT;
        // if (currentChunkPosition == _chunkPosition)
        //     return;
        
        // _chunkPosition = currentChunkPosition;
        
        foreach (VoxelPos renderOffset in _renderBounds)
        {
            VoxelPos chunkPosition = _chunkPosition + renderOffset - _renderBoundsHalf;
            
            Chunk chunk = World.Singleton.CurrentDimension.GetOrGenerateChunk(chunkPosition);
            
            if (!chunk.HasChangedSinceLastRender())
                continue;
            
            _chunkMeshesToCalculate.Add(chunk);
        }
        
    }
    
    private static void CalculateChunkMeshes()
    {
        
        if (_chunkMeshesToCalculate.IsEmpty)
            return;
        
        List<Chunk> takenChunksToCalculate = _chunkMeshesToCalculate.ToList();
        _chunkMeshesToCalculate.Clear();
            
        if (takenChunksToCalculate.Count > 1)
            takenChunksToCalculate.Sort(CompareChunksByDistance);
            
        foreach (Chunk chunk in takenChunksToCalculate)
        {
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
