using Godot;
using System;

public partial class TV : TextureRect
{
	private CanvasLayer tvUI; // Use CanvasLayer instead of CanvasItem
	private TextureRect UIbg;
	private TextureRect tasks;
	private Button ExitButton;

	public override void _Ready()
	{
		tvUI = GetNode<CanvasLayer>("TV_UI"); // Get TV_UI as CanvasLayer
		tvUI.Visible = false; // Hide UI initially
		UIbg = GetNode<TextureRect>("TV_UI/UIbg");
		UIbg.Position = new Vector2(184, 72);
		tasks = GetNode<TextureRect>("../Tasks");
		ExitButton = GetNode<Button>("../exitButton");
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
				tasks.SetProcessInput(false);
				ExitButton.MouseFilter = Control.MouseFilterEnum.Ignore;
			}
		}
	}

}
