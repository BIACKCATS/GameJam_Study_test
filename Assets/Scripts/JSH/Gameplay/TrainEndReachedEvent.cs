namespace JSH.Gameplay
{
    /// <summary>
    /// Raised once when the player exits the last car to the right (end of the train).
    /// The last car has already been judged (LifeLost = it had unresolved villains, life -1 applied).
    /// </summary>
    public readonly struct TrainEndReachedEvent
    {
        public readonly TrainCar LastCar;
        public readonly int UnresolvedVillains;
        public readonly bool LifeLost;

        public TrainEndReachedEvent(TrainCar lastCar, int unresolvedVillains, bool lifeLost)
        {
            LastCar = lastCar;
            UnresolvedVillains = unresolvedVillains;
            LifeLost = lifeLost;
        }
    }
}
