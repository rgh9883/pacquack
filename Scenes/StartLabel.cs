using Godot;
using System;
using System.Threading.Tasks;

public partial class StartLabel : Label {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Text = "Ready!";
	}


	private async void OnStartTimer() {
		Text = "Start!";
		await ToSignal(GetTree().CreateTimer(1.0), "timeout");
		Hide();
		Text = "Ready!";
	}
}
