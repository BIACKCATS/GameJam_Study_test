namespace JSH.Data
{
    public enum CounterattackReason
    {
        None = 0,
        AccusedPassenger = 1, // chose 퇴치 on a normal passenger
        PersuadeFailed = 2,   // persuasion missed or timed out (villain or passenger)
    }
}
