// using Microsoft.Graphics.Canvas;
using System.Collections.Generic;

namespace GORE.Engine
{
    /// <summary>
    /// Represents a weapon with its sprites and properties
    /// </summary>
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DamagePerRound { get; set; }
        public int MagazineSize { get; set; }
        public int MaxAmmo { get; set; }
        public int CurrentAmmo { get; set; }
        public int MagazineAmmo { get; set; }
        public string AmmoType { get; set; }
        public float FireRate { get; set; } // Seconds between shots
        public float SpriteScale { get; set; }
        public bool InfiniteAmmo { get; set; }

        // Sprite frame file paths (for OpenGL texture loading)
        // public CanvasBitmap IdleFrame { get; set; }
        // public CanvasBitmap FireFrame { get; set; }
        // public CanvasBitmap FireAltFrame { get; set; }
        // public CanvasBitmap ReloadFrame { get; set; }

        // File paths for loading
        public string IdlePath { get; set; }
        public string FirePath { get; set; }
        public string FireAltPath { get; set; }
        public string ReloadPath { get; set; }

        public Weapon()
        {
            // Default constructor for JSON deserialization
        }

        public Weapon(int id, string name, int damagePerRound, int magazineSize, int maxAmmo, 
                      string ammoType, float fireRate, float spriteScale, bool infiniteAmmo = false)
        {
            Id = id;
            Name = name;
            DamagePerRound = damagePerRound;
            MagazineSize = magazineSize;
            MaxAmmo = maxAmmo;
            CurrentAmmo = maxAmmo;
            MagazineAmmo = magazineSize;
            AmmoType = ammoType;
            FireRate = fireRate;
            SpriteScale = spriteScale;
            InfiniteAmmo = infiniteAmmo;
        }

        /// <summary>
        /// Check if weapon can fire (has ammo)
        /// </summary>
        public bool CanFire()
        {
            return InfiniteAmmo || MagazineAmmo > 0;
        }

        /// <summary>
        /// Check if weapon needs reload
        /// </summary>
        public bool NeedsReload()
        {
            return !InfiniteAmmo && MagazineAmmo == 0 && CurrentAmmo > 0;
        }

        /// <summary>
        /// Consume one ammo from magazine
        /// </summary>
        public void Fire()
        {
            if (!InfiniteAmmo && MagazineAmmo > 0)
            {
                MagazineAmmo--;
            }
        }

        /// <summary>
        /// Reload magazine from reserve ammo
        /// </summary>
        public void Reload()
        {
            if (InfiniteAmmo || MagazineSize == 0)
                return;

            int ammoNeeded = MagazineSize - MagazineAmmo;
            int ammoToLoad = System.Math.Min(ammoNeeded, CurrentAmmo);

            MagazineAmmo += ammoToLoad;
            CurrentAmmo -= ammoToLoad;
        }

        /// <summary>
        /// Add ammo to reserve up to max
        /// </summary>
        public void AddAmmo(int amount)
        {
            if (InfiniteAmmo)
                return;

            CurrentAmmo = System.Math.Min(CurrentAmmo + amount, MaxAmmo);
        }
    }
}
