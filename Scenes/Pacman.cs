using Godot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.XPath;

public partial class Pacman : CharacterBody2D {
    [Export] int speed = 300;
    [Export] Marker2D start_pos;
    [Signal] public delegate void DeathBeginEventHandler();
    [Signal] public delegate void DeathEndEventHandler();
    Vector2 movement_direction = Vector2.Zero;
    Vector2 next_movement_direction = Vector2.Zero;
    PhysicsShapeQueryParameters2D shape_query = new PhysicsShapeQueryParameters2D();

    private Sprite2D direction_pointer;
    private CollisionShape2D collisionShape2D;
    private AnimationPlayer animation_player;
    public override void _Ready() {
        direction_pointer = GetNode<Sprite2D>("DirectionPointer");
        collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
        animation_player = GetNode<AnimationPlayer>("AnimationPlayer");
        animation_player.Play("default");
        shape_query.Shape = collisionShape2D.Shape;
        shape_query.CollisionMask = 2;
    }

    public override void _Process(double delta) {
        getInput();

        if(movement_direction == Vector2.Zero){
            movement_direction = next_movement_direction;
            turnSprite();
        }
        if(canMoveInDirection(next_movement_direction, delta)){
            turnSprite();
            movement_direction = next_movement_direction;
        }

        Velocity = movement_direction * speed;
        MoveAndSlide();
    }

    public void getInput(){
        if (Input.IsActionPressed("left")) {
            next_movement_direction = Vector2.Left;
        }
        else if (Input.IsActionPressed("right")){
            next_movement_direction = Vector2.Right;
        }
        else if (Input.IsActionPressed("down")){
            next_movement_direction = Vector2.Down;
        }
        else if (Input.IsActionPressed("up")){
            next_movement_direction = Vector2.Up;
        }
    }

    public void turnSprite(){
        if (next_movement_direction == Vector2.Left) {
            RotationDegrees = 180;
        }
        else if (next_movement_direction == Vector2.Right){
            RotationDegrees = 0;
        }
        else if (next_movement_direction == Vector2.Down){
            RotationDegrees = 90;
        }
        else if (next_movement_direction == Vector2.Up){
            RotationDegrees = 270;
        }
    }

    public bool canMoveInDirection(Vector2 dir, double delta) {
        shape_query.Transform = GlobalTransform.Translated(dir * (float)(speed * delta * 2));
        var result = GetWorld2D().DirectSpaceState.IntersectShape(shape_query);
        return result.Count() == 0;
    }

    public void die() {
        RotationDegrees = 0;
        SetProcess(false);
        EmitSignal(nameof(DeathBegin));
        animation_player.Play("death");
    }

    private void OnAnimFinished(StringName anim_name) {
        if(anim_name == "death") {
            EmitSignal(nameof(DeathEnd));
            resetPlayer();
        }
    }

    private void resetPlayer() {
        SetProcess(true);
        animation_player.Play("default");
        next_movement_direction = Vector2.Zero;
        movement_direction = Vector2.Zero;
        Velocity = Vector2.Zero;
        Position = start_pos.Position;
    }
}

