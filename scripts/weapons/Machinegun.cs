using Godot;
using System;

public class Machinegun : Weapon {
    public Machinegun() {
        Knockback = 650f;
        Refire = 0.05f;

        Damage = 1;
        Range = 3000f;
        
        BaseAmmo = Ammo = 100;
        Reload = 1.5f;
    }
}
