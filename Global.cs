using Godot;
using System;

public partial class Global : Node {
	public static Global Instance {get; private set;}

	public int Score {get; set;} = 0;
	public int GhostSpeed {get; set;} = 120;
	public int Lives {get; set;} = 3;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Instance = this;
	}
}
