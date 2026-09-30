using Godot;
using System;

public partial class SpectatorCamera : Camera3D
{

    [Export] private float _moveSpeed = 6f;
    [Export] private float _sprintBonus = 2f;
    [Export] private float _lookSpeed = 1f;

	
    private Vector2 _playerRotation;


    public override void _Ready()
    {
        AddChild(Client.PlayerInput);
        
        Client.PlayerInput.OnInputEnded += Input_Look;
    }


    public override void _Process(double delta)
    {
        Process_Movement(delta);
        
        Process_Exit();
    }
    
    
    private void Process_Movement(double delta)
    {
        var rightVector = Transform.Basis.X;
        // var upVector = Transform.Basis.Y;
        // var forwardVector = -Transform.Basis.Z;
        
        var upVector = Vector3.Up;
        var forwardVector = upVector.Cross(rightVector);
        
        var sprintBonus = Client.PlayerInput.SprintHeld ? _sprintBonus : 1d;
        var multiplier = _moveSpeed * sprintBonus * delta;
        
        Position += (
            rightVector * Client.PlayerInput.MoveDirection.X
            + upVector * Client.PlayerInput.MoveDirection.Y
            + forwardVector * Client.PlayerInput.MoveDirection.Z
            ) * (float) multiplier;
    }
    
    private void Process_Exit()
    {
        if (!Client.PlayerInput.EscJustPressed)
            return;
        
        Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }
    
    private void Input_Look(InputEvent inputEvent)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured || inputEvent is not InputEventMouseMotion)
            return;
        
        var newRotation = _playerRotation + Client.PlayerInput.MouseDelta * (_lookSpeed * 0.5f);
        
        _playerRotation = new Vector2(
            newRotation.X,
            Mathf.Clamp(newRotation.Y, -90, 90)
        );
        
        RotationDegrees = new Vector3(
            -_playerRotation.Y,
            -_playerRotation.X,
            0f
        );
    }


}
