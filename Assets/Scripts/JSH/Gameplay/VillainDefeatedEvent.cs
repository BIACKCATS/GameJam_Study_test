namespace JSH.Gameplay
{
    /// <summary>Raised once when an accused villain is defeated (end of the eject event, just before it leaves the car).</summary>
    public readonly struct VillainDefeatedEvent
    {
        public readonly Villain Villain;

        public VillainDefeatedEvent(Villain villain) => Villain = villain;
    }
}
