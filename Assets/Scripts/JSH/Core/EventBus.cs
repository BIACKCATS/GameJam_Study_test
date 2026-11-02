using System;

namespace JSH.Core
{
    /// <summary>
    /// Zero-allocation typed event bus. Define events as readonly structs (see GameEvents.cs).
    /// Subscribe in OnEnable, Unsubscribe in OnDisable.
    /// Usage: EventBus&lt;GameStateChangedEvent&gt;.Raise(new GameStateChangedEvent(prev, next));
    /// </summary>
    public static class EventBus<T> where T : struct
    {
        private static event Action<T> _handlers;

        public static void Subscribe(Action<T> handler) => _handlers += handler;
        public static void Unsubscribe(Action<T> handler) => _handlers -= handler;
        public static void Raise(in T evt) => _handlers?.Invoke(evt);
        public static void Clear() => _handlers = null;
    }
}
