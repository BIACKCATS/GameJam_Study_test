using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when a minigame session starts (IsActive = true) or fully ends (false).
    /// Timing-fail -> Counterattack stays inside one session (no false/true flicker). Player input locks on this.
    /// </summary>
    public readonly struct MinigameStateChangedEvent
    {
        public readonly bool IsActive;
        public readonly MinigameType Type;
        public readonly Talkable Target;

        public MinigameStateChangedEvent(bool isActive, MinigameType type, Talkable target)
        {
            IsActive = isActive;
            Type = type;
            Target = target;
        }
    }
}
