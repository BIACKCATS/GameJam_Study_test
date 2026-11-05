using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when a counterattack event starts (presentation hook). Life -1 is applied right after this event.
    /// The session stays active (input locked) for MinigameDirector's counterattack duration.
    /// </summary>
    public readonly struct CounterattackEvent
    {
        public readonly Talkable Target;
        public readonly bool IsVillain;
        public readonly CounterattackReason Reason;

        public CounterattackEvent(Talkable target, bool isVillain, CounterattackReason reason)
        {
            Target = target;
            IsVillain = isVillain;
            Reason = reason;
        }
    }
}
