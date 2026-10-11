using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

public partial class PackData
{
    public readonly string Id;

    public readonly HashSet<GenericData> Data = [];
    public readonly HashSet<FilesystemInstance> Scripts = [];
    public readonly Dictionary<string, string> Globals = [];


    public PackData(string id)
    {
        Id = id;
    }


    public string ReplaceGlobals(string id)
    {
        foreach (Match match in GlobalSearch().Matches(id))
        {
            var globalId = match.Groups[1].Value;
            var global = Globals[globalId];

            id = id.Replace($"$({globalId})", global);
        }

        return id;
    }


    [GeneratedRegex("\\$\\((.+?)\\)")]
    private static partial Regex GlobalSearch();
}