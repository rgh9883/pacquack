using Godot;
using System;

public partial class movement_targets : Resource {
	[Export] public Node2D[] scatter_targets = Array.Empty<Node2D>();
	[Export] public Node2D[] at_home_targets = Array.Empty<Node2D>();
}
