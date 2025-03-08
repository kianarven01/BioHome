using Godot;

public partial class CanvasLayerScaler : CanvasLayer
{
	public override void _Ready()
	{
		AdaptResolution();
	}

	private void AdaptResolution()
	{
		Vector2I screenSize = DisplayServer.WindowGetSize();
		Vector2 baseResolution = new Vector2(1920, 1080); // Adjust this based on your original design resolution

		// Calculate the scale factor while keeping the aspect ratio
		float scaleX = (float)screenSize.X / baseResolution.X;
		float scaleY = (float)screenSize.Y / baseResolution.Y;
		float scaleFactor = Mathf.Min(scaleX, scaleY); // Maintain aspect ratio

		// Apply scale to the CanvasLayer
		Scale = new Vector2(scaleFactor, scaleFactor);

		GD.Print($"Screen adapted: {screenSize.X}x{screenSize.Y}, Scale: {scaleFactor}");
	}
}
