using Godot;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

public partial class PointsManager : Node {
	[Export] Ghost[] ghosts;
	[Export] Timer start_timer;
	[Export] Label start_label;
	public int pts_per_ghost {get; set;} = 200;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public async Task OnGhostEaten() {
		Global.Instance.Score += pts_per_ghost;
		pts_per_ghost += 200;
		GetTree().Paused = true;
		await ToSignal(GetTree().CreateTimer(1.0), "timeout");
		GetTree().Paused = false;
	}

	private void OnDeathBegin() {
		Global.Instance.Lives--;
		pts_per_ghost = 200;

		foreach(Ghost ghost in ghosts) {
			ghost.ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	private void OnDeathEnd() {
		if(Global.Instance.Lives == 0) {
			GetTree().ChangeSceneToFile("res://Scenes/end.tscn");
			return;
		}
		foreach(Ghost ghost in ghosts) {
			ghost.ProcessMode = ProcessModeEnum.Inherit;
			ghost.reset();
		}
		start_timer.Start();
		start_label.Show();
	}
}
