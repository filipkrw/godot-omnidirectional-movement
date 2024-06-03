using Godot;
using System;

public partial class Mannequin : CharacterBody3D
{
    [Export]
    private StaticBody3D floor;
    [Export]
    private Node3D marker;

    [Signal]
    public delegate void BlendSpace2DUpdatedEventHandler(Vector2 position);

    private AnimationTree animationTree;

    private Vector3 lookAtPosition;

    private Vector3 acceleration = Vector3.Zero;
    private float speed = 32.0f;
    private float maxVelocity = 5.5f;

    public float rotationSpeed = 20.0f;
    public float angularVelocity = 0.0f;
    public float maxAngularVelocity = 6.0f;


    public override void _Ready()
    {
        animationTree = GetNode<AnimationTree>("AnimationTree");
        floor.InputEvent += OnFloorInputEvent;
    }

    public override void _Process(double delta)
    {
        UpdateRunAnimation();
        UpdateTurnAnimation();
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateVelocity(delta);
        RotateToTarget(delta);
        MoveAndSlide();
    }

    private void UpdateVelocity(double delta)
    {
        Vector3 acceleration = Vector3.Zero;

        if (Input.IsActionPressed("ui_right"))
            acceleration.X += speed;
        if (Input.IsActionPressed("ui_left"))
            acceleration.X -= speed;
        if (Input.IsActionPressed("ui_up"))
            acceleration.Z -= speed;
        if (Input.IsActionPressed("ui_down"))
            acceleration.Z += speed;

        Velocity += acceleration * (float)delta;

        if (Velocity.LengthSquared() > maxVelocity * maxVelocity)
            Velocity = Velocity.Normalized() * maxVelocity;

        if (acceleration.Length() < 0.01f)
        {
            Velocity *= 0.7f;
        }

    }

    private void RotateToTarget(double delta)
    {
        // Get the node's global transform
        Transform3D currentTransform = GlobalTransform;

        // Calculate the horizontal vector to the target (ignoring Y component)
        Vector3 horizontalTargetVector = lookAtPosition - currentTransform.Origin;
        horizontalTargetVector.Y = 0;
        horizontalTargetVector = horizontalTargetVector.Normalized();

        // Current forward vector on the horizontal plane
        Vector3 horizontalForwardVector = -currentTransform.Basis.Z;
        horizontalForwardVector.Y = 0;
        horizontalForwardVector = horizontalForwardVector.Normalized();

        // Calculate the dot product
        float dot = horizontalForwardVector.Dot(horizontalTargetVector);

        // Clamp the dot product to avoid errors in Acos
        dot = Mathf.Clamp(dot, -1.0f, 1.0f);

        // Calculate the angle in radians
        float angleRadiansAbs = Mathf.Acos(dot);

        // Determine the direction to rotate using cross product
        Vector3 cross = horizontalForwardVector.Cross(horizontalTargetVector);

        float angleRadians = cross.Y > 0 ? angleRadiansAbs : -angleRadiansAbs;

        angularVelocity = Mathf.Clamp(angleRadians * rotationSpeed, -maxAngularVelocity, maxAngularVelocity);

        RotateY(angularVelocity * (float)delta);
    }

    private void UpdateRunAnimation()
    {
        Vector2 forward = new(GlobalTransform.Basis.Column2.X, GlobalTransform.Basis.Column2.Z);
        float angle = forward.AngleTo(new Vector2(0, 1));

        float blendMagnitude = Velocity.Length() / maxVelocity;
        Vector3 velocityNormalized = Velocity.Normalized();

        Vector2 blendPosition = new Vector2(velocityNormalized.X, velocityNormalized.Z).Rotated(angle).Normalized() * blendMagnitude;
        blendPosition.X *= -1.0f;

        animationTree.Set("parameters/BlendSpace2D/blend_position", blendPosition);

        EmitSignal(SignalName.BlendSpace2DUpdated, blendPosition);

        // Adjust animation speed based on blend position
        float x = Mathf.Abs(blendPosition.AngleTo(new Vector2(0, 1)));
        float t = x * 0.5f;
        if (blendPosition.Y > 0)
            t += Mathf.Abs(blendPosition.Y) * 1.8f;
        animationTree.Set("parameters/TimeScale/scale", 2.5f + t);
    }

    private void UpdateTurnAnimation()
    {
        // Multiplier is 1 when velocity is zero, and approaches 0 as velocity increases
        // In other words, turn animation is played only when velocity is equal or near to zero
        float k = 10;
        float multiplier = Math.Min(1, 1 / (k * Velocity.Length()));

        float blendAmount = Mathf.Clamp(angularVelocity / maxAngularVelocity, -1f, 1f) * -1f * multiplier;
        animationTree.Set("parameters/TurnBlend3/blend_amount", blendAmount);
    }

    private void OnFloorInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx)
    {
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            lookAtPosition = position;
            marker.Position = new Vector3(position.X, 0.01f, position.Z);
        }
    }
}
