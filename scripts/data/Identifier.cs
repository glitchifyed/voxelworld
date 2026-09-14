using Godot;
using System;

public class Identifier(string @namespace, string id)
{
    
    public readonly string Namespace = @namespace;
    public readonly string Id = id;


    public override string ToString()
        => $"{Namespace}:{Id}";

    
}
