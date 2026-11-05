namespace JSH.Data
{
    public enum ReflexResult
    {
        None = 0,
        VillainDefeatedOpen = 1,    // hit while open: defeated, no life loss
        VillainDefeatedGuarded = 2, // hit while guarding: villain counterattack, life -1, defeated
        PassengerHit = 3,           // hit a passenger (any state): passenger counterattack, life -1
        // 4 was TimedOut (removed: Reflex has no time limit). Do not reuse.
    }
}
