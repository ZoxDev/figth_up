using Sandbox;

public sealed class PlayerCamera : Component
{
	[Property] GameObject PlayerBody { get; set; }
	[Property] GameObject CameraGameObject { get; set; }
	protected override void OnStart()
	{
		base.OnStart();
		CameraGameObject.WorldRotation = new Rotation( 0f, 0f, 180f, 0f );
	}

	protected override void OnUpdate()
	{
		CameraGameObject.WorldPosition = PlayerBody.WorldPosition - new Vector3( -1250f, 0f, -65f );
	}
}
