using Sandbox;
using Sandbox.Citizen;

public sealed class DebugDummy : Component
{

	[RequireComponent] CharacterController CharacterController { get; set; }
	[RequireComponent] CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public Rigidbody Rigidbody;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		var gravity = Scene.PhysicsWorld.Gravity;
		Rigidbody.Sleeping = false;
		if ( !CharacterController.IsOnGround ) { CharacterController.Velocity += gravity * Time.Delta * 2; }
	}
}
