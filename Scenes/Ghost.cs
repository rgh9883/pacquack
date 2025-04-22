using Godot;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

public partial class Ghost : Area2D {
	[Export] movement_targets movement_targets;
	[Export] TileMap tile_map;
	[Export] public Color color;
	[Export] Node2D chase_target;
	[Export] PointsManager pts_manager;
	[Export] bool start_at_home;
	[Export] Timer start_timer;
	[Export] Marker2D start_pos;
	[Signal] public delegate void DirectionChangeEventHandler(string cur_direction);

	enum GhostState {
		SCATTER,
		CHASE,
		RUNAWAY,
		EATEN,
		STARTING
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
	private bool no_moving = true;
	private int cur_scatter_index = 0;
	private int cur_home_index = 0;
	private int speed;

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

		speed = Global.Instance.GhostSpeed;
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
		int cur_speed = cur_state == GhostState.EATEN ? (2 * speed) : speed;
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
		no_moving = false;
		navigation_agent.SetNavigationMap(tile_map.GetNavigationMap(0));
		NavigationServer2D.AgentSetMap(navigation_agent.GetRid(), tile_map.GetNavigationMap(0));
		if(start_at_home) {
			waitToStart();
		} else {
			scatter();
		}
	}

	private void waitToStart() {
		cur_state = GhostState.STARTING;
		start_timer.Start();
		navigation_agent.TargetPosition = movement_targets.at_home_targets[cur_home_index].Position;
	}

	private void scatter() {
		if(scatter_timer.IsStopped()) {
			scatter_timer.Start();
		}
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
		} else if(cur_state == GhostState.STARTING) {
			moveToNextHome();
		}
		
	}

	private void moveToNextHome() {
		cur_home_index = cur_home_index == 0 ? 1 : 0;
		navigation_agent.TargetPosition = movement_targets.at_home_targets[cur_home_index].Position;
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

	public void bigPelletEaten() {
		if(cur_state == GhostState.EATEN || cur_state == GhostState.STARTING) {
			return;
		}
		cur_state = GhostState.RUNAWAY;
		update_chase.Stop();
		scatter_timer.Stop();
		runaway_timer.Start();
		body.runningAway();
		eyes.hideEyes();
	}

	private void runAway() {
		Vector2 empty_cell_pos = tile_map.getRandomEmptyCell();
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

	private void OnBodyEntered(Node body) {
		if(body is Pacman) {
			if(cur_state == GhostState.RUNAWAY) {
				getEaten();
			} else if(cur_state == GhostState.SCATTER || cur_state == GhostState.CHASE) {
				SetCollisionMaskValue(1, false);
				update_chase.Stop();
				scatter();
				((Pacman)body).die();
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
	
	public void reset() {
		SetCollisionMaskValue(1, true);
		scatter_timer.Stop();
		update_chase.Stop();
		runaway_timer.Stop();
		body.normal();
		body.Show();
		eyes.showEyes();
		cur_state = GhostState.STARTING;
		Position = start_pos.Position;
		no_moving = true;
	}

	private void OnStartTimer() {
		CallDeferred("setup");
	}
}
