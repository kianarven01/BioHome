using Godot;

public partial class MainMenu : Node2D
{
	private AudioStreamPlayer2D musicPlayer;

	public override void _Ready()
	{
		// Get the AudioStreamPlayer2D node
		musicPlayer = GetNode<AudioStreamPlayer2D>("kahoot_jungle");

		// Play the music
		if (!musicPlayer.Playing)
		{
			musicPlayer.Play();
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Handle input events if needed
	}
}
