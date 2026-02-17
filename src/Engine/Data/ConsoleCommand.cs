using System;

namespace GORE.Engine
{
    public delegate void ConsoleCommandHandler(string[] args);

    public class ConsoleCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ConsoleCommandHandler Handler { get; set; }

        public ConsoleCommand(string name, string description, ConsoleCommandHandler handler)
        {
            Name = name;
            Description = description;
            Handler = handler;
        }

        public void Execute(string[] args)
        {
            Handler?.Invoke(args);
        }
    }
}
