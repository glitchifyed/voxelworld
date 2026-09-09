using Godot;
using System;

public partial class PlayerInput : Node
{
	
	public Vector3 MoveDirection { get; private set; }
	
	public Vector2 MouseDelta { get; private set; }
	public Vector2 MousePosition { get; private set; }
	
	public Action<InputEvent> OnInputEnded;
	
	public bool EscJustPressed;


    public override void _Ready()
    {
        
		
		
    }


    public override void _Input(InputEvent inputEvent)
    {
        
		CheckMouseEvent(inputEvent);
		CheckMouseMotion(inputEvent);
		
		OnInputEnded?.Invoke(inputEvent);
		
    }


	public void CheckInputs()
	{
		
		MoveDirection = new Vector3(
			Input.GetAxis("move_left", "move_right"),
			Input.GetAxis("move_down", "move_up"),
			Input.GetAxis("move_backward", "move_forward")
		).Normalized();
		
		EscJustPressed = Input.IsActionJustPressed("exit");
		
	}
	
	
	private void CheckMouseEvent(InputEvent inputEvent)
	{
		
		if (inputEvent is not InputEventMouse mouseEvent)
			return;
		
		MousePosition = mouseEvent.Position;
		
	}
	
	private void CheckMouseMotion(InputEvent inputEvent)
	{
		if (inputEvent is not InputEventMouseMotion mouseEvent)
			return;
		
		MouseDelta = mouseEvent.Relative;
	}
	
}
