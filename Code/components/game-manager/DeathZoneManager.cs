using System.Dynamic;

using Sandbox;

public sealed class DeathZoneManager : Component, Component.ITriggerListener
{
	[Property] Vector3 CollidersScale { get; set; }
	private PlatformManager _platformManager;
	private TimeSince _gameStart;
	private BoxCollider _leftSide;
	private BoxCollider _rightSide;
	private BoxCollider _bottomSide;
	private PlayerCombat _playerCombat;
	private float _colliderWidth;
	private float _colliderHeight;
	const float PixelsPerUnit = 100f;

	protected override void OnStart()
	{
		base.OnStart();
		_platformManager = Scene.Directory.FindByName( "platform_manager" ).First().GetComponent<PlatformManager>();
		if ( !_platformManager.IsValid() )
		{
			Log.Error( "no platform_manager found" );
			return;
		}

		_gameStart = 0;

		_leftSide = GameObject.AddComponent<BoxCollider>();
		_rightSide = GameObject.AddComponent<BoxCollider>();
		_bottomSide = GameObject.AddComponent<BoxCollider>();

		_colliderWidth = CollidersScale.y;
		_colliderHeight = CollidersScale.z;
		SetupColliders();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		GameObject.WorldPosition = _platformManager.GameObject.WorldPosition;
	}

	TimeUntil nextDeathZoneDamages;
	protected override void OnUpdate()
	{
		base.OnUpdate();
		CenterColliders();

		if ( nextDeathZoneDamages && isInDeathzone ) DeathZoneDamage();
	}

	TimeSince exitDeathZone;
	bool isInDeathzone;
	void ITriggerListener.OnTriggerEnter( GameObject other )
	{
		if ( !other.Tags.Has( "player" ) ) return;

		_playerCombat = other.GetComponent<PlayerCombat>();
		if ( !_playerCombat.IsValid ) return;

		isInDeathzone = true;
		nextDeathZoneDamages = 1.5;
	}

	void ITriggerListener.OnTriggerExit( GameObject other )
	{
		if ( !other.Tags.Has( "player" ) ) return;
		isInDeathzone = false;
		exitDeathZone = 0;
	}

	private void DeathZoneDamage()
	{
		_playerCombat.Health -= 10;
		nextDeathZoneDamages = 1.5;
	}

	private void CenterColliders()
	{
		float platformHalfWidth = _platformManager.BoxScale.y * PixelsPerUnit * 0.5f;
		float platformHalfHeight = _platformManager.BoxScale.z * PixelsPerUnit * 0.5f;

		float colliderHalfWidth = CollidersScale.y * 0.5f;
		float colliderHalfHeight = CollidersScale.z * 0.5f;

		_leftSide.Center = new Vector3( 0, -(platformHalfWidth + colliderHalfWidth), 0 );
		_rightSide.Center = new Vector3( 0, platformHalfWidth + colliderHalfWidth, 0 );
		_bottomSide.Center = new Vector3( 0, 0, -(platformHalfHeight + colliderHalfHeight) );
	}

	private void SetupColliders()
	{
		_leftSide.IsTrigger = true;
		_rightSide.IsTrigger = true;
		_bottomSide.IsTrigger = true;

		_leftSide.Scale = CollidersScale;
		_rightSide.Scale = CollidersScale;
		_bottomSide.Scale = new Vector3( CollidersScale.x, _platformManager.BoxScale.y * PixelsPerUnit, CollidersScale.z );
	}
}
