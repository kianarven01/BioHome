using Godot;
using System;
using System.Collections.Generic;

public partial class Codon : Node
{
	private Button backButton;
	private TextureButton startButton;
	private List<TextureRect> items; // List to store all items
	private int currentItemIndex = 0; // Track the current item index
	private int timeLeft = 10; // Countdown duration in seconds
	private int score = 0; // Player's score
	private Label scoreTotalLabel; // Label to display the score
	private bool isQuizActive = true; // Tracks whether the quiz is active
	private Timer globalTimer; // Global timer for the quiz

	private AudioStreamPlayer2D correctSound;
	private AudioStreamPlayer2D wrongSound;
	private AudioStreamPlayer2D themeMusic;
	private AudioStreamPlayer2D startMusic;
	private AudioStreamPlayer2D scoreMusic;



	private Dictionary<int, string> correctAnswers = new Dictionary<int, string>
	{
		{ 0, "button_q2" }, // Correct answer for first_item
		{ 1, "button_q3" }, // Correct answer for second_item
		{ 2, "button_q1" },  // Correct answer for third_item
		{ 3, "button_q3" },  // Correct answer for fourth_item
		{ 4, "button_q3" } // Correct answer for fifth_item
	};

	public override void _Ready()
	{
		// Back button to living_room.tscn
		backButton = GetNode<Button>("backButton");
		backButton.Pressed += OnBackButtonPressed;

		// Start button to show the first item
		startButton = GetNode<TextureButton>("Background/startButton");
		startButton.Pressed += OnStartButtonPressed;

		correctSound = GetNode<AudioStreamPlayer2D>("correct");
		wrongSound = GetNode<AudioStreamPlayer2D>("wrong");
		themeMusic = GetNode<AudioStreamPlayer2D>("kahoot");
		startMusic = GetNode<AudioStreamPlayer2D>("start_music");
		scoreMusic = GetNode<AudioStreamPlayer2D>("score_music");
		startMusic.Play();
		

		// Initialize items list
		items = new List<TextureRect>
		{
			GetNode<TextureRect>("first_item"),
			GetNode<TextureRect>("second_item"),
			GetNode<TextureRect>("third_item"),
			GetNode<TextureRect>("fourth_item"),
			GetNode<TextureRect>("fifth_item")
		};

		// Hide all items initially
		foreach (var item in items)
		{
			item.Visible = false;
		}

		// Reference the score_total label inside the score TextureRect
		
		var scoreTextureRect = GetNode<TextureRect>("score");
		scoreTotalLabel = scoreTextureRect.GetNode<Label>("score_total");
		scoreTextureRect.Show(); // Redundant but explicit
		UpdateScoreLabel();

		// Initialize the global timer
		globalTimer = GetNode<Timer>("GlobalTimer");
		globalTimer.WaitTime = 1.0f; // Set the timer to trigger every second
		globalTimer.OneShot = false; // Ensure the timer repeats
		globalTimer.Timeout += OnGlobalTimerTimeout; // Connect the timeout signal
	}

	private void OnBackButtonPressed()
	{
		GD.Print("Back button pressed");
		GetTree().ChangeSceneToFile("res://scenes/living_room.tscn");
	}

	private void OnStartButtonPressed()
	{
		GD.Print("Start button pressed");

		if (startMusic.Playing)
		{
			startMusic.Stop();
		}


		// Reset the quiz state
		ResetQuiz();

		// Show the first item
		ShowItem(0);

		// Start the global timer
		StartGlobalTimer();
	}

	private void ShowItem(int index)
	{
		// Hide all items
		foreach (var item in items)
		{
			item.Visible = false;
		}

		// Show the specified item
		if (index >= 0 && index < items.Count)
		{
			var currentItem = items[index];
			currentItem.Visible = true;
			currentItem.Position = new Vector2(0, 0); // Ensure the item is positioned at (0, 0)
			currentItemIndex = index;

			if (themeMusic.Playing)
			{
				themeMusic.Stop(); // Stop if already playing to reset it
			}
			themeMusic.Play(); // Play from the beginning

			// Connect button signals for the current item
			ConnectButtonsForCurrentItem(currentItem);

			// Get the TimerLabel inside the current item
			try
			{
				var timerLabel = currentItem.GetNode<Label>("TimerLabel");
				UpdateTimerLabel(timerLabel); // Update the label inside the item
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error fetching TimerLabel for item {index}: {e.Message}");
				return;
			}
		}
	}

	private void ConnectButtonsForCurrentItem(TextureRect currentItem)
	{
		foreach (var child in currentItem.GetChildren())
		{
			if (child is Button button)
			{
				// First, disconnect if already connected
				if (button.IsConnected("pressed", Callable.From(() => OnItemButtonPressed(button))))
				{
					button.Disconnect("pressed", Callable.From(() => OnItemButtonPressed(button)));
				}

				// Now connect using a lambda that captures the button reference
				button.Connect("pressed", Callable.From(() => OnItemButtonPressed(button)));
			}
		}
	}


	private async void OnItemButtonPressed(Button pressedButton)
	{
		GD.Print($"Button pressed: {pressedButton.Name}");

		if (correctAnswers.ContainsKey(currentItemIndex) && pressedButton.Name == correctAnswers[currentItemIndex])
		{
			GD.Print("Correct answer!");
			correctSound.Play();
		}
		else
		{
			GD.Print("Wrong answer!");
			wrongSound.Play();
		}

		// Wait 0.5 seconds to let the sound play
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

		if (correctAnswers.ContainsKey(currentItemIndex) && pressedButton.Name == correctAnswers[currentItemIndex])
		{
			score++;
			UpdateScoreLabel();
		}

		ShowNextItem();
	}

	private void ShowNextItem()
	{
		int nextIndex = currentItemIndex + 1;

		if (themeMusic.Playing)
		{
			themeMusic.Stop();
		}

		if (nextIndex < items.Count)
		{
			ShowItem(nextIndex);
			ResetTimer(); // Reset the timer for the next item
		}
		else
		{
			GD.Print("No more items to show. Displaying final score.");
			ShowScoreScreen(); // Transition to the score screen
		}
	}

	private void ShowScoreScreen()
	{
		StopGlobalTimer();
		isQuizActive = false;

		// Stop other music if playing
		if (themeMusic.Playing)
			themeMusic.Stop();

		if (startMusic.Playing)
			startMusic.Stop();

		// Play score screen music
		scoreMusic.Play();

		// Hide all items
		foreach (var item in items)
		{
			item.Visible = false;
		}

		// Show the score screen
		var scoreTextureRect = GetNode<TextureRect>("score");
		scoreTextureRect.Visible = true;
		scoreTextureRect.Position = new Vector2(0, 0);
		scoreTextureRect.ZIndex = 100;

		// Make sure the back button is above it
		backButton.Visible = true;
		backButton.Disabled = false;
		backButton.ZIndex = 200;

		GD.Print($"Score screen is now visible. Position: {scoreTextureRect.Position}, Visible: {scoreTextureRect.Visible}");
	}



	private void StartGlobalTimer()
	{
		if (!isQuizActive)
		{
			GD.Print("Quiz is no longer active. Timer will not start.");
			return;
		}

		timeLeft = 10; // Reset the timer duration
		UpdateTimerLabel(null); // Update the label immediately (will be updated in ShowItem)
		globalTimer.Start(); // Start the global timer
		GD.Print("Global timer started for 10 seconds.");
	}

	private void StopGlobalTimer()
	{
		if (globalTimer.IsStopped())
		{
			return; // Timer is already stopped
		}

		globalTimer.Stop(); // Stop the timer
		GD.Print("Global timer stopped.");
	}

	private void ResetTimer()
	{
		timeLeft = 10; // Reset the timer duration
		UpdateTimerLabel(null); // Update the label immediately (will be updated in ShowItem)
	}

	private async void OnGlobalTimerTimeout()
	{
		if (!isQuizActive)
		{
			GD.Print("Quiz is no longer active. Timer timeout ignored.");
			return;
		}

		timeLeft--;
		GD.Print($"Time left: {timeLeft}");

		var currentItem = items[currentItemIndex];
		var timerLabel = currentItem.GetNode<Label>("TimerLabel");
		timerLabel.Text = timeLeft.ToString();

		if (timeLeft <= 0)
		{
			GD.Print("Time's up!");
			wrongSound.Play();

			// Optional: small delay to let the sound play before moving on
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

			ShowNextItem();
		}
	}


	private void UpdateScoreLabel()
	{
		scoreTotalLabel.Text = $"{score}"; // Update the score label
		GD.Print($"Score updated: {score}");
	}

	private void UpdateTimerLabel(Label timerLabel)
	{
		// If timerLabel is null, do nothing (for global timer updates)
		if (timerLabel != null)
		{
			timerLabel.Text = timeLeft.ToString(); // Update the label with the remaining time
			GD.Print($"Timer label updated: {timerLabel.Text}");
		}
		else
		{
			GD.Print("No TimerLabel provided to update.");
		}
	}

	private void ResetQuiz()
	{
		GD.Print("Resetting quiz...");

		// Reset variables
		currentItemIndex = 0;
		timeLeft = 10;
		score = 0;
		isQuizActive = true;

		// Reset score label
		UpdateScoreLabel();

		// Hide all items
		foreach (var item in items)
		{
			item.Visible = false;
		}

		// Hide the score screen
		var scoreTextureRect = GetNode<TextureRect>("score");
		scoreTextureRect.Visible = false;

		// Show the background
		var background = GetNode<TextureRect>("Background");
		background.Visible = true;

		GD.Print("Quiz reset complete.");
	}
}
