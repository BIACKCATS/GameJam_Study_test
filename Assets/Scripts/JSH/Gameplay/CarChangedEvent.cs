namespace JSH.Gameplay
{
    /// <summary>
    /// Raised when the next car is set up (while the curtain is black: old NPCs gone, player at the start point,
    /// new NPCs spawned). Carries the judgment of the car that was left (once per car):
    ///  - UnresolvedVillains > 0 -> LifeLost (life -1)
    ///  - UnresolvedVillains == 0 -> Cleared (clean pass, life +1)
    /// </summary>
    public readonly struct CarChangedEvent
    {
        public readonly int FromIndex;
        public readonly int ToIndex;
        public readonly int UnresolvedVillains;
        public readonly bool LifeLost;
        public readonly bool Cleared;

        public CarChangedEvent(int fromIndex, int toIndex, int unresolvedVillains, bool lifeLost, bool cleared)
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
            UnresolvedVillains = unresolvedVillains;
            LifeLost = lifeLost;
            Cleared = cleared;
        }
    }
}
