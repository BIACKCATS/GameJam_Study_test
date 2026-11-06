namespace JSH.Data
{
    public enum CounterattackReason
    {
        None = 0,
        AccusedPassenger = 1, // chose 퇴치 on a normal passenger
        PersuadeFailed = 2,   // villain persuasion missed or timed out (passengers just restart the dialogue)
        DefeatFailed = 3,     // villain 퇴치 minigame missed or timed out
    }
}
