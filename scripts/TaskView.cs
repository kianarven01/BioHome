using Godot;
using System;

public partial class TaskView : TextureRect
{
	private TextureRect tasks;
	private Button closeButton;
	private TextureRect tv;
	private Button ExitButton;

	public override void _Ready()
	{
		tasks = GetNode<TextureRect>("../TaskView");
		tv = GetNode<TextureRect>("../TV");
		ExitButton = GetNode<Button>("../exitButton");

		tasks.Visible = false;
		closeButton = GetNode<Button>("../TaskView/close_button");
		closeButton.Pressed += CloseUI;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (tasks.Visible) return;

		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			Vector2 clickPosition = mouseEvent.Position;
			
			if (GetGlobalRect().HasPoint(clickPosition))
			{
				GD.Print("Tasks tapped!");
				tasks.Visible = true;
				tasks.Position = new Vector2(313,-332);
				
				tv.SetProcessInput(false);
				ExitButton.MouseFilter = Control.MouseFilterEnum.Ignore;
			}
		}
	}
	
	private void CloseUI()
	{
		tv.SetProcessInput(true);
		tasks.Visible = false;
		ExitButton.MouseFilter = Control.MouseFilterEnum.Stop;
	}
}
