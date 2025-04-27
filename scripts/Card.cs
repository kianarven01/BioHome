using Godot;
using System;

public partial class Card : Node2D
{
	[Export] public Texture2D FrontTexture;
	[Export] public Texture2D BackTexture;

	private Sprite2D _frontSprite;
	private Sprite2D _backSprite;
	private Area2D _area;
	private AudioStreamPlayer2D flipSound;
	private bool _isFlipped = false;
	private bool _isMatched = false;

	public int CardId { get; set; }

	public override void _Ready()
	{
		Scale = new Vector2(0.7f, 0.7f);

		_frontSprite = GetNode<Sprite2D>("Front");
		_backSprite = GetNode<Sprite2D>("Back");
		_area = GetNode<Area2D>("Area2D");
		flipSound = GetNode<AudioStreamPlayer2D>("flip");

		if (FrontTexture != null)
		{
			_frontSprite.Texture = FrontTexture;
			_frontSprite.Scale = new Vector2(1, 1);
		}

		if (BackTexture != null)
		{
			_backSprite.Texture = BackTexture;
			_backSprite.Scale = _frontSprite.Scale;
		}

		// Start with back side visible
		_frontSprite.Visible = false;
		_backSprite.Visible = true;

		_area.Connect("input_event", new Callable(this, nameof(OnCardClicked)));
	}

	private void OnCardClicked(Node viewport, InputEvent inputEvent, int shapeIdx)
	{
		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (!_isFlipped && !_isMatched)
			{
				FlipCard(true); // Flip face up
				GetTree().CallGroup("GameController", "CheckMatch", this);
			}
		}
	}

	public void FlipCard()
	{
		FlipCard(!_isFlipped);
	}

	public void FlipCard(bool faceUp)
	{
		flipSound.Play();
		if (_frontSprite == null || _backSprite == null)
		{
			GD.PrintErr("Sprites are not initialized properly.");
			return;
		}

		_isFlipped = faceUp;
		_frontSprite.Visible = faceUp;
		_backSprite.Visible = !faceUp;
	}

	public void SetMatched()
	{
		_isMatched = true;
	}
}
