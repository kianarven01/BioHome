using Godot;

public partial class MainMenu : Node2D
{
	public override void _Input(InputEvent @event)
	{
		// Detect any mouse click or screen tap
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			GetTree().ChangeSceneToFile("res://living_room.tscn"); // Change to your scene path
		}
	}
}
