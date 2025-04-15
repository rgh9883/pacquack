using Godot;
using System;

public partial class Ghost : Area2D {

	int cur_scatter_index = 0;

	[Export] int speed = 120;
	[Export] movement_targets movement_targets;
	[Export] TileMap tile_map;
	[Export] public Color color;
	[Export] Node2D chase_target;
	[Signal] public delegate void DirectionChangeEventHandler(string cur_direction);

	enum GhostState {
		SCATTER,
		CHASE,
		RUNAWAY
	}

	private NavigationAgent2D navigation_agent;
	private string direction = null;
	private GhostState cur_state;
	private Timer scatter_timer;
	private Timer update_chase;
	private Timer runaway_timer;
	private GhostBodySprite body;
	private GhostEyesSprite eyes;
	private bool is_blinking = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		navigation_agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		navigation_agent.PathDesiredDistance = 4.0f;
		navigation_agent.TargetDesiredDistance = 4.0f;

		scatter_timer = GetNode<Timer>("ScatterTimer");
		update_chase = GetNode<Timer>("UpdateChasePos");
		runaway_timer = GetNode<Timer>("RunAwayTimer");

		body = GetNode<GhostBodySprite>("BodySprite");
		eyes = GetNode<GhostEyesSprite>("EyesSprite");

		CallDeferred("setup");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if(!runaway_timer.IsStopped() && runaway_timer.TimeLeft < runaway_timer.WaitTime / 3 && !is_blinking) {
			startBlinking();
		}
		moveGhost(navigation_agent.GetNextPathPosition(), delta);
	}

	private void moveGhost(Vector2 next_pos, double delta) {
		Vector2 cur_agent_pos = GlobalPosition;

		Vector2 new_velo = (next_pos - cur_agent_pos).Normalized() * speed * (float) delta;

		Position += new_velo;

		calcDirection(new_velo);
	}

	private void calcDirection(Vector2 velocity) {
		string cur_direction;
		
		if(velocity.X > 1) {
			cur_direction = "right";
		} else if(velocity.X < -1) {
			cur_direction = "left";
		} else if(velocity.Y > 1) {
			cur_direction = "down";
		} else {
			cur_direction = "up";
		}

		if(cur_direction != direction) {
			direction = cur_direction;
			EmitSignal(nameof(DirectionChange), direction);
		}
	}

	private void setup() {
		navigation_agent.SetNavigationMap(tile_map.GetNavigationMap(0));
		NavigationServer2D.AgentSetMap(navigation_agent.GetRid(), tile_map.GetNavigationMap(0));
		scatter_timer.Start();
		scatter();
	}

	private void scatter() {
		cur_state = GhostState.SCATTER;
		navigation_agent.TargetPosition = movement_targets.scatter_targets[cur_scatter_index].Position;
	}

	private void OnPositionReached() {
		if(cur_state == GhostState.SCATTER) {
			cur_scatter_index++;
			if(cur_scatter_index > 3) {
				cur_scatter_index = 0;
			}
			scatter();
		} else if(cur_state == GhostState.CHASE) {
			GD.Print("KILL");
		} else if(cur_state == GhostState.RUNAWAY) {
			runAway();
		}
		
	}

	private void OnScatterTimeout() {
		startChase();
	}

	private void startChase() {
		if(chase_target == null) {
			GD.Print("NO TARGET");
		}
		update_chase.Start();
		cur_state = GhostState.CHASE;
		navigation_agent.TargetPosition = chase_target.Position; 
	}

	private void OnUpdateChasePos() {
		navigation_agent.TargetPosition = chase_target.Position;
	}

	public void runAway() {
		if(runaway_timer.IsStopped()) {
			runaway_timer.Start();
			cur_state = GhostState.RUNAWAY;
			update_chase.Stop();
			scatter_timer.Stop();
			body.runningAway();
			eyes.hideEyes();
		}

		Vector2 empty_cell_pos = tile_map.getRandomEmptyCell();
		GD.Print(empty_cell_pos);
		navigation_agent.TargetPosition = empty_cell_pos;
	}

	private void startBlinking() {
		is_blinking = true;
		body.blinking();
	}

	private void OnRunAwayTimeout() {
		is_blinking = false;
		runaway_timer.Stop();
		body.normal();
		eyes.showEyes();
		startChase();
	}
}
