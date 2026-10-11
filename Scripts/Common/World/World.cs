using Godot;
using NuLua;
using System;
using System.Collections.Generic;
using System.Linq;

public class World(string worldDataId, int seed)
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


    public Chunk GetChunkAtPosition(Coordinate position)
    {
        if (Chunks.TryGetValue(position, out Chunk storedChunk))
            return storedChunk;

        Chunk newChunk = new(position);
        Chunks[position] = newChunk;

        Endpoint.Universe.ChunksNode.CallDeferred(Node.MethodName.AddChild, newChunk.MeshInstance);

        return newChunk;
    }


    public string GetGeneratedVoxelId(Coordinate position)
    {
        var worldData = LoadedData.GetDataById<WorldData>(WorldDataId);

        var packFunction = LuauScriptManager.GetFunction(worldData.GeneratorId);
        var result = packFunction.Function.Invoke([position.X, position.Y, position.Z]);

        if (!result[0].TryRead(out string voxelId))
            throw new InvalidOperationException("Invalid voxel id.");

        if (voxelId != "")
            return packFunction.Pack.ReplaceGlobals(voxelId);

        return null;
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
