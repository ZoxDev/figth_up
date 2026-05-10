using System;
using System.Diagnostics;

public sealed class PlatformManager : Component
{
	[Property] bool ShowDebug { get; set; } = false;
	[Property] GameObject PlatformPrefab { get; set; }
	[Property] public Vector3 BoxScale { get; set; } = new Vector3( 1, 10, 5 );
	[Property] int MaxSumToRange { get; set; } = 5;
	[Property] float DistanceToNextPlatform { get; set; } = 450;
	private TimeSince _gameStart { get; set; }
	private TimeUntil _nextPlatformSpawn { get; set; }
	private WorldPanel _worldPanel { get; set; }

	const float PixelsPerUnit = 100f;
	protected override void OnAwake()
	{
		base.OnAwake();
		_worldPanel = GetComponentInChildren<WorldPanel>();
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
		if ( ShowDebug )
		{
			_worldPanel.AddComponent<PlatformManagerUi>();
		}

		GameObject.WorldScale = BoxScale;
	}

	private void MoveBox()
	{
		float sumRange = _gameStart.Relative > MaxSumToRange ? MaxSumToRange : _gameStart.Relative;
		Vector3 target = new( WorldPosition.x, WorldPosition.y, WorldPosition.z + sumRange / 2 );
		WorldPosition = target;
	}

	record XCord( int Left, int Right );
	record YCord( int Top, int Bottom );
	private Vector3 _lastPlatform { get; set; }
	const int MAX_ATTEMPTS = 20;
	private void SpawnPlatform()
	{
		_nextPlatformSpawn = 1;

		float halfWidth = BoxScale.y * PixelsPerUnit * 0.5f;
		float halfHeight = BoxScale.z * PixelsPerUnit * 0.5f;

		int left = (int)Math.Round( GameObject.WorldPosition.y - halfWidth );
		int right = (int)Math.Round( GameObject.WorldPosition.y + halfWidth );
		int top = (int)Math.Round( GameObject.WorldPosition.z + halfHeight );
		int bottom = (int)Math.Round( GameObject.WorldPosition.z - halfHeight );

		XCord xCord = new( left, right );
		YCord yCord = new( top, bottom > (int)Math.Round( _lastPlatform.z ) ? bottom : (int)Math.Round( _lastPlatform.z ) );

		Random random = new();

		for ( int attempt = 0; attempt < MAX_ATTEMPTS; attempt++ )
		{
			int randomX = random.Next( xCord.Left, xCord.Right );
			int randomY = random.Next( yCord.Bottom, yCord.Top );

			Vector3 horizontalTraceFrom = new( 0, randomX - 250, randomY );
			Vector3 horizontalTraceTo = new( 0, randomX + 250, randomY );

			SceneTraceResult horizontalTrace = Scene.Trace
				.Ray( horizontalTraceFrom, horizontalTraceTo )
				.WithTag( "platform" )
				.HitTriggers()
				.Run();

			Vector3 verticalTraceFrom = new( 0, randomX, randomY + 250 );
			Vector3 verticalTraceTo = new( 0, randomX, randomY - 250 );

			SceneTraceResult verticalTrace = Scene.Trace
				.Ray( verticalTraceFrom, verticalTraceTo )
				.WithTag( "platform" )
				.HitTriggers()
				.Run();

			Vector3 randomPos = new( 0, randomX, randomY );
			float distanceWithLastPlatform = Vector3.DistanceBetween( _lastPlatform, randomPos );

			if ( ShowDebug )
			{
				Log.Info( $"distanceWithLastPlatform: {distanceWithLastPlatform}, {distanceWithLastPlatform >= DistanceToNextPlatform}" );
				DebugOverlay.Trace( horizontalTrace, 1 );
				DebugOverlay.Trace( verticalTrace, 1 );
			}

			if ( horizontalTrace.Hit || verticalTrace.Hit || (distanceWithLastPlatform >= DistanceToNextPlatform) ) continue;

			_lastPlatform = randomPos;
			PlatformPrefab.Clone( randomPos );

			return;
		}
	}
}
