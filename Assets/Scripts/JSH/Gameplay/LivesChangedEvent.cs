namespace JSH.Gameplay
{
    /// <summary>Raised whenever the life count changes. Delta is negative on loss, positive on heal.</summary>
    public readonly struct LivesChangedEvent
    {
        public readonly int Current;
        public readonly int Max;
        public readonly int Delta;

        public LivesChangedEvent(int current, int max, int delta)
        {
            Current = current;
            Max = max;
            Delta = delta;
        }
    }
}
