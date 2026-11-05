namespace JSH.Gameplay
{
    /// <summary>Raised once when a villain is reformed (good-ending tracking listens to this).</summary>
    public readonly struct VillainConvertedEvent
    {
        public readonly Villain Villain;

        public VillainConvertedEvent(Villain villain) => Villain = villain;
    }
}
