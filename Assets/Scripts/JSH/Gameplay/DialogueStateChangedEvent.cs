namespace JSH.Gameplay
{
    /// <summary>Raised when a dialogue starts (IsActive = true) or ends (false). Player input locks on this.</summary>
    public readonly struct DialogueStateChangedEvent
    {
        public readonly bool IsActive;
        public readonly Talkable Target;

        public DialogueStateChangedEvent(bool isActive, Talkable target)
        {
            IsActive = isActive;
            Target = target;
        }
    }
}
