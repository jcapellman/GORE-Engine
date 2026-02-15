using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GORE.Engine
{
    public class GameConsole
    {
        private readonly Dictionary<string, ConsoleCommand> _commands = new();
        private readonly List<string> _history = new();
        private readonly List<string> _commandHistory = new();
        private int _commandHistoryIndex = -1;
        private const int MaxHistoryLines = 100;
        private GameConfig _config;

        public event Action OnHistoryChanged;

        public IReadOnlyList<string> History => _history;

        public GameConsole()
        {
            RegisterDefaultCommands();
        }

        public void SetConfig(GameConfig config)
        {
            _config = config;
            RegisterConfigCommands();
        }

        private void RegisterDefaultCommands()
        {
            RegisterCommand("help", "Display available commands", args =>
            {
                if (args.Length > 0)
                {
                    var cmdName = args[0].ToLower();
                    if (_commands.TryGetValue(cmdName, out var cmd))
                    {
                        AddToHistory($"{cmd.Name} - {cmd.Description}");
                    }
                    else
                    {
                        AddToHistory($"Unknown command: {cmdName}");
                    }
                }
                else
                {
                    AddToHistory("Available commands:");
                    foreach (var cmd in _commands.Values.OrderBy(c => c.Name))
                    {
                        AddToHistory($"  {cmd.Name} - {cmd.Description}");
                    }
                }
            });

            RegisterCommand("clear", "Clear console history", args =>
            {
                _history.Clear();
                OnHistoryChanged?.Invoke();
            });

            RegisterCommand("echo", "Echo text to console", args =>
            {
                AddToHistory(string.Join(" ", args));
            });

            RegisterCommand("quit", "Exit the game", args =>
            {
                Environment.Exit(0);
            });

            RegisterCommand("exit", "Exit the game", args =>
            {
                Environment.Exit(0);
            });
        }

        public void RegisterCommand(string name, string description, ConsoleCommandHandler handler)
        {
            var cmd = new ConsoleCommand(name.ToLower(), description, handler);
            _commands[cmd.Name] = cmd;
        }

        public void ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            // Add to command history
            _commandHistory.Add(input);
            _commandHistoryIndex = _commandHistory.Count;

            // Echo the command
            AddToHistory($"] {input}");

            // Parse command and arguments
            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            var commandName = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            // Execute command
            if (_commands.TryGetValue(commandName, out var command))
            {
                try
                {
                    command.Execute(args);
                }
                catch (Exception ex)
                {
                    AddToHistory($"Error executing command: {ex.Message}");
                }
            }
            else
            {
                AddToHistory($"Unknown command: {commandName}");
                AddToHistory("Type 'help' for a list of available commands");
            }
        }

        public void AddToHistory(string message)
        {
            _history.Add(message);
            
            // Limit history size
            while (_history.Count > MaxHistoryLines)
            {
                _history.RemoveAt(0);
            }

            OnHistoryChanged?.Invoke();
        }

        public string GetPreviousCommand()
        {
            if (_commandHistory.Count == 0)
                return string.Empty;

            _commandHistoryIndex--;
            if (_commandHistoryIndex < 0)
                _commandHistoryIndex = 0;

            return _commandHistory[_commandHistoryIndex];
        }

        public string GetNextCommand()
        {
            if (_commandHistory.Count == 0)
                return string.Empty;

            _commandHistoryIndex++;
            if (_commandHistoryIndex >= _commandHistory.Count)
            {
                _commandHistoryIndex = _commandHistory.Count;
                return string.Empty;
            }

            return _commandHistory[_commandHistoryIndex];
        }

        public void RegisterGameCommands(RaycastEngine engine, Action<int> setHealth, Action<int> setAmmo)
        {
            RegisterCommand("god", "Toggle god mode (invincibility)", args =>
            {
                setHealth(999);
                AddToHistory("God mode enabled");
            });

            RegisterCommand("give", "Give items (health, ammo, all)", args =>
            {
                if (args.Length == 0)
                {
                    AddToHistory("Usage: give <health|ammo|all> [amount]");
                    return;
                }

                var item = args[0].ToLower();
                var amount = args.Length > 1 && int.TryParse(args[1], out var val) ? val : 100;

                switch (item)
                {
                    case "health":
                        setHealth(amount);
                        AddToHistory($"Health set to {amount}");
                        break;
                    case "ammo":
                        setAmmo(amount);
                        AddToHistory($"Ammo set to {amount}");
                        break;
                    case "all":
                        setHealth(100);
                        setAmmo(200);
                        AddToHistory("Full health and ammo");
                        break;
                    default:
                        AddToHistory($"Unknown item: {item}");
                        break;
                }
            });

            RegisterCommand("teleport", "Teleport to position (x y)", args =>
            {
                if (args.Length < 2)
                {
                    AddToHistory("Usage: teleport <x> <y>");
                    return;
                }

                if (float.TryParse(args[0], out var x) && float.TryParse(args[1], out var y))
                {
                    engine.PlayerPosition = new Vector2(x, y);
                    AddToHistory($"Teleported to ({x}, {y})");
                }
                else
                {
                    AddToHistory("Invalid coordinates");
                }
            });

            RegisterCommand("pos", "Display current position", args =>
            {
                var pos = engine.PlayerPosition;
                AddToHistory($"Position: ({pos.X:F2}, {pos.Y:F2})");
            });

            RegisterCommand("noclip", "Toggle noclip mode (walk through walls)", args =>
            {
                AddToHistory("Noclip mode not yet implemented");
            });
        }

        private void RegisterConfigCommands()
        {
            RegisterCommand("cvarlist", "List all config variables", args =>
            {
                if (_config == null)
                {
                    AddToHistory("Config system not initialized");
                    return;
                }

                var filter = args.Length > 0 ? args[0] : "";
                var variables = string.IsNullOrEmpty(filter) 
                    ? _config.Variables.Values.ToList() 
                    : _config.Find(filter);

                AddToHistory($"Config Variables (showing {variables.Count}):");
                foreach (var variable in variables.OrderBy(v => v.Name))
                {
                    var flags = new List<string>();
                    if (variable.Flags.HasFlag(ConfigVariableFlags.Archive)) flags.Add("A");
                    if (variable.Flags.HasFlag(ConfigVariableFlags.ReadOnly)) flags.Add("R");
                    if (variable.Flags.HasFlag(ConfigVariableFlags.Cheat)) flags.Add("C");

                    var flagStr = flags.Count > 0 ? $"[{string.Join("", flags)}] " : "";
                    AddToHistory($"  {flagStr}{variable.Name} = {variable} - {variable.Description}");
                }
            });

            RegisterCommand("set", "Set a config variable (usage: set <name> <value>)", args =>
            {
                if (_config == null)
                {
                    AddToHistory("Config system not initialized");
                    return;
                }

                if (args.Length < 2)
                {
                    AddToHistory("Usage: set <variable> <value>");
                    return;
                }

                var name = args[0];
                var value = string.Join(" ", args.Skip(1));

                var variable = _config.Get(name);
                if (variable == null)
                {
                    AddToHistory($"Unknown variable: {name}");
                    AddToHistory("Use 'cvarlist' to see available variables");
                    return;
                }

                variable.SetValue(value);
                AddToHistory($"{variable.Name} = {variable}");
            });

            RegisterCommand("get", "Get a config variable value", args =>
            {
                if (_config == null)
                {
                    AddToHistory("Config system not initialized");
                    return;
                }

                if (args.Length == 0)
                {
                    AddToHistory("Usage: get <variable>");
                    return;
                }

                var variable = _config.Get(args[0]);
                if (variable == null)
                {
                    AddToHistory($"Unknown variable: {args[0]}");
                    return;
                }

                AddToHistory($"{variable.Name} = {variable}");
                if (!string.IsNullOrEmpty(variable.Description))
                {
                    AddToHistory($"  {variable.Description}");
                }
            });

            RegisterCommand("toggle", "Toggle a boolean config variable", args =>
            {
                if (_config == null)
                {
                    AddToHistory("Config system not initialized");
                    return;
                }

                if (args.Length == 0)
                {
                    AddToHistory("Usage: toggle <variable>");
                    return;
                }

                var variable = _config.Get(args[0]);
                if (variable == null)
                {
                    AddToHistory($"Unknown variable: {args[0]}");
                    return;
                }

                try
                {
                    var currentValue = variable.GetValue<bool>();
                    variable.SetValue(!currentValue);
                    AddToHistory($"{variable.Name} = {variable}");
                }
                catch
                {
                    AddToHistory($"{variable.Name} is not a boolean variable");
                }
            });

            RegisterCommand("reset", "Reset a config variable to default", args =>
            {
                if (_config == null)
                {
                    AddToHistory("Config system not initialized");
                    return;
                }

                if (args.Length == 0)
                {
                    AddToHistory("Usage: reset <variable>");
                    return;
                }

                var variable = _config.Get(args[0]);
                if (variable == null)
                {
                    AddToHistory($"Unknown variable: {args[0]}");
                    return;
                }

                variable.Reset();
                AddToHistory($"{variable.Name} reset to {variable}");
            });

            RegisterCommand("writeconfig", "Save config to config.json", args =>
            {
                if (_config == null)
                {
                    AddToHistory("Config system not initialized");
                    return;
                }

                _config.SaveConfig();
                AddToHistory("Configuration saved to config.json");
            });

            RegisterCommand("exec", "Execute config file", args =>
            {
                if (_config == null)
                {
                    AddToHistory("Config system not initialized");
                    return;
                }

                if (args.Length == 0)
                {
                    AddToHistory("Usage: exec <filename>");
                    return;
                }

                AddToHistory("Config file execution not yet implemented");
            });
        }
    }
}
