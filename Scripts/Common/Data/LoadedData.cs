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
        using var luauState = LuauState.Create();
        luauState.OpenLibraries();
        
        luauState["print"] = luauState.CreateFunction((state, arguments) =>
        {
            string printString = null;
            
            foreach (var argument in arguments)
                printString = printString == null ? argument.ToString() : $"{printString} {argument}";
            
            GD.Print(printString ?? "nil");
            
            return default;
        });
        
        // TODO: cancellation token when reloading
        
        foreach (var pack in packs)
            foreach (var scriptFile in pack.Scripts)
                luauState.DoString(scriptFile.ReadString());
        
        
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
    
    
    private static void Add(GenericData data)
        => _loadedData.Add(data.Id, data);
    
}
