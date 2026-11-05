namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when the "thrown off the train" event starts for an accused villain (presentation hook, same scene).
    /// After MinigameDirector's eject duration: VillainDefeatedEvent, then the villain is removed from the car.
    /// </summary>
    public readonly struct VillainEjectStartedEvent
    {
        public readonly Villain Villain;

        public VillainEjectStartedEvent(Villain villain) => Villain = villain;
    }
}
