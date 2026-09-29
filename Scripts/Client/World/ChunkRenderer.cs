using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ChunkRenderer
{
    
    private Coordinate? _chunkPosition;
    
    private ThreadedTasks<Chunk> _threadedVoxelFill;
    private ThreadedTasks<Chunk> _threadedMeshData;
    
    private readonly ConcurrentQueue<Chunk> _chunksMeshesToCreate = [];
    
    
    private readonly Coordinate _renderBounds = new(
        GameSettings.RENDER_DISTANCE_XZ,
        GameSettings.RENDER_DISTANCE_Y,
        GameSettings.RENDER_DISTANCE_XZ,
        
        Coordinate.FillBounds.FromCentre
        );
    
    private readonly Queue<Chunk> _currentChunks = [];
    
    
    
    public void OnReady()
    {
        _threadedVoxelFill = new(GameSettings.CHUNK_FILL_THREADS, SequencedVoxelFill);
        _threadedMeshData = new(GameSettings.CHUNK_MESH_THREADS, SequencedMeshData);
    }
    
    public void OnProcess()
    {
        RunChunkScanner();
        
        RunCreateMeshes();
    }
    
    
    private void RunChunkScanner()
    {
        if (Client.Viewpoint == null)
            return;
        
        // Only run again when crossing chunk borders
        Coordinate currentChunkPosition = Client.Viewpoint.GlobalPosition.ToCoordinate() / GameSettings.CHUNK_VOXEL_COUNT;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;
        
        
        while (_currentChunks.TryDequeue(out Chunk chunk))
            if (!(chunk.Position - currentChunkPosition).IsWithinBounds(_renderBounds))
                Endpoint.CurrentWorld.UnloadChunk(chunk);
        
        foreach (Coordinate position in _renderBounds)
        {
            Chunk chunk = Endpoint.CurrentWorld.GetChunkAtPosition(currentChunkPosition + position);
            
            _currentChunks.Enqueue(chunk);
            
            if (chunk.IsWithinRenderBounds)
                continue;
            
            chunk.IsWithinRenderBounds = true;
            
            
            if (chunk.RequiresVoxelFill())
                _threadedVoxelFill.AddTask(chunk);
        }
    }
    
    private void RunCreateMeshes()
    {
        while (_chunksMeshesToCreate.TryDequeue(out Chunk chunk))
        {
            if (chunk.RequiresCreateMesh())
                chunk.CreateMesh();
        }
    }
    
    
    public void SequencedVoxelFill(Chunk chunk)
    {
        if (!chunk.RequiresVoxelFill())
            return;
        
        chunk.FillVoxels();
        
        if (chunk.RequiresMeshData())
            _threadedMeshData.AddTask(chunk);
    }
    
    public void SequencedMeshData(Chunk chunk)
    {
        if (!chunk.RequiresMeshData())
            return;
        
        chunk.AddMeshData();
        
        if (chunk.RequiresCreateMesh())
            _chunksMeshesToCreate.Enqueue(chunk);
    }
    
}
