using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when one round ends (after its effects were applied).
    /// Timing: Success true/false (false is followed by a Counterattack round in the same session).
    /// Eject: Success = true (villain defeated). Counterattack: Success = false.
    /// </summary>
    public readonly struct MinigameFinishedEvent
    {
        public readonly Talkable Target;
        public readonly MinigameType Type;
        public readonly bool Success;

        public MinigameFinishedEvent(Talkable target, MinigameType type, bool success)
        {
            Target = target;
            Type = type;
            Success = success;
        }
    }
}
