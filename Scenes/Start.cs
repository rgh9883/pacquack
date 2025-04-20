using Godot;
using System;

public partial class Start : Node {
	public void onStartButtonPressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
	}
}
