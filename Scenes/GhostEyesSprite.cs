using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class GhostEyesSprite : Sprite2D {
	[ExportGroup("Eyes Textures")]
	[Export] Texture2D up;
	[Export] Texture2D down;
	[Export] Texture2D left;
	[Export] Texture2D right;

	private Dictionary<string, Texture2D> directions = new Dictionary<string, Texture2D>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		directions["down"] = down;
		directions["up"] = up;
		directions["left"] = left;
		directions["right"] = right;
		(GetParent() as Ghost).Connect(nameof(Ghost.DirectionChange), new Callable(this, nameof(OnDirectionChange)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private void OnDirectionChange(string new_direction) {
		Texture = directions[new_direction];
	}

	public void hideEyes() {
		Hide();
	}

	public void showEyes() {
		Show();
	}
}
