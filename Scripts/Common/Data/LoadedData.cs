using Godot;
using NuLua;
using NuLua.Luau;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public static class LoadedData
{

    public static LuauScriptManager LuauManager = new();

    private static readonly Dictionary<string, GenericData> _loadedData = [];


    public static void Load(DataFileManager data)
    {
        _loadedData.Clear();
        GenericData.Reset();

        // TODO: Add an ingame option to reset the pack instead
        // TODO: Use try catch to show error dialogue
        data.ClearPack("glitchifyed.base");
        data.GenerateFolderFromResources("data");

        var packs = data.CompileDataPacks("data");

        foreach (var pack in packs)
        {
            foreach (GenericData dataObject in pack.Data)
                Add(dataObject);
        }

        // Task.Run(() => RunScripts(packs));
        RunScripts(packs);
    }


    public static void RunScripts(HashSet<PackData> packs)
    {
        // TODO: cancellation token when reloading

        foreach (var pack in packs)
            foreach (var scriptFile in pack.Scripts)
                LuauManager.State.DoString(scriptFile.ReadString());


    }


    public static T GetDataById<T>(string id) where T : GenericData
    {
        if (_loadedData.TryGetValue(id, out GenericData value))
        {
            if (value is not T castValue)
                return null;

            return castValue;
        }

        // Unknown value
        GD.PushWarning($"Failed to get data: '{id}' - did you use the wrong identifier?");

        return default;
    }

    public static T GetDataByNumericId<T>(int id) where T : GenericData
        => GetDataById<T>(GenericData.GetIdFromNumeric(id));

    public static GenericData GetDataById(string id)
        => GetDataById<GenericData>(id);


    public static int LuauAddData(LuauState state, LuaFuncArguments arguments)
    {
        if (arguments.Length < 2 || !arguments[0].TryRead(out string id) || !arguments[1].TryRead(out LuaFunction function))
        {
            GD.PushWarning("Incorrect AddData arguments.");

            return default;
        }

        Add(new LuaFunctionData(id, function));

        return default;
    }


    private static void Add(GenericData data)
        => _loadedData[data.Id] = data;

}
