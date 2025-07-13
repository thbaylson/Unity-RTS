namespace RTS.EventBus
{
    public static class Bus<T> where T : IEvent
    {
        public delegate void Event(T args);
        // The event keyword means that only the Bus can invoke the delegate.
        public static event Event OnEvent;

        public static void Raise(T evt) => OnEvent?.Invoke(evt);
    }
}