using Godot;
using System;

public class VoxelData : GenericData
{
    public bool Renderable = true;
    public bool Translucent = false;
    public Color ColorTemp = Color.FromHsv(0, 0, 1);
    
    public VoxelData(string id, string langId = null) : base(id, langId) { }

}
