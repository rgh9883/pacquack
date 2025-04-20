using Godot;
using System;
using System.Threading.Tasks;

public partial class Ghost : Area2D {

	int cur_scatter_index = 0;

	[Export] int speed = 120;
	[Export] int eaten_speed = 240;
	[Export] movement_targets movement_targets;
	[Export] TileMap tile_map;
	[Export] public Color color;
	[Export] Node2D chase_target;
	[Export] PointsManager pts_manager;
	[Signal] public delegate void DirectionChangeEventHandler(string cur_direction);

	enum GhostState {
		SCATTER,
		CHASE,
		RUNAWAY,
		EATEN
	}

	private NavigationAgent2D navigation_agent;
	private string direction = null;
	private GhostState cur_state;
	private Timer scatter_timer;
	private Timer update_chase;
	private Timer runaway_timer;
	private GhostBodySprite body;
	private GhostEyesSprite eyes;
	private Label points;
	private bool is_blinking = false;
	private bool no_moving = false;

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

		points = GetNode<Label>("PointsLabel");

		CallDeferred("setup");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if(!runaway_timer.IsStopped() && runaway_timer.TimeLeft < runaway_timer.WaitTime / 3 && !is_blinking) {
			startBlinking();
		}
		if(!no_moving) {
			moveGhost(navigation_agent.GetNextPathPosition(), delta);
		}
		
	}

	private void moveGhost(Vector2 next_pos, double delta) {
		Vector2 cur_agent_pos = GlobalPosition;
		int cur_speed = cur_state == GhostState.EATEN ? eaten_speed : speed;
		Vector2 new_velo = (next_pos - cur_agent_pos).Normalized() * cur_speed * (float) delta;

		Position += new_velo;

		calcDirection(new_velo);
	}

	private void calcDirection(Godot.Vector2 velocity) {
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
		} else if(cur_state == GhostState.EATEN) {
			body.normal();
			body.Show();
			startChase();
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
		if(cur_state == GhostState.EATEN) {
			return;
		}
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
		pts_manager.pts_per_ghost = 200;
		is_blinking = false;
		runaway_timer.Stop();
		body.normal();
		eyes.showEyes();
		startChase();
	}

	private void OnBodyEntered(Node body) {
		if(body is Pacman) {
			if(cur_state == GhostState.RUNAWAY) {
				getEaten();
			} else if(cur_state == GhostState.SCATTER || cur_state == GhostState.CHASE) {
				SetCollisionMaskValue(1, false);
				update_chase.Stop();
				((Pacman)body).die();
				scatter_timer.WaitTime = 600;
				scatter();
			}
		}

	}

	private async Task getEaten() {
		body.Hide();
		eyes.showEyes();
		points.Text = pts_manager.pts_per_ghost.ToString();
		points.Show();
		await pts_manager.OnGhostEaten();
		points.Hide();
		runaway_timer.Stop();
		cur_state = GhostState.EATEN;
		navigation_agent.TargetPosition = movement_targets.at_home_targets[0].Position;
	}
}
