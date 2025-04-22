using Godot;
using System;

public partial class End : Node {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GetNode<Label>("ScoreLabel").Text = "Your Score: " + Global.Instance.Score.ToString();
		GetNode<Button>("RestartButton").GrabFocus();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public void OnStartButtonPressed() {
		Global.Instance.Score = 0;
		Global.Instance.GhostSpeed = 120;
		Global.Instance.Lives = 3;
		GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
	}

	public void OnQuitButtonPressed() {
		GetTree().Quit();
	}
}
