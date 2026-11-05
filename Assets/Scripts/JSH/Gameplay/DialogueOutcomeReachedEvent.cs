using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Raised after the final line of a dialogue tree was read and the dialogue closed (Outcome != None).
    /// MinigameDirector starts the matching minigame / event.
    /// </summary>
    public readonly struct DialogueOutcomeReachedEvent
    {
        public readonly Talkable Target;
        public readonly DialogueOutcome Outcome;

        public DialogueOutcomeReachedEvent(Talkable target, DialogueOutcome outcome)
        {
            Target = target;
            Outcome = outcome;
        }
    }
}
