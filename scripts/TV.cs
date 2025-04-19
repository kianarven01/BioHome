using Godot;
using System;

public partial class TV : TextureRect
{
	[Signal]
	public delegate void TVUIVisibilityChangedEventHandler(bool isVisible);

	private CanvasLayer tvUI;
	private TextureRect UIbg;
	private TextureRect tasks;
	private Button ExitButton;

	public override void _Ready()
	{
		tvUI = GetNode<CanvasLayer>("TV_UI");
		tvUI.Visible = false;

		if (tvUI is TV_UI uiScript)
		{
			uiScript.Connect("RequestCloseTVUI", new Callable(this, nameof(CloseTVUI)));
		}

		UIbg = GetNode<TextureRect>("TV_UI/UIbg");
		UIbg.Position = new Vector2(184, 72);
		tasks = GetNode<TextureRect>("../Tasks");
		ExitButton = GetNode<Button>("../exitButton");
	}

	public override void _Input(InputEvent @event)
	{
		if (tvUI.Visible) return;

		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			Vector2 clickPosition = mouseEvent.Position;

			if (GetGlobalRect().HasPoint(clickPosition))
			{
				GD.Print("TV tapped!");
				tvUI.Visible = true;

				EmitSignal("TVUIVisibilityChanged", true);

				tasks.SetProcessInput(false);
				ExitButton.MouseFilter = Control.MouseFilterEnum.Ignore;
			}
		}
	}

	public void CloseTVUI()
	{
		if (tvUI.Visible)
		{
			tvUI.Visible = false;
			EmitSignal("TVUIVisibilityChanged", false); // ✅ Now only this emits the signal
			tasks.SetProcessInput(true);
			ExitButton.MouseFilter = Control.MouseFilterEnum.Stop;
		}
	}

}
