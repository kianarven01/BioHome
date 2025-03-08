using Godot;
using System;

public partial class StartButton : Button
{
	public override void _Ready()
	{
		Pressed += OnButtonPressed;
	}

	private void OnButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/living_room.tscn"); // Path to the next scene
	}
}
