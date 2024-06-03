using Godot;
using System;

public partial class FollowCamera : Camera3D
{
	[Export]
	Node3D _followTarget;

	Vector3 _offset;

	public override void _Ready()
	{
		_offset = Position - _followTarget.Position;
	}

	public override void _Process(double delta)
	{
		Vector3 targetPosition = _followTarget.Position + _offset;
		Position = Position.Lerp(targetPosition, 1.0f);
	}
}
