using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using NuLua;
using NuLua.Luau;

public class LuauScriptManager
{
    private static ThreadLocal<LuauScriptManager> _managers;

    private readonly LuauScriptRunner[] _runners;
    private readonly Dictionary<string, PackFunction> _luaFunctionsById = [];

    private readonly Endpoint _endpoint;


    private LuauScriptManager(Endpoint endpoint, List<PackData> packs)
    {
        _endpoint = endpoint;
        
        _runners = new LuauScriptRunner[packs.Count];

        for (int i = 0; i < packs.Count; i++)
            _runners[i] = new(this, packs[i], LuauState.Create());
    }


    public static void Initialize(List<PackData> packs)
    {
        if (_managers != null)
            Clear();

        _managers = new(
            () => new(packs),
            trackAllValues: true
            );
    }

    public static void Clear()
    {
        if (_managers == null)
            return;

        foreach (var manager in _managers.Values)
        {
            foreach (var runner in manager._runners)
            {
                runner.State.Dispose();

                foreach (var table in runner.Tables)
                    table.Dispose();

                runner.Tables.Clear();
            }

            foreach (var function in manager._luaFunctionsById)
                function.Value.Function.Dispose();
        }

        _managers.Dispose();

        _managers = null;
    }


    public void SetupState(LuauScriptRunner runner)
    {
        var state = runner.State;
        state.OpenLibraries();


        var gameTable = runner.RegisterTable("Game");


        gameTable.RegisterValue("Print", state.CreateFunction((state, arguments) =>
        {
            string printString = null;

            foreach (var argument in arguments)
                printString = printString == null ? argument.ToString() : $"{printString} {argument}";

            GD.Print(printString ?? "nil");

            return 0;
        }));

        gameTable.RegisterValue("SetMethod", state.CreateFunction((state, arguments) =>
        {
            if (arguments.Length < 2 || !arguments[0].TryRead(out string id) || !arguments[1].TryRead(out LuaFunction function))
            {
                GD.PushWarning("Incorrect AddData arguments.");

                return 0;
            }

            _luaFunctionsById[runner.PackData.ReplaceGlobals(id)] = new(runner.PackData, function);

            return 0;
        }));

        gameTable.RegisterValue("GetGlobal", state.CreateFunction((state, arguments) =>
        {
            if (arguments.Length < 1 || !arguments[0].TryRead(out string id))
            {
                GD.PushWarning("Incorrect GetGlobal arguments.");

                return 0;
            }

            var global = runner.PackData.Globals[id];

            state.Push(global);

            return 1;
        }));
        
        
        var generationTable = state.CreateTable();
        
        generationTable.RegisterValue("ChunkVoxelSize", GameSettings.CHUNK_VOXEL_SIZE);

        generationTable.RegisterValue("GetNoise", state.CreateFunction((state, arguments) =>
        {
            if (arguments.Length < 4 || !arguments[0].TryRead(out float x) || !arguments[1].TryRead(out float y) || !arguments[2].TryRead(out float z) || !arguments[3].TryRead(out string worldId))
            {
                GD.PushWarning("Incorrect GetNoise XYZ.");

                return 0;
            }

            World world = _endpoint.Universe.GetWorld(worldId);

            var noiseUnbound = world.GeneratorFastNoise.GetNoise3D(x, y, z);
            var boundNoise = Mathf.Clamp(noiseUnbound + 1, 0, 2) / 2;

            state.Push(boundNoise);

            return 1;
        }));
        
        generationTable.RegisterValue("GetChunkIndex", state.CreateFunction((state, arguments) =>
        {
            if (arguments.Length < 3 || !arguments[0].TryRead(out int x) || !arguments[1].TryRead(out int y) || !arguments[2].TryRead(out int z))
            {
                GD.PushWarning("Incorrect GetChunkIndex XYZ.");

                return 0;
            }
            
            state.Push(new Coordinate(x, y, z).GetIndexInChunk());
            
            return 1;
        }));
        
        gameTable.RegisterValue("Generation", generationTable);


        foreach (var scriptFile in runner.PackData.Scripts)
            state.DoString(scriptFile.ReadString());
    }


    public static LuauScriptManager GetManager()
        => _managers.Value;

    // public static LuauState GetState(int packIndex)
    //     => GetManager().State;

    public static PackFunction GetFunction(string id)
        => GetManager()._luaFunctionsById[id];
}