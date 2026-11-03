using Godot;
using NuLua;
using System;
using System.Collections.Generic;
using System.Linq;

public class World(Universe universe, string worldDataId, int seed)
{
    public readonly FastNoiseLite GeneratorFastNoise = new()
    {
        NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
        FractalType = FastNoiseLite.FractalTypeEnum.None,
        Frequency = 0.01f,
        Seed = seed
    };

    public readonly string WorldDataId = worldDataId;

    public readonly Dictionary<Coordinate, Chunk> Chunks = [];
    
    public readonly Universe Universe = universe;


    public Chunk GetChunkAtPosition(Coordinate position)
    {
        if (Chunks.TryGetValue(position, out Chunk storedChunk))
            return storedChunk;

        Chunk newChunk = new(this, position);
        Chunks[position] = newChunk;

        Universe.ChunksNode.CallDeferred(Node.MethodName.AddChild, newChunk.MeshInstance);

        return newChunk;
    }

    
    public bool FillVoxels(Chunk chunk)
    {
        bool somethingWasChanged = false;
        
        var worldData = LoadedData.GetDataById<WorldData>(WorldDataId);
        
        var packFunction = LuauScriptManager.GetFunction(worldData.GeneratorId);
        var result = packFunction.Function.Invoke([chunk.GlobalPosition.X, chunk.GlobalPosition.Y, chunk.GlobalPosition.Z]);
        
        if (!result[0].TryRead(out LuaTable voxels))
            throw new InvalidOperationException("Invalid voxel ids.");
        
        int xEdgeOfChunk = -1, yEdgeOfChunk = -1, zEdgeOfChunk = -1;
        
        for (int index = 0; index < GameSettings.CHUNK_VOXEL_COUNT; index++)
        {
            var voxelIdValue = voxels[index];
            if (!voxelIdValue.TryRead(out string voxelId))
                continue;
            
            chunk.Voxels[index] = LoadedData.GetDataById(packFunction.Pack.ReplaceGlobals(voxelId)).NumericId;
            
            var position = Coordinate.GetArrayPositionInChunk(index);
            
            if (GetChunkBoundaryDirection(position.Z, out var zDirection))
                // chunk.AdjacentChunks[zDirection]?.RequestChunkMeshUpdate();
                zEdgeOfChunk = zDirection;
            
            if (GetChunkBoundaryDirection(position.X, out var xDirection))
                // chunk.AdjacentChunks[xDirection + 2]?.RequestChunkMeshUpdate();
                xEdgeOfChunk = xDirection;
            
            if (GetChunkBoundaryDirection(position.Y, out var yDirection))
                // chunk.AdjacentChunks[yDirection + 4]?.RequestChunkMeshUpdate();
                yEdgeOfChunk = yDirection;
            
            somethingWasChanged = true;
        }
        
        voxels.Dispose();
        
        // if (xEdgeOfChunk != -1)
        //     // chunk.AdjacentChunks[xEdgeOfChunk + 2]?.RequestChunkMeshUpdate();
        //     chunk.AdjacentChunks[xEdgeOfChunk + 2]?.SetValidity(false);
        
        // if (yEdgeOfChunk != -1)
        //     // chunk.AdjacentChunks[yEdgeOfChunk + 4]?.RequestChunkMeshUpdate();
        //     chunk.AdjacentChunks[yEdgeOfChunk + 4]?.SetValidity(false);
        
        // if (zEdgeOfChunk != -1)
        //     // chunk.AdjacentChunks[zEdgeOfChunk]?.RequestChunkMeshUpdate();
        //     chunk.AdjacentChunks[zEdgeOfChunk]?.SetValidity(false);
        
        if (somethingWasChanged)
        {
            foreach (var adjacentChunk in chunk.AdjacentChunks)
            {
                adjacentChunk.RequestChunkMeshUpdate();
            }
        }
        
        return somethingWasChanged;
    }
    
    private static bool GetChunkBoundaryDirection(int coordinate, out int direction)
    {
        if (coordinate == 0)
        {
            direction = 0;
            
            return true;
        }
        
        if (coordinate == GameSettings.CHUNK_VOXEL_SIZE - 1)
        {
            direction = 1;
            
            return true;
        }
        
        direction = -1;
        
        return false;
    }

    public bool TryGetVoxelIdAtPosition(Coordinate position, out string id)
    {
        Coordinate chunkPos = position / GameSettings.CHUNK_VOXEL_SIZE;

        if (Chunks.TryGetValue(chunkPos, out Chunk chunk) && chunk != null)
        {
            Coordinate localPos = position - chunkPos * GameSettings.CHUNK_VOXEL_SIZE;
            int? voxelId = chunk.GetVoxelIdAtPosition(localPos);

            if (voxelId.HasValue)
            {
                id = GenericData.GetIdFromNumeric(voxelId.Value);

                return id != null;
            }
        }

        // id = GetGeneratedVoxelId(position);
        id = null;

        return id != null;
    }

    public bool TryGetVoxelDataAtPosition(Coordinate position, out VoxelData voxelData)
    {
        bool exists = TryGetVoxelIdAtPosition(position, out string id);
        voxelData = exists ? LoadedData.GetDataById<VoxelData>(id) : null;

        return voxelData != null;
    }

}
