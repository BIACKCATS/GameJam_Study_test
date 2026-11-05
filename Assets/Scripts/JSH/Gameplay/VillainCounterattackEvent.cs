namespace JSH.Gameplay
{
    /// <summary>
    /// OBSOLETE (unused; replaced by CounterattackEvent, delete via Editor). Raised when the player hits a guarding villain in the Reflex minigame (villain-only counterattack feedback hook).
    /// Order: VillainCounterattackEvent -> life -1 -> VillainDefeatedEvent.
    /// </summary>
    public readonly struct VillainCounterattackEvent
    {
        public readonly Villain Villain;

        public VillainCounterattackEvent(Villain villain) => Villain = villain;
    }
}
