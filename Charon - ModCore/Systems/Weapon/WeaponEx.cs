namespace Charon.StarValor.ModCore {
    //to do: postfix projectile setup to grab weapon from ship, using same method as before
    //to do: postfix Fire and FireExtra to run setup methods instead of the above - Fire is straightforward; determine how to postfix IEnumerable effectively
    //to do: extend/replace Weapon in order to resolve the above in a normal way

    //WeaponCrafting.AddWeaponToShip is used to equip any weapon
    //PlayerControl.InstallWeapons() => for player ship, transDest == transform for PlayerControl component (== space ship)
    //resources => ObjManager.GetLaser() (etc), ObjManager.GetProj / ObjManager.GetAudio
    //objects are instantiated at WeaponCrafting.weaponObj static object, then parent set to relevant "Weapons" child transform for spaceship
    //

    //[Message:Star Valor Mod Core] Weapon
    //[Message:Star Valor Mod Core] component UnityEngine.Transform Weapon
    //[Message:Star Valor Mod Core] component Weapon Weapon
    //[Message:Star Valor Mod Core] Beam Weapon
    //[Message:Star Valor Mod Core] component UnityEngine.Transform Beam Weapon
    //[Message:Star Valor Mod Core] component UnityEngine.LineRenderer Beam Weapon
    //[Message:Star Valor Mod Core] component UnityEngine.AudioSource Beam Weapon
    //[Message:Star Valor Mod Core] component BeamWeapon Beam Weapon
    //[Message:Star Valor Mod Core] Missle
    //[Message:Star Valor Mod Core] component UnityEngine.Transform Missle
    //[Message:Star Valor Mod Core] component MissleWeapon Missle
    //weapon.projectileRef => GameObj holding projectile info

    //[Message:Star Valor Mod Core] Missle_2 UnityEngine.Transform
    //[Message:Star Valor Mod Core] Missle_2 UnityEngine.Rigidbody => size, mass, etc
    //[Message:Star Valor Mod Core] Missle_2 UnityEngine.CapsuleCollider => basic unity capsule-shaped collider; pick appropriate collider for projectile (missiles/fat lasers -> capsule? what is sphere?)
    //[Message:Star Valor Mod Core] Missle_2 ProjectileControl => projectile propulsion; see aggressive projectiles mod
    //[Message:Star Valor Mod Core] Missle_2 Missle ==> shows missile hp bar, assigns hitpoints, creates explosion FX => can probably be used outright, depending on special FX
    //[Message:Star Valor Mod Core] Missle_2 HideShowObject ==> used to hide or show object on minimap for player only => can be used outright
    //[Message:Star Valor Mod Core] Missle_1 UnityEngine.Transform
    //[Message:Star Valor Mod Core] Missle_1 UnityEngine.Rigidbody
    //[Message:Star Valor Mod Core] Missle_1 UnityEngine.CapsuleCollider
    //[Message:Star Valor Mod Core] Missle_1 ProjectileControl
    //[Message:Star Valor Mod Core] Missle_1 Missle
    //[Message:Star Valor Mod Core] Missle_1 HideShowObject
    //[Message:Star Valor Mod Core] Laser_Yellow UnityEngine.Transform
    //[Message:Star Valor Mod Core] Laser_Yellow UnityEngine.Rigidbody
    //[Message:Star Valor Mod Core] Laser_Yellow UnityEngine.CapsuleCollider
    //[Message:Star Valor Mod Core] Laser_Yellow ProjectileControl

    static class WeaponSystem {
        public static TAmmo CreateAmmo() {
            TAmmo tammo = new TAmmo() {
                itemID = 0, //itemId for ammo
                qnt = 1, //how much ammo is consumed when firing once
            };
            return tammo;
        }
        public static TWeapon CreateWeapon() {
            TAmmo ammo = null;
            TWeapon tweapon = new TWeapon() {
                //required data
                name = "name",
                description = "description",
                audioName = "??",
                index = -1, //zero-based index in weapon table; assigned below
                size = 0.8f, //physical size of the component
                spriteName = "??", //art resource
                tradable = true, //whether or not merchants can buy/sell the item; need to check where this is used

                //basic stats
                techLevel = 0, //required tech level for equip and crafting => could be more complex
                space = 1, //space used by weapon/cargo system, shouldn't necessarily be the same value
                craftingMaterials = new System.Collections.Generic.List<CraftMaterial>(), //list of materials used to craft
                materials = new System.Collections.Generic.List<CraftMaterial>(), //list of materials granted when dismantling
                critChance = 0, //crit chance/100
                damage = 0, //damage dealt by single hit
                heatGenMod = 1, //factor used in calculating heat gen => can patch TWeapon.heatGen{get} if necessary > note this also includes skill bonus factor
                energyCostMod = 1, //factor used in calculating energy cost => mod * damage * 0.12 (default) => can patch TWeapon.energyCost{get} if necessary
                rateOfFire = 0, //period between shots/period between bursts - not actual rate of fire
                range = 50, //range
                repReq = new ReputationRequisite(), //used for generating items => should be replaced with a more complex system
                dropLevel = DropLevel.Normal, //affects loot system (normal < elite < boss < legendary < dont drop)

                //  (these should be derived classes, not simple values)
                type = WeaponType.None, //flag used for in-game effects - should use a List<XYZ> instead
                turnSpeed = 10, //only used for missiles
                speed = 80, //projectile speed (is this used for beam weapons?)
                ammo = ammo, //ammmunition - does this describe an object, or the projectile fired?
                compType = WeaponCompType.WeaponObject, //primary operational mode (beam, normal, mine, missile)
                beamName = "??", //self explanatory
                projectileName = "??", //only a subclass because it's distinct from Beams

                //effects - replaceable with extra data
                damageType = DamageType.Normal, //type of damage dealt by the weapon/this should be replaced by extradata (normal, asteroid bonus, ignore shields, repair, shields only->actually ion/will hurt energy)
                piercing = 0, //number of targets allowed to pierce
                aoe = 0,
                burst = 0, //number of shots fired per burst, minus 1 (value of 1 => 2 total projectiles)
                shortCooldown = 0, //time between shots in a burst
                canHitProjectiles = false, //whether or not weapon is point defense; self explanatory
                explodeOnMaxRange = false, //whether or not projectile will automatically trigger an aoe when projectile dies -> flak weapons?
                timedFuse = false, //whether or not weapon explodes after a certain time (where is this value defined?)
                fluxDamageMod = 0, //damage boost multiplier upon consuming 1 flux?

                //  (crafting subcategory)
                boosterRangeMod = 0, //percentage multiplier of range
                boosterSpeedMod = 0, //percentage multiplier of speed

                //  (charge subcategory)
                chargedFireCooldown = 0, //extra cooldown time after a charged shot
                chargedFireTime = 0, //time allowed to fire after charging
                chargeTime = 0, //time required to charge weapon
            };
            int index = GameData.data.AddWeaponData(tweapon);
            tweapon.index = index;
            return tweapon;
        }
    }
    public class WeaponEx : Weapon {

    }
}
