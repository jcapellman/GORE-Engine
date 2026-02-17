using System;

namespace GORE.Engine
{
    public class HudSystem
    {
        private string _cachedHealthText = "100";
        private string _cachedAmmoText = "50";
        private string _cachedFpsText = "60fps";
        private bool _hudNeedsUpdate = true;
        private int _lastFps = 0;
        private int _fpsFrameCount = 0;
        private double _fpsAccumulator = 0.0;
        private bool _showFps = true;

        public string HealthText => _cachedHealthText;
        public string AmmoText => _cachedAmmoText;
        public string FpsText => _cachedFpsText;
        public bool ShowFps => _showFps;
        public bool NeedsUpdate => _hudNeedsUpdate;

        public void SetShowFps(bool showFps) => _showFps = showFps;

        public void UpdateHealth(int health)
        {
            var healthStr = health.ToString();
            if (_cachedHealthText != healthStr)
            {
                _cachedHealthText = healthStr;
                _hudNeedsUpdate = true;
            }
        }

        public void UpdateAmmo(string ammoStr)
        {
            if (_cachedAmmoText != ammoStr)
            {
                _cachedAmmoText = ammoStr;
                _hudNeedsUpdate = true;
            }
        }

        public void UpdateFPS(float deltaTime)
        {
            if (!_showFps) return;
            _fpsFrameCount++;
            _fpsAccumulator += deltaTime;
            if (_fpsAccumulator >= 1.0)
            {
                _lastFps = (int)(_fpsFrameCount / _fpsAccumulator);
                _fpsFrameCount = 0;
                _fpsAccumulator = 0.0;
                _cachedFpsText = $"{_lastFps}fps";
                _hudNeedsUpdate = true;
            }
        }

        public void ResetNeedsUpdate() => _hudNeedsUpdate = false;
    }
}
