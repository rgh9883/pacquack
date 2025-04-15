using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class GhostBodySprite : Sprite2D {
	private AnimationPlayer animation_player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		SelfModulate = (GetParent() as Ghost).color;
		animation_player = GetNode<AnimationPlayer>("../AnimationPlayer");
		animation_player.Play("moving");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
}
