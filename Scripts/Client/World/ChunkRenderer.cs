using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChunkRenderer
{
    
    private Coordinate? _chunkPosition;
    
    public ThreadedTasks<Chunk> ThreadedVoxelFill;
    public ThreadedTasks<Chunk> ThreadedMeshData;
    // public ThreadedTasks<Chunk> ThreadedReMeshData;
    // public ThreadedTasks<Coordinate> ThreadedChunkScanner;
    
    private readonly ConcurrentQueue<Chunk> _chunksMeshesToCreate = [];
    
    
    private Vector3? _viewpointPosition;
    
    
    private readonly Coordinate _renderBounds = new(
        GameSettings.RENDER_DISTANCE_XZ,
        GameSettings.RENDER_DISTANCE_Y,
        GameSettings.RENDER_DISTANCE_XZ
        );
    
    private readonly HashSet<Chunk> _currentChunks = [];
    
    
    
    public void OnReady()
    {
        StartChunkScanner();
        
        // ThreadedReMeshData = new(1, SequencedReMeshData);
        ThreadedVoxelFill = new(GameSettings.CHUNK_FILL_THREADS, SequencedVoxelFill);
        ThreadedMeshData = new(GameSettings.CHUNK_MESH_THREADS, SequencedMeshData);
    }
    
    public void OnProcess()
    {
        if (Client.Viewpoint != null)
            _viewpointPosition = Client.Viewpoint.GlobalPosition;
        
        // CheckChunkPosition();
        
        RunCreateMeshes();
    }
    
    
    public void RequestChunkMeshUpdate(Chunk chunk)
    {
        if (chunk.RequiresMeshData())
            ThreadedMeshData.AddTask(chunk);
    }
    
    // public void SendChunkForReAddMesh(Chunk chunk)
    // {
    //     ThreadedReMeshData.AddTask(chunk);
    // }
    
    
    
    private void StartChunkScanner()
    {
        try
        {
            Task.Run(() =>
            {
                while (true)
                    CheckChunkPosition();
            });
        }
        catch (Exception exception)
        {
            GD.PushError(exception);
        }
    }
    
    private void CheckChunkPosition()
    {
        if (!_viewpointPosition.HasValue)
            return;
        
        // Only run again when crossing chunk borders
        Coordinate currentChunkPosition = _viewpointPosition.Value.ToCoordinate() / GameSettings.CHUNK_VOXEL_SIZE;
        if (currentChunkPosition == _chunkPosition)
            return;
        
        _chunkPosition = currentChunkPosition;

        RunChunkScanner(currentChunkPosition);
        // ThreadedChunkScanner.RunTask(currentChunkPosition);
    }
    
    private void RunChunkScanner(Coordinate currentChunkPosition)
    {
        // Unload previous chunks
        foreach (Chunk chunk in _currentChunks)
        {
            if ((chunk.Position - currentChunkPosition).IsWithinBounds(_renderBounds))
                continue;
            
            _currentChunks.Remove(chunk);
            
            if (!Client.World.Chunks.Remove(chunk.Position))
                continue;
            
            chunk.Reset();
            
            UnloadChunk(chunk);
        }
        
        
        // Get currently loaded chunks
        foreach (Coordinate offset in _renderBounds.FloodFillFromCenter())
        {
            if (_chunkPosition != currentChunkPosition)
                break;
            
            var position = currentChunkPosition + offset;
            
            Chunk chunk = Client.World.GetChunkAtPosition(position);
            
            
            // Only do something if the chunk hasn't already been rendered
            if (chunk.IsWithinRenderBounds)
                continue;
            
            chunk.IsWithinRenderBounds = true;
            
            _currentChunks.Add(chunk);
            
            for (int i = 0; i < MeshStructures.ChunkFaceOffsets.Length; i++)
            {
                ref readonly var faceOffset = ref MeshStructures.ChunkFaceOffsets[i];
                
                var adjacentChunk = Client.World.GetChunkAtPosition(position + faceOffset);
                
                chunk.AdjacentChunks[i] = adjacentChunk;
                adjacentChunk.AdjacentChunks[Array.IndexOf(MeshStructures.ChunkFaceOffsets, faceOffset * -1)] = chunk;
            }
            
            
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
    
    // private void SequencedReMeshData(Chunk chunk)
    // {
    //     if (!chunk.IsWithinRenderBounds)
    //         return;
        
    //     chunk.AddMeshData(true);
        
    //     if (chunk.RequiresCreateMesh())
    //         _chunksMeshesToCreate.Enqueue(chunk);
    // }
    
    private void UnloadChunk(Chunk chunk)
    {
        if (chunk.IsWithinRenderBounds)
            return;
        
        chunk.Clear();
        
        for (int i = 0; i < MeshStructures.ChunkFaceOffsets.Length; i++)
		{
			var adjacentChunk = chunk.AdjacentChunks[i];
			chunk.AdjacentChunks[i] = null;
			
            var opposingIndex = MeshStructures.GetFaceOpposingIndex(i);
            
            if (adjacentChunk?.AdjacentChunks[opposingIndex] == chunk)
			    adjacentChunk.AdjacentChunks[opposingIndex] = null;
		}
        
        chunk.MeshInstance.QueueFree();
    }
    
}
