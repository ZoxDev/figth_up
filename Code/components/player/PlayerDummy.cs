using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerDummy : Component, Component.IDamageable
{
	[Property] float health { get; set; } = 100;
	[RequireComponent] CitizenAnimationHelper AnimationHelper { get; set; }

	void IDamageable.OnDamage( in DamageInfo damage )
	{
		health -= damage.Damage;
		AnimationHelper.ProceduralHitReaction( damage );

		Log.Info( health );
	}
}
