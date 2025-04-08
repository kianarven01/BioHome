using Godot;
using System;
using System.Collections.Generic;

public partial class Codon : Node
{
    private Button backButton;
    private TextureButton startButton;
    private List<TextureRect> items; // List to store all items
    private int currentItemIndex = 0; // Track the current item index
    private Button button1;
    private Button button2;
    private Button button3;
    private int timeLeft = 10; // Countdown duration in seconds

    public override void _Ready()
    {
        // Back button to living_room.tscn
        backButton = GetNode<Button>("backButton");
        backButton.Pressed += OnBackButtonPressed;

        // Start button to show the first item
        startButton = GetNode<TextureButton>("Background/startButton");
        startButton.Pressed += OnStartButtonPressed;

        // Initialize items list
        items = new List<TextureRect>
        {
            GetNode<TextureRect>("first_item"),
            GetNode<TextureRect>("second_item"),
        };

        // Hide all items initially
        foreach (var item in items)
        {
            item.Visible = false;
        }

        // Access buttons in first_item
        button1 = GetNode<Button>("first_item/button_q1");
        button2 = GetNode<Button>("first_item/button_q2");
        button3 = GetNode<Button>("first_item/button_q3");

        // Connect button signals
        button1.Pressed += OnFirstItemButtonPressed;
        button2.Pressed += OnFirstItemButtonPressed;
        button3.Pressed += OnFirstItemButtonPressed;
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

    private void OnFirstItemButtonPressed()
    {
        GD.Print($"Button in item {currentItemIndex} pressed");
        ShowNextItem(); // Move to the next item
        StartTimer(); // Restart the timer for the next item
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
        }
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
            GD.Print("No more items to show.");
        }
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
        timeLeft = 10; // Reset the timer duration
        UpdateTimerLabel(); // Update the label immediately
        var timer = GetCurrentTimer(); // Get the Timer for the current item
        timer.WaitTime = 1.0f; // Set the timer to trigger every second
        timer.OneShot = false; // Ensure the timer repeats
        timer.Timeout += OnTimerTimeout; // Connect the timeout signal
        timer.Start(); // Start the timer
        GD.Print("Timer started for 10 seconds.");
    }

    private void OnTimerTimeout()
    {
        timeLeft--;
        GD.Print($"Time left: {timeLeft}");
        UpdateTimerLabel();

        if (timeLeft <= 0)
        {
            GD.Print("Time's up!");
            var timer = GetCurrentTimer(); // Get the Timer for the current item
            timer.Stop(); // Stop the timer
            ShowNextItem(); // Automatically move to the next item
            StartTimer(); // Restart the timer for the next item
        }
    }

    private void UpdateTimerLabel()
    {
        var timerLabel = GetCurrentTimerLabel(); // Get the timer_label for the current item
        timerLabel.Text = timeLeft.ToString(); // Update the label with the remaining time
        GD.Print($"Timer label updated: {timerLabel.Text}");
    }
}