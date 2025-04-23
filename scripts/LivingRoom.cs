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
	private Button playMif;
	private Button playMta;
	private Button carboButton;
	private Button codonButton;
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
	[Export] public TextureRect LeftPage;
	[Export] public TextureRect RightPage;
	private bool _isFlipping = false;

	private List<Texture2D> _pages = new();
	private int _currentPageIndex = 0;

	
	public override void _Ready()
	{
		musicPlayer = GetNode<AudioStreamPlayer2D>("kahoot_lobby");
		if (!musicPlayer.Playing)
		{
			GD.Print("Music is not playing, playing now.");
			musicPlayer.Play();
		}
		else
		{
			GD.Print("Music is already playing.");
		}
	
		exitSign = GetNode<TextureRect>("Exit"); 
		exitSign.Visible = false; // Hide UI initially
		
		exitButton = GetNode<Button>("exitButton");
		exitButton.Pressed += OnExitButtonPressed;
		
		noButton = GetNode<Button>("Exit/noButton");
		noButton.Pressed += OnNoButtonPressed;
		
		yesButton = GetNode<Button>("Exit/yesButton");
		yesButton.Pressed += OnYesButtonPressed;
		
		// PlayMif button
		playMif = GetNode<Button>("TaskView/Function/Button");
		playMif.Pressed += OnPlayMifPressed;
		
		// PlayMta button
		playMta = GetNode<Button>("TaskView/Aminos/Button");
		playMta.Pressed += OnPlayMtaPressed;

		// PlayCarboLipids button
		carboButton = GetNode<Button>("TaskView/Carbo/Button");
		carboButton.Pressed += OnPlayCarboLipidsPressed;

		// PlayCodon button
		codonButton = GetNode<Button>("TaskView/Codon/Button");
		codonButton.Pressed += OnPlayCodonPressed;
		
		// Book Navigation
		nextPage = GetNode<Button>("Book/NextButton");
		nextPage.Pressed += FlipForward;
		
		prevPage = GetNode<Button>("Book/PrevButton");
		prevPage.Pressed += FlipBackward;
		
		closeBook = GetNode<Button>("Book/close_button");
		closeBook.Pressed += hideBook;
		
		lipidsBook = GetNode<Button>("Drawer/Book1");
		lipidsBook.Pressed += () => showBook("res://sprites/Flipbook/Lipids/");
		
		carbsBook = GetNode<Button>("Drawer/Book2");
		carbsBook.Pressed += () => showBook("res://sprites/Flipbook/Carbs/");
		
		naBook = GetNode<Button>("Drawer/Book3");
		naBook.Pressed += () => showBook("res://sprites/Flipbook/NucliecAcid/");
		
		proteinBook = GetNode<Button>("Drawer/Book4");
		proteinBook.Pressed += () => showBook("res://sprites/Flipbook/Protein/");

		tv = GetNode<TextureRect>("TV");
		tasks = GetNode<TextureRect>("Tasks");
		book = GetNode<TextureRect>("Book");

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
	
	private void showBook(string PagesDirectory)
	{
		LoadPagesFromFolder(PagesDirectory);
		UpdatePages();
		InitializePagePositions();
		InitializeNavigationButtons();
		book.Visible = true;
		book.Position = new Vector2(510, 215);	
		toggleObjects(true);
		tv.SetProcessInput(false);
		tasks.SetProcessInput(false);
	}
	
	private void hideBook()
	{
		book.Visible = false;
		toggleObjects(false);
		tv.SetProcessInput(true);
		tasks.SetProcessInput(true);
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
		GetTree().ChangeSceneToFile("res://scenes/MakeItFunction.tscn");
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
		var dir = DirAccess.Open(path);
		if (dir == null)
		{
			GD.PrintErr($"Can't open directory: {path}");
			return;
		}

		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (!string.IsNullOrEmpty(fileName))
		{
			if (fileName.EndsWith(".png") || fileName.EndsWith(".jpg"))
			{
				var tex = GD.Load<Texture2D>(path + fileName);
				if (tex != null)
					_pages.Add(tex);
			}
			fileName = dir.GetNext();
		}

		// Sort based on filename
		_pages = _pages.OrderBy(p => p.ResourcePath).ToList();
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
	}
	
	
}
