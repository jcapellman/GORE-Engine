using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GORE.Engine
{
    /// <summary>
    /// Root object for weapons.json
    /// </summary>
    public class WeaponsConfig
    {
        [JsonPropertyName("weapons")]
        public List<WeaponData> Weapons { get; set; }
    }

    /// <summary>
    /// JSON structure for weapon configuration
    /// </summary>
    public class WeaponData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("damage_per_round")]
        public int DamagePerRound { get; set; }

        [JsonPropertyName("magazine_size")]
        public int MagazineSize { get; set; }

        [JsonPropertyName("max_ammo")]
        public int MaxAmmo { get; set; }

        [JsonPropertyName("ammo_type")]
        public string AmmoType { get; set; }

        [JsonPropertyName("fire_rate")]
        public float FireRate { get; set; }

        [JsonPropertyName("sprite_scale")]
        public float SpriteScale { get; set; }

        [JsonPropertyName("infinite_ammo")]
        public bool InfiniteAmmo { get; set; }

        [JsonPropertyName("sprites")]
        public WeaponSprites Sprites { get; set; }
    }

    /// <summary>
    /// Sprite paths for a weapon
    /// </summary>
    public class WeaponSprites
    {
        [JsonPropertyName("idle")]
        public string Idle { get; set; }

        [JsonPropertyName("fire")]
        public string Fire { get; set; }

        [JsonPropertyName("fire_alt")]
        public string FireAlt { get; set; }

        [JsonPropertyName("reload")]
        public string Reload { get; set; }
    }
}
