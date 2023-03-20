using Godot;
using System;

public class RPG : Weapon {
    public RPG() {
        Knockback = 6000f;
        Refire = 0.5f;

        Damage = 80;
        Range = 3000f;
        
        BaseAmmo = Ammo = 1;
        Reload = 3.75f;
    }
}