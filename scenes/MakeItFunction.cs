using Godot;
using System;
using System.Collections.Generic;

public partial class MakeItFunction : Node2D
{
	private Button backButton;
	private Dictionary<int, Button> buttons = new Dictionary<int, Button>();
	private Dictionary<int, TextureRect> items = new Dictionary<int, TextureRect>();
	private Dictionary<int, Button> exitButtons = new Dictionary<int, Button>();

	public override void _Ready()
	{
		backButton = GetNode<Button>("backButton");
		backButton.Pressed += ReturnToLivingRoom;

		for (int i = 1; i <= 10; i++)
		{
			buttons[i] = GetNode<Button>($"Background/B{i}");
			items[i] = GetNode<TextureRect>($"{GetItemName(i)}");

			// Hide all items initially
			items[i].Visible = false;

			// Connect button pressed event dynamically
			int buttonIndex = i; // Capture variable in loop
			buttons[i].Pressed += () => OnButtonPressed(buttonIndex);
		}
		
		for(int i = 1; i <= 10; i++)
		{
			exitButtons[i] = GetNode<Button>($"{GetItemName(i)}/Exit");
			int buttonIndex = i;
			exitButtons[i].Pressed += () => hideSelf(buttonIndex);
		}
	}

	private void ReturnToLivingRoom()
	{
		GetTree().ChangeSceneToFile("res://scenes/living_room.tscn");    
	}

	private void OnButtonPressed(int index)
	{
		GD.Print($"Pressed {index}");
		items[index].Visible = true;
		if (index <= 10)
		{
			items[index].Position = new Vector2(583, -1);
		}
	}

	private string GetItemName(int index)
	{
		string[] itemNames = {
			"First_Item", "Second_Item", "Third_Item",
			"Forth_Item", "Fifth_Item", "Sixth_Item",
			"Seventh_Item", "Eigth_Item", "Ninth_Item", "Tenth_Item"
		};
		return itemNames[index - 1]; // Adjust for zero-based index
	}
	
	private void hideSelf(int index)
	{
		items[index].Visible = false;
	}
}
