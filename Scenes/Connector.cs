using Godot;
using System;

public partial class Connector : Node2D {

	private Area2D right_area2d;
	private Area2D left_area2d;

	bool allow_left = true;
	bool allow_right = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		right_area2d = GetNode<Area2D>("RightColorRect/Area2D");
		left_area2d = GetNode<Area2D>("LeftColorRect/Area2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private void OnRightBodyEntered(Node body) {
		if(((CharacterBody2D)body).Velocity.X > 0) {
			((Node2D)body).Position = new Vector2(left_area2d.GlobalPosition.X, ((Node2D)body).Position.Y);
			allow_left = false;
		}
		
	}

	private void OnRightBodyExited(Node body) {
		allow_left = true;
	}

	private void OnLeftBodyEntered(Node body) {
		if(((CharacterBody2D)body).Velocity.X < 0) {
			((Node2D)body).Position = new Vector2(right_area2d.GlobalPosition.X, ((Node2D)body).Position.Y);
			allow_right = false;
		}
	}

	private void OnLeftBodyExited(Node body) {
		allow_right = true;
	}
}
