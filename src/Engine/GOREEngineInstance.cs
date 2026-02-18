using System;
using System.Threading.Tasks;
using GORE.Engine.Systems;
using Microsoft.UI.Xaml;

namespace GORE.Engine
{
    public class GOREEngineInstance
    {
        public ConfigSystem ConfigSystem { get; private set; }
        public MapSystem MapSystem { get; private set; }
        public RendererSystem RendererSystem { get; set; }
        public RaycastEngine RaycastEngine { get; set; }
        public WeaponSystem WeaponSystem { get; private set; }
        public SoundEffectSystem SoundEffectSystem { get; private set; }
        public MusicSystem MusicSystem { get; private set; }
        public InputSystem InputSystem { get; private set; }
        public HudSystem HudSystem { get; private set; }
        public EventSystem EventSystem { get; private set; }
        public GameConsole GameConsole { get; private set; }
        public bool CriticalInitError { get; private set; }
        public string InitLog { get; private set; } = string.Empty;

        // Dependency Injection constructor
        public GOREEngineInstance(
            ConfigSystem configSystem,
            MapSystem mapSystem,
            WeaponSystem weaponSystem,
            RendererSystem rendererSystem,
            Action<string> log = null,
            Action<string> logError = null)
        {
            ConfigSystem = configSystem;
            MapSystem = mapSystem;
            WeaponSystem = weaponSystem;
            RendererSystem = rendererSystem;

            // Event system
            EventSystem = new EventSystem();

            // HUD
            HudSystem = new HudSystem();

            // Input
            InputSystem = new InputSystem();

            // Audio
            SoundEffectSystem = new SoundEffectSystem();
            MusicSystem = new MusicSystem();

            // Console
            GameConsole = new GameConsole();
            GameConsole.SetConfig(ConfigSystem.GetConfig());

            // Wire input to weapons
            InputSystem.NextWeaponRequested = () => WeaponSystem?.NextWeapon();
            InputSystem.PreviousWeaponRequested = () => WeaponSystem?.PreviousWeapon();
            InputSystem.WeaponNumberKeyPressed += idx => WeaponSystem?.SwitchToWeapon(idx);

            // Wire door event to sound
            EventSystem.Subscribe<DoorInteractedEvent>(_ => SoundEffectSystem?.Play("door"));
        }
    }
}
