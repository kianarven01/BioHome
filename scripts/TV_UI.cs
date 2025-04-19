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
		Button video1Button = _uiBg.GetNodeOrNull<Button>("video1");

		closeButton.ZIndex = 100;
		closeButton.Pressed += CloseUI;
		video1Button.Pressed += PlayVideo;
	}

	private void PlayVideo()
	{
		if (_videoPlayer.IsPlaying()) return; 

		_videoPlayer.Stream = GD.Load<VideoStream>("res://videos/video1.ogv");
		_videoPlayer.Play();
		_videoPlayer.Visible = true;

		GetNode<Button>("UIbg/video1").Disabled = true; 
	}

	private void CloseUI()
	{
		if (_videoPlayer.IsPlaying())
		{
			_videoPlayer.Stop();
			_videoPlayer.Visible = false;
			GetNode<Button>("UIbg/video1").Disabled = false;
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
