using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerController2D : Component, Component.INetworkSpawn
{

	[Property] float Speed { get; set; } = 200f;
	[Property] public float GroundFriction { get; set; } = 4.0f;
	[Property] public float AirFriction { get; set; } = 0.5f;
	[Property] public float JumpForce { get; set; } = 400f;

	[Property] public Rigidbody Rigidbody;

	[RequireComponent] CharacterController CharacterController { get; set; }
	[RequireComponent] CitizenAnimationHelper AnimationHelper { get; set; }

	private GameObject _body { get; set; }

	void INetworkSpawn.OnNetworkSpawn( Connection owner )
	{
		var dresser = GameObject.GetComponent<Dresser>();
		dresser.Apply();
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_body = GameObject.Children.Find( go => go.Name == "Body" );
	}
	protected override void OnStart()
	{
		base.OnStart();
		Mouse.Visibility = MouseVisibility.Visible;
	}

	protected override void OnFixedUpdate()
	{
		var wishVelocity = getWishVelocity();
		Move( wishVelocity );
		if ( Input.Pressed( "Jump" ) && CharacterController.IsOnGround )
		{
			Jump();
		}

		getMousePosition();
	}

	Vector3 getWishVelocity()
	{
		Vector3 analogMove = Input.AnalogMove;
		Vector3 wishVelocity = -analogMove.WithX( 0 ).WithZ( 0 ) * Speed;


		return wishVelocity;
	}

	void Move( Vector3 wishVelocity )
	{
		var gravity = Scene.PhysicsWorld.Gravity;
		Rigidbody.Sleeping = false;
		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithX( 0 ).WithZ( 0 );
			CharacterController.Accelerate( wishVelocity );
			CharacterController.ApplyFriction( GroundFriction );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 2;
			CharacterController.Accelerate( wishVelocity );
			CharacterController.ApplyFriction( AirFriction );
		}
		CharacterController.UseCollisionRules = true;
		CharacterController.Move();

		AnimateMove( wishVelocity );

		var mousePosition = getMousePosition();

		Log.Info( mousePosition );
		LookAt( mousePosition );
	}

	void AnimateMove( Vector3 wishVelocity )
	{
		if ( !AnimationHelper.IsValid() ) return;

		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.WithVelocity( wishVelocity );

		AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
		AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
	}

	void Jump()
	{
		CharacterController.Punch( Vector3.Up * JumpForce );
		AnimateJump();
	}

	void AnimateJump()
	{
		AnimationHelper.TriggerJump();
	}

	void LookAt( Vector2 mousePosition )
	{
		Vector3 directionX = new Vector3( 0, mousePosition.Normal.x, 0 );

		if ( mousePosition.x == 0 ) return;

		_body.WorldRotation = Rotation.LookAt( directionX ).Angles();
		AnimationHelper.WithLook( new Vector3( 0, 0, -mousePosition.y - EYE_POSITION_Z ) );
	}

	const float EYE_POSITION_Z = 60f;
	Vector2 getMousePosition()
	{
		var screenCenter = new Vector2( Screen.Size.x, Screen.Size.y ) * 0.5f;
		var mousePosition = new Vector2( Mouse.Position.x, Mouse.Position.y );

		return new Vector2( mousePosition - screenCenter - new Vector2( 0, EYE_POSITION_Z ) );
	}
}
