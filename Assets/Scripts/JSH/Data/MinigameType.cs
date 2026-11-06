namespace JSH.Data
{
    /// <summary>What MinigameDirector is running for the current target (one session can go Timing -> Counterattack).</summary>
    public enum MinigameType
    {
        None = 0,
        Qte = 5,
        // 1 was Reflex (removed 2026-11-05). Do not reuse.
        Timing = 2,        // 설득 minigame
        Eject = 3,         // 퇴치 on a villain: in-scene "thrown off the train" event
        Counterattack = 4, // 퇴치 on a passenger, or failed persuasion: counterattack event, life -1
    }
}
