using Godot;
using System;

public partial class SpectatorCamera : Camera3D
{

    [Export] private float _moveSpeed = 6f;
    [Export] private float _lookSpeed = 12f;

	private PlayerInput _playerInput = new();
    
    private Vector2 _playerRotation;


    public override void _Ready()
    {
        
        AddChild(_playerInput);
        
        Engine.MaxFps = 30;
        
    }


    public override void _Process(double delta)
    {
        
        var deltaFloat = (float)delta;
		
        _playerInput.StartInputs();
        
        Process_Movement(deltaFloat);
        Process_Look(deltaFloat);
        
        _playerInput.EndInputs();
        
    }
    
    
    private void Process_Movement(float delta)
    {
        
        var forwardVector = -Transform.Basis.Z;
        var rightVector = Transform.Basis.X;
        
        Position +=
            (forwardVector * _playerInput.MoveDirection.Y
            + rightVector * _playerInput.MoveDirection.X)
            
            * _moveSpeed * delta;
        
    }
    
    private void Process_Look(float delta)
    {
        
        _playerRotation += _playerInput.MouseDelta;
        
        RotationDegrees = new Vector3(
            -_playerRotation.Y,
            -_playerRotation.X,
            0f
        )
        * _lookSpeed;
        
    }

}
