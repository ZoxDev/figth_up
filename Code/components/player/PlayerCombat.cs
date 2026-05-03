using System;

using Sandbox.Citizen;

public sealed class PlayerCombat : Component, Component.IDamageable
{
	[Property] float health { get; set; } = 100;
	[Property] CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] CharacterController CharacterController { get; set; }

	TimeUntil nextAttack = 0;
	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( Input.Pressed( "Attack1" ) && nextAttack <= -0.5f )
		{
			Attack();
		}
	}

	void IDamageable.OnDamage( in DamageInfo damage )
	{
		health -= damage.Damage;

		Vector3 damagePosition = damage.Position;
		bool damageFromLeft = damagePosition.y < GameObject.WorldPosition.y;
		float heightDifference = GameObject.WorldPosition.z - damage.Attacker.WorldPosition.z;

		if ( heightDifference > 0 )
		{
			CharacterController.Punch( Vector3.Up * (10 * heightDifference) );
		}
		else
		{
			CharacterController.Punch( Vector3.Down * (10 * Math.Abs( heightDifference )) );
		}

		if ( damageFromLeft )
		{
			CharacterController.Punch( Vector3.Left * 400 );
		}
		else
		{
			CharacterController.Punch( Vector3.Right * 400 );
		}
	}

	void Attack()
	{
		nextAttack = 0;

		if ( !AnimationHelper.IsValid() ) return;
		if ( AnimationHelper.Target == null ) return;

		AnimationHelper.Target.Set( "b_attack", true );

		Vector2 mousePosition = PlayerUtils.GetMousePosition();

		Vector3 traceFrom = new Vector3( GameObject.WorldPosition.x, GameObject.WorldPosition.y, (GameObject.WorldPosition.z + PlayerController2D.EYE_POSITION_Z) );
		Vector3 traceTo = traceFrom + new Vector3( 0, mousePosition.x, (-mousePosition.y) - PlayerController2D.EYE_POSITION_Z ).Normal * 100;

		DebugOverlay.Line( new Line( traceFrom, traceTo ), Color.Red, 5f );
		SceneTraceResult trace = Scene.Trace.Ray( traceFrom, traceTo )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithTag( "player" )
			.Run();

		if ( trace.Hit )
		{
			IDamageable hitDamageable = trace.GameObject.GetComponentInParent<IDamageable>();

			DamageInfo damageInfo = new()
			{
				Damage = 10,
				Attacker = GameObject,
				Position = trace.HitPosition,
			};

			hitDamageable.OnDamage( damageInfo );
		}
	}
}
