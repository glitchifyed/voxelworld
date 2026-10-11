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


    private LuauScriptManager(List<PackData> packs)
    {
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
        
        gameTable.RegisterFunction("Print", state.CreateFunction((state, arguments) =>
        {
            string printString = null;

            foreach (var argument in arguments)
                printString = printString == null ? argument.ToString() : $"{printString} {argument}";

            GD.Print(printString ?? "nil");

            return 0;
        }));
        
        gameTable.RegisterFunction("SetMethod", state.CreateFunction((state, arguments) =>
        {
            if (arguments.Length < 2 || !arguments[0].TryRead(out string id) || !arguments[1].TryRead(out LuaFunction function))
            {
                GD.PushWarning("Incorrect AddData arguments.");

                return 0;
            }

            _luaFunctionsById[runner.PackData.ReplaceGlobals(id)] = new(runner.PackData, function);

            return 0;
        }));
        
        gameTable.RegisterFunction("GetGlobal", state.CreateFunction((state, arguments) =>
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
        
        gameTable.RegisterFunction("GetNoise", state.CreateFunction((state, arguments) =>
        {
            if (arguments.Length < 3 || !arguments[0].TryRead(out float x) || !arguments[1].TryRead(out float y) || !arguments[2].TryRead(out float z))
            {
                GD.PushWarning("Incorrect GetNoise XY.");

                return 0;
            }
            
            var noiseUnbound = Endpoint.CurrentWorld.GeneratorFastNoise.GetNoise3D(x, y, z);
            var boundNoise = Mathf.Clamp(noiseUnbound + 1, 0, 2) / 2;
            
            state.Push(boundNoise);

            return 1;
        }));
        

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