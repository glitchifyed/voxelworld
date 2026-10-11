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


    public LuauScriptRunner(LuauScriptManager manager, PackData packData)
    {
        Manager = manager;
        PackData = packData;
        
        State = Manager.GetState(this);
    }
}