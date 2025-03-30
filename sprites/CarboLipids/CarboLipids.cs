using Godot;
using System;

public partial class CarboLipids : Node
{
    private Button backButton;

    // For game select buttons
    private Button carboButton;
    private Button lipidButton;

    // For questionnaires
    private TextureRect firstItem;
    private TextureRect q2;

    // For answer buttons
    private TextureButton a1;
    private TextureButton a2;

    // For timer
    private Timer timer;
    private Label timerLabel;
    private int timeLeft = 15;

    private bool isTimerActive = false; // To track if the timer is running

    public override void _Ready()
    {
        // Back button to living_room.tscn
        backButton = GetNode<Button>("backButton");
        backButton.Pressed += OnBackButtonPressed;

        // Carbo button to first_item.tscn
        carboButton = GetNode<Button>("Background/Carbohydrate");
        firstItem = GetNode<TextureRect>("first_item");
        carboButton.Pressed += OnCarboButtonPressed;
        firstItem.Visible = false; // Hide the first item initially

        // Timer and label setup
        timer = GetNode<Timer>("first_item/Timer");
        timerLabel = GetNode<Label>("first_item/timer_label");
        timer.Timeout += OnTimerTimeout;

        // Answer buttons and Q2 setup
        a1 = GetNode<TextureButton>("first_item/A1");
        a2 = GetNode<TextureButton>("first_item/A2");
        q2 = GetNode<TextureRect>("first_item/Q2");

        // Set the initial texture for Q2
        q2.Texture = GD.Load<Texture2D>("res://sprites/CarboLipids/question.png");
        q2.Visible = true; // Ensure Q2 is visible initially

        // Connect answer buttons to the same method
        a1.Pressed += () => OnAnswerPressed(a1);
        a2.Pressed += () => OnAnswerPressed(a2);
    }

    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/living_room.tscn");
    }

    private void OnCarboButtonPressed()
    {
        firstItem.Visible = true; // Show the first item
        firstItem.Position = new Vector2(0, 0); // Set position as needed

        // Reset Q2 with the default question mark texture
        q2.Texture = GD.Load<Texture2D>("res://sprites/CarboLipids/question.png");
        q2.Visible = true; // Ensure Q2 is visible

        // Start the timer
        timeLeft = 10; // Reset the countdown
        UpdateTimerLabel(); // Update the label immediately
        timer.WaitTime = 1.0f; // Set the timer to tick every second
        timer.OneShot = false; // Keep the timer running
        timer.Start();
        isTimerActive = true; // Enable input while the timer is active
    }

    private void OnAnswerPressed(TextureButton selectedButton)
    {
        if (!isTimerActive) return; // Ignore input if the timer is not active

        // Set Q2's texture to match the selected button's texture
        var originalTexture = selectedButton.TextureNormal;

        // Create a new ImageTexture to scale the texture
        var image = originalTexture.GetImage();
        image.Resize(320, 300, Image.Interpolation.Bilinear); // Resize to fit Q2
        var scaledTexture = ImageTexture.CreateFromImage(image);

        // Assign the scaled texture to Q2
        q2.Texture = scaledTexture;

        // Ensure Q2's texture fits its defined size
        q2.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

        // Make Q2 visible
        q2.Visible = true;

        GD.Print($"Q2 filled with scaled texture from {selectedButton.Name}");
    }
    private void OnTimerTimeout()
    {
        timeLeft--;

        if (timeLeft <= 0)
        {
            timer.Stop(); // Stop the timer
            isTimerActive = false; // Disable input
            firstItem.Visible = false; // Hide the first item

            // Show Q2 with the default question mark texture
            q2.Visible = true;

            GD.Print("Time's up! Q2 set to question mark.");
        }

        UpdateTimerLabel();
    }

    private void UpdateTimerLabel()
    {
        int seconds = timeLeft % 60;
        timerLabel.Text = $"{seconds:D2}"; // Format as MM:SS
    }
}