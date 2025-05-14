using Godot;

public partial class TV_UI : CanvasLayer
{
	[Signal]
	public delegate void TVUIVisibilityChangedEventHandler(bool isVisible);  // Declare the signal
	[Signal]
	public delegate void RequestCloseTVUIEventHandler();

	private VideoStreamPlayer _videoPlayer;
	private TextureRect _uiBg; 
	private TextureRect tasks;
	private Button ExitButton;

	public override void _Ready()
	{
		_uiBg = GetNodeOrNull<TextureRect>("UIbg");
		_videoPlayer = _uiBg.GetNodeOrNull<VideoStreamPlayer>("VideoStreamPlayer");
		tasks = GetNode<TextureRect>("../../Tasks");
		ExitButton = GetNode<Button>("../../exitButton");
		
		// Set Video Player properties
		_videoPlayer.Size = _uiBg.Size;
		_videoPlayer.Position = Vector2.Zero; 
		_videoPlayer.Expand = true;
		_videoPlayer.ZIndex = 10;

		// Connect buttons (inside UIbg)
		Button closeButton = _uiBg.GetNodeOrNull<Button>("close_button");
		Button video1Button = _uiBg.GetNodeOrNull<Button>("carboBtn");
		Button video2Button = _uiBg.GetNodeOrNull<Button>("naBtn");
		Button video3Button = _uiBg.GetNodeOrNull<Button>("lipidsBtn");
		Button video4Button = _uiBg.GetNodeOrNull<Button>("proteinBtn");

		closeButton.ZIndex = 100;
		closeButton.Pressed += CloseUI;
		video1Button.Pressed += () => PlayVideo("res://videos/1Carbo.ogv");
		video2Button.Pressed += () => PlayVideo("res://videos/2Nucliec.ogv");
		video3Button.Pressed += () => PlayVideo("res://videos/4Lipids.ogv");
		video4Button.Pressed += () => PlayVideo("res://videos/3Protein.ogv");
	}

	private void PlayVideo(string videoDir)
	{
		if (_videoPlayer.IsPlaying()) return; 

		_videoPlayer.Stream = GD.Load<VideoStream>(videoDir);
		_videoPlayer.Play();
		_videoPlayer.Visible = true;
		
		GetNode<Button>("UIbg/carboBtn").Disabled = true;	
		GetNode<Button>("UIbg/naBtn").Disabled = true;	
		GetNode<Button>("UIbg/proteinBtn").Disabled = true;	
		GetNode<Button>("UIbg/lipidsBtn").Disabled = true;	
	}

	private void CloseUI()
	{
		if (_videoPlayer.IsPlaying())
		{
			_videoPlayer.Stop();
			_videoPlayer.Visible = false;
			GetNode<Button>("UIbg/carboBtn").Disabled = false;
			GetNode<Button>("UIbg/naBtn").Disabled = false;
			GetNode<Button>("UIbg/proteinBtn").Disabled = false;
			GetNode<Button>("UIbg/lipidsBtn").Disabled = false;
		}
		else
		{
			// Notify parent to handle closing logic and emit proper signal
			EmitSignal("RequestCloseTVUI");
		}

		// Optional debug log
		GD.Print("Requesting TV to close UI");
	}


}
