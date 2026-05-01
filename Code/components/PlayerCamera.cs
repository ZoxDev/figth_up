using Sandbox;

public sealed class PlayerCamera : Component
{
	[Property] GameObject playerBody { get; set; }
	[Property] GameObject cameraGameObject { get; set; }
	protected override void OnStart()
	{
		base.OnStart();
		cameraGameObject.WorldRotation = new Rotation( 0f, 0f, 180f, 0f );
	}

	protected override void OnUpdate()
	{
		cameraGameObject.WorldPosition = playerBody.WorldPosition - new Vector3( -800f, 0f, -65f );
	}
}
