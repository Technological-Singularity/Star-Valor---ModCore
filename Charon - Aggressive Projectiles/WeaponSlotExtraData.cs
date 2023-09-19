using System.Collections.Generic;
using UnityEngine;

namespace Charon.StarValor.AggressiveProjectiles {
    public class WeaponSlotExtraData : MonoBehaviour {
        List<TurretInfo> turretInfos = new List<TurretInfo>();

        public TurretInfo this[int idx] => turretInfos[idx];

        public void Initialize(SpaceShip ss) {
            turretInfos.Clear();
            for (int i = 0; i < ss.weaponSlots.childCount; ++i) {
                var turret = ss.weaponSlots.GetChild(i).GetComponent<WeaponTurret>();
                if (turret == null)
                    turretInfos.Add(null);
                else
                    turretInfos.Add(new TurretInfo(ss, turret, i));
            }

        }
        public void Refresh() {
            foreach (var o in turretInfos)
                o?.Refresh();
        }
    }
}

//The following functions should be updated to have altered range based on the velocity of the target:
//WeaponTurret.CanFireAt
//AIControl.FireAllWeapons

