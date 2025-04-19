using Godot;

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
	private AudioStreamPlayer2D musicPlayer;

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

		tv = GetNode<TextureRect>("TV");
		tasks = GetNode<TextureRect>("Tasks");

		if (tv is TV tvScript)
		{
			GD.Print("Connecting signal TVUIVisibilityChanged");
			tvScript.Connect("TVUIVisibilityChanged", new Callable(this, nameof(OnTVUIVisibilityChanged)));
		}
		else
		{
			GD.PrintErr("Failed to cast TV node to TV script.");
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
}
