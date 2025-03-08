using Godot;
using System;

public partial class TV : TextureRect
{
	private CanvasLayer tvUI; // Use CanvasLayer instead of CanvasItem

	public override void _Ready()
	{
		tvUI = GetNode<CanvasLayer>("TV_UI"); // Get TV_UI as CanvasLayer
		tvUI.Visible = false; // Hide UI initially
	}

	public override void _Input(InputEvent @event)
	{
		// Prevent TV from being clickable when UI is open
		if (tvUI.Visible) return;

		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			Vector2 clickPosition = mouseEvent.Position;

			// Check if the click is inside the TV's area
			if (GetGlobalRect().HasPoint(clickPosition))
			{
				GD.Print("TV tapped!");
				tvUI.Visible = true; // Show UI
			}
		}
	}

}
