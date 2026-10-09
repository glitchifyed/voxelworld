using System;
using Godot;
using NuLua;
using NuLua.Luau;

public class LuauScriptManager
{
    public LuauState State;

    public LuauScriptManager()
    {
        State = LuauState.Create();
        State.OpenLibraries();

        State.RegisterFunction("print", (state, arguments) =>
        {
            string printString = null;

            foreach (var argument in arguments)
                printString = printString == null ? argument.ToString() : $"{printString} {argument}";

            GD.Print(printString ?? "nil");

            return default;
        });

        State.RegisterFunction("AddData", (state, arguments) => LoadedData.LuauAddData(state, arguments));
    }

    public LuauState CreateThread()
        => State.CreateThread();
}