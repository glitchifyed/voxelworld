using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ChunkRenderer
{
    
    private Coordinate? _chunkPosition;
    
    private ThreadedTasks<Chunk> _threadedVoxelFill;
    private ThreadedTasks<Chunk> _threadedMeshData;
    private ThreadedTasks<Chunk> _threadedUnloadChunks;
    
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
        _threadedUnloadChunks = new(1, SequencedUnloadChunk);
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
        
        
        // Unload previous chunks
        // TODO: multithread this?
        while (_currentChunks.TryDequeue(out Chunk chunk))
            if (!(chunk.Position - currentChunkPosition).IsWithinBounds(_renderBounds))
                // Endpoint.CurrentWorld.UnloadChunk(chunk);
            {
                if (!Endpoint.CurrentWorld.Chunks.Remove(chunk.Position))
                    continue;
                
                chunk.Reset();
                
                _threadedUnloadChunks.AddTask(chunk);
            }
        
        
        // Get currently loaded chunks
        foreach (Coordinate offset in _renderBounds)
        {
            var position = currentChunkPosition + offset;
            
            Chunk chunk = Endpoint.CurrentWorld.GetChunkAtPosition(position);
            
            _currentChunks.Enqueue(chunk);
            
            
            // Only do something if the chunk hasn't already been rendered
            if (chunk.IsWithinRenderBounds)
                continue;
            
            chunk.IsWithinRenderBounds = true;
            
            
            if (chunk.RequiresVoxelFill())
                 _threadedVoxelFill.AddTask(chunk);
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
        
        if (chunk.RequiresMeshData())
            _threadedMeshData.AddTask(chunk);
    }
    
    private void SequencedMeshData(Chunk chunk)
    {
        if (!chunk.RequiresMeshData())
            return;
        
        chunk.AddMeshData();
        
        if (chunk.RequiresCreateMesh())
            _chunksMeshesToCreate.Enqueue(chunk);
    }
    
    private void SequencedUnloadChunk(Chunk chunk)
    {
        if (chunk.IsWithinRenderBounds)
            return;
        
        chunk.Clear();
        
        chunk.MeshInstance.QueueFree();
    }
    
}
