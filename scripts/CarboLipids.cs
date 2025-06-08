using Godot;
using System;

public partial class CarboLipids : Node
{
	private Button backButton;
	private Button carboButton;
	private Button lipidsButton;

	private TextureRect[] quizItems;
	private int currentItemIndex = 0;

	private TextureRect currentItem;
	private TextureButton a1, a2, a3;
	private TextureRect q2;
	private Label timerLabel;

	private Timer globalTimer;
	private int timeLeft = 10;
	private bool isTimerActive = false;

	private bool isCarboQuiz = true;

	// Score tracking
	private int score = 0;
	private TextureRect scoreScreen;
	private Label scoreLabel;
	private string[] correctAnswers;


	// Delegates for signal cleanup
	private Action a1Handler;
	private Action a2Handler;
	private Action a3Handler;
	private TextureButton previousA1;
	private TextureButton previousA2;
	private TextureButton previousA3;

	private AudioStreamPlayer2D correctSound;
	private AudioStreamPlayer2D wrongSound;
	private AudioStreamPlayer2D themeMusic;
	private AudioStreamPlayer2D startMusic;
	private AudioStreamPlayer2D scoreMusic;

	public override void _Ready()
	{
		backButton = GetNode<Button>("backButton");
		backButton.Pressed += OnBackButtonPressed;

		carboButton = GetNode<Button>("Background/Carbohydrate");
		carboButton.Pressed += OnCarboButtonPressed;

		lipidsButton = GetNode<Button>("Background/Lipids");
		lipidsButton.Pressed += OnLipidsButtonPressed;

		correctSound = GetNode<AudioStreamPlayer2D>("correct");
		wrongSound = GetNode<AudioStreamPlayer2D>("wrong");
		themeMusic = GetNode<AudioStreamPlayer2D>("kahoot");
		startMusic = GetNode<AudioStreamPlayer2D>("start_music");
		scoreMusic = GetNode<AudioStreamPlayer2D>("score_music");
		startMusic.Play();

		globalTimer = GetNode<Timer>("GlobalTimer");
		globalTimer.Timeout += OnTimerTimeout;

		scoreScreen = GetNode<TextureRect>("score");
		scoreLabel = scoreScreen.GetNode<Label>("score_total");
		scoreScreen.Visible = false;

		// Initialize Carbo quiz by default
		InitializeCarboQuiz();
	}

	private void InitializeCarboQuiz()
	{
		isCarboQuiz = true;
		quizItems = new TextureRect[]
		{
			GetNode<TextureRect>("first_item"),
			GetNode<TextureRect>("second_item"),
			GetNode<TextureRect>("third_item"),
			GetNode<TextureRect>("fourth_item"),
		};

		correctAnswers = new string[]
		{
			"A2",
			"A2",
			"A1",
			"A1",
		};

		foreach (var item in quizItems)
			item.Visible = false;
	}

	private void InitializeLipidsQuiz()
	{
		isCarboQuiz = false;
		quizItems = new TextureRect[]
		{
			GetNode<TextureRect>("fifth_item"),
			GetNode<TextureRect>("sixth_item"),
			GetNode<TextureRect>("seventh_item"),
			GetNode<TextureRect>("eighth_item"),
		};

		correctAnswers = new string[]
		{
			"A3",
			"A2",
			"A1",
			"A3",
		};

		foreach (var item in quizItems)
			item.Visible = false;
	}

	private void OnBackButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/living_room.tscn");
	}

	private void OnCarboButtonPressed()
	{
		score = 0;
		currentItemIndex = 0;
		scoreScreen.Visible = false;
		startMusic.Stop();

		InitializeCarboQuiz();
		ShowItem(currentItemIndex);
	}

	private void OnLipidsButtonPressed()
	{
		score = 0;
		currentItemIndex = 0;
		scoreScreen.Visible = false;
		startMusic.Stop();
		
		InitializeLipidsQuiz();
		ShowItem(currentItemIndex);
	}

	private void ShowItem(int index)
	{
		if (index >= quizItems.Length)
		{
			ShowScoreScreen();
			return;
		}

		foreach (var item in quizItems)
			item.Visible = false;

		currentItem = quizItems[index];
		currentItem.Visible = true;
		currentItem.Position = new Vector2(0, 0);

		timerLabel = currentItem.GetNode<Label>("timer_label");
		q2 = currentItem.GetNode<TextureRect>("Q2");
		a1 = currentItem.GetNodeOrNull<TextureButton>("A1");
		a2 = currentItem.GetNodeOrNull<TextureButton>("A2");
		a3 = currentItem.GetNodeOrNull<TextureButton>("A3");

		q2.Texture = GD.Load<Texture2D>("res://sprites/CarboLipids/question.png");
		q2.Visible = true;

		// Disconnect previous handlers if they exist
		if (previousA1 != null && a1Handler != null) previousA1.Pressed -= a1Handler;
		if (previousA2 != null && a2Handler != null) previousA2.Pressed -= a2Handler;
		if (previousA3 != null && a3Handler != null) previousA3.Pressed -= a3Handler;

		// Create and connect new handlers
		a1Handler = () => OnAnswerPressed(a1);
		a2Handler = () => OnAnswerPressed(a2);
		a3Handler = () => OnAnswerPressed(a3);

		if (a1 != null) a1.Pressed += a1Handler;
		if (a2 != null) a2.Pressed += a2Handler;
		if (a3 != null) a3.Pressed += a3Handler;

		previousA1 = a1;
		previousA2 = a2;
		previousA3 = a3;

		timeLeft = 10;
		UpdateTimerLabel();
		globalTimer.WaitTime = 1.0f;
		globalTimer.OneShot = false;
		globalTimer.Start();
		isTimerActive = true;

		// Restart theme music on each new item
		themeMusic.Stop();
		themeMusic.Play();

	}

	private void OnAnswerPressed(TextureButton selectedButton)
	{
		if (!isTimerActive || selectedButton == null) return;

		isTimerActive = false;
		globalTimer.Stop();
		themeMusic.Stop();

		var originalTexture = selectedButton.TextureNormal;
		var image = originalTexture.GetImage();
		image.Resize(320, 300, Image.Interpolation.Bilinear);
		var scaledTexture = ImageTexture.CreateFromImage(image);

		q2.Texture = scaledTexture;
		q2.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		q2.Visible = true;

		GD.Print($"Answer selected from {selectedButton.Name}");

		if (selectedButton.Name == correctAnswers[currentItemIndex])
		{
			score++;
			GD.Print("Correct! Score: " + score);
			correctSound.Play(); 
		}
		else
		{
			GD.Print("Wrong answer!");
			wrongSound.Play();
		}


		GetTree().CreateTimer(1.5f).Timeout += () =>
		{
			currentItemIndex++;
			ShowItem(currentItemIndex);
		};
	}

	private void OnTimerTimeout()
	{
		timeLeft--;

		if (timeLeft <= 0)
		{
			globalTimer.Stop();
			isTimerActive = false;

			q2.Texture = GD.Load<Texture2D>("res://sprites/CarboLipids/question.png");
			q2.Visible = true;

			GD.Print("Time's up! Moving to next item...");

			wrongSound.Play();

			GetTree().CreateTimer(1.5f).Timeout += () =>
			{
				currentItemIndex++;
				ShowItem(currentItemIndex);
			};
		}

		else
		{
			UpdateTimerLabel();
		}
	}

	private void UpdateTimerLabel()
	{
		timerLabel.Text = $"{timeLeft:D2}";
	}

	private void ShowScoreScreen()
	{
		foreach (var item in quizItems)
			item.Visible = false;

		scoreScreen.Visible = true;
		scoreScreen.Position = new Vector2(0, 0);
		scoreLabel.Text = $"{score}";

		scoreMusic.Play();

		GD.Print("Score screen should now be visible");
	}

}
