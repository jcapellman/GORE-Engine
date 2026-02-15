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

        public event Action OnHistoryChanged;

        public IReadOnlyList<string> History => _history;

        public GameConsole()
        {
            RegisterDefaultCommands();
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
    }
}
