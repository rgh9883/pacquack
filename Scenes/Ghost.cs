using Godot;
using System;

public partial class Ghost : Area2D {

	int cur_scatter_index = 0;

	[Export] int speed = 120;
	[Export] movement_targets movement_targets;
	[Export] TileMap tile_map;

	private NavigationAgent2D navigation_agent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		navigation_agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		navigation_agent.PathDesiredDistance = 4.0f;
		navigation_agent.TargetDesiredDistance = 4.0f;

		CallDeferred("setup");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		moveGhost(navigation_agent.GetNextPathPosition(), delta);
	}

	private void moveGhost(Vector2 next_pos, double delta) {
		Vector2 cur_agent_pos = GlobalPosition;

		Position += (next_pos - cur_agent_pos).Normalized() * speed * (float) delta;
	}

	private void setup() {
		navigation_agent.SetNavigationMap(tile_map.GetNavigationMap(0));
		NavigationServer2D.AgentSetMap(navigation_agent.GetRid(), tile_map.GetNavigationMap(0));
		scatter();
	}

	private void scatter() {
		navigation_agent.TargetPosition = movement_targets.scatter_targets[cur_scatter_index].Position;
	}

	private void OnPositionReached() {
		cur_scatter_index++;
		if(cur_scatter_index > 3) {
			cur_scatter_index = 0;
		}
		GD.Print(cur_scatter_index);
		scatter();
	}
}
