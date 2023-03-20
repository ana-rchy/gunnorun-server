using Godot;
using System;

public class Shotgun : Weapon {
    public Shotgun() {
        Knockback = 2000f;
        Refire = 1f;

        Damage = 25;
        Range = 500f;

        BaseAmmo = Ammo = null;
        Reload = -1f;
    }
}
