using Godot;
using System;
using System.Collections.Generic;

public partial class Codon : Node
{
    private Button backButton;
    private TextureButton startButton;
    private List<TextureRect> items; // List to store all items
    private int currentItemIndex = 0; 
    private Button button1;
    private Button button2;
    private Button button3;
    private bool isTimerRunning = false;
    private int timeLeft = 10; 
    private int score = 0; 
    private Label scoreTotalLabel; 
    private bool isQuizActive = true; 
    
    private AudioStreamPlayer2D correctSound;
    private bool waitForSound = false;



    private Dictionary<int, string> correctAnswers = new Dictionary<int, string>
    {
        { 0, "button_q2" }, // Correct answer for first_item
        { 1, "button_q3" }, // Correct answer for second_item
        { 2, "button_q1" }  // Correct answer for third_item
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
        correctSound.Finished += OnCorrectSoundFinished;



        // Initialize items list
        items = new List<TextureRect>
        {
            GetNode<TextureRect>("first_item"),
            GetNode<TextureRect>("second_item"),
            GetNode<TextureRect>("third_item"),
        };

        // Hide all items initially
        foreach (var item in items)
        {
            item.Visible = false;
        }

        // Reference the score_total label inside the score TextureRect
        var scoreTextureRect = GetNode<TextureRect>("score");
        scoreTotalLabel = scoreTextureRect.GetNode<Label>("score_total");
        UpdateScoreLabel();
    }

    private void OnBackButtonPressed()
    {
        GD.Print("Back button pressed");
        GetTree().ChangeSceneToFile("res://scenes/living_room.tscn");
    }

    private void OnStartButtonPressed()
    {
        GD.Print("Start button pressed");
        ShowItem(0); // Show the first item
        StartTimer(); // Start the timer
    }

    private void OnFirstItemButtonPressed(Button pressedButton)
    {
        GD.Print($"Button in item {currentItemIndex} pressed: {pressedButton.Name}");

            // Check if the pressed button is the correct answer
        if (correctAnswers.ContainsKey(currentItemIndex) && pressedButton.Name == correctAnswers[currentItemIndex])
        {
            GD.Print("Correct answer!");
            score++;
            UpdateScoreLabel();
            correctSound.Play();
            waitForSound = true;
            return; // Wait until sound finishes before continuing
        }
        else
        {
            GD.Print("Wrong answer!");
            ShowNextItem(); // Immediately show next item on wrong answer
            if (currentItemIndex < items.Count)
                StartTimer();
        }

    }

    private void OnCorrectSoundFinished()
    {
        if (waitForSound)
        {
            waitForSound = false;
            ShowNextItem();
            if (currentItemIndex < items.Count)
                StartTimer();
        }
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
            items[index].Visible = true;
            items[index].Position = new Vector2(0, 0); // Ensure the item is positioned at (0, 0)
            currentItemIndex = index;
            GD.Print($"Showing item {index}");

            // Connect button signals for the current item
            ConnectButtonsForCurrentItem();
        }
    }

    private void ConnectButtonsForCurrentItem()
    {
        // Get the current item
        var currentItem = items[currentItemIndex];

        // Get all buttons in the current item dynamically
        foreach (var button in currentItem.GetChildren())
        {
            if (button is Button btn)
            {
                // Check if the signal is already connected before disconnecting
                if (btn.IsConnected("pressed", new Callable(this, nameof(OnFirstItemButtonPressed))))
                {
                    btn.Pressed -= () => OnFirstItemButtonPressed(btn);
                }

                // Connect the button to the signal
                btn.Pressed += () => OnFirstItemButtonPressed(btn);
            }
        }
    }

    private void ShowBackground()
    {
        // Hide all items
        foreach (var item in items)
        {
            item.Visible = false;
        }

        // Show the Background TextureRect
        var background = GetNode<TextureRect>("Background");
        background.Visible = true;
        background.Position = new Vector2(0, 0); // Ensure it's positioned correctly
        currentItemIndex = -1; // Reset the index or set it to a special value
        GD.Print("Background is now visible.");
    }

    private void ShowNextItem()
    {
        int nextIndex = currentItemIndex + 1;
        if (nextIndex < items.Count)
        {
            ShowItem(nextIndex);
        }
        else
        {
            GD.Print("No more items to show. Displaying final score.");
            ShowScoreScreen(); // Transition to the score screen

            // Stop the timer explicitly
            var timer = GetCurrentTimer();
            if (timer != null && !timer.IsStopped())
            {
                timer.Stop();
                GD.Print("Timer stopped after finishing all items.");
            }

            isQuizActive = false; // Mark the quiz as inactive
        }
    }

    private void ShowScoreScreen()
    {
        // Stop the timer if it's running
        // Try disconnecting from all timers in case we're at the end
        foreach (var item in items)
        {
            var timer = item.GetNode<Timer>("Timer");
            if (timer != null)
            {
                if (!timer.IsStopped())
                {
                    timer.Stop();
                    GD.Print($"Timer stopped for item.");
                }

                if (timer.IsConnected("timeout", new Callable(this, nameof(OnTimerTimeout))))
                {
                    timer.Timeout -= OnTimerTimeout;
                    GD.Print("Disconnected timeout signal from item.");
                }
            }
        }

        // Set the quiz state to inactive
        isQuizActive = false;

        // Hide all items
        foreach (var item in items)
        {
            item.Visible = false;
        }

        // Hide the Background TextureRect
        var background = GetNode<TextureRect>("Background");
        background.Visible = false;

        // Show the score TextureRect
        var scoreTextureRect = GetNode<TextureRect>("score");
        scoreTextureRect.Visible = true;
        scoreTextureRect.Position = new Vector2(0, 0); // Ensure it's positioned correctly
        GD.Print("Score screen is now visible.");
    }

    private void UpdateScoreLabel()
    {
        scoreTotalLabel.Text = $"{score}"; // Update the score label
        GD.Print($"Score updated: {score}");
    }

    private Timer GetCurrentTimer()
    {
        // Get the Timer node from the current item
        return items[currentItemIndex].GetNode<Timer>("Timer");
    }

    private Label GetCurrentTimerLabel()
    {
        // Get the timer_label node from the current item
        return items[currentItemIndex].GetNode<Label>("timer_label");
    }

    private void StartTimer()
    {
        if (!isQuizActive || isTimerRunning) // Prevent starting the timer if it's already running
        {
            GD.Print("Quiz is no longer active or timer is already running.");
            return;
        }

        var timer = GetCurrentTimer();
        if (timer == null)
        {
            GD.Print("No timer found for current item.");
            return;
        }

        // Always stop the timer before restarting
        if (!timer.IsStopped())
        {
            timer.Stop();
            GD.Print("Stopped previous timer before starting a new one.");
        }

        // Disconnect previous connection if any
        if (timer.IsConnected("timeout", new Callable(this, nameof(OnTimerTimeout))))
        {
            timer.Timeout -= OnTimerTimeout;
            GD.Print("Disconnected previous timeout signal.");
        }

        // Reconnect
        timer.Timeout += OnTimerTimeout;

        // Reset the timeLeft to 10 seconds every time the timer starts
        timeLeft = 10;
        UpdateTimerLabel(); // Update the label immediately to show 10 seconds

        timer.WaitTime = 1.0f;  // Each timeout will happen every second
        timer.OneShot = false;  // Repeat the timer (not one-shot)
        timer.Start(); // Start the timer
        isTimerRunning = true; // Mark the timer as running
        GD.Print("Timer started for 10 seconds.");
    }


    private void OnTimerTimeout()
    {
        if (!isQuizActive)
        {
            GD.Print("Quiz is no longer active. Timer timeout ignored.");
            return;
        }

        timeLeft--;
        GD.Print($"Time left: {timeLeft}");

        if (currentItemIndex >= 0 && currentItemIndex < items.Count)
        {
            UpdateTimerLabel();
        }

        if (timeLeft <= 0)
        {
            GD.Print("Time's up!");
            var timer = GetCurrentTimer();
            timer.Stop();
            isTimerRunning = false;

            // Move to the next item
            if (currentItemIndex >= 0 && currentItemIndex < items.Count)
            {
                ShowNextItem();

                // ✅ Reset time before starting again
                timeLeft = 10;
                UpdateTimerLabel();

                StartTimer(); // Now it starts from 10
            }
            else
            {
                GD.Print("No more items. Timer will not restart.");
                isQuizActive = false;
            }
        }
    }


    private void UpdateTimerLabel()
    {
        // Ensure the currentItemIndex is valid
        if (currentItemIndex < 0 || currentItemIndex >= items.Count)
        {
            GD.Print("No valid item to update the timer label.");
            return; // Exit if there is no valid item
        }

        var timerLabel = GetCurrentTimerLabel(); // Get the timer_label for the current item
        timerLabel.Text = timeLeft.ToString(); // Update the label with the remaining time
        GD.Print($"Timer label updated: {timerLabel.Text}");
    }

    
}