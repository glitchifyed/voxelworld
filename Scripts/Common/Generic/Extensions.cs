using Godot;
using NuLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static partial class Extensions
{
    public static Coordinate ToCoordinate(this Vector3 position)
        => new(
            Mathf.FloorToInt(position.X),
            Mathf.FloorToInt(position.Y),
            Mathf.FloorToInt(position.Z)
            );


    public static void CheckError(this Error error, string message = null)
    {
        if (error == Error.Ok)
            return;

        GD.PushWarning(error);

        throw new Exception(message ?? $"An error occurred.");
    }

    public static void RegisterValue(this LuaTable table, string name, LuaValue luaValue)
        => table[LuaValue.FromString(name)] = luaValue;


    public static Color ColorFromString(string colorString)
    {
        var match = ColorSplitter().Match(colorString);

        if (match.Groups.Count < 3 || match.Groups[1].Value == "#")
            return Color.FromString(colorString, Colors.White);

        var colorType = match.Groups[1].Value;
        var colorProperties = match.Groups[2].Value;

        if (colorType != "HSV" && colorType != "RGB")
            throw new InvalidOperationException($"Invalid color '{colorString}'");

        var componentMatches = GetColorComponents().Matches(colorProperties);
        if (componentMatches.Count < 3 || componentMatches.Count > 4)
            throw new InvalidOperationException($"Invalid color '{colorString}'");

        var one = float.Parse(componentMatches[0].Value);
        var two = float.Parse(componentMatches[1].Value);
        var three = float.Parse(componentMatches[2].Value);
        var alpha = componentMatches.Count == 4 ? float.Parse(componentMatches[3].Value) : 1f;

        return colorType == "HSV" ?
            Color.FromHsv(one, two, three, alpha) :
            new Color(one, two, three, alpha);
    }

    public static IEnumerator<LuaValue> GetEnumerator(this LuaFuncArguments arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
            yield return arguments[i];
    }


    [GeneratedRegex("^(HSV|RGB|#)(.+)$")]
    private static partial Regex ColorSplitter();

    [GeneratedRegex("[01]\\.?\\d*")]
    private static partial Regex GetColorComponents();
}