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
	[Property] public float JumpForce { get; set; } = 1000f;

	[Property] public Rigidbody Rigidbody;

	[RequireComponent] CharacterController CharacterController { get; set; }
	[RequireComponent] CitizenAnimationHelper AnimationHelper { get; set; }

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

		if ( Input.Pressed( "Backward" ) && !CharacterController.IsOnGround )
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
			CharacterController.Velocity += gravity * Time.Delta * 2;
			CharacterController.Accelerate( wishVelocity );
			CharacterController.ApplyFriction( AirFriction );
		}
		CharacterController.UseCollisionRules = true;
		CharacterController.Move();

		if ( !isFastFalling )
		{
			AnimateMove( wishVelocity );
		}


		var mousePosition = _getMousePosition();
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
		_body.WorldRotation = Rotation.Lerp( _body.WorldRotation, target, Time.Delta * 15f );
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
		float characterFallingVelocity = CharacterController.Velocity.z;
		if ( characterFallingVelocity <= 100 && characterFallingVelocity >= -100 )
		{
			isFastFalling = true;
			CharacterController.Punch( Vector3.Down * 1000 );

			AnimationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.Roll;
			AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		}
	}

	const float EYE_POSITION_Z = 60f;
	private Vector2 _getMousePosition()
	{
		var screenCenter = new Vector2( Screen.Size.x, Screen.Size.y ) * 0.5f;
		var mousePosition = new Vector2( Mouse.Position.x, Mouse.Position.y );

		return new Vector2( mousePosition - screenCenter - new Vector2( 0, EYE_POSITION_Z ) );
	}
}
