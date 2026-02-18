using System;
using System.Collections.Generic;

namespace GORE.Engine.Systems
{
    public class ConsoleSystem
    {
        public event Action HistoryChanged;
        public List<string> History { get; } = new();
        private readonly Dictionary<string, (string description, Func<string[], System.Threading.Tasks.Task> handler)> _commands = new();
        private int _historyIndex = -1;
        private string _lastInput = string.Empty;

        public void AddToHistory(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                History.Add(message);
                HistoryChanged?.Invoke();
            }
        }

        public void RegisterCommand(string name, string description, Func<string[], System.Threading.Tasks.Task> handler)
        {
            _commands[name.ToLower()] = (description, handler);
        }

        public void RegisterCommand(string name, string description, Action<string[]> handler)
        {
            RegisterCommand(name, description, args => { handler(args); return System.Threading.Tasks.Task.CompletedTask; });
        }

        public async void ExecuteCommand(string input)
        {
            AddToHistory($"> {input}");
            _lastInput = input;
            _historyIndex = History.Count;
            if (string.IsNullOrWhiteSpace(input)) return;
            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLower();
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();
            if (_commands.TryGetValue(cmd, out var entry))
            {
                await entry.handler(args);
            }
            else
            {
                AddToHistory($"Unknown command: {cmd}");
            }
        }

        public string GetPreviousCommand()
        {
            if (History.Count == 0) return string.Empty;
            _historyIndex = Math.Max(0, _historyIndex - 1);
            return History[_historyIndex];
        }

        public string GetNextCommand()
        {
            if (History.Count == 0) return string.Empty;
            _historyIndex = Math.Min(History.Count - 1, _historyIndex + 1);
            return History[_historyIndex];
        }
    }
}
