using System;
using Godot;
using NuLua;

public class PackFunction(PackData packData, LuaFunction luaFunction)
{
    public PackData Pack = packData;
    public LuaFunction Function = luaFunction;
}