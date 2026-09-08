using Godot;
using System;

public partial class PlayerInput : Node
{
	
	public Vector2 MoveDirection { get; private set; }
	
	public Vector2 MouseDelta { get; private set; }
	public Vector2 MousePosition { get; private set; }


    public override void _Ready()
    {
        
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
    }


    public override void _Input(InputEvent inputEvent)
    {
        
		CheckMouseEvent(inputEvent);
		CheckMouseMotion(inputEvent);
		
    }


	public void StartInputs()
	{
		
		MoveDirection = Input.GetVector("move_left", "move_right", "move_down", "move_up");
		
	}
	
	public void EndInputs()
	{
		
		MouseDelta = Vector2.Zero;
		
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
