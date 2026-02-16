using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace GORE.Engine
{
    /// <summary>
    /// Manages weapon inventory, animations, and rendering
    /// </summary>
    public class WeaponSystem
    {
        private readonly List<Weapon> _weapons;
        private int _currentWeaponIndex;
        private WeaponAnimationState _animationState;
        private float _animationTimer;
        private float _fireTimer;

        // Animation frame durations (in seconds)
        private const float FIRE_ANIM_DURATION = 0.15f;
        private const float RELOAD_ANIM_DURATION = 0.3f;

        public Weapon CurrentWeapon => _weapons[_currentWeaponIndex];
        public int CurrentWeaponIndex => _currentWeaponIndex;
        public WeaponAnimationState AnimationState => _animationState;

        public WeaponSystem()
        {
            _weapons = new List<Weapon>();
            _currentWeaponIndex = 0;
            _animationState = WeaponAnimationState.Idle;
            _animationTimer = 0f;
            _fireTimer = 0f;
        }

        /// <summary>
        /// Load weapon definitions from weapons.json
        /// </summary>
        public void LoadWeaponsConfig(string configPath = null)
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                if (configPath == null)
                {
                    configPath = Path.Combine(baseDirectory, "gt1", "weapons.json");
                }

                if (!File.Exists(configPath))
                {
                    System.Diagnostics.Debug.WriteLine($"⚠ Weapons config not found: {configPath}");
                    System.Diagnostics.Debug.WriteLine("  Using default weapon configuration");
                    InitializeDefaultWeapons();
                    return;
                }

                var jsonText = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<WeaponsConfig>(jsonText);

                if (config?.Weapons == null || config.Weapons.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠ No weapons found in config, using defaults");
                    InitializeDefaultWeapons();
                    return;
                }

                _weapons.Clear();
                foreach (var weaponData in config.Weapons)
                {
                    var weapon = new Weapon(
                        weaponData.Id,
                        weaponData.Name,
                        weaponData.DamagePerRound,
                        weaponData.MagazineSize,
                        weaponData.MaxAmmo,
                        weaponData.AmmoType,
                        weaponData.FireRate,
                        weaponData.SpriteScale,
                        weaponData.InfiniteAmmo
                    )
                    {
                        IdlePath = weaponData.Sprites.Idle,
                        FirePath = weaponData.Sprites.Fire,
                        FireAltPath = weaponData.Sprites.FireAlt,
                        ReloadPath = weaponData.Sprites.Reload
                    };

                    _weapons.Add(weapon);
                }

                System.Diagnostics.Debug.WriteLine($"✓ Loaded {_weapons.Count} weapons from {Path.GetFileName(configPath)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error loading weapons config: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("  Using default weapon configuration");
                InitializeDefaultWeapons();
            }
        }

        /// <summary>
        /// Fallback: Initialize default weapons if JSON loading fails
        /// </summary>
        private void InitializeDefaultWeapons()
        {
            _weapons.Clear();

            // Define 8 default weapons
            _weapons.Add(new Weapon(0, "Fists", 10, 0, 0, "none", 0.5f, 0.25f, true)
            {
                IdlePath = "gt1/weapons/vp0_idle.png",
                FirePath = "gt1/weapons/vp0_fire.png",
                FireAltPath = "gt1/weapons/vp0_fire_alt.png",
                ReloadPath = "gt1/weapons/vp0_reload.png"
            });

            _weapons.Add(new Weapon(1, "Pistol", 15, 12, 50, "bullets", 0.3f, 0.25f)
            {
                IdlePath = "gt1/weapons/vp1_idle.png",
                FirePath = "gt1/weapons/vp1_fire.png",
                FireAltPath = "gt1/weapons/vp1_fire_alt.png",
                ReloadPath = "gt1/weapons/vp1_reload.png"
            });

            _weapons.Add(new Weapon(2, "Shotgun", 70, 8, 24, "shells", 0.8f, 0.25f)
            {
                IdlePath = "gt1/weapons/vp2_idle.png",
                FirePath = "gt1/weapons/vp2_fire.png",
                FireAltPath = "gt1/weapons/vp2_fire_alt.png",
                ReloadPath = "gt1/weapons/vp2_reload.png"
            });

            _weapons.Add(new Weapon(3, "Chaingun", 12, 50, 200, "bullets", 0.1f, 0.25f)
            {
                IdlePath = "gt1/weapons/vp3_idle.png",
                FirePath = "gt1/weapons/vp3_fire.png",
                FireAltPath = "gt1/weapons/vp3_fire_alt.png",
                ReloadPath = "gt1/weapons/vp3_reload.png"
            });

            _weapons.Add(new Weapon(4, "Rocket Launcher", 150, 1, 20, "rockets", 1.0f, 0.25f)
            {
                IdlePath = "gt1/weapons/vp4_idle.png",
                FirePath = "gt1/weapons/vp4_fire.png",
                FireAltPath = "gt1/weapons/vp4_fire_alt.png",
                ReloadPath = "gt1/weapons/vp4_reload.png"
            });

            _weapons.Add(new Weapon(5, "Plasma Rifle", 25, 40, 100, "cells", 0.15f, 0.25f)
            {
                IdlePath = "gt1/weapons/vp5_idle.png",
                FirePath = "gt1/weapons/vp5_fire.png",
                FireAltPath = "gt1/weapons/vp5_fire_alt.png",
                ReloadPath = "gt1/weapons/vp5_reload.png"
            });

            _weapons.Add(new Weapon(6, "BFG", 500, 4, 40, "cells", 2.0f, 0.25f)
            {
                IdlePath = "gt1/weapons/vp6_idle.png",
                FirePath = "gt1/weapons/vp6_fire.png",
                FireAltPath = "gt1/weapons/vp6_fire_alt.png",
                ReloadPath = "gt1/weapons/vp6_reload.png"
            });

            _weapons.Add(new Weapon(7, "Chainsaw", 20, 0, 0, "none", 0.2f, 0.25f, true)
            {
                IdlePath = "gt1/weapons/vp7_idle.png",
                FirePath = "gt1/weapons/vp7_fire.png",
                FireAltPath = "gt1/weapons/vp7_fire_alt.png",
                ReloadPath = "gt1/weapons/vp7_reload.png"
            });

            System.Diagnostics.Debug.WriteLine("  Loaded 8 default weapons");
        }

        /// <summary>
        /// Load all weapon sprites asynchronously
        /// </summary>
        public async Task LoadWeaponSpritesAsync(CanvasDevice device)
        {
            var baseDirectory = AppContext.BaseDirectory;

            System.Diagnostics.Debug.WriteLine($"=== Loading Weapon Sprites ===");
            System.Diagnostics.Debug.WriteLine($"Base directory: {baseDirectory}");

            foreach (var weapon in _weapons)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"\nWeapon {weapon.Id}: {weapon.Name}");

                    // Load all 4 frames for each weapon
                    var idlePath = Path.Combine(baseDirectory, weapon.IdlePath);
                    var firePath = Path.Combine(baseDirectory, weapon.FirePath);
                    var fireAltPath = Path.Combine(baseDirectory, weapon.FireAltPath);
                    var reloadPath = Path.Combine(baseDirectory, weapon.ReloadPath);

                    int loadedCount = 0;

                    // Only load if files exist (allows for partial weapon sets during development)
                    if (File.Exists(idlePath))
                    {
                        weapon.IdleFrame = await CanvasBitmap.LoadAsync(device, idlePath);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Loaded idle frame");
                        loadedCount++;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ Missing: {weapon.IdlePath}");
                    }

                    if (File.Exists(firePath))
                    {
                        weapon.FireFrame = await CanvasBitmap.LoadAsync(device, firePath);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Loaded fire frame");
                        loadedCount++;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ Missing: {weapon.FirePath}");
                    }

                    if (File.Exists(fireAltPath))
                    {
                        weapon.FireAltFrame = await CanvasBitmap.LoadAsync(device, fireAltPath);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Loaded fire_alt frame");
                        loadedCount++;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ Missing: {weapon.FireAltPath}");
                    }

                    if (File.Exists(reloadPath))
                    {
                        weapon.ReloadFrame = await CanvasBitmap.LoadAsync(device, reloadPath);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Loaded reload frame");
                        loadedCount++;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ Missing: {weapon.ReloadPath}");
                    }

                    if (loadedCount == 4)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✓ Weapon {weapon.Id} ({weapon.Name}): All 4 frames loaded");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ⚠ Weapon {weapon.Id} ({weapon.Name}): Only {loadedCount}/4 frames loaded");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Failed to load weapon {weapon.Id} ({weapon.Name}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Switch to a specific weapon by index (0-7)
        /// </summary>
        public bool SwitchToWeapon(int index)
        {
            if (index >= 0 && index < _weapons.Count)
            {
                _currentWeaponIndex = index;
                _animationState = WeaponAnimationState.Idle;
                _animationTimer = 0f;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Switch to next weapon
        /// </summary>
        public void NextWeapon()
        {
            _currentWeaponIndex = (_currentWeaponIndex + 1) % _weapons.Count;
            _animationState = WeaponAnimationState.Idle;
            _animationTimer = 0f;
        }

        /// <summary>
        /// Switch to previous weapon
        /// </summary>
        public void PreviousWeapon()
        {
            _currentWeaponIndex--;
            if (_currentWeaponIndex < 0)
                _currentWeaponIndex = _weapons.Count - 1;
            _animationState = WeaponAnimationState.Idle;
            _animationTimer = 0f;
        }

        /// <summary>
        /// Attempt to fire the current weapon
        /// </summary>
        public bool TryFire()
        {
            // Check if weapon can fire (cooldown + ammo)
            if (_fireTimer > 0f || !CurrentWeapon.CanFire())
                return false;

            // Fire the weapon
            CurrentWeapon.Fire();
            _animationState = WeaponAnimationState.Firing;
            _animationTimer = FIRE_ANIM_DURATION;
            _fireTimer = CurrentWeapon.FireRate;

            return true;
        }

        /// <summary>
        /// Reload current weapon
        /// </summary>
        public void Reload()
        {
            if (CurrentWeapon.NeedsReload() || 
                (!CurrentWeapon.InfiniteAmmo && CurrentWeapon.MagazineAmmo < CurrentWeapon.MagazineSize && CurrentWeapon.CurrentAmmo > 0))
            {
                _animationState = WeaponAnimationState.Reloading;
                _animationTimer = RELOAD_ANIM_DURATION;
            }
        }

        /// <summary>
        /// Update weapon animation and cooldown timers
        /// </summary>
        public void Update(float deltaTime)
        {
            _frameCount++;

            // Update fire cooldown
            if (_fireTimer > 0f)
            {
                _fireTimer -= deltaTime;
            }

            // Update animation timer
            if (_animationTimer > 0f)
            {
                _animationTimer -= deltaTime;

                // When animation completes, return to idle
                if (_animationTimer <= 0f)
                {
                    if (_animationState == WeaponAnimationState.Reloading)
                    {
                        CurrentWeapon.Reload();
                    }
                    _animationState = WeaponAnimationState.Idle;
                }
            }
        }

        /// <summary>
        /// Get the current frame to render based on animation state
        /// </summary>
        public CanvasBitmap GetCurrentFrame()
        {
            var weapon = CurrentWeapon;

            return _animationState switch
            {
                WeaponAnimationState.Idle => weapon.IdleFrame,
                WeaponAnimationState.Firing => 
                    // Alternate between fire frames for muzzle flash effect
                    (_animationTimer > FIRE_ANIM_DURATION * 0.5f) ? weapon.FireFrame : weapon.FireAltFrame,
                WeaponAnimationState.Reloading => weapon.ReloadFrame,
                _ => weapon.IdleFrame
            };
        }

        /// <summary>
        /// Render the current weapon sprite on screen
        /// </summary>
        public void Render(CanvasDrawingSession session, float screenWidth, float screenHeight)
        {
            var frame = GetCurrentFrame();
            if (frame == null)
            {
                // Only log once per second to avoid spam
                if (_frameCount % 60 == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠ Weapon render: frame is NULL for weapon {CurrentWeapon.Id} ({CurrentWeapon.Name}) in state {_animationState}");
                }
                return;
            }

            // Use weapon's configured sprite scale
            float weaponScale = CurrentWeapon.SpriteScale;
            float weaponWidth = (float)frame.Size.Width * weaponScale;
            float weaponHeight = (float)frame.Size.Height * weaponScale;

            float x = (screenWidth - weaponWidth) / 2f;
            float y = screenHeight - weaponHeight;

            // Draw weapon sprite with destination rectangle for proper scaling
            var destRect = new Windows.Foundation.Rect(x, y, weaponWidth, weaponHeight);
            session.DrawImage(frame, destRect, 
                new Windows.Foundation.Rect(0, 0, frame.Size.Width, frame.Size.Height),
                1.0f, CanvasImageInterpolation.NearestNeighbor);
        }

        private int _frameCount = 0;

        /// <summary>
        /// Get weapon by index
        /// </summary>
        public Weapon GetWeapon(int index)
        {
            if (index >= 0 && index < _weapons.Count)
                return _weapons[index];
            return null;
        }

        /// <summary>
        /// Add ammo to a specific weapon
        /// </summary>
        public void AddAmmo(int weaponIndex, int amount)
        {
            var weapon = GetWeapon(weaponIndex);
            weapon?.AddAmmo(amount);
        }

        /// <summary>
        /// Dispose all weapon sprites
        /// </summary>
        public void Dispose()
        {
            foreach (var weapon in _weapons)
            {
                weapon.IdleFrame?.Dispose();
                weapon.FireFrame?.Dispose();
                weapon.FireAltFrame?.Dispose();
                weapon.ReloadFrame?.Dispose();
            }
        }
    }

    /// <summary>
    /// Weapon animation states
    /// </summary>
    public enum WeaponAnimationState
    {
        Idle,
        Firing,
        Reloading
    }
}
