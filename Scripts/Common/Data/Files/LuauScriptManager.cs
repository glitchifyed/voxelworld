using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using NuLua;
using NuLua.Luau;

public class LuauScriptManager
{
    private static ThreadLocal<LuauScriptManager> _localManager;

    public readonly LuauState State;

    private readonly Dictionary<string, LuaFunction> _luaFunctionsById = [];


    private LuauScriptManager(HashSet<PackData> packs)
    {
        State = LuauState.Create();
        State.OpenLibraries();

        State.RegisterFunction("print", (state, arguments) =>
        {
            string printString = null;

            foreach (var argument in arguments)
                printString = printString == null ? argument.ToString() : $"{printString} {argument}";

            GD.Print(printString ?? "nil");

            return 0;
        });

        State.RegisterFunction("AddData", (state, arguments) =>
        {
            if (arguments.Length < 2 || !arguments[0].TryRead(out string id) || !arguments[1].TryRead(out LuaFunction function))
            {
                GD.PushWarning("Incorrect AddData arguments.");

                return default;
            }

            _luaFunctionsById[id] = function;

            return default;
        });
        
        foreach (var pack in packs)
            foreach (var scriptFile in pack.Scripts)
                State.DoString(scriptFile.ReadString());
    }


    public static void Initialize(HashSet<PackData> packs)
    {
        _localManager = new(() => {
            return new(packs);
        }, trackAllValues: true);
    }


    public static LuauScriptManager GetManager()
        => _localManager.Value;

    public static LuauState GetState()
        => GetManager().State;

    public static LuaFunction GetFunction(string id)
        => GetManager()._luaFunctionsById[id];


    public static void Clear()
    {
        if (_localManager == null)
            return;

        foreach (var value in _localManager.Values)
        {
            value.State.Dispose();
            
            foreach (var function in value._luaFunctionsById)
                function.Value.Dispose();
        }

        _localManager.Dispose();
    }
}