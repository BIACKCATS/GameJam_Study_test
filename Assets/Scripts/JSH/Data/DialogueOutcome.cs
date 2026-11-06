namespace JSH.Data
{
    /// <summary>Result written on the last line of a dialogue tree (CSV "Outcome" column).</summary>
    public enum DialogueOutcome
    {
        None = 0,    // dialogue just ends, no event
        Embrace = 1, // 포용: Timing minigame (persuade)
        Defeat = 2,  // 퇴치: Timing minigame. Villain success = eject; passenger = counterattack (life -1) either way
    }
}
