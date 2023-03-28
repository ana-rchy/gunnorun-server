using Godot;
using System;
using static Godot.GD;

public partial class Player : RigidBody2D {
    [Export] float MAXIMUM_VELOCITY = 4000f;

    Timer ActionTimer;

    public Weapon[] Weapons;
    public Weapon CurrentWeapon;
    float MomentumMultiplier;

    public override void _Ready() {
        ActionTimer = GetNode<Timer>("ActionTimer");

        Weapons = new Weapon[] {new Shotgun(), new Machinegun(), new RPG()};
        CurrentWeapon = Weapons[0];
    }

    //---------------------------------------------------------------------------------//
    #region | rpc-related

    public void Shoot(Vector2 velocityDirection) {
        var ammoNotEmpty = CurrentWeapon.Ammo > 0 || CurrentWeapon.Ammo == null;

        if (ammoNotEmpty && ActionTimer.IsStopped()) {
            Print("shoot");
            SetVelocity(velocityDirection);
            ActionTimer.Start(CurrentWeapon.Refire);
        }
    }

    public void Reload() {
        ActionTimer.Start(CurrentWeapon.Reload);
        CurrentWeapon.Ammo = CurrentWeapon.BaseAmmo;
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | loops

    public override void _IntegrateForces(PhysicsDirectBodyState2D state) {
        state.LinearVelocity = ClampVelocity();
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | other funcs

    void SetVelocity(Vector2 velocityDirection) {
        //LinearVelocity = (LinearVelocity * GetMomentumMultiplier(LinearVelocity, velocityDirection)) + velocityDirection.Normalized() * CurrentWeapon.Knockback;
        LinearVelocity = velocityDirection * CurrentWeapon.Knockback;
    }

    Vector2 ClampVelocity() {
        if (LinearVelocity.DistanceTo(new Vector2(0, 0)) > MAXIMUM_VELOCITY) {
            return LinearVelocity.Normalized() * MAXIMUM_VELOCITY;
        }
        return LinearVelocity;
    }

    float GetMomentumMultiplier(Vector2 currentVelocity, Vector2 mousePosToPlayerPos) {
        float angleDelta = currentVelocity.AngleTo(mousePosToPlayerPos);
        if (Mathf.RadToDeg(angleDelta) <= 45) // if less than 45 degrees change, keep all momentum
            return 1f;
        
        angleDelta -= MathF.Round(MathF.PI / 4, 4);

        return MathF.Round((MathF.Cos((4/3) * angleDelta) + 1) / 2, 4); // scale the momentum over a range of 135*
    }

    #endregion
}