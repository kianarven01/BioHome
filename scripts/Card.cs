using Godot;
using System;

public partial class Card : Area2D
{
	[Export] public Texture2D FrontTexture;  // Front image
	[Export] public Texture2D BackTexture;   // Back image (same for all cards)

	private Sprite2D _frontSprite;
	private Sprite2D _backSprite;
	private bool _isFlipped = false;
	private bool _isMatched = false;

	public int CardId { get; set; }

	public override void _Ready()
	{
		_frontSprite = GetNode<Sprite2D>("Front");
		_backSprite = GetNode<Sprite2D>("Back");

		if (FrontTexture != null)
			_frontSprite.Texture = FrontTexture;

		if (BackTexture != null)
			_backSprite.Texture = BackTexture;

		_frontSprite.Visible = false; // Start with back visible

		Connect("input_event", new Callable(this, nameof(OnCardClicked)));
	}

	private void OnCardClicked(Node viewport, InputEvent inputEvent, int shapeIdx)
	{
		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (!_isFlipped && !_isMatched)
			{
				FlipCard();
				GetTree().CallGroup("GameController", "CheckMatch", this);
			}
		}
	}

	public void FlipCard()
	{
		_isFlipped = !_isFlipped;
		_frontSprite.Visible = _isFlipped;
		_backSprite.Visible = !_isFlipped;
	}

	public void SetMatched()
	{
		_isMatched = true;
	}
}
