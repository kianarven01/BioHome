using Godot;

public partial class TV_UI : CanvasLayer
{
	private VideoStreamPlayer _videoPlayer;
	private TextureRect _uiBg; // Reference to UIbg
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
		_videoPlayer.Position = Vector2.Zero; // Align with UIbg
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
		if (_videoPlayer.IsPlaying()) return; // Prevent re-triggering if already playing

		_videoPlayer.Stream = GD.Load<VideoStream>("res://videos/video1.ogv");
		_videoPlayer.Play();
		_videoPlayer.Visible = true;

		GetNode<Button>("UIbg/video1").Disabled = true; // Disable the button while playing
	}

	private void CloseUI()
	{
		if (_videoPlayer.IsPlaying())
		{
			_videoPlayer.Stop();
			_videoPlayer.Visible = false;
			GetNode<Button>("UIbg/video1").Disabled = false; // Re-enable the button
		}
		else
		{
			Visible = false; // Hide the entire UI
			tasks.SetProcessInput(true);
			ExitButton.MouseFilter = Control.MouseFilterEnum.Stop;
		}
	}
}
