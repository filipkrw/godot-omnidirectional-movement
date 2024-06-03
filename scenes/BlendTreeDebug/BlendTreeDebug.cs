using Godot;
using System;

public partial class BlendTreeDebug : Node2D
{
	[Export]
	Mannequin character;

	Line2D line;

	public override void _Ready()
	{
		line = GetNode<Line2D>("Line2D");

		line.Points = new Vector2[] { Vector2.Zero, Vector2.Zero };

		character.BlendSpace2DUpdated += OnBlendSpace2DUpdated;
	}

	private void OnBlendSpace2DUpdated(Vector2 position)
	{
		float width = GetViewportRect().Size.X;
		float height = GetViewportRect().Size.Y;

		Vector2 origin = new Vector2(width / 2, height / 2);

		line.Points = new Vector2[] {
			new(position.X * 0.5f * height + origin.X, position.Y * 0.5f * height + origin.Y),
			origin
		};
	}

}
