using System;

namespace GORE.Engine
{
    /// <summary>
    /// Configuration variable (similar to Quake 2's cvars)
    /// </summary>
    public class ConfigVariable
    {
        public string Name { get; }
        public string Description { get; }
        public ConfigVariableFlags Flags { get; }
        
        private object _value;
        private readonly object _defaultValue;
        private readonly Type _valueType;

        public event Action<ConfigVariable> OnChanged;

        public ConfigVariable(string name, object defaultValue, string description = "", ConfigVariableFlags flags = ConfigVariableFlags.None)
        {
            Name = name;
            Description = description;
            Flags = flags;
            _defaultValue = defaultValue;
            _value = defaultValue;
            _valueType = defaultValue.GetType();
        }

        public T GetValue<T>()
        {
            return (T)_value;
        }

        public void SetValue(object value, bool silent = false)
        {
            if (Flags.HasFlag(ConfigVariableFlags.ReadOnly))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot modify read-only variable: {Name}");
                return;
            }

            try
            {
                _value = Convert.ChangeType(value, _valueType);
                
                if (!silent)
                {
                    OnChanged?.Invoke(this);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set {Name}: {ex.Message}");
            }
        }

        public void Reset()
        {
            SetValue(_defaultValue);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "null";
        }
    }

    [Flags]
    public enum ConfigVariableFlags
    {
        None = 0,
        Archive = 1,      // Save to config file
        ReadOnly = 2,     // Cannot be modified
        Cheat = 4,        // Requires cheats enabled
        ServerInfo = 8    // Server configuration (for future multiplayer)
    }
}
