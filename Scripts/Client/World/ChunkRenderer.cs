using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ChunkRenderer
{
    
    private Coordinate? _chunkPosition;
    
    public ThreadedTasks<Chunk> ThreadedVoxelFill;
    public ThreadedTasks<Chunk> ThreadedMeshData;
    public ThreadedTasks<Coordinate> ThreadedChunkScanner;
    
    private readonly ConcurrentQueue<Chunk> _chunksMeshesToCreate = [];
    
    
    private readonly Coordinate _renderBounds = new(
        GameSettings.RENDER_DISTANCE_XZ,
        GameSettings.RENDER_DISTANCE_Y,
        GameSettings.RENDER_DISTANCE_XZ
        );
    
    private readonly HashSet<Chunk> _currentChunks = [];
    
    
    
    public void OnReady()
    {
        ThreadedVoxelFill = new(GameSettings.CHUNK_FILL_THREADS, SequencedVoxelFill);
        ThreadedMeshData = new(GameSettings.CHUNK_MESH_THREADS, SequencedMeshData);
        ThreadedChunkScanner = new(1, RunChunkScanner);
    }
    
    public void OnProcess()
    {
        // RunChunkScanner();
        CheckChunkPosition();
        
        RunCreateMeshes();
    }
    
    
    public void RequestChunkMeshUpdate(Chunk chunk)
    {
        if (chunk.RequiresMeshData())
            ThreadedMeshData.AddTask(chunk);
    }
    
    
    private void CheckChunkPosition()
    {
        if (Client.Viewpoint == null)
            return;
        
        // Only run again when crossing chunk borders
        Coordinate currentChunkPosition = Client.Viewpoint.GlobalPosition.ToCoordinate() / GameSettings.CHUNK_VOXEL_SIZE;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        // RunChunkScanner(currentChunkPosition);
        ThreadedChunkScanner.AddTask(currentChunkPosition);
    }
    
    private void RunChunkScanner(Coordinate currentChunkPosition)
    {
        // Unload previous chunks
        foreach (Chunk chunk in _currentChunks)
        {
            if ((chunk.Position - currentChunkPosition).IsWithinBounds(_renderBounds))
                continue;
            
            _currentChunks.Remove(chunk);
            
            if (!Endpoint.CurrentWorld.Chunks.Remove(chunk.Position))
                continue;
            
            chunk.Reset();
            
            UnloadChunk(chunk);
        }
        
        
        // Get currently loaded chunks
        foreach (Coordinate offset in _renderBounds.FloodFillFromCenter())
        {
            var position = currentChunkPosition + offset;
            
            Chunk chunk = Endpoint.CurrentWorld.GetChunkAtPosition(position);
            
            
            for (int i = 0; i < MeshStructures.ChunkFaceOffsets.Length; i++)
            {
                ref readonly var faceOffset = ref MeshStructures.ChunkFaceOffsets[i];
                
                var adjacentChunk = Endpoint.CurrentWorld.GetChunkAtPosition(position + faceOffset);
                
                chunk.AdjacentChunks[i] = adjacentChunk;
                adjacentChunk.AdjacentChunks[Array.IndexOf(MeshStructures.ChunkFaceOffsets, faceOffset * -1)] = chunk;
            }
            
            
            // Only do something if the chunk hasn't already been rendered
            if (chunk.IsWithinRenderBounds)
                continue;
            
            chunk.IsWithinRenderBounds = true;
            
            _currentChunks.Add(chunk);
            
            
            if (chunk.RequiresVoxelFill())
                ThreadedVoxelFill.AddTask(chunk);
        }
    }
    
    // Run on the main thread. Instructs ArrayMeshes to construct geometry.
    private void RunCreateMeshes()
    {
        for (int i = 0; i < GameSettings.MAX_CHUNK_MESHES_PER_FRAME; i++)
        {
            if (!_chunksMeshesToCreate.TryDequeue(out Chunk chunk))
                break;
            
            if (chunk.RequiresCreateMesh())
                chunk.CreateMesh();
        }
    }
    
    
    private void SequencedVoxelFill(Chunk chunk)
    {
        if (!chunk.RequiresVoxelFill())
            return;
        
        chunk.FillVoxels();
        
        RequestChunkMeshUpdate(chunk);
    }
    
    private void SequencedMeshData(Chunk chunk)
    {
        if (!chunk.RequiresMeshData())
            return;
        
        chunk.AddMeshData();
        
        if (chunk.RequiresCreateMesh())
            _chunksMeshesToCreate.Enqueue(chunk);
    }
    
    private void UnloadChunk(Chunk chunk)
    {
        if (chunk.IsWithinRenderBounds)
            return;
        
        chunk.Clear();
        
        chunk.MeshInstance.QueueFree();
    }
    
}
