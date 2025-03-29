using Godot;
using System;

public partial class CarboLipids : Node
{
    private Button backButton;
    private Button carboButton;
    private TextureRect firstItem;

    public override void _Ready()
    {
        // for back button to living_room.tscn
        backButton = GetNode<Button>("backButton");
        backButton.Pressed += OnBackButtonPressed;

        // for carbo button to first_item.tscn
        carboButton = GetNode<Button>("Background/Carbohydrate");
        firstItem = GetNode<TextureRect>("first_item");
        carboButton.Pressed += OnCarboButtonPressed;
        firstItem.Visible = false; // Hide the first item initially
    }

    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/living_room.tscn");
    }

    private void OnCarboButtonPressed()
    {
        firstItem.Visible = true; // Show the first item
        firstItem.Position = new Vector2(0, 0); // Set position as needed
        
    }
}
