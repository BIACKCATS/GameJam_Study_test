namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when the player crosses into another train car (platform).
    /// UnresolvedVillains / LifeLost describe the judgment of the car being left (forward moves only).
    /// </summary>
    public readonly struct CarChangedEvent
    {
        public readonly TrainCar From;
        public readonly TrainCar To;
        public readonly bool IsForward;
        public readonly int UnresolvedVillains;
        public readonly bool LifeLost;

        public CarChangedEvent(TrainCar from, TrainCar to, bool isForward, int unresolvedVillains, bool lifeLost)
        {
            From = from;
            To = to;
            IsForward = isForward;
            UnresolvedVillains = unresolvedVillains;
            LifeLost = lifeLost;
        }
    }
}
