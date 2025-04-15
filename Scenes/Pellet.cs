using Godot;
using System;

public partial class Pellet : Area2D {
	[Export] bool can_eat_ghosts;
	
	[Signal] public delegate void PelletEatenEventHandler(bool can_eat_ghosts);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private void OnBodyEntered(Node body) {
		if(body is Pacman) {
			EmitSignal(nameof(PelletEaten), can_eat_ghosts);
            QueueFree();
		}
	}

}
