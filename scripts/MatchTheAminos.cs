using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MatchTheAminos : Node2D
{
	private PackedScene CardScene = (PackedScene)ResourceLoader.Load("res://scenes/Card.tscn");
	private List<Texture2D> _textures = new List<Texture2D>();
	private List<Card> _selectedCards = new List<Card>();
	private List<Card> _cards = new List<Card>();
	private Control _lifeContainer;
	private List<TextureRect> _hearts = new List<TextureRect>();
	private Texture2D _backTexture;
	private float _yOffset = 10f;
	private Button structButton;
	private Button funcButton;
	private TextureRect mainGame;

	// Manually defined card pairs
	private List<(int, int)> _manualPairs = new List<(int, int)>
	{
		(0, 4), (2, 3), (6, 8), (5, 7), (1, 9)  // Five pairs of unique IDs
	};

	public override void _Ready()
	{
		structButton = GetNode<Button>("../../Background/structureButton");
		funcButton = GetNode<Button>("../../Background/functionButton");
		mainGame = GetNode<TextureRect>("../../MainBG");
		mainGame.Visible = false;
		
		structButton.Pressed += () => LoadGame("res://sprites/Aminos/Assets/structureImages/");
		funcButton.Pressed += () => LoadGame("res://sprites/Aminos/Assets/functionImages/");
	}
	
	private void LoadGame(string cardDir)
	{
		mainGame.Visible = true;
		mainGame.Position = new Vector2(0, 0);
		_backTexture = (Texture2D)ResourceLoader.Load("res://sprites/Aminos/Assets/FaceDownCards.png");
		LoadTexturesFromFolder(cardDir);
		SpawnCards();
		AddToGroup("GameController");
		CallDeferred(nameof(SpawnLifeIndicator));
	}
	
	private void SpawnLifeIndicator()
	{
		Node parent = GetParent();
		if (parent is not Control parentControl)
		{
			GD.PrintErr("Parent is not a Control! Cannot place life indicator.");
			return;
		}

		Vector2 parentSize = parentControl.Size;
		float heartSize = 30;
		float spacing = 17;
		float totalWidth = (heartSize * 3) + (spacing * 2);
		_lifeContainer = new Control
		{
			Name = "LifeContainer",
			Position = new Vector2(parentSize.X - totalWidth - 40, _yOffset),  // Centered horizontally, calibrated y-position
		};
		//parentControl.AddChild(_lifeContainer);
		parent.CallDeferred("add_child", _lifeContainer);
		for (int i = 0; i < 3; i++)
		{
			TextureRect heart = new TextureRect
			{
				Texture = (Texture2D)ResourceLoader.Load("res://sprites/Aminos/Assets/Life.png"),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(30, 30),
				Position = new Vector2(i * (heartSize + spacing), 0)
			};
		
			_hearts.Add(heart);
			_lifeContainer.CallDeferred("add_child", heart); // Defer adding hearts
		}
	}

	private void LoadTexturesFromFolder(string _imageFolder)
	{
		DirAccess dir = DirAccess.Open(_imageFolder);
		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (fileName != "")
			{
				if (fileName.EndsWith(".png") || fileName.EndsWith(".jpg"))
				{
					string path = _imageFolder + fileName;
					Texture2D texture = (Texture2D)ResourceLoader.Load(path);
					if (texture != null)
						_textures.Add(texture);
				}
				fileName = dir.GetNext();
			}
		}
		
		if (_textures.Count > 10)
			_textures = _textures.Take(10).ToList();
	}

	private void SpawnCards()
	{
		if (CardScene == null)
		{
			GD.PrintErr("CardScene is not assigned!");
			return;
		}

		int columns = 5;
		int rows = 2;

		// Get the parent's (TextureRect) size
		TextureRect parent = GetParent<TextureRect>();
		if (parent == null)
		{
			GD.PrintErr("Parent is not a TextureRect!");
			return;
		}
		Vector2 parentSize = parent.Size;

		// Calculate card size dynamically
		float cardWidth = parentSize.X  / (columns + 2.0f);
		float cardHeight = parentSize.Y  / (rows + 2.0f);
		Vector2 cardSize = new Vector2(cardWidth + 20, cardHeight + 20);

		// Calculate spacing dynamically
		float xSpacing = parentSize.X / (columns + 2.5f);
		float ySpacing = parentSize.Y / (rows + 1.2f);

		// Midpoint reference
		Vector2 center = parentSize / 2;
		float yOffset = parentSize.Y * 0.15f; // Adjust percentage as needed

		Vector2 startPosition = center - new Vector2((columns - 1) * xSpacing / 2, (rows - 1) * ySpacing / 2) 
						+ new Vector2(0, yOffset);

		// Shuffle the card IDs
		List<int> cardIds = _manualPairs.SelectMany(pair => new List<int> { pair.Item1, pair.Item2 }).ToList();
		cardIds = cardIds.OrderBy(_ => GD.Randf()).ToList(); // Shuffle placement

		for (int i = 0; i < cardIds.Count; i++)
		{
			int row = i / columns;
			int col = i % columns;

			Node newNode = CardScene.Instantiate();
			if (newNode is Card newCard)
			{
				// Set position relative to the midpoint
				newCard.Position = startPosition + new Vector2(col * xSpacing, row * ySpacing);

				// Set size dynamically
				newCard.Scale = new Vector2(cardSize.X / 400f, cardSize.Y / 400f); // Assuming original size is 200x200px

				// Assign card ID and textures
				newCard.CardId = cardIds[i];
				newCard.FrontTexture = _textures[cardIds[i]];
				newCard.BackTexture = _backTexture;

				// Adjust CollisionShape2D size safely
				if (newCard.HasNode("Area2D/CollisionShape2D"))
				{
					CollisionShape2D collisionShape = newCard.GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
					if (collisionShape.Shape is RectangleShape2D rectShape)
					{
						rectShape.Size = cardSize;
					}
				}

				// Use CallDeferred to prevent setup conflicts
				parent.CallDeferred("add_child", newCard);
				_cards.Add(newCard);
			}
			else
			{
				GD.PrintErr("CardScene is not of type Card!");
			}
		}
	}

	public void CheckMatch(Card selectedCard)
	{
		if (_selectedCards.Contains(selectedCard))
			return;

		_selectedCards.Add(selectedCard);

		if (_selectedCards.Count == 2)
		{
			Card firstCard = _selectedCards[0];
			Card secondCard = _selectedCards[1];

			if (_manualPairs.Any(pair => (pair.Item1 == firstCard.CardId && pair.Item2 == secondCard.CardId) ||
										 (pair.Item2 == firstCard.CardId && pair.Item1 == secondCard.CardId)))
			{
				firstCard.SetMatched();
				secondCard.SetMatched();
			}
			else
			{
				ReduceLife();
				GetTree().CreateTimer(1.0f).Timeout += () =>
				{
					firstCard.FlipCard();
					secondCard.FlipCard();
				};
			}

			_selectedCards.Clear();
		}
	}
	
	private void ReduceLife()
	{
		if (_hearts.Count > 0)
		{
			_hearts[_hearts.Count - 1].QueueFree(); // Remove first heart
			_hearts.RemoveAt(_hearts.Count - 1);
		}else{
			
		}
	}
}
