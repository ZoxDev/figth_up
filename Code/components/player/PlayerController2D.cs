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

	public bool hasDoubleJunp;
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

		if ( Input.Pressed( "Jump" ) && (CharacterController.IsOnGround || hasDoubleJunp) )
		{
			Jump();
		}

		if ( Input.Pressed( "Backward" ) && !CharacterController.IsOnGround && !isFastFalling )
		{
			FastFall();
		}

		if ( Input.Pressed( "Run" ) && canDash )
		{
			Dash();
		}
		if ( isDashing )
		{
			dashTimer += Time.Delta;
			float t = dashTimer / dashTime;
			Log.Info( t );
			t = t * t * (2f - 1f * t);
			GameObject.WorldPosition = Vector3.Lerp(
				dashStart,
				dashTarget,
				t
			);

			if ( t >= 1f )
			{
				isDashing = false;
				GameObject.WorldPosition = dashTarget;
			}
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
		if ( hasDoubleJunp ) CharacterController.Velocity = new Vector3( CharacterController.Velocity.x, CharacterController.Velocity.y, 0 );
		CharacterController.Punch( Vector3.Up * JumpForce );

		AnimationHelper.TriggerJump();
		hasDoubleJunp = !hasDoubleJunp;
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

		Log.Info( trace.Hit );

		if ( trace.Hit ) return;

		isFastFalling = true;
		CharacterController.Punch( Vector3.Down * 1000 );

		AnimationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.Roll;
		AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
	}

	TimeUntil canDash;
	Vector3 dashStart;
	Vector3 dashTarget;
	bool isDashing;
	float dashTime = 0.1f;
	float dashTimer;
	void Dash()
	{
		canDash = 1f;
		if ( isDashing ) return;

		Vector3 dir = -Input.AnalogMove.Normal;

		if ( dir.Length == 0 )
			return;

		dashStart = GameObject.WorldPosition;
		dashTarget = dashStart + dir * 150;

		dashTimer = 0f;
		isDashing = true;
	}

}
