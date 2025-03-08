using Godot;

public partial class TV_UI : CanvasLayer
{
	private VideoStreamPlayer _videoPlayer;
	private Control _uiBg; // Reference to UIbg

	public override void _Ready()
	{
		
		_videoPlayer = GetNode<VideoStreamPlayer>("VideoStreamPlayer");
		_uiBg = GetNode<Control>("UIbg"); // Assuming UIbg is a Control or ColorRect

		// Adjust VideoStreamPlayer size to match UIbg
		_videoPlayer.Size = _uiBg.Size;
		_videoPlayer.Position = _uiBg.Position;
		_videoPlayer.Expand = true;
		_videoPlayer.ZIndex = 10; // Ensure it's above other UI elements


		// Connect buttons
		GetNode<Button>("close_button").ZIndex = 100;
		GetNode<Button>("video1").Pressed += PlayVideo;
		GetNode<Button>("close_button").Pressed += CloseUI;
	}

	private void PlayVideo()
	{
		_videoPlayer.Stream = GD.Load<VideoStream>("res://videos/video1.ogv"); // Replace with correct format
		_videoPlayer.Play();
		_videoPlayer.Visible = true; // Ensure it is visible
		GD.Print($"Video Size: {_videoPlayer.Size}, UIbg Size: {_uiBg.Size}");

	}

	private void CloseUI()
	{
		_videoPlayer.Stop();
		_videoPlayer.Visible = false;
		Visible = false;
	}
}
