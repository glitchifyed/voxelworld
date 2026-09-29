using Godot;
using System;
using System.Collections.Generic;

public class World
{
    
    public readonly Dictionary<Coordinate, Chunk> Chunks = [];
    
    private readonly FastNoiseLite _generatorFastNoise = new()
    {
        NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin
    };
    
    
    public Chunk GetChunkAtPosition(Coordinate position)
    {
        if (Chunks.TryGetValue(position, out Chunk storedChunk))
            return storedChunk;
        
        Chunk newChunk = new(position);
        Chunks[position] = newChunk;
        
        Endpoint.Universe.ChunksNode.AddChild(newChunk.MeshInstance);
        
        return newChunk;
    }
    
    public void UnloadChunk(Chunk chunk)
    {
        if (!Chunks.Remove(chunk.Position))
            return;
        
        chunk.Reset();
        
        chunk.MeshInstance.QueueFree();
    }
    
    
    public string GetGeneratedVoxelId(Coordinate position)
    {
        float heightPercent = Mathf.Clamp(_generatorFastNoise.GetNoise2D(position.X, position.Z) * 10 + 1, -10f, 20f) / 2f;
        int height = Mathf.FloorToInt(20 * heightPercent);
        
        if (position.Y > height)
            return null;
        
        if (position.Y == height)
            return "game.voxel.grass";
        
        if (position.Y >= height - 3)
            return "game.voxel.dirt";
        
        return "game.voxel.stone";
    }
    
    public bool TryGetGeneratedVoxelData(Coordinate voxelPos, out VoxelData voxelData)
    {
        string generatedId = GetGeneratedVoxelId(voxelPos);
        
        if (generatedId == null)
        {
            voxelData = null;
            
            return false;
        }
        
        voxelData = LoadedData.GetDataById<VoxelData>(generatedId);
        
        return voxelData != null;
    }
    
    public bool TryGetVoxelIdAtPosition(Coordinate position, out string id)
    {
        Coordinate chunkPos = position / GameSettings.CHUNK_VOXEL_COUNT;
        
        if (Chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            Coordinate localPos = position - chunkPos * GameSettings.CHUNK_VOXEL_COUNT;
            int? voxelId = chunk.GetVoxelIdAtPosition(localPos);
            
            if (voxelId.HasValue)
            {
                id = GenericData.GetIdFromNumeric(voxelId.Value);
                
                return id != null;
            }
        }
        
        id = GetGeneratedVoxelId(position);
        
        return id != null;
    }
    
    public bool TryGetVoxelDataAtPosition(Coordinate position, out VoxelData voxelData)
    {
        bool exists = TryGetVoxelIdAtPosition(position, out string id);
        voxelData = exists ? LoadedData.GetDataById<VoxelData>(id) : null;
        
        return voxelData != null;
    }
    
}
