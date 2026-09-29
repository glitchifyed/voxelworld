using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

public class ChunkRenderer
{
    
    private Coordinate? _chunkPosition;
    
    
    private readonly Coordinate _renderBounds = new(
        GameSettings.RENDER_DISTANCE_XZ,
        GameSettings.RENDER_DISTANCE_Y,
        GameSettings.RENDER_DISTANCE_XZ,
        
        Coordinate.FillBounds.FromCentre
        );
    
    private readonly Queue<Chunk> _currentChunks = [];
    
    
    public void OnProcess()
    {
        RunChunkScanner();
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
            if (!chunk.Position.IsWithinBounds(_renderBounds))
                Endpoint.CurrentWorld.UnloadChunk(chunk);
        
        foreach (Coordinate position in _renderBounds)
        {
            Chunk chunk = Endpoint.CurrentWorld.GetChunkAtPosition(position);
            
            
        }
    }
    
}
