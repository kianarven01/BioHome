using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MatchTheAminos : Node2D
{
	private PackedScene _cardScene = (PackedScene)ResourceLoader.Load("res://Card.tscn");

	private string _imageFolder = "res://sprites/Aminos/Assets/structureImages/";
	private List<Texture2D> _textures = new List<Texture2D>();
	private List<int> _cardIds = new List<int>();
	private List<Card> _selectedCards = new List<Card>();

	private Texture2D _backTexture; // Back texture for all cards

	public override void _Ready()
	{
		_backTexture = (Texture2D)ResourceLoader.Load("res://sprites/Aminos/Assets/FaceDownCards.png");
		LoadTexturesFromFolder();
		SpawnCards();
		AddToGroup("GameController");
	}

	private void LoadTexturesFromFolder()
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

		// Duplicate each texture to create matching pairs
		for (int i = 0; i < _textures.Count; i++)
		{
			_cardIds.Add(i);
			_cardIds.Add(i);
		}

		// Shuffle the cards
		_cardIds = _cardIds.OrderBy(x => GD.Randf()).ToList();
	}

	private void SpawnCards()
	{
		for (int i = 0; i < _cardIds.Count; i++)
		{
			Card card = (Card)_cardScene.Instantiate();
			card.CardId = _cardIds[i];
			card.FrontTexture = _textures[_cardIds[i]];
			card.BackTexture = _backTexture; // Assign the same back texture
			AddChild(card);

			// Positioning in a grid layout
			card.Position = new Vector2((i % 4) * 150, (i / 4) * 200);
		}
	}

	public void CheckMatch(Card card)
	{
		if (_selectedCards.Count < 2)
		{
			_selectedCards.Add(card);
		}

		if (_selectedCards.Count == 2)
		{
			if (_selectedCards[0].CardId == _selectedCards[1].CardId)
			{
				_selectedCards[0].SetMatched();
				_selectedCards[1].SetMatched();
			}
			else
			{
				GetTree().CreateTimer(1.0f).Timeout += () =>
				{
					_selectedCards[0].FlipCard();
					_selectedCards[1].FlipCard();
				};
			}

			_selectedCards.Clear();
		}
	}
}
