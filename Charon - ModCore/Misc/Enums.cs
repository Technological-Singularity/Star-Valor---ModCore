namespace Charon.StarValor.ModCore {
    public enum ItemRarity : int {
        Poor_0 = 0,
        Common_1 = 1,
        Uncommon_2 = 2,
        Rare_3 = 3,
        Epic_4 = 4,
        Legendary_5 = 5,
    }
    public enum Layer : int {
        Default = 0, //lasers, bullets
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,
        Object = 8,
        Spaceship = 9, //player and NPC ships
        Asteroid = 10, //asteroids
        Collectible = 11, //drifting objects
        MinimapOnly = 12,
        Station = 13, //stations
        SmallObject = 14, //drones
        GalaxyMap = 15,
        Missiles = 16, //missiles
    }
    public enum UniqueLevel : int {
        None = 0,
        Light = 1,
        Heavy = 2,
    }
}
