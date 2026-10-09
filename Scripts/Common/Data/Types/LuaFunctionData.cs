using Godot;
using NuLua;
using System;

public class LuaFunctionData(string id, LuaFunction luaFunction) : GenericData(id)
{
    public readonly LuaFunction LuaFunction = luaFunction;
}
