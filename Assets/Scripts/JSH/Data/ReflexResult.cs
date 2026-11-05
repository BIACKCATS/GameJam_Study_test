namespace JSH.Data
{
    public enum ReflexResult
    {
        None = 0,
        VillainDefeatedOpen = 1,    // hit while open: defeated, no life loss
        VillainDefeatedGuarded = 2, // hit while guarding: defeated, life -1
        PassengerHit = 3,           // hit a passenger (any state): life -1, counterattack
        TimedOut = 4,               // no hit within the time limit: no effect
    }
}
