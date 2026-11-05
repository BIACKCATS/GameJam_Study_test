using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when one minigame round ends (after its effects were applied).
    /// Reflex: see ReflexResult. Timing: Success true/false (false is followed by a Reflex round).
    /// </summary>
    public readonly struct MinigameFinishedEvent
    {
        public readonly Talkable Target;
        public readonly MinigameType Type;
        public readonly bool Success;
        public readonly ReflexResult ReflexResult;

        public MinigameFinishedEvent(Talkable target, MinigameType type, bool success, ReflexResult reflexResult)
        {
            Target = target;
            Type = type;
            Success = success;
            ReflexResult = reflexResult;
        }
    }
}
