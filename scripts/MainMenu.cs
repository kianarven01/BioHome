using Godot;

public partial class MainMenu : Node2D
{
	private AudioStreamPlayer2D musicPlayer;
	private TextureRect ratingsPnl;
	private Button rateBtn;
	private Button closeRatings;
	private ButtonGroup react;

	public override void _Ready()
	{
		// Get the AudioStreamPlayer2D node
		musicPlayer = GetNode<AudioStreamPlayer2D>("kahoot_jungle");
		ratingsPnl = GetNode<TextureRect>("LandingScene/RatingPanel");
		rateBtn = GetNode<Button>("LandingScene/rateBtn");
		closeRatings = GetNode<Button>("LandingScene/RatingPanel/close_button");
		
		rateBtn.Pressed += showRatings;
		closeRatings.Pressed += closeRating;
		
		react =  GD.Load<ButtonGroup>("res://react.tres");

		// Play the music
		if (!musicPlayer.Playing)
		{
			musicPlayer.Play();
		}
	}
	
	private void showRatings()
	{
		ratingsPnl.Position = new Vector2(-146,-176);
		ratingsPnl.Visible = true;
	}
	
	private void closeRating()
	{
		ratingsPnl.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		// Handle input events if needed
	}
	

}
