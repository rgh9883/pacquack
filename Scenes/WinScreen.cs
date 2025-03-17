using Godot;
using System;

public partial class WinScreen : CanvasLayer {

	private CenterContainer center_container;
	private Node pelletsManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		center_container = GetNode<CenterContainer>("MarginContainer/CenterContainer");
		pelletsManager = GetNode<Node>("../Pellets");
		pelletsManager.Connect(nameof(PelletsManager.AllPelletsEaten), new Callable(this, nameof(gameWon)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
	}

	private void gameWon() {
		center_container.Show();
	}
}
