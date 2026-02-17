using System;
using System.Collections.Generic;

namespace GORE.Engine
{
    public class EventSystem
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();
            _subscribers[type].Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (_subscribers.TryGetValue(type, out var handlers))
                handlers.Remove(handler);
        }

        public void Publish<TEvent>(TEvent evt)
        {
            var type = typeof(TEvent);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    ((Action<TEvent>)handler)?.Invoke(evt);
                }
            }
        }
    }

    // Example event for door interaction
    public struct DoorInteractedEvent { }
}
