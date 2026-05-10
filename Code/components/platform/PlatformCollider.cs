using Sandbox;

public sealed class PlatformCollider : Component, Component.ITriggerListener
{
	[Property] BoxCollider PlatformColliderBox { get; set; }
	[Property] int Health { get; set; } = 4;

	private ModelRenderer _modelRenderer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_modelRenderer = GameObject.GetComponent<ModelRenderer>();
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		GameObject collidedGameObject = other.GameObject;
		if ( collidedGameObject.IsProxy || !collidedGameObject.Tags.Has( "player" ) ) return;

		CharacterController characterController = collidedGameObject.GetComponentInParent<CharacterController>();
		if ( !characterController.IsValid ) return;

		float characterUpwardVelocity = characterController.Velocity.z;
		if ( characterUpwardVelocity > 0 )
		{
			PlatformColliderBox.IsTrigger = true;
		}
		else
		{
			Health--;
			PlatformColliderBox.IsTrigger = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( Health == 1 ) _modelRenderer.Tint = "#ff4747";
		if ( Health == 0 ) GameObject.Destroy();
	}
}
