using Godot;
using System;
using System.Collections.Generic;

public partial class MakeItFunction : Node2D
{
	private Button backButton;
	private Dictionary<int, Button> buttons = new Dictionary<int, Button>();
	private Dictionary<int, TextureRect> items = new Dictionary<int, TextureRect>();
	private Dictionary<int, Button> exitButtons = new Dictionary<int, Button>();
	private Dictionary<ButtonGroup, List<Button>> buttonGroups = new();
	private Dictionary<Timer, Label> timers = new();
	private Dictionary<Timer, int> timeLeft = new();
	private HashSet<Timer> startedTimers = new();
	private Label scoreLbl;
	private int score = 0;
	private int completedCount = 0;
	private double totalStartTime = -1;
	private Label resultLabel;
	private TextureRect endGame;
	private Button doneButton;
	private HashSet<Timer> finishedTimers = new();
	private Button retryButton;

	public override void _Ready()
	{
		scoreLbl = GetNode<Label>("Background/scoreLabel");
		backButton = GetNode<Button>("Background/backButton");
		backButton.Pressed += ReturnToLivingRoom;
		doneButton = GetNode<Button>("EndGame/Button");
		doneButton.Pressed += ReturnToLivingRoom;
		endGame = GetNode<TextureRect>("EndGame");
		endGame.Visible = false;
		resultLabel = GetNode<Label>("EndGame/resultLabel"); // Add a Label node in your scene and hide it initially
		resultLabel.Visible = false;
		retryButton = GetNode<Button>("EndGame/Retry");
		retryButton.Pressed += RestartGame;

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
		
		for (int i = 1; i <= 10; i++)  // Assuming 10 groups
		{
			ButtonGroup group = new ButtonGroup();
			List<Button> buttons = new();
			string[] correctList = {
				"First_Item/Button2", "Second_Item/Button4", "Third_Item/Button2"
				, "Forth_Item/Button4", "Fifth_Item/Button3", "Sixth_Item/Button2"
				, "Seventh_Item/Button2", "Eigth_Item/Button3", "Ninth_Item/Button2"
				, "Tenth_Item/Button3"
				};
			List<Button> correctButtons = new();
			
			foreach (string path in correctList)
			{
				correctButtons.Add(GetNode<Button>(path));
			}

			for (int j = 1; j <= 4; j++)  // Assuming 4 buttons per group
			{
				string buttonPath = $"{GetItemName(i)}/Button{j}"; // Adjust node path as needed
				Button button = GetNode<Button>(buttonPath);
				button.ButtonGroup = group;
				button.Pressed += () => OnButtonPressed2(button, group, correctButtons);
				buttons.Add(button);
			}

			buttonGroups[group] = buttons;
		}
		
		for (int i = 1; i <= 10; i++) // Loop through 10 timers
		{
			// Get parent dynamically (adjust node names as needed)
			Node parent = GetNode<Node>($"{GetItemName(i)}"); // Example: Parent_1, Parent_2, etc.

			// Find the Timer and Label inside the parent
			Timer timer = parent.GetNode<Timer>("Timer");
			Label timerLabel = parent.GetNode<Label>("Label");

			// Store references but don't start the timer yet
			timers[timer] = timerLabel;
			timeLeft[timer] = 10; // 10-second countdown
		}
	}
	
	public override void _Process(double delta)
	{
		foreach (var pair in timers)
		{
			Timer timer = pair.Key;
			Label label = pair.Value;
			Control parent = label.GetParent<Control>(); // Ensure it's a Control node


			if (parent.Visible && !startedTimers.Contains(timer)) // Start only once
			{
				StartTimer(timer, parent);
				startedTimers.Add(timer); // Mark as started
			}
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
			items[index].Position = new Vector2(0, 0);
		}
		
		if (totalStartTime < 0)
		{
			totalStartTime = Time.GetTicksMsec() / 1000.0; // Start in seconds
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
	
	private string GetButtonName(int index)
	{
		string[] buttonNames = {
			"Button", "Button2", "Button3", "Button4"
		};
		return buttonNames[index - 1]; // Adjust for zero-based index
	}
	
	private void hideSelf(int index)
	{
		items[index].Visible = false;
	}
	
	private async void OnButtonPressed2(Button pressedButton, ButtonGroup group, List<Button> allButtons)
	{
		foreach (Button button in buttonGroups[group])
		{
			button.Disabled = true;

			if (!allButtons.Contains(button))
			{
				StyleBox normalStyle = button.GetThemeStylebox("normal");
				button.AddThemeStyleboxOverride("disabled", normalStyle);
			}
		}
		
		if (allButtons.Contains(pressedButton))
		{
			score += 1;
			scoreLbl.Text = $"Score: {score}";
		}
		
		if (pressedButton.GetParent() is Control parent)
		{
			foreach (var timer in timers.Keys)
			{
				if (timer.GetParent() == parent)
				{
					StopTimer(timer);
					break;
				}
			}
		}
		
		 await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		
		// Hide the parent of the pressed button
		if (pressedButton.GetParent() is Control _parent)
		{
			_parent.Visible = false;
		}
		
		completedCount += 1;

		if (completedCount == 10)
		{
			ShowFinalResults();
		}
	}
	
	 private void StartTimer(Timer timer, Control parent)
	{
		timer.WaitTime = 1.0f; // 1-second interval
		timer.OneShot = false; // Keep running
		timer.Timeout += () => OnTimerTimeout(timer, parent);
		timer.Start();
		UpdateLabel(timer);
	}

	private async void OnTimerTimeout(Timer timer, Control parent)
	{
		if (!timeLeft.ContainsKey(timer)) return;

		timeLeft[timer]--;

		if (timeLeft[timer] <= -1)
		{
			 foreach (Node child in parent.GetChildren())
			{
				if (child is Button button && button.Name != "Exit")
				{
					button.Disabled = true;
				}
			}
			await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
			parent.Visible = false;
			timer.Stop();
			
			if (!finishedTimers.Contains(timer))
			{
				completedCount += 1;
				finishedTimers.Add(timer);

				if (completedCount == 10)
				{
					ShowFinalResults();
				}
			}
		}
		UpdateLabel(timer);
	}

	private void UpdateLabel(Timer timer)
	{
		if (timers.ContainsKey(timer) && timers[timer] != null)
		{
			int minutes = timeLeft[timer] / 60;
			int seconds = timeLeft[timer] % 60;
			timers[timer].Text = $"{minutes:D2}:{seconds:D2}"; // Format MM:SS
		}
	}
	
	private void StopTimer(Timer timer)
	{
		if (timers.ContainsKey(timer))
		{
			timer.Stop();
			if (timeLeft.ContainsKey(timer))
			{
				timeLeft[timer] = 0;
			}
			UpdateLabel(timer); // Optional: update label to show 00:00 or final time
		}
	}
	
	private void ShowFinalResults()
	{
		endGame.Visible = true;
		double endTime = Time.GetTicksMsec() / 1000.0;
		double totalTime = endTime - totalStartTime;
	
		string message = $"✅ Final Score: {score}/10\n⏱️ Total Time: {totalTime:F2} seconds";

		if (resultLabel != null)
		{
			resultLabel.Text = message;
			resultLabel.Visible = true;
		}
		
		foreach (var btn in buttons.Values)
		{
			btn.Disabled = true;
		}

		// ❌ Disable back button
		backButton.Disabled = true;
	}
	
	private void RestartGame()
	{
		// Reset score and UI
		score = 0;
		scoreLbl.Text = "Score: 0";
		completedCount = 0;
		totalStartTime = -1;

		// Reset finished and started timers
		startedTimers.Clear();
		finishedTimers.Clear();

		// Reset buttons
		foreach (var btn in buttons.Values)
		{
			btn.Disabled = false;
		}

		backButton.Disabled = false;

		// Hide result panel
		endGame.Visible = false;
		resultLabel.Visible = false;

		// Reset items and exit buttons
		for (int i = 1; i <= 10; i++)
		{
			items[i].Visible = false;
		}

		// Reset button groups
		foreach (var group in buttonGroups.Keys)
		{
			foreach (Button btn in buttonGroups[group])
			{
				btn.Disabled = false;
				btn.ButtonPressed = false;
				btn.RemoveThemeStyleboxOverride("disabled");
			}
		}

		// Reset timers and labels
		foreach (var timer in timers.Keys)
		{
			timeLeft[timer] = 10;
			UpdateLabel(timer);
			timer.Stop();
		}

		// Reset group buttons selection (optional)
		foreach (var group in buttonGroups.Keys)
		{
			foreach (Button btn in buttonGroups[group])
			{
				btn.ButtonPressed = false;
			}
		}
	}

}
