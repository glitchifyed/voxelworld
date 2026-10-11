using System;
using Godot;

public partial class DebugScreen : CanvasLayer
{
    [Export] private Label _coordinatesLabel;
    [Export] private Label _fillingLabel;
    [Export] private Label _meshingLabel;


    public override void _Process(double delta)
    {
        var position = Client.Viewpoint.GlobalPosition;

        _coordinatesLabel.Text = $"XYZ: {Mathf.Floor(position.X)}, {Mathf.Floor(position.Y)}, {Mathf.Floor(position.Z)}";

        _fillingLabel.Text = $"Filling: {Client.ChunkRenderer.ThreadedVoxelFill.GetCount()}";
        _meshingLabel.Text = $"Meshing: {Client.ChunkRenderer.ThreadedMeshData.GetCount()}";
    }
}