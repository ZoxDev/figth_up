using Sandbox;

public sealed class PlatformCollider : Component, Component.ITriggerListener
{
	[Property] BoxCollider platformCollider { get; set; }
	[Property] int health { get; set; } = 3;

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		GameObject collidedGameObject = other.GameObject;
		if ( collidedGameObject.IsProxy || !collidedGameObject.Tags.Has( "player" ) ) return;

		CharacterController characterController = collidedGameObject.GetComponentInParent<CharacterController>();
		if ( !characterController.IsValid ) return;

		float characterUpwardVelocity = characterController.Velocity.z;
		if ( characterUpwardVelocity > 0 )
		{
			platformCollider.IsTrigger = true;
		}
		else
		{
			health--;
			platformCollider.IsTrigger = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( health == 0 ) GameObject.Destroy();
	}
}
