using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LivingRoom : Node2D
{
	/*private AnimatedSprite2D animatedSprite;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite.Play("default"); // Play animation on scene load
	}*/
	
	private TextureRect exitSign; 
	private Button exitButton;
	private Button noButton;
	private Button yesButton;
	private TextureRect tv;
	private TextureRect tasks;
	private Button backButton;
	private Button nextPage;
	private Button prevPage;
	private Button closeBook; 
	private AudioStreamPlayer2D musicPlayer;
	private TextureRect book;
	private Button lipidsBook;
	private Button carbsBook;
	private Button naBook;
	private Button proteinBook;
	private Button finalTaskButton;
	private TextureRect Tutorial1;
	private TextureRect Tutorial2;
	private TextureRect Tutorial3;
	private Button t1;
	private Button t2;
	private Button t3;
	private AudioStreamPlayer2D tutorialPlayer;
	private AudioStreamPlayer2D flipbookPlayer;
	private TextureRect taskviewer;
	private TextureButton task1;
	private TextureButton task2;
	private TextureButton task3;
	private TextureButton task4;
	private TextureButton taskfinal;
	private Button viewerExit;
	private Button playBtn;
	private Button qrButton;
	private string currentTask;
	private VideoStreamPlayer lp;
	private Button proceed;
	private Button skip;
	
	[Export] public TextureRect LeftPage;
	[Export] public TextureRect RightPage;
	private bool _isFlipping = false;

	private List<Texture2D> _pages = new();
	private int _currentPageIndex = 0;
	
	public override void _Ready()
	{
		var global = GetNode<GlobalState>("/root/GlobalState");
		
		musicPlayer = GetNode<AudioStreamPlayer2D>("kahoot_lobby");
		tutorialPlayer = GetNode<AudioStreamPlayer2D>("tutorial");
		flipbookPlayer = GetNode<AudioStreamPlayer2D>("flipbook");
		Tutorial1 = GetNode<TextureRect>("Tutorial1");
		lp = GetNode<VideoStreamPlayer>("LessonPlayer");
		proceed = GetNode<Button>("LessonPlayer/Button");
		proceed.Pressed += hideVideo;
		if (!global.TutorialShown)
		{
			Tutorial1.Visible = true;
			tutorialPlayer.Stream = GD.Load<AudioStream>("res://sounds/tutorialSound/step_1.mp3");
			tutorialPlayer.Play();
			global.TutorialShown = true;
		}else{
			musicPlayer.Play();
		}
	
		exitSign = GetNode<TextureRect>("Exit"); 
		exitSign.Visible = false; // Hide UI initially
		
		exitButton = GetNode<Button>("exitButton");
		exitButton.Pressed += OnExitButtonPressed;
		
		noButton = GetNode<Button>("Exit/noButton");
		noButton.Pressed += OnNoButtonPressed;
		
		yesButton = GetNode<Button>("Exit/yesButton");
		yesButton.Pressed += OnYesButtonPressed;
		
		// Book Navigation
		nextPage = GetNode<Button>("Book/NextButton");
		nextPage.Pressed += FlipForward;
		
		prevPage = GetNode<Button>("Book/PrevButton");
		prevPage.Pressed += FlipBackward;
		
		closeBook = GetNode<Button>("Book/close_button");
		closeBook.Pressed += hideBook;
		
		lipidsBook = GetNode<Button>("Drawer/Book1");
		lipidsBook.Pressed += () => showBook("res://sprites/Flipbook/Lipids/", "res://sounds/flipbookSound/lipids.mp3");
		
		carbsBook = GetNode<Button>("Drawer/Book2");
		carbsBook.Pressed += () => showBook("res://sprites/Flipbook/Carbs/", "res://sounds/flipbookSound/carbo.mp3");
		
		naBook = GetNode<Button>("Drawer/Book3");
		naBook.Pressed += () => showBook("res://sprites/Flipbook/NucliecAcid/", "res://sounds/flipbookSound/na.mp3");
		
		proteinBook = GetNode<Button>("Drawer/Book4");
		proteinBook.Pressed += () => showBook("res://sprites/Flipbook/Protein/", "res://sounds/flipbookSound/protein.mp3");
		
		t1 = GetNode<Button>("Tutorial1/Button");
		t1.Pressed += () => tutorial(true, false);
		
		t2 = GetNode<Button>("Tutorial2/Button");
		t2.Pressed += () => tutorial(false, true);
		
		t3 = GetNode<Button>("Tutorial3/Button");
		t3.Pressed += () => tutorial(false, false);		
		
		task1 = GetNode<TextureButton>("TaskView/Task1");
		task1.Pressed += () => displayTask("res://sprites/Extras/task1.png","MiF");
		
		task2 = GetNode<TextureButton>("TaskView/Task2");
		task2.Pressed += () => displayTask("res://sprites/Extras/task2.png","Codon");
		
		task3 = GetNode<TextureButton>("TaskView/Task3");
		task3.Pressed += () => displayTask("res://sprites/Extras/task3.png","Aminos");
		
		task4 = GetNode<TextureButton>("TaskView/Task4");
		task4.Pressed += () => displayTask("res://sprites/Extras/task4.png","Carbo");
		
		taskfinal = GetNode<TextureButton>("TaskView/FinalTask");
		taskfinal.Pressed += () => displayTask("res://sprites/Extras/QR.png","Final");
		
		viewerExit = GetNode<Button>("TaskViewer/exitBtn");
		viewerExit.Pressed += exitViewer;
		
		playBtn = GetNode<Button>("TaskViewer/playBtn");
		
		qrButton = GetNode<Button>("TaskViewer/qrBtn");
		qrButton.Pressed += OnFinalTestPressed;
		
		skip = GetNode<Button>("LessonPlayer/skipBtn");
		skip.Pressed += skipTutorial;
		
		tv = GetNode<TextureRect>("TV");
		tasks = GetNode<TextureRect>("Tasks");
		book = GetNode<TextureRect>("Book");
		Tutorial2 = GetNode<TextureRect>("Tutorial2");
		Tutorial3 = GetNode<TextureRect>("Tutorial3");
		taskviewer = GetNode<TextureRect>("TaskViewer");
		
		if(tutorialPlayer.Playing){
			tv.SetProcessInput(false);
			tasks.SetProcessInput(false);
			exitButton.Disabled = true;
		}else{
			tv.SetProcessInput(true);
			tasks.SetProcessInput(true);
			exitButton.Disabled = false;
		}

		if (tv is TV tvScript)
		{
			GD.Print("Connecting signal TVUIVisibilityChanged");
			tvScript.Connect("TVUIVisibilityChanged", new Callable(this, nameof(OnTVUIVisibilityChanged)));
		}
		else
		{
			GD.PrintErr("Failed to cast TV node to TV script.");
		}
		
		LeftPage = GetNode<TextureRect>("Book/LeftPage");
		RightPage = GetNode<TextureRect>("Book/RightPage");
	}
	
	private void exitViewer()
	{
		taskviewer.Visible = false;	
		if (playBtn.IsConnected("pressed", Callable.From(OnPlayBtnPressed)))
		{
			playBtn.Disconnect("pressed", Callable.From(OnPlayBtnPressed));
		}
	}
	
	private void displayTask(string path, string task){
		currentTask = task;
		
		taskviewer.Texture = null;
		taskviewer.Texture = ResourceLoader.Load<Texture2D>(path);
		taskviewer.Visible = true; 
		if(task == "Final"){
			playBtn.Visible = false;
			qrButton.Visible = true;
		}else{
			playBtn.Visible = true;
			qrButton.Visible = false;
		}
		
		if (playBtn.IsConnected("pressed", Callable.From(OnPlayBtnPressed)))
		{
			playBtn.Disconnect("pressed", Callable.From(OnPlayBtnPressed));
		}
		playBtn.Pressed += OnPlayBtnPressed;
	}
	
	private void OnPlayBtnPressed()
	{
		defineGame(currentTask);
	}
	
	private void defineGame(string kind){
		switch(kind){
			case "MiF":
				OnPlayMifPressed();
				taskviewer.Visible = false;
				break;
			case "Aminos":
				OnPlayMtaPressed();
				taskviewer.Visible = false;
				break;
			case "Codon":
				OnPlayCodonPressed();
				taskviewer.Visible = false;
				break;
			case "Carbo":
				OnPlayCarboLipidsPressed();
				taskviewer.Visible = false;
				break;
			default:
				break;
		}
	}

	private void OnTVUIVisibilityChanged(bool isVisible)
	{
		GD.Print("Received signal: TVUIVisibilityChanged(" + isVisible + ")");

		if (isVisible)
		{
			GD.Print("Stopping music");
			musicPlayer.Stop();
		}
		else
		{
			GD.Print("Playing music");
			musicPlayer.Play();
		}
	}
	
	private void showBook(string PagesDirectory, string path)
	{
		LoadPagesFromFolder(PagesDirectory);
		UpdatePages();
		InitializePagePositions();
		InitializeNavigationButtons();
		musicPlayer.Stop();
		flipbookPlayer.Stop();
		flipbookPlayer.Stream = GD.Load<AudioStream>(path);
		flipbookPlayer.Play();
		book.Visible = true;	
		book.Position = new Vector2(510, 215);	
		toggleObjects(true);
		tv.SetProcessInput(false);
		tasks.SetProcessInput(false);
	}
	
	private void hideBook()
	{
		_currentPageIndex = 0;  // Reset to first page
		UpdatePages(); 
		
		book.Visible = false;
		toggleObjects(false);
		tv.SetProcessInput(true);
		tasks.SetProcessInput(true);
		flipbookPlayer.Stop();
		musicPlayer.Play();
	}

	private void OnExitButtonPressed()
	{
		GD.Print("Exit button pressed!");
		exitSign.Visible = true; // Show exit sign UI
		exitSign.Position = new Vector2(545, 271);
		tv.SetProcessInput(false);
		tasks.SetProcessInput(false);
	}
	
	private void OnNoButtonPressed()
	{
		exitSign.Visible = false;
		tv.SetProcessInput(true);
		tasks.SetProcessInput(true);
	}
	
	private void OnYesButtonPressed()
	{
		GetTree().Quit();
	}
	
	private void OnPlayMifPressed()
	{
		PackedScene scene = GD.Load<PackedScene>("res://scenes/MakeItFunction.tscn");

		if (scene != null)
		{
			GetTree().ChangeSceneToPacked(scene);
		}
		else
		{
			GD.PrintErr("Failed to load the scene.");
		}
	}
	
	private void OnFinalTestPressed()
	{
		OS.ShellOpen("https://quizizz.com/join/quiz/680b84e9bec0c8006e02735a/start?studentShare=true");
	}
	
	private void OnPlayMtaPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/MatchTheAminos.tscn");
	}

	private void OnPlayCarboLipidsPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/CarboLipids.tscn");
	}

	private void OnPlayCodonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/Codon.tscn");
	}
	
	private void InitializePagePositions()
	{
		LeftPage.Size = new Vector2(500, 700);
		LeftPage.StretchMode = TextureRect.StretchModeEnum.KeepAspect;

		RightPage.Size = new Vector2(500, 700);
		RightPage.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
		// Set the initial position for LeftPage and RightPage
		LeftPage.Position = new Vector2(0, 0);  // Left side of the screen
		RightPage.Position = new Vector2(LeftPage.Size.X - 5, 0);  // Right side of the screen
	}
	
	private void InitializeNavigationButtons()
	{
		Vector2 buttonSize = new Vector2(120, 50); // Width, Height
		float buttonY = LeftPage.Position.Y + LeftPage.Size.Y + 20;

		// Position prevButton centered under LeftPage
		prevPage.Size = buttonSize;
		prevPage.Position = new Vector2(
			LeftPage.Position.X + (LeftPage.Size.X - buttonSize.X) / 2,
			buttonY
		);

		// Position nextButton centered under RightPage
		nextPage.Size = buttonSize;
		nextPage.Position = new Vector2(
			RightPage.Position.X + (RightPage.Size.X - buttonSize.X) / 2,
			buttonY
		);
	}

	private void LoadPagesFromFolder(string path)
	{
		_pages.Clear();

		// First count how many PNG files exist
		var dir = DirAccess.Open(path);
		if (dir == null)
		{
			GD.PrintErr($"Can't open directory: {path}");
			return;
		}

		int pngCount = 0;
		switch(path){
			case "res://sprites/Flipbook/NucliecAcid/":
				pngCount = 10;
				break;
			case "res://sprites/Flipbook/Carbs/":
				pngCount = 10;
				break;
			case "res://sprites/Flipbook/Lipids/":
				pngCount = 14;
				break;
			case "res://sprites/Flipbook/Protein/":
				pngCount = 24;
				break;
			default:
				break;
		}
		
		// Then load the pages based on the count
		for (int i = 1; i <= pngCount; i++)
		{
			string filePath = $"{path}page ({i}).jpg";
			Texture2D tex = ResourceLoader.Load<Texture2D>(filePath);
			if (tex != null)
				_pages.Add(tex);
			else
				GD.PrintErr($"Failed to load page: {filePath}");
		}
	}

	private void UpdatePages()
	{
		LeftPage.Texture = (_currentPageIndex < _pages.Count) ? _pages[_currentPageIndex] : null;
		RightPage.Texture = (_currentPageIndex + 1 < _pages.Count) ? _pages[_currentPageIndex + 1] : null;
	}

	private async void FlipForward()
	{
		if (_isFlipping || _currentPageIndex + 2 >= _pages.Count)
			return;

		_isFlipping = true;

		// Animate right page flipping out (scale X from 1 to 0)
		for (float i = 1f; i >= 0f; i -= 0.1f)
		{
			RightPage.Scale = new Vector2(i, 1);
			await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
		}

		// Change page content
		_currentPageIndex += 2;
		UpdatePages();

		// Animate right page flipping in (scale X from 0 to 1)
		for (float i = 0f; i <= 1f; i += 0.1f)
		{
			RightPage.Scale = new Vector2(i, 1);
			await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
		}

		RightPage.Scale = Vector2.One;
		_isFlipping = false;
	}

	private async void FlipBackward()
	{
		if (_isFlipping || _currentPageIndex - 2 < 0)
			return;

		_isFlipping = true;

		// Animate right page flipping out (scale X from 1 to 0)
		for (float i = 1f; i >= 0f; i -= 0.1f)
		{
			RightPage.Scale = new Vector2(i, 1);  // Scale down right page
			await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
		}

		// Change page content
		_currentPageIndex -= 2;
		UpdatePages();

		// Animate right page flipping in (scale X from 0 to 1)
		for (float i = 0f; i <= 1f; i += 0.1f)
		{
			RightPage.Scale = new Vector2(i, 1);  // Scale right page back to normal
			await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
		}

		RightPage.Scale = Vector2.One;  // Reset scale to normal
		_isFlipping = false;
	}
		
	private void toggleObjects(bool state)
	{
		lipidsBook.Disabled = state;
		carbsBook.Disabled = state;
		naBook.Disabled = state;
		proteinBook.Disabled = state;
		exitButton.Disabled = state;
		tv.SetProcessInput(!state);
		tasks.SetProcessInput(!state);
	}
	
	private void tutorial(bool state2 = false, bool state3 = false){
		toggleObjects(true);
		tutorialPlayer.Stop();
		musicPlayer.Stop();
		if(state2){
			Tutorial1.Visible = !state2;
			Tutorial2.Visible = state2;
			tutorialPlayer.Stream = GD.Load<AudioStream>("res://sounds/tutorialSound/step_3.mp3");
			tutorialPlayer.Play();
		}else if(state3){
			Tutorial2.Visible = !state3;
			Tutorial3.Visible = state3;
			tutorialPlayer.Stream = GD.Load<AudioStream>("res://sounds/tutorialSound/step_2.mp3");
			tutorialPlayer.Play();
		}else{
			Tutorial3.Visible = state3;
			lp.Visible = true;
			lp.Position = new Vector2(0,0);
			playVideo();
		}
	}
	
	private void playVideo()
	{
		lp.Play();

		// Disconnect first in case already connected (prevents multiple triggers)
		if (lp.IsConnected("finished", Callable.From(OnVideoFinished)))
		{
			lp.Disconnect("finished", Callable.From(OnVideoFinished));
		}

		lp.Finished += OnVideoFinished;
	}

	private void OnVideoFinished()
	{
		skip.Visible = false;
		proceed.Visible = true;
	}
	
	private void hideVideo(){
		lp.Visible = false;
		toggleObjects(false);
		musicPlayer.Play();
	}
	
	private void skipTutorial()
	{
		lp.Stop();
		hideVideo();
	}
}
