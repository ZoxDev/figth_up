using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerController2D : Component, Component.INetworkSpawn, Component.ICollisionListener
{

	[Property] float Speed { get; set; } = 200f;
	[Property] public float GroundFriction { get; set; } = 4.0f;
	[Property] public float AirFriction { get; set; } = 0.5f;
	[Property] public float JumpForce { get; set; } = 1250;
	[Property] CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public Rigidbody Rigidbody;

	[RequireComponent] CharacterController CharacterController { get; set; }

	public const float EYE_POSITION_Z = 60f;

	private GameObject _body { get; set; }

	public static PlayerController2D LocalPlayer
	{
		get
		{
			if ( !_local.IsValid() )
			{
				_local = Game.ActiveScene.GetAllComponents<PlayerController2D>().FirstOrDefault( x => x.Network.IsOwner );
			}
			return _local;
		}
	}
	private static PlayerController2D _local = null;

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

		if ( Input.Pressed( "Backward" ) && !CharacterController.IsOnGround && !isFastFalling )
		{
			FastFall();
		}
		if ( isFastFalling && CharacterController.IsOnGround )
		{
			AnimationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.None;
			isFastFalling = false;
		}
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
			CharacterController.Velocity += gravity * Time.Delta * 7.5f;
			CharacterController.Accelerate( wishVelocity );
			CharacterController.ApplyFriction( AirFriction );
		}
		// lock x axis movement
		GameObject.WorldPosition = GameObject.WorldPosition.WithX( 0 );

		CharacterController.UseCollisionRules = true;
		CharacterController.Move();

		if ( !isFastFalling )
		{
			AnimateMove( wishVelocity );
		}

		var mousePosition = PlayerUtils.GetMousePosition();
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

	void LookAt( Vector2 mousePosition )
	{
		Vector3 directionX = new Vector3( 0, mousePosition.Normal.x, 0 );

		if ( mousePosition.x == 0 ) return;

		var target = Rotation.LookAt( directionX );
		_body.WorldRotation = Rotation.Slerp( _body.WorldRotation, target, Time.Delta * 15f );
		AnimationHelper.WithLook( new Vector3( 0, -mousePosition.x, -mousePosition.y - EYE_POSITION_Z ) );
	}

	void Jump()
	{
		CharacterController.Punch( Vector3.Up * JumpForce );
		AnimationHelper.TriggerJump();
	}

	bool isFastFalling { get; set; } = false;
	void FastFall()
	{
		Vector3 traceFrom = GameObject.WorldPosition;
		Vector3 traceTo = traceFrom + Vector3.Down * 200;

		DebugOverlay.Line( new Line( traceFrom, traceTo ), Color.Blue, 5f );
		SceneTraceResult trace = Scene.Trace.Ray( traceFrom, traceTo )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( ["player"] )
			.HitTriggers()
			.Run();

		if ( trace.Hit ) return;

		isFastFalling = true;
		CharacterController.Punch( Vector3.Down * 1000 );

		AnimationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.Roll;
		AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
	}
}
