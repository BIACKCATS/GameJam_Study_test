namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when the player crosses into another train car (platform).
    /// Judgment of the car being left (forward moves only, once per car):
    ///  - UnresolvedVillains > 0 -> LifeLost (life -1)
    ///  - UnresolvedVillains == 0 -> Cleared (clean pass, life +1). Next-car NPC setup hooks in here.
    /// </summary>
    public readonly struct CarChangedEvent
    {
        public readonly TrainCar From;
        public readonly TrainCar To;
        public readonly bool IsForward;
        public readonly int UnresolvedVillains;
        public readonly bool LifeLost;
        public readonly bool Cleared;

        public CarChangedEvent(TrainCar from, TrainCar to, bool isForward, int unresolvedVillains, bool lifeLost, bool cleared)
        {
            From = from;
            To = to;
            IsForward = isForward;
            UnresolvedVillains = unresolvedVillains;
            LifeLost = lifeLost;
            Cleared = cleared;
        }
    }
}
