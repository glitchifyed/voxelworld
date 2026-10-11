using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using NuLua;
using NuLua.Luau;

public class LuauScriptRunner
{
    public readonly LuauScriptManager Manager;
    public readonly LuauState State;
    public readonly PackData PackData;
    public readonly HashSet<LuaTable> Tables = [];


    public LuauScriptRunner(LuauScriptManager manager, PackData packData, LuauState state)
    {
        Manager = manager;
        PackData = packData;
        
        State = state;
        
        manager.SetupState(this);
    }
    
    
    public LuaTable RegisterTable(string name)
    {
        var table = State.CreateTable();
        
        State[name] = table;
        
        Tables.Add(table);
        
        return table;
    }
}