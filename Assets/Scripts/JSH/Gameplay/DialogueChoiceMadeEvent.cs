using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>Raised when the player picks a choice. Villain / lives / ending systems react to Effect.</summary>
    public readonly struct DialogueChoiceMadeEvent
    {
        public readonly Talkable Target;
        public readonly int NodeIndex;
        public readonly int ChoiceIndex;
        public readonly DialogueEffect Effect;

        public DialogueChoiceMadeEvent(Talkable target, int nodeIndex, int choiceIndex, DialogueEffect effect)
        {
            Target = target;
            NodeIndex = nodeIndex;
            ChoiceIndex = choiceIndex;
            Effect = effect;
        }
    }
}
