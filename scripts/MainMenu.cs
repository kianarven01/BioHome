using Godot;
using System.Collections.Generic;

public partial class MainMenu : Node2D
{
	private AudioStreamPlayer2D musicPlayer;
	private TextureRect ratingsPnl;
	private Button rateBtn;
	private Button closeRatings;
	private ButtonGroup react;
	private ButtonGroup ease;
	private ButtonGroup effectivity;
	private ButtonGroup learn;
	private ButtonGroup satisfaction;
	private Control reactGroup;
	private Control easeGroup;
	private Control effectivityGroup;
	private Control learnGroup;
	private Control satisfactionGroup;
	private List<string> ratings;

	public override void _Ready()
	{
		// Get the AudioStreamPlayer2D node
		musicPlayer = GetNode<AudioStreamPlayer2D>("kahoot_jungle");
		ratingsPnl = GetNode<TextureRect>("LandingScene/RatingPanel");
		rateBtn = GetNode<Button>("LandingScene/rateBtn");
		closeRatings = GetNode<Button>("LandingScene/RatingPanel/close_button");
		ratings = new List<string>{"", "", "", "", ""};
		
		rateBtn.Pressed += showRatings;
		closeRatings.Pressed += closeRating;
		
		react =  GD.Load<ButtonGroup>("res://groups/react.tres");
		ease =  GD.Load<ButtonGroup>("res://groups/ease.tres");
		effectivity =  GD.Load<ButtonGroup>("res://groups/effectivity.tres");
		learn =  GD.Load<ButtonGroup>("res://groups/learn.tres");
		satisfaction =  GD.Load<ButtonGroup>("res://groups/satisfaction.tres");
		
		reactGroup = GetNode<Control>("LandingScene/RatingPanel/React");
		effectivityGroup = GetNode<Control>("LandingScene/RatingPanel/Effectivity");
		easeGroup = GetNode<Control>("LandingScene/RatingPanel/Ease");
		learnGroup = GetNode<Control>("LandingScene/RatingPanel/Learn");
		satisfactionGroup = GetNode<Control>("LandingScene/RatingPanel/Satisfaction");
		
		cbAction(react, reactGroup);
		cbAction(effectivity, effectivityGroup);
		cbAction(ease, easeGroup);
		cbAction(learn, learnGroup);
		cbAction(satisfaction, satisfactionGroup);

		// Play the music
		if (!musicPlayer.Playing)
		{
			musicPlayer.Play();
		}
	}
	
	private void showRatings()
	{
		ratingsPnl.Position = new Vector2(-146,-176);
		ratingsPnl.Visible = true;
	}
	
	private void closeRating()
	{
		ratingsPnl.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		// Handle input events if needed
	}
	
	private void cbAction(ButtonGroup buttonGroup, Control groupParent)
	{
		foreach (Node child in groupParent.GetChildren())
		{
			if (child is CheckButton cb)
			{
				// Force assign the correct ButtonGroup
				cb.ButtonGroup = buttonGroup;

				GD.Print($"Connecting: {cb.Name} to group {buttonGroup.ResourcePath}");
				cb.Toggled += (bool pressed) => OnButtonToggled(pressed, buttonGroup, groupParent);
			}
		}
	}

	private void OnButtonToggled(bool pressed, ButtonGroup buttonGroup, Control groupParent)
	{
		if (!pressed)
			return;

		foreach (Node child in groupParent.GetChildren())
		{
			if (child is CheckButton cb && cb.ButtonGroup == buttonGroup && cb.ButtonPressed)
			{
				GD.Print($"Selected: {cb.Name}");
				identifyGroup(cb);
			}
		}
	}
	
	private void identifyGroup(CheckButton cb){
		List<string> groupDir = new List<string>{"res://groups/react.tres", "res://groups/effectivity.tres", "res://groups/ease.tres", "res://groups/learn.tres", "res://groups/satisfaction.tres"};
		ratings[groupDir.IndexOf(cb.ButtonGroup.ResourcePath)] = cb.Name;
		string result = string.Join(", ", ratings);
		GD.Print(result);
	}
}
