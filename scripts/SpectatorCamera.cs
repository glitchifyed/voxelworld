using Godot;
using System;

public partial class SpectatorCamera : Camera3D
{

    [Export] private float _moveSpeed = 6f;
    [Export] private float _lookSpeed = 1f;

	private PlayerInput _playerInput = new();
    
    private Vector2 _playerRotation;


    public override void _Ready()
    {
        
        AddChild(_playerInput);
        
        _playerInput.OnInputEnded += Input_Look;
        
    }


    public override void _Process(double delta)
    {
        
        var deltaFloat = (float)delta;
		
        _playerInput.CheckInputs();
        
        Process_Movement(deltaFloat);
        
        Process_Exit();
        
    }
    
    
    private void Process_Movement(float delta)
    {
        
        var rightVector = Transform.Basis.X;
        var upVector = Transform.Basis.Y;
        var forwardVector = -Transform.Basis.Z;
        
        Position +=
            (rightVector * _playerInput.MoveDirection.X
            + upVector * _playerInput.MoveDirection.Y
            + forwardVector * _playerInput.MoveDirection.Z)
            
            * _moveSpeed * delta;
        
    }
    
    private void Process_Exit()
    {
        
        if (!_playerInput.EscJustPressed)
            return;
        
        Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        
    }
    
    private void Input_Look(InputEvent inputEvent)
    {
        
        if (Input.MouseMode != Input.MouseModeEnum.Captured || inputEvent is not InputEventMouseMotion)
            return;
        
        _playerRotation += _playerInput.MouseDelta * _lookSpeed * 0.5f;
        
        RotationDegrees = new Vector3(
            -_playerRotation.Y,
            -_playerRotation.X,
            0f
        );
        
    }


}
