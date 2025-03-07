using Godot;
using System;

public partial class DropDown : TextureRect
{
	private AnimationPlayer animPlayer;

	public override void _Ready()
	{
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animPlayer.Play("dropdown");
	}
}
