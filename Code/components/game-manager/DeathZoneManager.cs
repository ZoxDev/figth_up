using System.Dynamic;

using Sandbox;

public sealed class DeathZoneManager : Component, Component.ITriggerListener
{
	[Property] Vector3 CollidersScale { get; set; }
	[Property] Model BoxModel { get; set; }
	private PlatformManager _platformManager;
	private TimeSince _gameStart;
	private BoxCollider _leftSide;
	private BoxCollider _rightSide;
	private BoxCollider _bottomSide;
	private PlayerCombat _playerCombat;
	const float PixelsPerUnit = 100f;
	private GameObject _leftSideGo;
	private GameObject _rightSideGo;
	private GameObject _bottomSideGo;

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

		SetupColliders();
		SetupColliderModel();
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
		_leftSide = GameObject.AddComponent<BoxCollider>();
		_rightSide = GameObject.AddComponent<BoxCollider>();
		_bottomSide = GameObject.AddComponent<BoxCollider>();

		_leftSide.IsTrigger = true;
		_rightSide.IsTrigger = true;
		_bottomSide.IsTrigger = true;

		_leftSide.Scale = CollidersScale;
		_rightSide.Scale = CollidersScale;
		_bottomSide.Scale = new Vector3( CollidersScale.x, _platformManager.BoxScale.y * PixelsPerUnit, CollidersScale.z );
	}

	private void SetupColliderModel()
	{
		_leftSideGo = new()
		{
			Name = "death_zone_left_side",
			Parent = GameObject
		};
		_rightSideGo = new()
		{
			Name = "death_zone_right_side",
			Parent = GameObject
		};
		_bottomSideGo = new()
		{
			Name = "death_zone_bottom_side",
			Parent = GameObject
		};

		float platformHalfWidth = _platformManager.BoxScale.y * PixelsPerUnit * 0.5f;
		float platformHalfHeight = _platformManager.BoxScale.z * PixelsPerUnit * 0.5f;
		float colliderHalfWidth = CollidersScale.y * 0.5f;
		float colliderHalfHeight = CollidersScale.z * 0.5f;

		_leftSideGo.WorldPosition = new Vector3( 0, -(platformHalfWidth + colliderHalfWidth), GameObject.WorldPosition.z );
		_rightSideGo.WorldPosition = new Vector3( 0, platformHalfWidth + colliderHalfWidth, GameObject.WorldPosition.z );
		_bottomSideGo.WorldPosition = new Vector3( 0, 0, GameObject.WorldPosition.z - (platformHalfHeight + colliderHalfHeight) );

		_leftSideGo.LocalScale = CollidersScale;
		_rightSideGo.LocalScale = CollidersScale;
		_bottomSideGo.LocalScale = new Vector3( CollidersScale.x, _platformManager.BoxScale.y * PixelsPerUnit, CollidersScale.z );

		ModelRenderer leftSideModel = _leftSideGo.AddComponent<ModelRenderer>();
		leftSideModel.Model = BoxModel;
		leftSideModel.LocalScale = new Vector3( 1, _leftSideGo.LocalScale.y / (PixelsPerUnit * 0.5f), _leftSideGo.LocalScale.z / (PixelsPerUnit * 0.5f) );
		leftSideModel.Tint = new Color( 1f, 0f, 0f, 0.22f );

		ModelRenderer rightSideModel = _rightSideGo.AddComponent<ModelRenderer>();
		rightSideModel.Model = BoxModel;
		rightSideModel.LocalScale = new Vector3( 1, _rightSideGo.LocalScale.y / (PixelsPerUnit * 0.5f), _rightSideGo.LocalScale.z / (PixelsPerUnit * 0.5f) );
		rightSideModel.Tint = new Color( 1f, 0f, 0f, 0.22f );

		ModelRenderer bottomSideModel = _bottomSideGo.AddComponent<ModelRenderer>();
		bottomSideModel.Model = BoxModel;
		bottomSideModel.LocalScale = new Vector3( 1, _bottomSideGo.LocalScale.y / (PixelsPerUnit * 0.5f), _bottomSideGo.LocalScale.z / (PixelsPerUnit * 0.5f) );
		bottomSideModel.Tint = new Color( 1f, 0f, 0f, 0.22f );

	}
}
