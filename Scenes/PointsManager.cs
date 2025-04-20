using Godot;
using System;
using System.Threading.Tasks;

public partial class PointsManager : Node {

	public int pts_per_ghost {get; set;} = 200;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public async Task OnGhostEaten() {
		pts_per_ghost += 200;
		GetTree().Paused = true;
		await ToSignal(GetTree().CreateTimer(1.0), "timeout");
		GetTree().Paused = false;
	}
}
