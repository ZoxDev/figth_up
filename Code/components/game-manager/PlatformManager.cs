using System;
using System.Diagnostics;
using System.Dynamic;
using System.Numerics;
using System.Security.Cryptography;

using Sandbox;

public sealed class PlatformManager : Component
{
	[Property] GameObject PlatformPrefab { get; set; }
	[Property] Vector3 BoxScale { get; set; } = new Vector3( 1, 20, 10 );
	[Property] int MaxSumToRange { get; set; } = 5;
	[Property] int BoxTraceHalfSize = 125;

	private BoxCollider _boxCollider { get; set; }
	private ModelRenderer _boxModel { get; set; }

	private TimeSince _gameStart { get; set; }
	private TimeUntil _nextPlatformSpawn { get; set; }
	protected override void OnAwake()
	{
		base.OnAwake();
		_boxCollider = AddComponent<BoxCollider>();
		_boxModel = AddComponent<ModelRenderer>();
		_gameStart = 0;
		_nextPlatformSpawn = 0;

		SetupBox();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		MoveBox();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( _nextPlatformSpawn ) SpawnPlatform();
	}

	private void SetupBox()
	{
		WorldPosition = new Vector3( 0, 0, WorldPosition.z + (BoxScale.z * 25) );

		_boxCollider.IsTrigger = true;
		_boxCollider.Scale = BoxScale;

		_boxModel.Model = Model.Load( "models/dev/box.vmdl" );
		_boxModel.WorldScale = BoxScale;
		_boxModel.Tint = Color.Green.WithAlpha( 0.05f );
	}

	private void MoveBox()
	{
		float sumRange = _gameStart.Relative > MaxSumToRange ? MaxSumToRange : _gameStart.Relative;
		Vector3 target = new( WorldPosition.x, WorldPosition.y, WorldPosition.z + sumRange / 2 );

		WorldPosition = target;
	}

	record XCord( int Left, int Right );
	record YCord( int Top, int Bottom );
	private int _lastPlatformOnY { get; set; }
	private void SpawnPlatform()
	{
		_nextPlatformSpawn = 1;

		float xWidth = BoxScale.y;
		float yWidth = BoxScale.z;

		int left = (int)Math.Round( GameObject.WorldPosition.y - (xWidth * 25) );
		int right = (int)Math.Round( GameObject.WorldPosition.y + (xWidth * 25) );
		int top = (int)Math.Round( GameObject.WorldPosition.z + (yWidth * 25) );
		int bottom = (int)Math.Round( GameObject.WorldPosition.z - (yWidth * 25) );

		XCord xCord = new( left, right );
		YCord yCord = new( top, bottom > _lastPlatformOnY ? bottom : _lastPlatformOnY );

		Random random = new();
		int randomX = random.Next( xCord.Left, xCord.Right );
		int randomY = random.Next( yCord.Bottom, yCord.Top );

		Vector3 center = new( 0, randomX, randomY );
		Vector3 halfExtents = new( BoxTraceHalfSize, BoxTraceHalfSize * 5f, BoxTraceHalfSize );

		SceneTraceResult traceBox = Scene.Trace.Box( halfExtents, center, center ).WithTag( "platform" ).HitTriggers().Run();
		DebugOverlay.Box( BBox.FromPositionAndSize( center, halfExtents * 2 ), Color.Green, 1.0f );

		Log.Info( traceBox.Hit );
		if ( traceBox.Hit )
		{
			SpawnPlatform();
			return;
		}

		_lastPlatformOnY = randomY;
		PlatformPrefab.Clone( new Vector3( 0, randomX, randomY ) );
	}
}
