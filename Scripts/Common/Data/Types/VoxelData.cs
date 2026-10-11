using Godot;
using System;

public class VoxelData(string id) : GenericData(id)
{
    public bool Renderable = true;
    public bool Translucent = false;
    public Color ColorTemp = Color.FromHsv(0, 0, 1);
}
