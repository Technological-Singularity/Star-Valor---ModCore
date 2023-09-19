namespace Charon.StarValor.AggressiveProjectiles {
    /// <summary>
    /// Weapon category, ordered from low to high by priority (highest priority = first)
    /// </summary>
    public enum WeaponCategory : int {
        PointDefense = 0,
        RepairBeam = 1,
        Normal = 2,
        Beam = 3,
        Invalid = int.MaxValue,
    }
}

//The following functions should be updated to have altered range based on the velocity of the target:
//WeaponTurret.CanFireAt
//AIControl.FireAllWeapons

