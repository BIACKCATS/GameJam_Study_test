namespace JSH.Gameplay
{
    /// <summary>Raised when the player hits a normal passenger in the Reflex minigame (counterattack feedback hook).</summary>
    public readonly struct PassengerCounterattackEvent
    {
        public readonly Talkable Passenger;
        public readonly bool WasGuarding;

        public PassengerCounterattackEvent(Talkable passenger, bool wasGuarding)
        {
            Passenger = passenger;
            WasGuarding = wasGuarding;
        }
    }
}
