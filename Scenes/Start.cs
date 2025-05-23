using Godot;
using System;

public partial class Start : Node {

	public override void _Ready() {
		GetNode<Button>("StartButton").GrabFocus();
		GetNode<Button>("StartButton").Text = "Start";
		GetNode<Button>("QuitButton").Text = "Quit";
		GetNode<Label>("Title").Text = "PacQuack";
	}
	public void OnStartButtonPressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
	}

	public void OnQuitButtonPressed() {
		GetTree().Quit();
	}
}
