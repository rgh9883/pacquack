using Godot;
using System;
using System.Linq;

public partial class PelletsManager : Node {
	int total_pellets_count;

	int pellets_eaten = 0;
	[Export] Ghost[] ghosts;

	[Signal] public delegate void AllPelletsEatenEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Node[] pellets = GetChildren().ToArray();
		total_pellets_count = GetChildCount();
		for(int i = 0; i < total_pellets_count; i++){
			pellets[i].Connect(nameof(Pellet.PelletEaten), new Callable(this, nameof(OnPelletEaten)));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private void OnPelletEaten(bool can_eat_ghosts) {
		pellets_eaten++;
		if(can_eat_ghosts) {
			foreach(Ghost ghost in ghosts) {
				ghost.runAway();
			}
		}

		if(pellets_eaten == total_pellets_count) {
			EmitSignal(nameof(AllPelletsEaten));
			return;
		}
	}
}
