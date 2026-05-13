using Sandbox;

public sealed class PlatformCollider : Component, Component.ITriggerListener, Component.ICollisionListener
{
	[Property] BoxCollider PlatformColliderBox { get; set; }
	[Property] BoxCollider GateColliderBox { get; set; }
	[Property] int Health { get; set; } = 4;
	private ModelRenderer _modelRenderer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_modelRenderer = GameObject.GetComponent<ModelRenderer>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( Health == 1 ) _modelRenderer.Tint = "#ff0000";
		if ( Health == 0 ) GameObject.Destroy();
		CheckPlatformTrigger();
	}

	void ITriggerListener.OnTriggerExit( GameObject other )
	{
		if ( !playerOnPlatform ) return;
		var playController = other.GetComponent<PlayerController2D>();
		playController.hasDoubleJunp = true;

		Health--;
		playerOnPlatform = false;
	}

	bool playerOnPlatform;
	float heightDiff;
	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		GameObject go = collision.Other.GameObject;
		if ( !go.Tags.Has( "player" ) || go.IsProxy ) return;

		CharacterController characterController = go.GetComponentInParent<CharacterController>();
		heightDiff = go.WorldPosition.z - GameObject.WorldPosition.z;

		if ( heightDiff >= 0 && heightDiff <= 15 ) playerOnPlatform = true;

		bool isPlayerStuck = IsPlayerStuck( characterController );
		if ( isPlayerStuck )
		{
			if ( heightDiff >= -70 && heightDiff <= 5 )
			{
				go.WorldPosition = new Vector3( go.WorldPosition.x, go.WorldPosition.y, GameObject.WorldPosition.z + 10f );
			}
		}
	}

	private void CheckPlatformTrigger()
	{
		foreach ( var collider in GateColliderBox.Touching )
		{
			if ( collider.GameObject.Tags.Has( "player" ) && !collider.IsProxy )
			{
				CharacterController characterController = collider.GetComponentInParent<CharacterController>();
				if ( !characterController.IsValid ) return;

				float characterUpwardVelocity = characterController.Velocity.z;
				if ( characterUpwardVelocity > 0 )
				{
					PlatformColliderBox.IsTrigger = true;
				}
				else
				{
					PlatformColliderBox.IsTrigger = false;
				}
			}
		}
	}

	Vector3 lastPlayerKnownPosition;
	private bool IsPlayerStuck( CharacterController characterController )
	{
		Vector3 playerPosition = characterController.GameObject.WorldPosition;
		if ( playerPosition == lastPlayerKnownPosition )
		{
			lastPlayerKnownPosition = characterController.GameObject.WorldPosition;
			return true;
		}

		lastPlayerKnownPosition = characterController.GameObject.WorldPosition;
		return false;
	}
}
