using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerDummy : Component, Component.IDamageable
{
	[Property] float Health { get; set; } = 100;
	[RequireComponent] CitizenAnimationHelper AnimationHelper { get; set; }

	void IDamageable.OnDamage( in DamageInfo damage )
	{
		Health -= damage.Damage;
		AnimationHelper.ProceduralHitReaction( damage );

		Log.Info( Health );
	}
}
