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

	void ITriggerListener.OnTriggerExit( GameObject other )
	{
		if ( !_playerOnPlatform ) return;
		Health--;
		_playerOnPlatform = false;
	}

	private bool _playerOnPlatform;
	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		float heightDiff = collision.Other.GameObject.WorldPosition.z - GameObject.WorldPosition.z;
		if ( heightDiff >= 0 && heightDiff <= 15 ) _playerOnPlatform = true;
	}
}
