namespace JSH.Gameplay
{
    /// <summary>Raised once when a villain is defeated in the Reflex minigame.</summary>
    public readonly struct VillainDefeatedEvent
    {
        public readonly Villain Villain;

        public VillainDefeatedEvent(Villain villain) => Villain = villain;
    }
}
