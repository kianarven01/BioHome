// GlobalState.cs
using Godot;
using System;

public partial class GlobalState : Node
{
	public bool TutorialShown = false;

	public override void _Ready()
	{
		// Optional: This node can persist across scenes
		SetProcess(false);
	}
}
