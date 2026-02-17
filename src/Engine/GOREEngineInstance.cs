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

        public async Task<bool> InitializeAsync(Action<string> log, Action<string> logError)
        {
            try
            {
                // Config
                ConfigSystem = new ConfigSystem();
                ConfigSystem.Load();
                ConfigSystem.ValidateAndClamp();

                // Event system
                EventSystem = new EventSystem();

                // HUD
                HudSystem = new HudSystem();

                // Input
                InputSystem = new InputSystem();

                // Map
                MapSystem = new MapSystem();
                var mapData = await MapSystem.LoadInitialMapAsync("e1m1");

                // Raycast
                RaycastEngine = new RaycastEngine(mapData.Grid, EventSystem);
                RaycastEngine.PlayerPosition = mapData.PlayerStart;

                // Renderer
                RendererSystem = new RendererSystem();
                RendererSystem.Initialize(ConfigSystem.RenderWidth, ConfigSystem.RenderHeight, RaycastEngine);

                // Weapons
                WeaponSystem = new WeaponSystem();
                WeaponSystem.LoadWeaponsConfig();

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

                // (Font/icon/texture loading can be added here as needed)

                return true;
            }
            catch (Exception ex)
            {
                logError?.Invoke($"Initialization failed: {ex.Message}");
                CriticalInitError = true;
                return false;
            }
        }
    }
}
