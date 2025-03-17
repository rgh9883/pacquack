using Godot;
using System;

public partial class Pellet : Area2D {
	[Export] bool should_allow_eating_ghosts = false;
	
	[Signal] public delegate void PelletEatenEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private void OnBodyEntered(Node body) {
		if(body is Pacman) {
			EmitSignal(nameof(PelletEaten));
            QueueFree();
			if(should_allow_eating_ghosts) {
				return;
			}
		}
	}

}
